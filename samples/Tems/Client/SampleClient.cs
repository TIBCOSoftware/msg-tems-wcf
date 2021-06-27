/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using System.Transactions;
using com.tibco.sample.custom;
using com.tibco.wcf.tems;
using TIBCO.EMS;

namespace com.tibco.sample.client
{
    public class SampleClient : IServiceDuplexCallback, IServiceDuplexSessionCallback, IServiceDuplexTransactionCallback, ITemsPassword
    {
        string endpointName = "";
        ServiceEndpoint endpoint;
        TemsTransportBindingElement bindingElement;
        string key = "";
        long defaultIterations = 1;
        long returnMessageSize = 0;
        bool autoRun = false;
        IDisposable proxy = null;
        InstanceContext instanceContext;
        int callbackOneCount = 0;
        int callbackTwoCount = 0;
        int callbackThreeCount = 0;
        int totalCallbackCount = -1;
        int requestReplyAsyncCallbackCount = 0;
        Hashtable requestReplyAsyncReplies;
        int messagesCalledPerIteration = 1;
        long iterations = 0;
        int timeLogModulus = 1000;

        // If true, RequestReply is called with an client side asnyc method.
        bool callRequestReplyAsync = true;
        bool error = false;
        ConnectionPool pool = null;  // used for AppManagesConnections example
      
        SampleClient(string[] args)
        {
            ParseArgs(args);
            //Trace.Listeners.Add(new ConsoleTraceListener());
            //System.Console.WriteLine("Press any key when the service is ready.");
            //System.Console.ReadKey(true);

            // Note: SampleClient is the instance context as it implements IServiceDuplexCallback.
            instanceContext = new InstanceContext(this);
            RunTestLoop();
        }

        private void RunTestLoop()
        {
            if (autoRun)
            {
                iterations = defaultIterations;
                RunTestMain();
            }
            else
            {
                while (!error)
                {
                    ReadIterations();
                    RunTestMain();
                }
            }

            if (error)
            {
                System.Console.WriteLine("Exiting the client program due to error.");
            }
            else
            {
                System.Console.WriteLine("Press any key to exit.");
            }

            System.Console.ReadKey(true);
        }

        public static void Main(string[] args)
        {
            new SampleClient(args);
        }

