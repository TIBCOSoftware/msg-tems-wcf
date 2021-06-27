/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel.Configuration;
using System.Threading;
using System.Web.Configuration;

using WebAdmin = Microsoft.Web.Administration;
using EmsMessage = TIBCO.EMS.Message;

namespace com.tibco.wcf.tems.ActivatorService.Activation
{
    [DataContract]
    class AppInstance
    {
        static int currentId = 1;

        [DataMember]
        int id;

        [DataMember]
        string appKey;

        [DataMember]
        string virtualPath;

        /**
         * name of .svc file
         **/
        [DataMember]
        string svcFileName;  

        internal AppInstance(string appKey, string virtualPath, string svcFileName)
        {
            id = Interlocked.Increment(ref currentId);
            this.appKey = appKey;
            this.virtualPath = virtualPath;
            this.svcFileName = svcFileName;
        }

        internal int Id
        {
            get
            {
                return this.id;
            }
        }

        internal string AppKey
        {
            get
            {
                return this.appKey;
            }
        }

        internal string VirtualPath
        {
            get
            {
                return virtualPath;
            }
        }

        internal string SvcFileName
        {
            get
            {
                return this.svcFileName;
            }
        }

        internal byte[] Serialize()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(AppInstance));
                serializer.WriteObject(memoryStream, this);
                return memoryStream.ToArray();
            }
        }

        internal static AppInstance Deserialize(byte[] blob)
        {
            using (MemoryStream memoryStream = new MemoryStream(blob))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(AppInstance));
                return (AppInstance)serializer.ReadObject(memoryStream);
            }
        }
    }

    class App
    {
         // save only one app instance per service 
        Dictionary<string, AppInstance> instances;
        // key = filename of .svc, value = service name from web.config
        internal Dictionary<string, string> wcfServices;
        string appPoolId;
        string path;
        string appKey;
        string host;
        string[] hosts;
        int port;
        int[] ports;
        //protocol:host:port
        string prefixFilter;
        string siteName;
        int siteid;
        bool requestsBlocked;
        internal List<string> listenerKeys;

        internal App(string appKey, string path, int siteId, string appPoolId, string[] bindingInfos)
        {
            this.path = path;
            this.siteid = siteId;
            this.appPoolId = appPoolId;
            this.appKey = appKey;
            this.requestsBlocked = true;
            siteName = "Default Web Site";
            WebAdmin.ServerManager serverManager = new WebAdmin.ServerManager();

            foreach (WebAdmin.Site site in serverManager.Sites)
            {
                if (site.Id == siteId)
                {
                    siteName = site.Name;
                }
            }

            instances = new Dictionary<string, AppInstance>();
            wcfServices = new Dictionary<string, string>();
            listenerKeys = new List<string>();
        }

        internal string Path
        {
            get
            {
                return path;
            }
        }

        internal string AppPoolId
        {
            get
            { 
                return appPoolId;
            }

            set
            {
                appPoolId = value;
            }
        }

        internal string AppKey
        {
            get
            {
                return appKey;
            }
        }

        internal bool RequestsBlocked
        {
            get
            {
                return this.requestsBlocked;
            }

            set
            {
                this.requestsBlocked = value;
            }
        }

        internal string SiteName
        {
            get
            {
                return siteName;
            }
        }

        internal bool UpdateBindingInfo(string[] bindingInfos, bool isUpdatePrefix, string appName, string siteName)
        {
            /**
             * Only report errors in bindings so that if app is being created it can continue
             **/
            int i;
            Uri appPrefix;
            bool isFound = false;
            string[] parts;
            string errMsg = "";
            bool isValidApp = true;

            if (bindingInfos != null)
            { 
                if (bindingInfos.Length == 0)
                {
                    isValidApp = false;
                    errMsg = "App:UpdateBindingInfo - Invalid site bindings - No net.tems bindings configured for appKey = " + appKey + " , siteName = " + siteName ;
                    WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), errMsg);
                    throw new ArgumentException(errMsg);
                }

                hosts = new string[bindingInfos.Length];
                ports = new int[bindingInfos.Length];

                for (i = 0; i < bindingInfos.Length; i++)
                {
                    // Binding format: protocol:[host]:port
                    parts = bindingInfos[i].Split(':');

                    if (!(parts.Length == 2 || parts.Length == 3) || !int.TryParse(parts[parts.Length - 1], out ports[i]))
                    {
                        isValidApp = false;
                        errMsg = "App:UpdateBindingInfo - Binding Information expected format is  protocol:[host]:port,  bindinginfo = " + bindingInfos[i] + ", appKey = " + appKey + " , siteName = " + siteName;
                        WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), errMsg);
                        throw new ArgumentException(errMsg);
                    }

                    if (parts.Length == 3)
                        hosts[i] = parts[1];
                    else
                        hosts[i] = "localhost";
                }
            }

            i = 0;

            if (isUpdatePrefix)
            { 
                // check the web.config to see if the prefix filter has changed
                prefixFilter = "";
                Configuration rc = WebConfigurationManager.OpenWebConfiguration(path, siteName);
                if (rc.HasFile)
                {
                    ServiceHostingEnvironmentSection smeSection = (ServiceHostingEnvironmentSection)rc.GetSection("system.serviceModel/serviceHostingEnvironment");

                    foreach (BaseAddressPrefixFilterElement pfe in smeSection.BaseAddressPrefixFilters)
                    {
                        if (pfe.Prefix.AbsoluteUri.IndexOf(WasTemsInfo.TemsScheme) != -1)
                        {
                            prefixFilter = pfe.Prefix.AbsoluteUri;
                            break;
                        }
                    }
                }
                else
                {
                    isValidApp = false;
                    WasHelper.ts.TraceEvent(TraceEventType.Warning, WasHelper.GetErrSeq(), "App:UpdateBindingInfo - no web.config file found at path ={0} for appKey ={1} ", rc.FilePath, appKey);
                }
            }

            if (hosts.Length > 1)
            {
                if (prefixFilter == string.Empty)
                {
                    WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), "App:UpdateBindingInfo - more than one net.tems binding, must specify baseAddressPrefixFilters for appKey = " + appKey);
                }
                else
                {
                    for (int j = 0; j < ports.Length; j++)
                    {
                        /**<summary>
                         * If here, then more than one net.tems binding configured in IIS
                         *  A app can only listen on one URI - using web config to see if filter set.
                         * </summary>
                         **/
                        appPrefix = new UriBuilder(WasTemsInfo.TemsScheme, hosts[j], ports[j]).Uri;

                        if (prefixFilter == appPrefix.AbsoluteUri)
                        {
                            i = j;
                            isFound = true;
                            break;
                        }
                    }

                    if (!isFound)
                        WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), "App:UpdateBindingInfo - more than one net.tems binding, baseAddressPrefixFilters does not match site bindings for appKey = " + appKey);
                }
            }

            this.host = hosts[i];
            this.port = ports[i];

            return isValidApp;
        }

        internal string Host
        {
            get
            {
                return host;
            }
        }

        internal int Port
        {
            get
            {
                return port;
            }
        }

        internal Dictionary<string, AppInstance> Instances
        {
            get
            {
                 return this.instances;
            }
            set
            {
                    this.instances = value;
            }
        }
    }
 
    internal class AppManager
    {
        Dictionary<string, App> apps;

        internal AppManager()
        {
            apps = new Dictionary<string, App>(StringComparer.OrdinalIgnoreCase);
        }

        internal Dictionary<string, App> Apps
        {
            get
            {
                return this.apps;
            }
        }
    }
}
