/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel.Configuration;
using System.Threading;
using System.Web.Configuration;

using TIBCO.EMS;
using com.tibco.wcf.tems;

using EmsMessage = TIBCO.EMS.Message;

namespace com.tibco.wcf.tems.ActivatorService.Activation
{
    class TemsListener : IExceptionListener
    {
        // There is one TemsListener per .svc file (i.e. WCF service)
        string serviceName;   // name of service
        Hashtable endPoints;  // key = endpoint name, Value is the TemsTransportBindingElement 
        
        DataReceivedCallback dataReceivedCallback;
        internal long listenerCnt;

        internal TemsListener(Uri uri, String siteName, DataReceivedCallback dataReceivedCallback, Configuration rc, ServiceEndpointElementCollection endpoints, string serviceName)
        {
            this.serviceName = serviceName;  // Service Name
            TemsTransportExtensionElement temsEE = null;
            TemsTransportBindingElement temsBE = null;
            endPoints = new Hashtable();
            WasHelper.ts.TraceInformation("TemsListener: constructor uri = {0} siteName = {1}", uri.AbsoluteUri, siteName);
            listenerCnt = 0;

            /**
             * Uncomment following for debug
             * int delay = 20000;
             * Thread.Sleep(delay);
             **/ 

            string bindingConfigName = "";
            BindingsSection bs = BindingsSection.GetSection(rc);
            Hashtable custBindings = new Hashtable();  // key = bindingConfiguration Value = TemsTransportBindingElement
            // App with multiple net.tems endpoints will have a queue and ems listener for each endpoint
            foreach (ServiceEndpointElement endpoint in endpoints)
            {
                if (endpoint.Binding.Equals("customBinding", StringComparison.OrdinalIgnoreCase))
                {
                    // see if endpoint binding is a TemsTransportExtensionElement
                    bindingConfigName = endpoint.BindingConfiguration;
                    CustomBindingElement cbe = bs.CustomBinding.Bindings[bindingConfigName];

                    // see if using same binding as other endpoint
                    if (!custBindings.ContainsKey(bindingConfigName))
                    {
                        foreach (PropertyInformation pi in cbe.ElementInformation.Properties)
                        {
                            if (pi.Value.GetType().FullName == typeof(TemsTransportExtensionElement).FullName)
                            {
                                temsEE = (TemsTransportExtensionElement)pi.Value;
                                break;
                            }
                        }

                        if (temsEE == null)
                        {
                            // no binding in config file
                            temsEE = new TemsTransportExtensionElement();
                            WasHelper.ts.TraceInformation("TemsListener: constructor - creating default TemsTransportExtensionElement");
                        }

                        temsBE = (TemsTransportBindingElement)temsEE.CreateDefaultBindingElement();

                        // See if there are overridden configuration values for this binding
                        AppSettingsSection asx = (AppSettingsSection)rc.GetSection("appSettings");
                        if (asx != null)
                        {
                            /** Note:  Allow runtime override of configuration values
                             *  if key is WasITemsBindingExtension then will override all bindings with that configuration
                             * if WasITemsBindingExtension key does not exist then will check for binding configuration name
                             * to override the specific named binding.
                             **/ 
                            KeyValueConfigurationElement apExt = asx.Settings["WasITemsBindingExtension"];
                            
                            if (apExt == null)
                                apExt = asx.Settings[bindingConfigName];

                            if (apExt != null)
                            {
                                int ix = apExt.Value.IndexOf(",");
                                string typeName = apExt.Value.Substring(0, ix + 1); // include comma as assembly name appended later
                                string asmInfo = apExt.Value.Substring(ix + 1);
                                WasHelper.ts.TraceInformation("TemsListener: constructor - bindingName= {0}, typeName = {1}, assembly = {2}", bindingConfigName, typeName, asmInfo);
                                Assembly asm = Assembly.Load(asmInfo);

                                Type t = Type.GetType(typeName + asm.FullName);
                                object custObj = Activator.CreateInstance(t);

                                MethodInfo mi = t.GetMethod("CustomizeTemsBinding");
                                object[] args = new object[] { temsBE };
                                WasHelper.ts.TraceInformation("TemsListener: constructor - Invoking CustomizeTemsBinding.");
                                mi.Invoke(custObj, args);
                            }
                        }

                        CreateConnection(temsBE, uri);
                        custBindings.Add(bindingConfigName, temsBE);
                    }
                    else // use existing binding
                    {
                        temsBE = (TemsTransportBindingElement)custBindings[bindingConfigName];
                    }

                    string destAddr = GetDestAddr(endpoint.Address.OriginalString);

                    if (destAddr == null)
                        throw new ArgumentException("TemsListener: constructor - configuration error invalid endpoint = " + endpoint.Address.OriginalString + " No Tems Listener started. Check web.config for uri = " + uri.AbsoluteUri + ", Service Name = " + serviceName);

                    endPoints.Add(destAddr, temsBE);
                }
            }

            listenerCnt = endPoints.Count;
            this.dataReceivedCallback = dataReceivedCallback;
            WasHelper.ts.TraceInformation("TemsListener: constructor - complete for service = " + serviceName + ", listener cnt = " + listenerCnt);
        }

         private string GetDestAddr(string aEndPointAddr)
         {
             string dest, destAddr = null;
             int i = aEndPointAddr.IndexOf(".svc/");

             if (i > 0 && aEndPointAddr.Length > i + 5)
             {
                 dest = aEndPointAddr.Substring(i + 5);
             }
             else
             {
                 dest = aEndPointAddr;
             }

             string[] segments = dest.Split(new char[] { '/' });

             if (segments.Length == 1 && segments[0] != string.Empty)
             {
                 destAddr = segments[0] + "/Q";
             }
             else if (segments.Length == 2 && segments[1] != string.Empty)
             {
                 destAddr = segments[1] + "/" + segments[0].Substring(0, 1);
             }

             return destAddr;
         }