        private void RunTestMain()
        {
            try
            {
                SetProxy();
                System.Console.WriteLine("Start sending {0} messages...", iterations * messagesCalledPerIteration);
                Time.StartTimer();
                
                using (proxy)
                {
                    BindingElementCollection elements = ((CustomBinding)endpoint.Binding).Elements;
                    
                    // Only set TemsChannelTransport static values if the Tems transport is being used.
                    if (endpointName.IndexOf("Tcp") == -1 &&
                        endpointName.IndexOf("Pipe") == -1 &&
                        endpointName.IndexOf("Http") == -1)
                    {
                        // Uncomment any of the example method calls below to see how these are used.
                        // Check that the change is applied to both the client and service application...
                        // but AppManagesConnectionsExample is only shown for pooling client connections

                        //SetConnectionFactoryExample();
                        bindingElement = (TemsTransportBindingElement)elements[elements.Count - 1];

                        SetContextJNDI();
                        //SetJNDIConnectionFactoryExample();
                        //SetSSLConnectionFactoryExample();
                        //SetSSLBindingPropertiesExample();
                        //AppManagesConnectionsExample();
                        SetEndpointDestinationExample();
                        SetCallbackDestinationExample();
                        //SetCustomMessageProtocolExample();
                        ManipulatePasswordExample();  // De-crypt, un-obfuscate or whatever... to get clear string password
                    }

                    string reply = "";
                    string expectedReply = "";

                    OpenProxy();

                    DateTime totalStartTime = DateTime.UtcNow;
                    DateTime currentStartTime = DateTime.UtcNow;

                    if (((int)(timeLogModulus / messagesCalledPerIteration)) * messagesCalledPerIteration != timeLogModulus)
                    {
                        timeLogModulus = timeLogModulus * messagesCalledPerIteration;
                    }

                    requestReplyAsyncReplies = new Hashtable();

                    for (int i = 0; i < iterations; i++)
                    {
                        string arg;

                        if (returnMessageSize > 0)
                        {
                            arg = "size=" + returnMessageSize;
                        }
                        else
                        {
                            string unicodeTestString = "Pi (\u03a0) and Sigma (\u03a3),\u0306\u01FD\u03B2\uD8FF\uDCFF,`¬!£$%^&*()-_=+]}[{#~'@;:/?.>,<\\|éricó ÓéíóáÁ#$£^";
                            arg = iterations == 1 ? key : "test iteration: " + i;
                            arg += String.Format(" unicodeTestString = {0}", unicodeTestString);
                            expectedReply = GetExpectedReply(arg);
                        }

                        TemsTrace.WriteLine(TraceLevel.Verbose, "Calling1 proxy.ServiceMethod(arg) with arg = {0}", arg);

                        reply = CallService(arg, expectedReply);
                        
                        if (i == 0)
                        {
                            totalStartTime = DateTime.UtcNow;
                            currentStartTime = DateTime.UtcNow;
                        }

                        // Do not do validation of return value for testing large text.
                        if (returnMessageSize == 0)
                        {
                            if (callRequestReplyAsync && isContractType("RequestReplyAsync"))
                            {
                                lock (requestReplyAsyncReplies)
                                {
                                    if (!requestReplyAsyncReplies.ContainsKey(expectedReply))
                                    {
                                        requestReplyAsyncReplies.Add(expectedReply, null);
                                    }
                                }
                            }
                            else if (reply == null || !reply.Equals(expectedReply))
                            {
                                TemsTrace.WriteLine(TraceLevel.Error, "Result: {0} does not match expected result: {1}", reply == null ? "null" : reply, expectedReply);
                            }
                        }
                        
                        int msgCount = i > 0 ? messagesCalledPerIteration * i : 1;

                        if (msgCount % timeLogModulus == 0)
                        {
                            DateTime now = DateTime.UtcNow;
                            double totalElapsed = (now - totalStartTime).TotalMilliseconds;
                            double totalMsgsPerSec = Math.Round((msgCount / totalElapsed) * 1000);
                            double currentElapsed = (now - currentStartTime).TotalMilliseconds;
                            double currentMsgsPerSec = Math.Round((timeLogModulus / currentElapsed) * 1000);
                            TemsTrace.WriteLine(TraceLevel.Info, "Messages Sent: {0}, msgs / sec (total / current) = ({1} / {2})", msgCount, totalMsgsPerSec, currentMsgsPerSec);
                            currentStartTime = now;
                        }

                        TemsTrace.WriteLine(TraceLevel.Verbose, "{0} proxy.ServiceMethod(key) reply = {1}", endpointName, reply);
                    }

                    // Delay closing the proxy until all expected callbacks have been received.
                    bool isDuplexSession = isContractType("DuplexSession");
                    bool isDuplexTransaction = isContractType("DuplexTransaction");

                    if (isContractType("Duplex") || isDuplexSession || isDuplexTransaction)
                    {
                        int lastCount = -2;

                        while (totalCallbackCount < 3 * iterations && lastCount != totalCallbackCount)
                        {
                            int sleepPeriod = 1000;
                            TemsTrace.WriteLine(TraceLevel.Info,
                                        "Waiting {0} ms for all expected callbacks, still pending: {1}",
                                        sleepPeriod,
                                        totalCallbackCount - (3 * iterations));
                            // For sessionless, if multiple clients are running the totalCallbackCount
                            // usually will not match the expected count because the service will process
                            // any requests in a sessionless manner.  This means that a given client 
                            // request may be answered with a callback that is received by any other client.
                            // The lastCount check is used to detect that no change in count has occurred to
                            // detect this condition.  For sessionful duplex, the counts should always match.

                            if (!(isDuplexSession || isDuplexTransaction))
                            {
                                lastCount = totalCallbackCount;
                            }

                            Thread.Sleep(sleepPeriod);
                            totalCallbackCount = callbackOneCount + callbackTwoCount + callbackThreeCount;
                        }

                        // Allow time for any pending callbacks to be counted.
                        lastCount = 0;
                        int sleepCount = 0;
                        totalCallbackCount = callbackOneCount + callbackTwoCount + callbackThreeCount;
                        int preSleepCount = totalCallbackCount;

                        TemsTrace.WriteLine(TraceLevel.Info, "lastCount = {0}, totalCallbackCount = {1}", lastCount, totalCallbackCount);

                        while (lastCount != totalCallbackCount)
                        {
                            totalCallbackCount = callbackOneCount + callbackTwoCount + callbackThreeCount;
                            Thread.Sleep(100);
                            sleepCount++;
                            lastCount = callbackOneCount + callbackTwoCount + callbackThreeCount;
                        }

                        if (preSleepCount != totalCallbackCount)
                        {
                            TemsTrace.WriteLine(TraceLevel.Info, "Sleep to allow time for any pending callbacks to be counted.");
                            TemsTrace.WriteLine(TraceLevel.Info, "Pre-Sleep count: {0}", preSleepCount);
                            TemsTrace.WriteLine(TraceLevel.Info, "Post-Sleep count: {0} sleepCount: {1}", totalCallbackCount, sleepCount);
                        }

                        if (callbackOneCount == iterations &&
                                callbackTwoCount == iterations &&
                                callbackThreeCount == iterations)
                        {
                            TemsTrace.WriteLine(TraceLevel.Info, "************************************");
                            TemsTrace.WriteLine(TraceLevel.Info, "All {0} expected callbacks received.", 3 * iterations);
                            TemsTrace.WriteLine(TraceLevel.Info, "************************************");
                        }
                        else
                        {
                            totalCallbackCount = callbackOneCount + callbackTwoCount + callbackThreeCount;
                            var msg = "\nTotal callbacks received: {0}";
                            msg += "\n                expected: {1}";
                            msg += "\n                    diff: {2}";
                            msg += "\n\nThis is usually due to multiple clients running sessionless Duplex.";
                            msg += "\nCheck counts from other client logs.";
                            TemsTrace.WriteLine(TraceLevel.Info, msg, totalCallbackCount, 3 * iterations, totalCallbackCount - 3 * iterations);
                            msg = "\nThe following are the callback counts:";
                            msg += "\n    callbackOneCount = {0}\n    callbackTwoCount = {1}\n    callbackThreeCount = {2}\n";
                            TemsTrace.WriteLine(TraceLevel.Info, msg, callbackOneCount, callbackTwoCount, callbackThreeCount);
                        }
                    }

                    // Delay closing the proxy until all expected async callbacks have been received.
                    if (isContractType("RequestReplyAsync"))
                    {
                        while (requestReplyAsyncCallbackCount < iterations)
                        {
                            int sleepPeriod = 1000;
                            TemsTrace.WriteLine(TraceLevel.Info,
                                        "Waiting {0} ms for all expected async callbacks, still pending: {1}",
                                        sleepPeriod,
                                        requestReplyAsyncCallbackCount - iterations);
                            Thread.Sleep(sleepPeriod);
                        }

                        TemsTrace.WriteLine(TraceLevel.Info, "All {0} expected async callbacks received.", requestReplyAsyncCallbackCount);

                        // Do not do validation of return value for testing large text.
                        if (returnMessageSize == 0)
                        {
                            foreach (DictionaryEntry de in requestReplyAsyncReplies)
                            {
                                TemsTrace.WriteLine(TraceLevel.Info, "Expected reply: {0} not found.", de.Key);
                            }
                        }
                    }

                    //System.Console.WriteLine("About to close proxy.");
                    CloseProxy();
                    //System.Console.WriteLine("After close proxy.");
                    callbackOneCount = 0;
                    callbackTwoCount = 0;
                    callbackThreeCount = 0;
                    totalCallbackCount = -1;
                    requestReplyAsyncCallbackCount = 0;

                    if (pool != null)
                    {
                        pool.Release();   // release pool resources 
                        pool = null;
                    }

                    Time.StopTimer();
                    System.Console.WriteLine("Elapsed ms to run " + iterations + " iterations = " + Time.Elapsed());
                } 
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine("Client Exception: " + e.Message);
                System.Console.Error.WriteLine(e.StackTrace);
                error = true;
                //System.Environment.Exit(-1);
            }
        }

