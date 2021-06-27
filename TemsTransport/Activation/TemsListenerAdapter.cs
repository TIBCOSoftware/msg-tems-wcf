/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Threading;
using System.Web.Configuration;

using com.tibco.wcf.tems.ActivatorService;
using EmsMessage = TIBCO.EMS.Message;

namespace com.tibco.wcf.tems.ActivatorService.Activation
{
    public class TemsListenerAdapter : CommunicationObject
    {
        static TemsListenerAdapter singleton;
        WebhostListenerCallbacks webHostCallbacks;
        int protocolHandle;
        ManualResetEvent initializedEvent;
        UriLookupTable<App> appQueue;
        AppManager appManager;
        Dictionary<string, TemsListener> listeners;
        DataReceivedCallback dataReceivedCallback;

        TemsListenerAdapter()
        {
            webHostCallbacks = new WebhostListenerCallbacks();
            webHostCallbacks.dwBytesInCallbackStructure = Marshal.SizeOf(webHostCallbacks);

            webHostCallbacks.applicationAppPoolChanged = new WCB.ApplicationAppPoolChanged(OnApplicationAppPoolChanged);
            webHostCallbacks.applicationBindingsChanged = new WCB.ApplicationBindingsChanged(OnApplicationBindingsChanged);
            webHostCallbacks.applicationCreated = new WCB.ApplicationCreated(OnApplicationCreated);
            webHostCallbacks.applicationDeleted = new WCB.ApplicationDeleted(OnApplicationDeleted);
            webHostCallbacks.applicationPoolAllListenerChannelInstancesStopped = new WCB.ApplicationPoolAllListenerChannelInstancesStopped(OnApplicationPoolAllListenerChannelInstancesStopped);
            webHostCallbacks.applicationPoolCanOpenNewListenerChannelInstance = new WCB.ApplicationPoolCanOpenNewListenerChannelInstance(OnApplicationPoolCanOpenNewListenerChannelInstance);
            webHostCallbacks.applicationPoolCreated = new WCB.ApplicationPoolCreated(OnApplicationPoolCreated);
            webHostCallbacks.applicationPoolDeleted = new WCB.ApplicationPoolDeleted(OnApplicationPoolDeleted);
            webHostCallbacks.applicationPoolIdentityChanged = new WCB.ApplicationPoolIdentityChanged(OnApplicationPoolIdentityChanged);
            webHostCallbacks.applicationPoolStateChanged = new WCB.ApplicationPoolStateChanged(OnApplicationPoolStateChanged);
            webHostCallbacks.applicationRequestsBlockedChanged = new WCB.ApplicationRequestsBlockedChanged(OnApplicationRequestsBlockedChanged);
            webHostCallbacks.configManagerConnected = new WCB.ConfigManagerConnected(OnConfigManagerConnected);
            webHostCallbacks.configManagerDisconnected = new WCB.ConfigManagerDisconnected(OnConfigManagerDisconnected);
            webHostCallbacks.configManagerInitializationCompleted = new WCB.ConfigManagerInitializationCompleted(OnConfigManagerInitializationCompleted);

            initializedEvent = new ManualResetEvent(false);
            appManager = new AppManager();
            appQueue = new UriLookupTable<App>();
            listeners = new Dictionary<string, TemsListener>();  // key is listenerURI + svcFileName
            dataReceivedCallback = new DataReceivedCallback(OnDataReceived);
            WasHelper.ts.TraceInformation("TemsListenerAdapter: constructor - complete");
        }

        public static void Start()
        {
            if (singleton != null)
            {
                throw new InvalidOperationException("Start - Only one instance of TemsListenerAdapter is allowed.");
            }

            singleton = new TemsListenerAdapter();
            singleton.Open();
        }

        public static void Stop()
        {
            if (singleton == null)
            {
                throw new InvalidOperationException("Stop - The TemsListenerAdapter is not started.");
            }
          
            singleton.Close(singleton.DefaultCloseTimeout);
            singleton = null;
        }

        string GetSvcFileName(string aEndPointAddr)
        {
            // returns name of .svc file from input string
            string svcFileName = null;

            if (aEndPointAddr != null && aEndPointAddr.Length > 0)
            {
                int idx = aEndPointAddr.IndexOf("net.tems");

                if (idx == 0)
                { // only look at net.tems endpoints addresses
                    idx = aEndPointAddr.IndexOf(".svc");
                    if (idx != -1)
                    {
                        svcFileName = aEndPointAddr.Substring(0, idx);
                        svcFileName = svcFileName.Substring(svcFileName.LastIndexOf('/') + 1);
                        svcFileName = svcFileName.ToLower();
                    }
                }
            }
            return svcFileName;
        }