        /** 
         * <summary>
         *  Creates the Connection that will be used for this transport channel.
         *  
         *  If the application has set the bindingElement ConnectionFactory property then this is
         *  used to create the connection.  
         *  
         *  Otherwise a new ConnectionFactory is created.  When a new ConnectionFactory is
         *  created, it is configured from the properties set on the TemsTransportBindingElement.
         *  </summary>
         **/  
        private void CreateConnection(TemsTransportBindingElement temsBE, Uri uri)
        {
            if (temsBE.connection == null)
            {
                /** 
                 * Note: To enable reconnection behavior and fault tolerance, the serverURL parameter must
                 *  be a comma-separated list of two or more URLs. In a situation with only one server,
                 * you may supply two copies of that server's URL to enable client reconnection
                 * (for example, tcp://localhost:7222,tcp://localhost:7222).
                 **/ 
                string serverUrl = "tcp" + Uri.SchemeDelimiter + uri.Authority;
                if (temsBE.ReconnAttemptCount > 0)
                {
                    serverUrl += "," + serverUrl;
                }

                ConnectionFactory factory;
                Connection connection;
                temsBE.SessionAcknowledgeMode = TIBCO.EMS.SessionMode.ExplicitClientAcknowledge;
                temsBE.AppHandlesMessageAcknowledge = true;

                if (temsBE.IsPropertySet("ConnectionFactory"))
                {
                    factory = temsBE.ConnectionFactory;
                }
                else
                {
                    temsBE.ClientID = "";  // leave blank or will get error if more than one connection factory with this id

                    if (temsBE.ServerUrl.Length == 0)
                    {
                        WasHelper.ts.TraceInformation("TemsListener: CreateConnection - EMS connection created to serverUrl = {0}", serverUrl);
                        factory = new ConnectionFactory(serverUrl);
                    }
                    else
                    {
                        factory = new ConnectionFactory();
                    }

                    temsBE.InitializeConnectionFactory(factory);
                }

                connection = factory.CreateConnection();

                // set the exception listener
                connection.ExceptionListener = this;
                connection.Start();
                temsBE.connection = connection;
            }
        }

        public void OnException(EMSException ex)
        {
            string errMsg = String.Format("TemsListener OnException: {0}", ex.Message);

            if (ex.InnerException != null)
            {
                errMsg = errMsg + ", Inner Exception = " + ex.InnerException.Message;
            }

            WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), errMsg + "\n" + ex.StackTrace);
            EventLog.WriteEntry(WasTemsInfo.ServiceName, errMsg, EventLogEntryType.Error);
        }

        internal void Listen()
        {
            foreach (string destAddr in endPoints.Keys)
            {
                // wait for Tems message
                WaitCallback receiveCallback = new WaitCallback(Receive);
                ThreadPool.QueueUserWorkItem(receiveCallback, destAddr);
            }
            while (Interlocked.Read(ref listenerCnt) != 0)
            {
                Thread.Sleep(10); // let other "Receive" threads run to complete
            }
        }

        internal void StopListen()
        {
            WasHelper.ts.TraceInformation("TemsListener: StopListen for Service Name = " + serviceName);

            lock (endPoints)
            {
                foreach (TemsTransportBindingElement temsBE in endPoints.Values)
                {
                    if (temsBE.connection != null)
                    {
                        temsBE.connection.Close();
                        temsBE.connection = null;
                    }
                }
            }
        }

        private void Receive(object state)
        {
            EmsMessage msg = null;
            Destination requestDestination = null;
            MessageConsumer requestMessageConsumer;
            bool isTopic = false;
            string dest = (string)state;
            string[] segments = dest.Split(new char[] { '/' });  // dest = name/T  OR name/Q for name being queue or topic
            string destAddr = segments[0];
            
            if (segments[1].Equals("T",StringComparison.OrdinalIgnoreCase))
               isTopic = true;

            TemsTransportBindingElement temsBE = (TemsTransportBindingElement)endPoints[dest];
            TIBCO.EMS.SessionMode sessionAcknowledgeMode = temsBE.SessionAcknowledgeMode;
            Session session = temsBE.connection.CreateSession(false, sessionAcknowledgeMode);
            WasHelper.ts.TraceInformation("TemsListener: CreateSession successful for dest = " + dest);

            if (temsBE.IsPropertySet("EndpointDestination"))
            {
                requestDestination = temsBE.EndpointDestination;
            }
            if (requestDestination == null)
            {
                requestDestination = isTopic ?
                                    (Destination)session.CreateTopic(destAddr) :
                                    (Destination)session.CreateQueue(destAddr);
            }

            WasHelper.ts.TraceInformation("TemsListener: InitializeSession - complete");

            if (temsBE.MessageSelector == String.Empty)
                requestMessageConsumer = session.CreateConsumer(requestDestination);
            else
                requestMessageConsumer = session.CreateConsumer(requestDestination, temsBE.MessageSelector);

            Interlocked.Decrement(ref listenerCnt);
            msg = requestMessageConsumer.Receive();  // wait for message from EMS QUEUE
            StopListen();  // close connection so worker process can get message from queue

            if (msg != null)
            {
                string wcfDestination = msg.GetStringProperty(TemsChannelTransport.WcfToProperty);
                dataReceivedCallback(wcfDestination); // will start worker process
                WasHelper.ts.TraceInformation("TemsListener : Receive - exit until worker process exits.");
            }
        } 
    }
}