        /// <summary>
        /// This example shows how a pre-configured or administered ConnectionFactory
        /// can be set on the TemsTransportBindingElement ConnectionFactory property.
        /// </summary>
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

        /// <summary>
        /// This example shows how an administered ConnectionFactory can be retrieved
        /// using JNDI and set on the TemsTransportBindingElement ConnectionFactory property.
        /// </summary>
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
                ConnectionFactory connectionFactory = (ConnectionFactory)searcher.Lookup("GenericConnectionFactory");

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

        /// <summary>
        /// This example shows how EMS SSL properties can be set on a ConnectionFactory instance.
        /// </summary>
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

        /// <summary>
        /// This example shows how EMS SSL properties can be set on the TemsTransportBindingElement.
        /// Note: This has no affect on a ConnectionFactory set using
        /// TemsTransportBindingElement.ConnectionFactory property.
        /// </summary>
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
                // Note that actual username and password should be substituted in example below:

                bindingElement.Username = "user";
                bindingElement.Password = "user.password";
            }
            else
            {
                throw new Exception("Cannot set bindingElement properties because AllowBindingChanges=\"false\".");
            }
        }

        /// <summary>
        /// This example shows how a pre-configured or administered endpoint Destination
        /// can be set on the TemsTransportBindingElement EndpointDestination property.
        /// </summary>
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

        /// <summary>
        /// This example shows how an application can manage (create/share/close) EMS connections.
        /// See ConnectionPool Class and where it is reference for example of a very simple connection pool. 
        /// You must set AppManagesConnections = true in the config file
        /// </summary>
        private void AppManagesConnectionsExample()
        {
            pool = new ConnectionPool();
            if (bindingElement.ConnectionFactory == null)
            {
                bindingElement.ConnectionFactory = new ConnectionFactory();
                //  Username/password only has an affect if authorization enabled on EMS Server
                //  then actual username and password should be substituted or use values from config file as shown:
                bindingElement.ConnectionFactory.SetUserName(bindingElement.Username);
                bindingElement.ConnectionFactory.SetUserPassword(Manage(bindingElement.Password));
            }
            pool.Initialize(bindingElement.ConnectionFactory, 3);
        }

        /// <summary>
        /// This example shows how to set the class that implements the ITemsPassword interface
        /// in order to manipulate a EMS server password that is retrieved
        /// from the password element of the TEMS binding. (see Manage method which follows)
        /// SETUP - before running this sample, set Password in the config file.
        /// </summary>
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
                TemsTrace.WriteLine(TraceLevel.Info, "App created client connection factory, password on bindingElement = {0} ", bindingElement.Password);
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
            TemsTrace.WriteLine(TraceLevel.Info, "EMS password from client config {0} after manipulation = {1} ", configPassword, clearPassword);
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

        /// <summary>
        /// This example shows how a pre-configured or administered callback Destination
        /// can be set on the TemsTransportBindingElement CallbackDestination property.
        /// </summary>
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

        /// <summary>
        /// This example shows how a custom implementation of TemsMessageProtocol : ITemsMessageProtocol
        /// can be set on the TemsTransportBindingElement CustomMessageProtocol property.
        /// </summary>
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

        private void SetProxy()
        {
            if (isContractType("BookClient"))
            {
                proxy = new BookOrderPTClient(endpointName);
                endpoint = ((BookOrderPTClient)proxy).Endpoint;
                ((BookOrderPTClient)proxy).ClientCredentials.UserName.UserName = "test";
                ((BookOrderPTClient)proxy).ClientCredentials.UserName.Password = "test";
            }
            else if (isContractType("RequestReply"))
            {
                proxy = new ServiceRequestReplyClient(endpointName);
                endpoint = ((ServiceRequestReplyClient)proxy).Endpoint;
                ((ServiceRequestReplyClient)proxy).ClientCredentials.UserName.UserName = "test";
                ((ServiceRequestReplyClient)proxy).ClientCredentials.UserName.Password = "test";
            }
            else if (isContractType("RequestReplySession"))
            {
                proxy = new ServiceRequestReplySessionClient(endpointName);
                endpoint = ((ServiceRequestReplySessionClient)proxy).Endpoint;
                ((ServiceRequestReplySessionClient)proxy).ClientCredentials.UserName.UserName = "test";
                ((ServiceRequestReplySessionClient)proxy).ClientCredentials.UserName.Password = "test";
                messagesCalledPerIteration = 2;
            }
            else if (isContractType("RequestReplyAsync"))
            {
                proxy = new ServiceRequestReplyAsyncClient(endpointName);
                endpoint = ((ServiceRequestReplyAsyncClient)proxy).Endpoint;
                ((ServiceRequestReplyAsyncClient)proxy).ClientCredentials.UserName.UserName = "test";
                ((ServiceRequestReplyAsyncClient)proxy).ClientCredentials.UserName.Password = "test";
            }
            else if (isContractType("Datagram"))
            {
                proxy = new ServiceDatagramClient(endpointName);
                endpoint = ((ServiceDatagramClient)proxy).Endpoint;
                ((ServiceDatagramClient)proxy).ClientCredentials.UserName.UserName = "test";
                ((ServiceDatagramClient)proxy).ClientCredentials.UserName.Password = "test";
            }
            else if (isContractType("DatagramSession"))
            {
                proxy = new ServiceDatagramSessionClient(endpointName);
                endpoint = ((ServiceDatagramSessionClient)proxy).Endpoint;
                ((ServiceDatagramSessionClient)proxy).ClientCredentials.UserName.UserName = "test";
                ((ServiceDatagramSessionClient)proxy).ClientCredentials.UserName.Password = "test";
                messagesCalledPerIteration = 2;
            }
            else if (isContractType("Duplex"))
            {
                proxy = new ServiceDuplexClient(instanceContext, endpointName);
                endpoint = ((ServiceDuplexClient)proxy).Endpoint;
                ((ServiceDuplexClient)proxy).ClientCredentials.UserName.UserName = "test";
                ((ServiceDuplexClient)proxy).ClientCredentials.UserName.Password = "test";
                messagesCalledPerIteration = 3;
            }
            else if (isContractType("DuplexSession"))
            {
                proxy = new ServiceDuplexSessionClient(instanceContext, endpointName);
                endpoint = ((ServiceDuplexSessionClient)proxy).Endpoint;
                ((ServiceDuplexSessionClient)proxy).ClientCredentials.UserName.UserName = "test";
                ((ServiceDuplexSessionClient)proxy).ClientCredentials.UserName.Password = "test";
                messagesCalledPerIteration = 3;
            }
            else if (isContractType("DuplexTransaction"))
            {
                proxy = new ServiceDuplexTransactionClient(instanceContext, endpointName);
                endpoint = ((ServiceDuplexTransactionClient)proxy).Endpoint;
                ((ServiceDuplexTransactionClient)proxy).ClientCredentials.UserName.UserName = "test";
                ((ServiceDuplexTransactionClient)proxy).ClientCredentials.UserName.Password = "test";
                messagesCalledPerIteration = 3;
            }
        }

        private void OpenProxy()
        {
            if (pool != null)
            {
                bindingElement.Connection = pool.GetConnection();
            }

            if (isContractType("BookClient"))
            {
                ((BookOrderPTClient)proxy).Open();
            }
            else if (isContractType("RequestReply"))
            {
                ((ServiceRequestReplyClient)proxy).Open();
            }
            else if (isContractType("RequestReplySession"))
            {
                ((ServiceRequestReplySessionClient)proxy).Open();
            }
            else if (isContractType("RequestReplyAsync"))
            {
                ((ServiceRequestReplyAsyncClient)proxy).Open();
            }
            else if (isContractType("Datagram"))
            {
                ((ServiceDatagramClient)proxy).Open();
            }
            else if (isContractType("DatagramSession"))
            {
                ((ServiceDatagramSessionClient)proxy).Open();
            }
            else if (isContractType("Duplex"))
            {
                ((ServiceDuplexClient)proxy).Open();
            }
            else if (isContractType("DuplexSession"))
            {
                ((ServiceDuplexSessionClient)proxy).Open();
            }
            else if (isContractType("DuplexTransaction"))
            {
                ((ServiceDuplexTransactionClient)proxy).Open();
            }
        }

        private void CloseProxy()
        {
            if (pool != null)
            {
                pool.ReturnConnection(bindingElement.Connection);
                bindingElement.Connection = null;
            }

            if (isContractType("BookClient"))
            {
                ((BookOrderPTClient)proxy).Close();
            }
            else if (isContractType("RequestReply"))
            {
                ((ServiceRequestReplyClient)proxy).Close();
            }
            else if (isContractType("RequestReplySession"))
            {
                ((ServiceRequestReplySessionClient)proxy).Close();
            }
            else if (isContractType("RequestReplyAsync"))
            {
                ((ServiceRequestReplyAsyncClient)proxy).Close();
            }
            else if (isContractType("Datagram"))
            {
                ((ServiceDatagramClient)proxy).Close();
            }
            else if (isContractType("DatagramSession"))
            {
                ((ServiceDatagramSessionClient)proxy).Close();
            }
            else if (isContractType("Duplex"))
            {
                ((ServiceDuplexClient)proxy).Close();
            }
            else if (isContractType("DuplexSession"))
            {
                ((ServiceDuplexSessionClient)proxy).Close();
            }
            else if (isContractType("DuplexTransaction"))
            {
                ((ServiceDuplexTransactionClient)proxy).Close();
            }

            TemsTrace.WriteLine(TraceLevel.Info, "Proxy is closed.");
        }

        private void RequestReplyAsyncCallback(IAsyncResult result)
        {
            string reply;
#if ManualAcknowledgeSample 
            using (new OperationContextScope(((ServiceRequestReplyAsyncClient)proxy).InnerChannel))
            {
                reply = ((ServiceRequestReplyAsyncClient)proxy).EndServiceMethodAsync(result);
                SendAcknowledge();
            }
#else
            reply = ((ServiceRequestReplyAsyncClient)proxy).EndServiceMethodAsync(result);
#endif
            string[] asyncState = (string[]) result.AsyncState;
            //TemsTrace.WriteLine(TraceLevel.Info, "arg = {0}, reply = {1}, expected reply = {2}", asyncState[0], reply, asyncState[1]);

            if (returnMessageSize == 0)
            {
                lock (requestReplyAsyncReplies)
                {
                    if (requestReplyAsyncReplies.ContainsKey(reply))
                    {
                        requestReplyAsyncReplies.Remove(reply);
                    }
                }
            }

            requestReplyAsyncCallbackCount++;

            if (requestReplyAsyncCallbackCount % timeLogModulus == 0)
            {
                TemsTrace.WriteLine(TraceLevel.Info, "requestReplyAsyncCallbackCount = {0}", requestReplyAsyncCallbackCount);
            }
            //TemsTrace.WriteLine(TraceLevel.Error, "RequestReplyAsyncCallback reply = {0}", reply);
        }

        private void SendAcknowledge()
        {
            TemsMessage temsMessage = null;
            object msgProperty = null;

            if (OperationContext.Current == null)
                throw new Exception ("Samples using appHandlesManualAcknowledge but Client OperationContext.Current is null");

            if (OperationContext.Current.IncomingMessageProperties.TryGetValue(TemsMessage.key, out msgProperty))
            {
                temsMessage = (TemsMessage)msgProperty;
                temsMessage.Acknowledge();
            }
        }

        private string CallService(string arg, string expectedReply)
        {
            string reply = "";

            if (isContractType("BookClient"))
            {
                orderBookRequest request = new orderBookRequest();
                request.bookName = "Harry Potter and the Goblet of Fire";
                request.creditCardNum = "1234";
                request.price = 2.5;
                request.purchaser = "Asquith";
                request.quantity = 20;

#if ManualAcknowledgeSample
                using (new OperationContextScope(((BookOrderPTClient)proxy).InnerChannel))
                {
                    ((BookOrderPTClient)proxy).orderBook(request);
                    SendAcknowledge();
                }
#else
                    ((BookOrderPTClient)proxy).orderBook(request);
#endif

                reply = "Reply is N/A";
            }
            else if (isContractType("RequestReply"))
            {
#if ManualAcknowledgeSample
                using (new OperationContextScope(((ServiceRequestReplyClient)proxy).InnerChannel))
                {
                    reply = ((ServiceRequestReplyClient)proxy).ServiceMethod(arg);
                    SendAcknowledge();
                }
#else
                    reply = ((ServiceRequestReplyClient)proxy).ServiceMethod(arg);
#endif
            }
            else if (isContractType("RequestReplySession"))
            {
#if ManualAcknowledgeSample
                using (new OperationContextScope(((ServiceRequestReplySessionClient)proxy).InnerChannel))
                {
                    reply = ((ServiceRequestReplySessionClient)proxy).ServiceMethodRequestReplyInitiating(arg);
                    SendAcknowledge();
                }
                using (new OperationContextScope(((ServiceRequestReplySessionClient)proxy).InnerChannel))
                {
                    reply = ((ServiceRequestReplySessionClient)proxy).ServiceMethodRequestReplySession(arg);
                    SendAcknowledge();
                }
#else 
                reply = ((ServiceRequestReplySessionClient)proxy).ServiceMethodRequestReplyInitiating(arg);
                reply = ((ServiceRequestReplySessionClient)proxy).ServiceMethodRequestReplySession(arg);
#endif
                // If terminating method is called, all subsequent calls will fail with exception:
                //     Exception: This channel cannot send any more messages because IsTerminating
                //     operation 'ServiceMethodRequestReplyTerminating' has already been called.
                //reply = ((ServiceRequestReplySessionClient)proxy).ServiceMethodRequestReplyTerminating(arg);
            }
            else if (isContractType("RequestReplyAsync"))
            {
                // Note: The client can make either an async or sync call on a service method regardless of
                //       how the service is implemented.  How the service processes the request (async or sync)
                //       is a service implementation detail that the client call has no affect on.
                //       If the service implements both a synchronous and asynchronous versions of the operation
                //       the default behavior on the service is to invoke the synchronous version.
                if (callRequestReplyAsync)
                {
                    Thread.Sleep(0);
                    string[] asyncState = { arg, expectedReply };

                        ((ServiceRequestReplyAsyncClient)proxy).BeginServiceMethodAsync(arg, RequestReplyAsyncCallback, asyncState);
                }
                else
                {

#if ManualAcknowledgeSample
                    using (new OperationContextScope(((ServiceRequestReplyAsyncClient)proxy).InnerChannel))
                    {
                        reply = ((ServiceRequestReplyAsyncClient)proxy).ServiceMethodAsync(arg);
                        SendAcknowledge();
                    }
#else
                    reply = ((ServiceRequestReplyAsyncClient)proxy).ServiceMethodAsync(arg);
#endif
                    if (returnMessageSize == 0 && ! reply.Equals(expectedReply))
                    {
                        requestReplyAsyncReplies.Add(reply, null);
                    }
                    requestReplyAsyncCallbackCount++;
                }
                //TemsTrace.WriteLine(TraceLevel.Error, "RequestReplyAsyncCallback service called.");
            }
            else if (isContractType("Datagram"))
            {
                ((ServiceDatagramClient)proxy).ServiceMethodDatagram(arg);
                reply = "Reply is N/A";
            }
            else if (isContractType("DatagramSession"))
            {
                // If ServiceMethodDatagramInitiating is commented out and
                // ServiceMethodDatagramSession (IsInitiating=false) is called first, the call will
                // fail on the service side.  If the service has implemented
                // System.ServiceModel.Dispatcher.IErrorHandler then the IErrorHandler HandleError
                // and ProvideFault methods will be called with an exception message similar to:
                //     Exception: The message with Action '.../ServiceMethodDatagramSession'
                //     cannot be processed at the receiver, due to a ContractFilter mismatch at
                //     the EndpointDispatcher. ...
                ((ServiceDatagramSessionClient)proxy).ServiceMethodDatagramInitiating(arg);
                ((ServiceDatagramSessionClient)proxy).ServiceMethodDatagramSession(arg);
                
                // If terminating method is called, all subsequent calls will fail with client exception:
                //     Exception: This channel cannot send any more messages because IsTerminating
                //     operation 'ServiceMethodDatagramTerminating' has already been called.
                //((ServiceDatagramSessionClient)proxy).ServiceMethodDatagramTerminating(arg);
                reply = "Reply is N/A";
            }
            else if (isContractType("Duplex"))
            {
                ((ServiceDuplexClient)proxy).ServiceMethodOne(arg);
                ((ServiceDuplexClient)proxy).ServiceMethodTwo(arg);
                ((ServiceDuplexClient)proxy).ServiceMethodThree(arg);
                reply = "Reply is N/A";
            }
            else if (isContractType("DuplexSession"))
            {
                // See notes under: isContractType("DatagramSession") regarding Initiating/Terminating.
                ((ServiceDuplexSessionClient)proxy).ServiceMethodDuplexInitiating(arg);
                ((ServiceDuplexSessionClient)proxy).ServiceMethodDuplexSession(arg);
                ((ServiceDuplexSessionClient)proxy).ServiceMethodThreeDuplexSession(arg);
                //((ServiceDuplexSessionClient)proxy).ServiceMethodDuplexTerminating(arg);
                reply = "Reply is N/A";
            }
            else if (isContractType("DuplexTransaction"))
            {
                using (TransactionScope scope1 = new TransactionScope())
                {
                    string transactionLocalId1 = Transaction.Current.TransactionInformation.LocalIdentifier.ToString();
                    ((ServiceDuplexTransactionClient)proxy).ServiceTransactionMethodOne(String.Format("{0}, client transactionId.Local1 = {1}", arg, transactionLocalId1));
                    string transactionDistId1 = Transaction.Current.TransactionInformation.DistributedIdentifier.ToString();
                    TemsTrace.WriteLine(TraceLevel.Verbose, "client transactionId.Dist1 = {0}", transactionDistId1);
                    scope1.Complete();
                    using (TransactionScope scope2 = new TransactionScope())
                    {
                        string transactionLocalId2 = Transaction.Current.TransactionInformation.LocalIdentifier.ToString();
                        ((ServiceDuplexTransactionClient)proxy).ServiceTransactionMethodTwo(String.Format("{0}, client transactionId.Local2 = {1}", arg, transactionLocalId2));
                        string transactionDistId2 = Transaction.Current.TransactionInformation.DistributedIdentifier.ToString();
                        TemsTrace.WriteLine(TraceLevel.Verbose, "client transactionId.Dist2 = {0}", transactionDistId2);
                        using (TransactionScope scope3 = new TransactionScope())
                        {
                            string transactionLocalId3 = Transaction.Current.TransactionInformation.LocalIdentifier.ToString();
                            ((ServiceDuplexTransactionClient)proxy).ServiceTransactionMethodThree(String.Format("{0}, client transactionId.Local3 = {1}", arg, transactionLocalId3));
                            string transactionDistId3 = Transaction.Current.TransactionInformation.DistributedIdentifier.ToString();
                            TemsTrace.WriteLine(TraceLevel.Verbose, "client transactionId.Dist3 = {0}", transactionDistId3);
                            //Transaction.Current.Rollback();
                            scope3.Complete();
                        }
                        //Transaction.Current.Rollback();
                        scope2.Complete();
                    }
                    //Transaction.Current.Rollback();
                    //scope1.Complete();
                }
                //using (TransactionScope scope = new TransactionScope())
                //{
                //    string transactionId = "Client Transaction ID = " + Transaction.Current.TransactionInformation.DistributedIdentifier;
                //    ((ServiceDuplexTransactionClient)proxy).ServiceTransactionMethodTwo(String.Format("{0}, client transactionId = {1}", arg, transactionId));
                //    scope.Complete();
                //}

                //using (TransactionScope scope = new TransactionScope())
                //{
                //    string transactionId = "Client Transaction ID = " + Transaction.Current.TransactionInformation.DistributedIdentifier;
                //    ((ServiceDuplexTransactionClient)proxy).ServiceTransactionMethodThree(String.Format("{0}, client transactionId = {1}", arg, transactionId));
                //    scope.Complete();
                //}
                reply = "Reply is N/A";
            }
            return reply;
        }

        private string GetExpectedReply(string arg)
        {
            string reply = "";
            if (isContractType("RequestReply") ||
                isContractType("RequestReplySession") ||
                isContractType("RequestReplyAsync"))
            {
                reply = arg.ToUpper();
            }
            else if (isContractType("BookClient") ||
                        isContractType("Datagram") ||
                        isContractType("DatagramSession") ||
                        isContractType("Duplex") ||
                        isContractType("DuplexSession") ||
                        isContractType("DuplexTransaction"))
            {
                reply = "Reply is N/A";
            }

            return reply;
        }

        private void ReadIterations()
        {
            System.Console.WriteLine(String.Format("Enter number of iterations (default = {0}), or x to exit:", defaultIterations));
            key = System.Console.ReadLine();

            if (key.ToUpper() == "X")
            {
                Environment.Exit(0);
            }

            try
            {
                iterations = Convert.ToInt64(key);
            }
            catch (Exception)
            {
                iterations = defaultIterations;
            }
        }

        private void ParseArgs(String[] args)
        {
            int i = 0;

            while (i < args.Length)
            {
                if (args[i].CompareTo("-ep") == 0)
                {
                    if ((i + 1) >= args.Length) { Usage(); }
                    endpointName = args[i + 1];
                    i += 2;
                }
                else if (args[i].CompareTo("-log") == 0)
                {
                    if ((i + 1) >= args.Length) { Usage(); }
                    int logValue = Convert.ToInt32(args[i + 1]);
                    TemsTrace.TraceLevel = (TraceLevel)logValue;
                    i += 2;
                }
                else if (args[i].CompareTo("-iter") == 0)
                {
                    if ((i + 1) >= args.Length) { Usage(); }
                    defaultIterations = Convert.ToInt64(args[i + 1]);
                    i += 2;
                }
                else if (args[i].CompareTo("-size") == 0)
                {
                    if ((i + 1) >= args.Length) { Usage(); }
                    returnMessageSize = Convert.ToInt64(args[i + 1]);
                    i += 2;
                }
                else if (args[i].CompareTo("-run") == 0)
                {
                    autoRun = true;
                    i += 1;
                }
                else
                {
                    System.Console.Error.WriteLine("Unrecognized parameter: " + args[i]);
                    Usage();
                }
            }
        }

        private bool isContractType(string typeName)
        {
            return typeName == endpointName.Substring(endpointName.IndexOf(".") + 1);
        }

        private void Usage()
        {
            System.Console.WriteLine("\nUsage: Client [options]");
            System.Console.WriteLine("");
            System.Console.WriteLine("   where options are:");
            System.Console.WriteLine("");
            System.Console.WriteLine("   -ep       <endpoint name>       - The endpoint name.");
            System.Console.WriteLine("   -log      <trace log level>     - The trace log level to display.");
            System.Console.WriteLine("   -iter     <default iterations>  - Default number of iterations.");
            System.Console.WriteLine("   -size     <return message size> - For Request-Reply specifies a size for the return message.");
            System.Console.WriteLine("   -run                            - If present, client runs without user prompt for iterations.");
            Environment.Exit(0);
        }

        #region IServiceDuplexCallback / IServiceDuplexSessionCallback Members

        public void CallbackMethodOne(string key)
        {
#if ManualAcknowledgeSample

            SendAcknowledge();
#endif
            callbackOneCount++;
            LogCallbackTotalCount();
        }

        public void CallbackMethodTwo(string key)
        {
#if ManualAcknowledgeSample

            SendAcknowledge();
#endif
            callbackTwoCount++;
            LogCallbackTotalCount();
        }

        public void CallbackMethodThree(string key)
        {
#if ManualAcknowledgeSample

            SendAcknowledge();
#endif
            callbackThreeCount++;
            LogCallbackTotalCount();
        }

        #endregion

        #region IServiceTransactionCallback Members

        public void CallbackTransactionMethodOne(string key)
        {
#if ManualAcknowledgeSample

            SendAcknowledge();
#endif
            callbackOneCount++;
            LogCallbackTotalCount();
        }

        public void CallbackTransactionMethodTwo(string key)
        {
#if ManualAcknowledgeSample

            SendAcknowledge();
#endif
            callbackTwoCount++;
            LogCallbackTotalCount();
        }

        public void CallbackTransactionMethodThree(string key)
        {
#if ManualAcknowledgeSample

            SendAcknowledge();
#endif
            callbackThreeCount++;
            LogCallbackTotalCount();
        }

        #endregion

        private void LogCallbackTotalCount()
        {
            totalCallbackCount = callbackOneCount + callbackTwoCount + callbackThreeCount;
            if (totalCallbackCount % timeLogModulus == 0)
            {
                //TemsTrace.WriteLine(TraceLevel.Info, "totalCallbackCount = {0}, callbackOneCount = {1}, callbackTwoCount = {2}, callbackThreeCount = {3}", totalCallbackCount, callbackOneCount, callbackTwoCount, callbackThreeCount);
                TemsTrace.WriteLine(TraceLevel.Info, "totalCallbackCount = {0}", totalCallbackCount);
            }
        }
    }
}