        void OnDataReceived(string wcfDestination)
        {
            // called from TemsListener.Receive and runs in that thread.
            try
            {
                Uri uri = new UriBuilder(wcfDestination).Uri;
                WasHelper.ts.TraceInformation("TemsListenerAdapter: onDataReceived - " + uri.AbsoluteUri);
                int hresult = 0;
                AppInstance appInstance = null;
                App app = appQueue.Lookup(uri);

                if (app == null)
                {
                    throw new ArgumentNullException("No App found for client destination = " + wcfDestination);
                }

                string svcFileName = GetSvcFileName(wcfDestination);

                lock (ThisLock)
                {
                    appInstance = new AppInstance(app.AppKey, uri.LocalPath, svcFileName);
                    WasHelper.ts.TraceInformation("TemsListenerAdapter: OnDataReceived - AppInstance created appkey = {0}, virtualPath = {1}, instanceId = {2}, svcFileName = {3}", app.AppKey, uri.LocalPath, appInstance.Id, svcFileName);
                    hresult = WasHelper.OpenListenerChannelInstance(this.protocolHandle,
                                                            app.AppPoolId, appInstance.Id, appInstance.Serialize());
                }

                if (hresult != 0)
                {
                    String errmsg = "TemsListenerAdapter: OnDataReceived failed hresult = " + hresult + " , wcfDestination = ";
                    WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), errmsg);
                    throw new Exception(errmsg);
                }
                else
                {
                    lock (app.Instances)
                    {
                        // save only one appInstance per .svc
                        if (!app.Instances.ContainsKey(svcFileName))
                        {
                            app.Instances.Add(svcFileName, appInstance);
                        }
                    }
                    WasHelper.ts.TraceInformation("TemsListenerAdapter: onDataReceived - OpenListenerChannelInstance successful appkey = " + app.AppKey + ", virutalPath = " + uri.LocalPath);
                }
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("TemsListener: OnDataReceived Exception: {0}", ex.Message);
                if (ex.InnerException != null)
                {
                    errMsg = errMsg + ", Inner Exception = " + ex.InnerException.Message;
                }
                WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), errMsg + "\n" + ex.StackTrace);
                EventLog.WriteEntry(WasTemsInfo.ServiceName, errMsg, EventLogEntryType.Error);
            }
        }

        void StartListen(App app, string aSvcFileName)
        {
            if (!app.RequestsBlocked)
            {
                Uri listenerURI = new UriBuilder(WasTemsInfo.TemsScheme, app.Host, app.Port, app.Path).Uri;
                TemsListener listener;
                try
                {
                    Configuration rc = WebConfigurationManager.OpenWebConfiguration(listenerURI.AbsolutePath, app.SiteName);

                    if (rc.HasFile)
                    {
                        ServicesSection ss = (ServicesSection)rc.SectionGroups["system.serviceModel"].Sections["services"];
                        BindingsSection bs = BindingsSection.GetSection(rc);

                        if (ss == null || bs == null || ss.Services.Count == 0)
                        {
                            throw new ArgumentException("TemsListener: constructor - configuration error - missing Bindings and/or Services sections or no service defined. No Tems Listener started. Check web.config for uri = " + listenerURI.AbsoluteUri);
                        }

                        ServiceElementCollection startServices;

                        if (aSvcFileName != null)  // if true then start only that service
                        {
                            startServices = new ServiceElementCollection();
                            string wcfServiceName;

                            if (app.wcfServices.TryGetValue(aSvcFileName, out wcfServiceName))
                            {
                                // find service to start listener
                                for (int cnt = 0; cnt < ss.Services.Count; cnt++)
                                {
                                    if (ss.Services[cnt].Name.Equals(wcfServiceName))
                                    {
                                        startServices.Add(ss.Services[cnt]);
                                        break;
                                    }
                                }
                            }
                            else
                            { // should NEVER be here ... this would be a logic error!
                                string errMsg = String.Format("TemsListenerAdapter: StartListen - missing Service Name = {0}, listenerURI = {1}, appkey = {2}", aSvcFileName, listenerURI.AbsoluteUri, app.AppKey);
                                WasHelper.ts.TraceEvent(TraceEventType.Warning, WasHelper.GetErrSeq(), errMsg);
                                EventLog.WriteEntry(WasTemsInfo.ServiceName, errMsg, EventLogEntryType.Warning);
                            }
                        }
                        else
                        {
                            startServices = ss.Services;
                            lock (app.listenerKeys)
                            {
                                app.listenerKeys.Clear();  // rebuild list
                                app.wcfServices.Clear();  // maps .svc filename to wcf service name
                            }
                        }

                        string serviceName = "na"; // name of service (not necessarily name of .svc file)
                        string listenerKey;
                        string svcFileName = null;  // name of .svc file

                        for (int cnt = 0; cnt < startServices.Count; cnt++)
                        {
                            serviceName = startServices[cnt].Name;

                            if (startServices[cnt].Endpoints.Count > 0)
                            {
                                if (aSvcFileName == null)
                                {
                                    foreach (ServiceEndpointElement endpoint in startServices[cnt].Endpoints)
                                    {
                                        svcFileName = GetSvcFileName(endpoint.Address.OriginalString);
                                        if (svcFileName != null)
                                            break;
                                    }
                                }
                                else
                                {
                                    svcFileName = aSvcFileName;
                                }

                                if (svcFileName != null)
                                {
                                    listenerKey = (listenerURI.AbsoluteUri + svcFileName);

                                    if (!listeners.TryGetValue(listenerKey, out listener))
                                    {
                                        AppInstance instance = null;

                                        if (app.Instances.TryGetValue(svcFileName, out instance))
                                        {
                                            WasHelper.ts.TraceEvent(TraceEventType.Warning, WasHelper.GetErrSeq(), "TemsListenerAdapter: StartListen - app instance already exists listenerURI = " + listenerURI.AbsoluteUri + ", appkey = " + app.AppKey + ", svcFileName = " + svcFileName + ", instance id = " + instance.Id);
                                            lock (app.Instances)
                                            {
                                                app.Instances.Remove(svcFileName);  // set so new worker process will be started when data received
                                            }
                                        }

                                        listener = new TemsListener(listenerURI, app.SiteName, dataReceivedCallback, rc, startServices[cnt].Endpoints, serviceName);

                                        if (listener.listenerCnt > 0)
                                        {
                                            lock (listeners)
                                            {
                                                listeners.Add(listenerKey, listener);
                                            }
                                            lock (app.listenerKeys)
                                            {
                                                if (aSvcFileName == null)
                                                {
                                                    app.listenerKeys.Add(listenerKey);  // rebuild list
                                                    app.wcfServices.Add(svcFileName, serviceName);
                                                }
                                            }
                                            appQueue.Add(listenerURI, app);
                                            WasHelper.ts.TraceInformation("TemsListenerAdapter: StartListen - Add to appque listenerURI = " + listenerURI.AbsoluteUri + ", Service Name = " + serviceName);
                                            listener.Listen();
                                        }
                                        else  // no tems endpoints so listener will not be used.
                                        {
                                            WasHelper.ts.TraceInformation("TemsListenerAdapter: StartListen - no TEMS endpoints listenerURI = " + listenerURI.AbsoluteUri + ", Service Name = " + serviceName);
                                        }
                                    }
                                    else
                                    { // should NEVER be here ... this would be a logic error!
                                        string errMsg = String.Format("TemsListenerAdapter: StartListen - listener already exists Service Name = {0}, listenerURI = {1}, appkey = {2}", serviceName, listenerURI.AbsoluteUri, app.AppKey);
                                        WasHelper.ts.TraceEvent(TraceEventType.Warning, WasHelper.GetErrSeq(), errMsg);
                                        EventLog.WriteEntry(WasTemsInfo.ServiceName, errMsg, EventLogEntryType.Warning);
                                    }
                                }
                                else
                                {
                                    WasHelper.ts.TraceInformation("TemsListenerAdapter: StartListen - No TEMS endpoints on service listenerURI = " + listenerURI.AbsoluteUri + ", Service Name = " + serviceName);
                                }
                            }
                            else
                            {
                                throw new ArgumentException("TemsListener: constructor - configuration error - no endpoints defined. Check web.config for uri = " + listenerURI.AbsoluteUri + " , Service Name = " + serviceName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = String.Format("TemsListenerAdapter: StartListen Exception: {0}", ex.Message);
                    if (ex.InnerException != null)
                    {
                        errMsg = errMsg + ", Inner Exception = " + ex.InnerException.Message;
                    }
                    WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), errMsg);
                    EventLog.WriteEntry(WasTemsInfo.ServiceName, errMsg, EventLogEntryType.Error);
                }
            }
        }

        void StopListen(App app)
        {
            Uri listenerURI = new UriBuilder(WasTemsInfo.TemsScheme, app.Host, app.Port, app.Path).Uri;
            TemsListener listener;

            lock (app.listenerKeys)
            {
                foreach (string listenerKey in app.listenerKeys)
                {
                    lock (listeners)
                    {
                        if (listeners.TryGetValue(listenerKey, out listener))
                        {
                            listener.StopListen();
                            listeners.Remove(listenerKey);
                        }
                    }
                }
            }

            appQueue.Remove(listenerURI);
            WasHelper.ts.TraceInformation("TemsListenerAdapter: StopListen - appKey = " + app.AppKey + ", AppPoolId = " + app.AppPoolId);
        }

        void StopListen(string listenerKey)
        {
            TemsListener listener;
            WasHelper.ts.TraceInformation("TemsListenerAdapter: StopListen - Key = " + listenerKey);

            lock (listeners)
            {
                if (listeners.TryGetValue(listenerKey, out listener))
                {
                    listener.StopListen();
                    listeners.Remove(listenerKey);
                }
            }
        }

        void StopWorker(App app)
        {
            int result = 0;
            lock (app.Instances)
            {
                foreach (KeyValuePair<string, AppInstance> instance in app.Instances)
                {
                    result = WasHelper.CloseAllListenerChannelInstances(this.protocolHandle, app.AppPoolId, instance.Value.Id);
                    WasHelper.ts.TraceInformation("TemsListenerAdapter: StopWorker - appKey = " + app.AppKey + ", AppPoolId = " + app.AppPoolId + ", svcFileName = " + instance.Value.SvcFileName + ", status = " + result);
                }
                app.Instances.Clear();
            }
        }

        string[] ParseBindings(IntPtr bindingsMultiSz, int numberOfBindings)
        {
            if (bindingsMultiSz == IntPtr.Zero)
            {
                throw new ArgumentNullException("bindingsMultiSz");
            }

            if (numberOfBindings < 0)
            {
                throw new ArgumentException("numberOfBindings");
            }

            string[] bindings = new string[numberOfBindings];
            IntPtr bindingsBufferPtr = bindingsMultiSz;

            for (int i = 0; i < numberOfBindings; i++)
            {
                string bindingString = Marshal.PtrToStringUni(bindingsBufferPtr);
                if (string.IsNullOrEmpty(bindingString))
                {
                    throw new ArgumentException("bindingsMultiSz");
                }

                bindings[i] = bindingString;
                bindingsBufferPtr = (IntPtr)(bindingsBufferPtr.ToInt64() + (bindingString.Length + 1) * sizeof(ushort));
            }

            return bindings;
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            bool isTimeout;

            try
            {
                initializedEvent.Reset();
                WasHelper.RegisterProtocol(WasTemsInfo.TemsScheme, ref webHostCallbacks, out protocolHandle);
                isTimeout = initializedEvent.WaitOne(timeout, true);
                WasHelper.ts.TraceInformation("TemsListenerAdapter: OnOpen - initializedEvent timeout = " + isTimeout + " timeout (ms) = " + timeout.TotalMilliseconds );
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("TemsListenerAdapter: OnOpen Exception: {0}", ex.Message);
                if (ex.InnerException != null)
                {
                    errMsg = errMsg + ", Inner Exception = " + ex.InnerException.Message;
                }
                WasHelper.ts.TraceEvent(TraceEventType.Critical, WasHelper.GetErrSeq(), errMsg + "\n" + ex.StackTrace);
                EventLog.WriteEntry(WasTemsInfo.ServiceName, errMsg, EventLogEntryType.Error);

            }
        }

        protected override void OnClosing()
        {
            App app;
            WasHelper.ts.TraceInformation("TemsListenerAdapter: OnClosing");

            try
            {
                base.OnClosing();
                string[] appKeys = new string[appManager.Apps.Count];
                appManager.Apps.Keys.CopyTo(appKeys, 0);
                foreach (string appKey in appKeys)
                {
                    app = appManager.Apps[appKey];
                    StopListen(app);   // stop and remove TEMS listener if running
                    StopWorker(app);
                }
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("TemsListenerAdapter: OnClosing Exception: {0}", ex.Message);
                if (ex.InnerException != null)
                {
                    errMsg = errMsg + ", Inner Exception = " + ex.InnerException.Message;
                }
                WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), errMsg);
            }
        }
        
        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                return TimeSpan.FromSeconds(10);
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                return TimeSpan.FromSeconds(30);
            }
        }

        #region webhost_callback_impl
        void OnConfigManagerConnected(IntPtr context)
        {
            WasHelper.ts.TraceInformation("TemsListenerAdapter CB: OnConfigManagerConnected");
        }

        void OnConfigManagerConnectRejected(IntPtr context, int hresult)
        {
            WasHelper.ts.TraceInformation("TemsListenerAdapter CB: OnConfigManagerConnectRejected hresult = " + hresult);
        }

        void OnConfigManagerDisconnected(IntPtr context, int hresult)
        {
            WasHelper.ts.TraceInformation("TemsListenerAdapter CB: OnConfigManagerDisconnected hresult = " + hresult);     
        }

        void OnConfigManagerInitializationCompleted(IntPtr context)
        {
            Debug.Assert(this.State == CommunicationState.Opening);
            initializedEvent.Set();
            WasHelper.ts.TraceInformation("TemsListenerAdapter CB: OnConfigManagerInitializationCompleted - initializedEvent set");
        }

        void OnApplicationCreated(IntPtr context, [MarshalAs(UnmanagedType.LPWStr)] string appKey, [MarshalAs(UnmanagedType.LPWStr)] string path, int siteId, [MarshalAs(UnmanagedType.LPWStr)] string appPoolId, IntPtr bindingsMultiSz, int numberOfBindings, bool requestsBlocked)
        {

            App app = null;
            string[] bindings = null;
            WasHelper.ts.TraceInformation("TemsListenerAdapter CB: OnApplicationCreated appKey = " + appKey);

            try
            {
                bindings = ParseBindings(bindingsMultiSz, numberOfBindings);

                // An app can only listen on one queue (i.e. only one URI)
                app = new App(appKey, path, siteId, appPoolId, bindings);

                if (!app.UpdateBindingInfo(bindings, true, appKey, app.SiteName))
                {
                    app.RequestsBlocked = true;  // block requests until properly configured
                }
                else 
                {
                    app.RequestsBlocked = requestsBlocked;
                }

                lock (appManager.Apps)
                {
                    appManager.Apps.Add(appKey, app);
                }

                StartListen(app, null);
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("TemsListenerAdapter: OnApplicationCreated: appKey = {0}, err = {1}", appKey, ex.Message);

                if (ex.InnerException != null)
                {
                    errMsg = errMsg + ", Inner Exception = " + ex.InnerException.Message;
                }

                WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), errMsg);
                EventLog.WriteEntry(WasTemsInfo.ServiceName, errMsg, EventLogEntryType.Error);
            }
        }

        void OnApplicationPoolAllListenerChannelInstancesStopped(IntPtr context, [MarshalAs(UnmanagedType.LPWStr)] string appPoolId, int listenerChannelId)
        {
            WasHelper.ts.TraceInformation("TemsListenerAdapter CB: PoolAllListenerChannelInstancesStopped");
        }

        void OnApplicationAppPoolChanged(IntPtr context, [MarshalAs(UnmanagedType.LPWStr)] string appKey, [MarshalAs(UnmanagedType.LPWStr)] string appPoolId)
        {
            App app = null;

            WasHelper.ts.TraceInformation("TemsListenerAdapter CB: OnApplicationAppPoolChanged appKey = " + appKey + ", appPoolId = " + appPoolId);
            try
            {
                if (appManager.Apps.ContainsKey(appKey))
                {
                    app = appManager.Apps[appKey];
                    app.AppPoolId = appPoolId;
                    lock (app.Instances)
                    {
                        app.Instances.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("TemsListenerAdapter: OnApplicationAppPoolChanged appKey {0}, pool id = {1}, error = {2}", appKey, appPoolId, ex.Message);
                if (ex.InnerException != null)
                {
                    errMsg = errMsg + ", Inner Exception = " + ex.InnerException.Message + "\n" + ex.InnerException.StackTrace;
                }
                WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), errMsg + "\n" + ex.StackTrace);
                EventLog.WriteEntry(WasTemsInfo.ServiceName, errMsg, EventLogEntryType.Error);
            }
        }

        void OnApplicationBindingsChanged(IntPtr context, [MarshalAs(UnmanagedType.LPWStr)] string appKey, IntPtr bindingsMultiSz, int numberOfBindings)
        {
            WasHelper.ts.TraceInformation("TemsListenerAdapter CB: OnApplicationBindingsChanged - app.AppKey = " + appKey);
            string[] bindingInfos;
            Uri listenerURI;
            App app;

            try
            {
                if (appManager.Apps.ContainsKey(appKey))
                {
                    app = appManager.Apps[appKey];
                    bindingInfos = ParseBindings(bindingsMultiSz, numberOfBindings);
                    listenerURI = new UriBuilder(WasTemsInfo.TemsScheme, app.Host, app.Port, app.Path).Uri;
                    WasHelper.ts.TraceInformation("TemsListenerAdapter: OnApplicationBindingsChanged - stopped listener - " + listenerURI.AbsoluteUri + ", appKey = " + app.AppKey);
                    StopListen(app);
                    StopWorker(app);
                    app.UpdateBindingInfo(bindingInfos, false, appKey, app.SiteName);
                    StartListen(app, null);
                }
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("TemsListenerAdapter: OnApplicationBindingsChanged appKey {0}, {1}", appKey, ex.Message);

                if (ex.InnerException != null)
                {
                    errMsg = errMsg + ", Inner Exception = " + ex.InnerException.Message;
                }

                WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), errMsg);
                EventLog.WriteEntry(WasTemsInfo.ServiceName, errMsg, EventLogEntryType.Error);
            }
        }

        void OnApplicationDeleted(IntPtr context, [MarshalAs(UnmanagedType.LPWStr)] string appKey)
        {
            Uri listenerURI;
            WasHelper.ts.TraceInformation("TemsListenerAdapter CB: OnApplicationDeleted - appkey = " + appKey);
            App app;

            try
            {
                if (appManager.Apps.ContainsKey(appKey))
                {
                    app = appManager.Apps[appKey];
                    listenerURI = new UriBuilder(WasTemsInfo.TemsScheme, app.Host, app.Port, app.Path).Uri;
                    StopListen(app);
                    StopWorker(app);

                    lock (appManager.Apps)
                    {
                        appManager.Apps.Remove(appKey);
                    }
                }
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("TemsListenerAdapter: OnApplicationDeleted appKey {0}, {1}", appKey, ex.Message);

                if (ex.InnerException != null)
                {
                    errMsg = errMsg + ", Inner Exception = " + ex.InnerException.Message;
                }

                WasHelper.ts.TraceEvent(TraceEventType.Warning, WasHelper.GetErrSeq(), errMsg);
            }
        }

        void OnApplicationPoolCanOpenNewListenerChannelInstance(IntPtr context, [MarshalAs(UnmanagedType.LPWStr)] string appPoolId, int listenerChannelId)
        {
            // Event occurs on worker thread timeout or starting app pool (after stopping it with worker thread running) ==> listener stopped
            Uri listenerURI;
            string svcFileName = null;
            string listenerKey = "na";
            string appKey = "na";

            try
            {
                WasHelper.ts.TraceInformation("TemsListenerAdapter CB: OnApplicationPoolCanOpenNewListenerChannelInstance appPoolId = " + appPoolId + ", listenerChannelId = " + listenerChannelId);

                foreach (App app in appManager.Apps.Values)
                {
                    lock (app.Instances)
                    {
                        foreach (AppInstance instance in app.Instances.Values)
                        {
                            if (instance.Id == listenerChannelId)
                            {
                                svcFileName = instance.SvcFileName;
                                break;
                            }
                        }
                    }

                    if (svcFileName != null)  // found app instance
                    {
                        listenerURI = new UriBuilder(WasTemsInfo.TemsScheme, app.Host, app.Port, app.Path).Uri;
                        listenerKey = listenerURI + svcFileName;
                        WasHelper.ts.TraceInformation("TemsListenerAdapter: OnApplicationPoolCanOpenNewListenerChannelInstance stopped listener - " + listenerURI.AbsoluteUri + ", appKey = " + app.AppKey + ", svc file =" + svcFileName);
                        StopListen(listenerKey);

                        lock (app.Instances)
                        {
                            app.Instances.Remove(svcFileName);  // no worker to stop
                        }

                        app.UpdateBindingInfo(null, true, app.AppKey, app.SiteName);
                        StartListen(app, svcFileName);
                        WasHelper.ts.TraceInformation("TemsListenerAdapter: OnApplicationPoolCanOpenNewListenerChannelInstance started listener - " + listenerURI.AbsoluteUri + ", appKey = " + app.AppKey + ", svc file =" + svcFileName);
                        break;  // found app instance
                    }
                }
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("TemsListenerAdapter: OnApplicationPoolCanOpenNewListenerChannelInstance exception appKey={0}, appPoolId={1}, listenerChannelId={2}\nerr = {3}", appKey, appPoolId, listenerChannelId, ex.Message);

                if (ex.InnerException != null)
                {
                    errMsg = errMsg + ", Inner Exception = " + ex.InnerException.Message;
                }

                WasHelper.ts.TraceEvent(TraceEventType.Warning, WasHelper.GetErrSeq(), errMsg);
                EventLog.WriteEntry(WasTemsInfo.ServiceName, errMsg, EventLogEntryType.Warning);
            }
        }

        void OnApplicationPoolCreated(IntPtr context, [MarshalAs(UnmanagedType.LPWStr)] string appPoolId, IntPtr sid)
        {
            WasHelper.ts.TraceInformation("TemsListenerAdapter CB: OnApplicationPoolCreated - appPoolId = " + appPoolId);
        }

        void OnApplicationPoolDeleted(IntPtr context, [MarshalAs(UnmanagedType.LPWStr)] string appPoolId)
        {
            WasHelper.ts.TraceInformation("TemsListenerAdapter CB: OnApplicationPoolDeleted - appPoolId = " + appPoolId);
        }

        void OnApplicationPoolIdentityChanged(IntPtr context, [MarshalAs(UnmanagedType.LPWStr)] string appPoolId, IntPtr sid)
        {
            WasHelper.ts.TraceInformation("TemsListenerAdapter CB: OnApplicationPoolIdentityChanged - appPoolId = " + appPoolId);
        }

        void OnApplicationPoolStateChanged(IntPtr context, [MarshalAs(UnmanagedType.LPWStr)] string appPoolId, bool isEnabled)
        { // Don't really care about application pool state.  If application pool is disabled will still listen to EMS Q and
          // and IIS will remember request to ensure worker started when application pool started.
            WasHelper.ts.TraceInformation("TemsListenerAdapter CB: OnApplicationPoolStateChanged - appPoolId = " + appPoolId + " isEnabled = " + isEnabled);
        }

        void OnApplicationRequestsBlockedChanged(IntPtr context, [MarshalAs(UnmanagedType.LPWStr)] string appKey, bool requestsBlocked)
        { // This is called when net.tems binding is added or removed from app
            App app = null;
            WasHelper.ts.TraceInformation("TemsListenerAdapter CB: OnApplicationRequestsBlockedChanged - appKey = " + appKey + " requestsBlocked = " + requestsBlocked);

            if (appManager.Apps.ContainsKey(appKey))
            {
                app = appManager.Apps[appKey];
                app.RequestsBlocked = requestsBlocked;

                if (!requestsBlocked)
                {
                    StartListen(app, null);
                    WasHelper.ts.TraceInformation("TemsListenerAdapter: OnApplicationRequestsBlockedChanged - StartListen appKey = " + appKey);
                }
                else
                {
                    StopListen(app);
                    StopWorker(app);
                }
            }
            else
            {
                WasHelper.ts.TraceInformation("TemsListenerAdapter: OnApplicationRequestsBlockedChanged key not in table - appKey = " + appKey);
            }
        }
        #endregion

        protected override void OnAbort()
        {
            WasHelper.ts.TraceInformation("TemsListenerAdapter: OnAbort");
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnClose(timeout);

            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnOpen(timeout);
            WasHelper.ts.TraceInformation("TemsListenerAdapter: OnBeginOpen");

            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            WasHelper.ts.TraceInformation("TemsListenerAdapter: OnClose timeout (ms) = " + timeout.TotalMilliseconds);
            WasHelper.UnregisterProtocol(protocolHandle);
            WasHelper.ts.TraceInformation("TemsListenerAdapter: OnClose - After UnregisterProtocol");
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            WasHelper.ts.TraceInformation("TemsListenerAdapter: OnEndClose");
            CompletedAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            WasHelper.ts.TraceInformation("TemsListenerAdapter: OnEndOpen");
            CompletedAsyncResult.End(result);
        }
    }
}
