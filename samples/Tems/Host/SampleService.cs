/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using BookOrderService;
using com.tibco.sample.custom;
using com.tibco.sample.service;
using com.tibco.wcf.tems;
using TIBCO.EMS;

namespace com.tibco.sample.host
{
    public class SampleService : IErrorHandler, IEndpointBehavior, ITemsPassword
    {
        string serviceType = "ServiceRequestReplyType";
        ServiceEndpoint endpoint;
        TemsTransportBindingElement bindingElement;
        SampleService(string[] args)
        {
            ParseArgs(args);
            //Trace.Listeners.Add(new ConsoleTraceListener());

            //WaitCallback runTestDel = (serviceType) => RunTest(serviceType);
            //ThreadPool.QueueUserWorkItem(runTestDel, GetServiceType());
            //System.Console.WriteLine("This is a test.");
            //Thread.Sleep(Timeout.Infinite);
            RunTest(GetServiceType());
        }

        public static void Main(string[] args)
        {
            new SampleService(args);
        }

        private void RunTest(Object serviceType)
        {
            /** Note: Two try-catch blocks are used so that any exceptions inside the using statement
             * are logged.  Without this only the outer exception is logged which loses detail:
             * "The communication object, System.ServiceModel.ServiceHost, cannot be used for
             * communication because it is in the Faulted state."
             **/ 

            try
            {
                using (ServiceHost host = new ServiceHost((Type)serviceType))
                {
                    try
                    {
                        // Only set TemsChannelTransport static values if the Tems transport is being used.
                        if (this.serviceType.IndexOf("Tcp") == -1 &&
                            this.serviceType.IndexOf("Pipe") == -1 &&
                            this.serviceType.IndexOf("Http") == -1)
                        {
                            endpoint = host.Description.Endpoints[0];
                            BindingElementCollection elements = ((CustomBinding)endpoint.Binding).Elements;
                            bindingElement = (TemsTransportBindingElement)elements[elements.Count - 1];

                            //endpoint.Behaviors.Add(new MessageInspector());
                            endpoint.Behaviors.Add(this);

                            /**
                             * Uncomment any of the example method calls below to see how these are used.
                             * Check that the change is applied to both the client and service application
                              **/
                          
                            //SetConnectionFactoryExample();

                            SetContextJNDI();
                            //SetJNDIConnectionFactoryExample();
                            //SetSSLConnectionFactoryExample();
                            //SetSSLBindingPropertiesExample();
                            SetEndpointDestinationExample();
                            //SetCallbackDestinationExample();
                            //SetCustomMessageProtocolExample();
                            //ManipulatePasswordExample();  // De-crypt, un-obfuscate or whatever... to get clear string password
                        }
                        host.Open();

                        System.Console.WriteLine("The Tems service is available.");
                        //Thread.Sleep(1000);
                        System.Console.ReadKey(true);
                        System.Console.WriteLine("The Tems service is closing....");
                        host.Close();
                    }
                    catch (Exception e)
                    {
                        System.Console.Error.WriteLine("Service Exception: " + e.Message);
                        System.Console.Error.WriteLine(e.StackTrace);
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine("Service Exception: " + e.Message);
                System.Console.Error.WriteLine(e.StackTrace);
            }

            System.Console.WriteLine("The Tems service is closed.");
            System.Console.ReadKey(true);
        }

        /** 
         * <summary>
         *  This example shows how a pre-configured or administered ConnectionFactory
         *  can be set on the TemsTransportBindingElement ConnectionFactory property.
         *  </summary>
         **/  
        
        private void SetConnectionFactoryExample()
        {
            // Check if setting the property is allowed.
            // If it is not allowed and the setter is called an exception is thrown.
            if (bindingElement.AllowAdministratedConnFactory)
            {
                ConnectionFactory connectionFactory = new ConnectionFactory();
                // Set any configuration values here:
                connectionFactory.SetClientID("Test using SetConnectionFactoryExample - " + Guid.NewGuid().ToString());

                //  Username/password only has an affect if authorization enabled on EMS Server
                //  then actual username and password should be substituted or use values from config file as shown:
                connectionFactory.SetUserName(bindingElement.Username);
                connectionFactory.SetUserPassword(Manage(bindingElement.Password));
                bindingElement.ConnectionFactory = connectionFactory;
            }
            else
            {
                throw new Exception("Cannot set bindingElement.ConnectionFactory because AllowAdministratedConnFactory=\"false\".");
            }
        }

        /** 
         * <summary>
         *  This example shows how an administered ConnectionFactory can be retrieved
         *  using JNDI and set on the TemsTransportBindingElement ConnectionFactory property.
         *  </summary>
         **/  
        private void SetJNDIConnectionFactoryExample()
        {
            // Check if setting the property is allowed.
            // If it is not allowed and the setter is called an exception is thrown.
            if (bindingElement.AllowAdministratedConnFactory)
            {
                Hashtable env = SetContextJNDI();
                LookupContextFactory factory = new LookupContextFactory();
                ILookupContext searcher = factory.CreateContext(
                                            LookupContextFactory.TIBJMS_NAMING_CONTEXT, env);
                ConnectionFactory connectionFactory = (ConnectionFactory)searcher.Lookup("FTQueueConnectionFactory");

                //  Username/password only has an affect if authorization enabled on EMS Server
                //  then actual username and password should be substituted or use values from config file as shown:
                connectionFactory.SetUserName(bindingElement.Username);
                connectionFactory.SetUserPassword(Manage(bindingElement.Password));
                bindingElement.ConnectionFactory = connectionFactory;
            }
            else
            {
                throw new Exception("Cannot set bindingElement.ConnectionFactory because AllowAdministratedConnFactory=\"false\".");
            }
        }

        private Hashtable SetContextJNDI()
        {
            if (bindingElement.ContextJNDI == null)
            {
                Hashtable env = new Hashtable();
                Uri listenUri = endpoint.ListenUri;
                string namingUrl = LookupContextFactory.TIBJMS_NAMING_CONTEXT + Uri.SchemeDelimiter + listenUri.Authority;

                // Java names can be explicitly added if required for a MEX WSDL <jndi:context> element.
                //env.Add(TemsWsdlExportExtension.JavaProviderUrlName, namingUrl + ".java.test");
                //env.Add(TemsWsdlExportExtension.JavaSecurityPrincipalName, "Username.java.test");
                //env.Add(TemsWsdlExportExtension.JavaSecurityCredentialsName, "Password.java.test");
                //env.Add(TemsWsdlExportExtension.JavaSecurityProtocolName, "ProtocolName.java.test");

                env.Add(LookupContext.PROVIDER_URL, namingUrl);

                //  Username/password only has an affect if authorization enabled on EMS Server
                //  then actual username and password should be substituted or use values from config file as shown:
                env.Add(LookupContext.SECURITY_PRINCIPAL, bindingElement.Username);
                env.Add(LookupContext.SECURITY_CREDENTIALS, Manage(bindingElement.Password));

                env.Add(LookupContext.SECURITY_PROTOCOL, "ProtocolName");
                bindingElement.ContextJNDI = env;
            }

            return bindingElement.ContextJNDI;
        }

        /** 
         * <summary>
         *  This example shows how EMS SSL properties can be set on a ConnectionFactory instance.
         *  </summary>
         **/  
        private void SetSSLConnectionFactoryExample()
        {
            // Check if setting the property is allowed.
            // If it is not allowed and the setter is called an exception is thrown.
            if (bindingElement.AllowAdministratedConnFactory)
            {
                ConnectionFactory connectionFactory = new ConnectionFactory();

                #region Set SSL Configuration
                String sslClientIdentity = "D:/tools/TIBCO/ems/samples/certs/client_identity.p12";
                String sslPassword = "password";
                String sslTargetHostname = "server";
                String sslServerUrl = "ssl://localhost:7243";

                EMSSSLFileStoreInfo storeInfo = new EMSSSLFileStoreInfo();
                storeInfo.SetSSLClientIdentity(sslClientIdentity);
                storeInfo.SetSSLPassword(sslPassword.ToCharArray());

                connectionFactory.SetCertificateStoreType(EMSSSLStoreType.EMSSSL_STORE_TYPE_FILE, storeInfo);
                connectionFactory.SetTargetHostName(sslTargetHostname);
                connectionFactory.SetServerUrl(sslServerUrl);
                #endregion

                // Set any configuration values here:
                connectionFactory.SetClientID("Test using SetSSLConnectionFactoryExample - " + Guid.NewGuid().ToString());

                //  Username/password only has an affect if authorization enabled on EMS Server
                //  then actual username and password should be substituted or use values from config file as shown:
                connectionFactory.SetUserName(bindingElement.Username);
                connectionFactory.SetUserPassword(Manage(bindingElement.Password));

                bindingElement.ConnectionFactory = connectionFactory;
            }
            else
            {
                throw new Exception("Cannot set bindingElement.ConnectionFactory because AllowAdministratedConnFactory=\"false\".");
            }
        }

        /** 
         * <summary>
         *  This example shows how EMS SSL properties can be set on the TemsTransportBindingElement.
         *  Note: This has no affect on a ConnectionFactory set using
         *  TemsTransportBindingElement.ConnectionFactory property.
         *  </summary>
         **/
        private void SetSSLBindingPropertiesExample()
        {
            // Check if setting the property is allowed.
            // If it is not allowed and the setter is called an exception is thrown.
            if (bindingElement.AllowBindingChanges)
            {
                #region Set SSL Configuration
                String sslClientIdentity = "D:/tools/TIBCO/ems/samples/certs/client_identity.p12";
                String sslPassword = "password";
                String sslTargetHostname = "server";
                String sslServerUrl = "ssl://localhost:7243";

                EMSSSLFileStoreInfo storeInfo = new EMSSSLFileStoreInfo();
                storeInfo.SetSSLClientIdentity(sslClientIdentity);
                storeInfo.SetSSLPassword(sslPassword.ToCharArray());

                bindingElement.CertificateStoreType = EMSSSLStoreType.EMSSSL_STORE_TYPE_FILE;
                bindingElement.CertificateStoreTypeInfo = storeInfo;
                bindingElement.SSLTargetHostName = sslTargetHostname;
                bindingElement.ServerUrl = sslServerUrl;
                #endregion

                // Set any configuration values here:
                bindingElement.ClientID = "Test using SetSSLBindingPropertiesExample - " + Guid.NewGuid().ToString();
                
                // Note that actual username and password should be substituted in example below (if want to override what is in config file):

                //bindingElement.Username = "user";  
                //bindingElement.Password = "user.password";
            }
            else
            {
                throw new Exception("Cannot set bindingElement properties because AllowBindingChanges=\"false\".");
            }
        }

        /**
         * <summary>
         *  This example shows how a pre-configured or administered endpoint Destination
         *  can be set on the TemsTransportBindingElement EndpointDestination property.
         *  </summary>
         **/ 
        private void SetEndpointDestinationExample()
        {
            // Check if setting the property is allowed.
            // If it is not allowed and the setter is called an exception is thrown.
            if (bindingElement.AllowAdministratedEndpointDest)
            {
                bool isTopic = false;
                Uri listenUri = endpoint.ListenUri;
                string[] segments = listenUri.Segments;
                string destType = segments[segments.Length - 2];
                isTopic = destType.Equals(TemsChannelTransport.TOPIC,
                                                    StringComparison.OrdinalIgnoreCase);
                Hashtable env = SetContextJNDI();
                LookupContextFactory factory = new LookupContextFactory();
                ILookupContext searcher = factory.CreateContext(
                                            LookupContextFactory.TIBJMS_NAMING_CONTEXT, env);
                Destination endpointDestination = isTopic ?
                                        (Destination)searcher.Lookup("Tems.Topic.Endpoint") :
                                        (Destination)searcher.Lookup("Tems.Queue.Endpoint");
                bindingElement.EndpointDestination = endpointDestination;
            }
            else
            {
                throw new Exception("Cannot set bindingElement.EndpointDestination because AllowAdministratedEndpointDest=\"false\".");
            }
        }

        /** 
         * <summary>
         *  This example shows how to set the class that implements the ITemsPassword interface
         *  in order to manipulate a EMS server password that is retrieved
         *  from the password element of the TEMS binding. (see Manage method which follows)
         *  SETUP - before running this sample, set Password in the config file.
         *  </summary>
         **/ 
        private void ManipulatePasswordExample()
        {
            if (bindingElement.ConnectionFactory == null)
            {
                // if here then connection factory will be created and initialized when
                // channel is open ... therefore just set object that implements ITemsPassword
                bindingElement.TemsPasswordImpl = this;
            }
            else
            {
                // Sample app has created the Connection Factory so app needs to call Manage when
                // configuring the connection factory.
                string xxx = (string)bindingElement.ConnectionFactory.Properties["UserName"];
                TemsTrace.WriteLine(TraceLevel.Info, "App created service connection factory, password on bindingElement = {0} ", bindingElement.Password);
            }
        }

        #region ITemsPassword Members

        public string Manage(string configPassword)
        {
            //  Username/password only has an affect if authorization enabled on EMS Server
            string clearPassword = configPassword;
            // manipulate password here ... In this sample, whatever password is in config file is just being passed through;
            // If EMS authentication is on, then password returned from this routine must be clear text password that was set
            // for the user on the EMS server.
            TemsTrace.WriteLine(TraceLevel.Info, "EMS password from service config = {0}, after manipulation = {1} ", configPassword, clearPassword);
            return clearPassword;  // must match password that has been set on EMS Server
        }

        public string ManageSSL(string configSSLPassword)
        { // this sample just returning configured SSLPassword
            return configSSLPassword;
        }

        public string ManageSSLProxyAuth(string sslProxyAuthPassword)
        { // this sample just returning configured SSLPassword
            return sslProxyAuthPassword;
        }

        #endregion

        /** 
         * <summary>
         *  This example shows how a pre-configured or administered callback Destination
         *  can be set on the TemsTransportBindingElement CallbackDestination property.
         *  </summary>
         **/  
        private void SetCallbackDestinationExample()
        {
            // Check if setting the property is allowed.
            // If it is not allowed and the setter is called an exception is thrown.
            if (bindingElement.AllowAdministratedCallbackDest)
            {
                bool isTopic = false;
                Uri listenUri = endpoint.ListenUri;
                string[] segments = listenUri.Segments;
                string destType = segments[segments.Length - 2];
                isTopic = destType.Equals(TemsChannelTransport.TOPIC,
                                                    StringComparison.OrdinalIgnoreCase);
                Hashtable env = SetContextJNDI();
                LookupContextFactory factory = new LookupContextFactory();
                ILookupContext searcher = factory.CreateContext(
                                            LookupContextFactory.TIBJMS_NAMING_CONTEXT, env);
                Destination callbackDestination = isTopic ?
                                        (Destination)searcher.Lookup("Tems.Topic.Callback") :
                                        (Destination)searcher.Lookup("Tems.Queue.Callback");
                bindingElement.CallbackDestination = callbackDestination;
            }
            else
            {
                throw new Exception("Cannot set bindingElement.CallbackDestination because AllowAdministratedCallbackDest=\"false\".");
            }
        }

        /** 
         * <summary>
         *  This example shows how a custom implementation of TemsMessageProtocol : ITemsMessageProtocol
         *  can be set on the TemsTransportBindingElement CustomMessageProtocol property.
         *  </summary>
         **/  
        private void SetCustomMessageProtocolExample()
        {
            // Check if setting the property is allowed.
            // If it is not allowed and the setter is called an exception is thrown.
            if (bindingElement.AllowCustomMessageProtocol)
            {
                bindingElement.MessageProtocol = TemsMessageProtocolType.Custom;
                string assemblyQualifiedName = new SampleMessageProtocol().GetType().AssemblyQualifiedName;
                bindingElement.CustomMessageProtocolType = assemblyQualifiedName;
            }
            else
            {
                throw new Exception("Cannot set bindingElement.CustomMessageProtocol because AllowCustomMessageProtocol=\"false\".");
            }
        }

        private Type GetServiceType()
        {
            Type type = null;
            if (serviceType.Equals("ServiceRequestReplyType"))
            {
                type = typeof(ServiceRequestReplyType);
            }
            else if (serviceType.Equals("ServiceRequestReplyTypeHttp"))
            {
                type = typeof(ServiceRequestReplyTypeHttp);
            }
            else if (serviceType.Equals("ServiceRequestReplySessionType"))
            {
                type = typeof(ServiceRequestReplySessionType);
            }
            else if (serviceType.Equals("ServiceRequestReplyAsyncType"))
            {
                type = typeof(ServiceRequestReplyAsyncType);
            }
            else if (serviceType.Equals("ServiceDatagramType"))
            {
                type = typeof(ServiceDatagramType);
            }
            else if (serviceType.Equals("ServiceDatagramSessionType"))
            {
                type = typeof(ServiceDatagramSessionType);
            }
            else if (serviceType.Equals("ServiceDatagramSessionTypePipe"))
            {
                type = typeof(ServiceDatagramSessionTypePipe);
            }
            else if (serviceType.Equals("ServiceDatagramSessionTypeTcp"))
            {
                type = typeof(ServiceDatagramSessionTypeTcp);
            }
            else if (serviceType.Equals("ServiceDuplexType"))
            {
                type = typeof(ServiceDuplexType);
            }
            else if (serviceType.Equals("ServiceDuplexSessionType"))
            {
                type = typeof(ServiceDuplexSessionType);
            }
            else if (serviceType.Equals("ServiceDuplexTransactionType"))
            {
                type = typeof(ServiceDuplexTransactionType);
            }
            else if (serviceType.Equals("BookServiceType"))
            {
                type = typeof(BookServiceType);
            }
            return type;
        }

        private void ParseArgs(String[] args)
        {
            int i = 0;

            while (i < args.Length)
            {
                if (args[i].CompareTo("-type") == 0)
                {
                    if ((i + 1) >= args.Length) { Usage(); }
                    serviceType = args[i + 1];
                    i += 2;
                }
                else if (args[i].CompareTo("-log") == 0)
                {
                    if ((i + 1) >= args.Length) { Usage(); }
                    int logValue = Convert.ToInt32(args[i + 1]);
                    TemsTrace.TraceLevel = (TraceLevel)logValue;
                    i += 2;
                }
                else
                {
                    System.Console.Error.WriteLine("Unrecognized parameter: " + args[i]);
                    Usage();
                }
            }
        }

        private void Usage()
        {
            System.Console.WriteLine("\nUsage: Host [options]");
            System.Console.WriteLine("");
            System.Console.WriteLine("   where options are:");
            System.Console.WriteLine("");
            System.Console.WriteLine("   -type     <service type>       - The service type.");
            System.Console.WriteLine("   -log      <trace log level>    - The trace log level to display.");
            Environment.Exit(0);
        }

        #region IErrorHandler Members

        public bool HandleError(Exception error)
        {
            string msg = String.Format("IErrorHandler HandleError called: {0}", error.Message);
            Console.WriteLine(msg);
            return false;
        }

        public void ProvideFault(Exception error, System.ServiceModel.Channels.MessageVersion version, ref System.ServiceModel.Channels.Message fault)
        {
            string msg = String.Format("IErrorHandler ProvideFault called: {0}", error.Message);
            Console.WriteLine(msg);
        }

        #endregion

        #region IEndpointBehavior Members

        public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new MessageInspector());
            ChannelDispatcher dispatcher = endpointDispatcher.ChannelDispatcher;
            Collection<IErrorHandler> errorHandlers = dispatcher.ErrorHandlers;
            errorHandlers.Add(this);
        }

        public void AddBindingParameters(ServiceEndpoint serviceEndpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {

        }

        public void Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        #endregion
    }
}
