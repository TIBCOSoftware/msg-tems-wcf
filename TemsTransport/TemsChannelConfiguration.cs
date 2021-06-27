/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using TIBCO.EMS;
using System;
using System.Security.Cryptography.X509Certificates;

namespace com.tibco.wcf.tems
{
    static class TemsConfigurationStrings
    {
        #region Base_Properties

        /**
         * The maximum size (in bytes) of the buffer pool.
         * 
         * Default = 524,288 bytes (512 * 1024).
         */
        public const string MaxBufferPoolSize = "maxBufferPoolSize";

        /**
         * The maximum allowable message size (in bytes) that can be received.
         * 
         * Default = 65,536 bytes (64 * 1024).
         */
        public const string MaxReceivedMessageSize = "maxReceivedMessageSize";

        #endregion

        #region EMS_Session_Properties

        /**
         * When true, the session has transaction semantics, and
         * AcknowledgeMode is irrelevant.
         * When false, it has non-transaction semantics.
         * 
         * Default = false.
         */
        //public const string SessionTransacted = "sessionTransacted";

        /**
         * This mode governs message acknowledgement and redelivery for
         * consumers associated with the session.
         * 
         *     Enumeration options:
         *     
         *         AutoAcknowledge
         *         ClientAcknowledge
         *         DupsOkAcknowledge
         *         ExplicitClientAcknowledge
         *         ExplicitClientDupsOkAcknowledge
         *         NoAcknowledge
         *         
         * Default = AutoAcknowledge
         */
        public const string SessionAcknowledgeMode = "sessionAcknowledgeMode";

        #endregion

        #region EMS_MessageProducer_Properties

        /**
         * Delivery mode instructs the server concerning persistent storage.
         * Programs can use this property to define a default delivery mode
         * for messages that this producer sends. Individual sending calls can
         * override this default value.
         * 
         *     Enumeration options:
         * 
         *         NonPersistent
         *         Persistent
         *         ReliableDelivery
         *         
         * Default = Persistent.
         */

        public const string MessageDeliveryMode = "messageDeliveryMode";

        /**
         * Applications that do not require message IDs can reduce overhead
         * costs by disabling IDs (set this property to true).
         * 
         * Default = false.
         */
        public const string DisableMessageID = "disableMessageID";

        /**
         * Applications that do not require timestamps can reduce overhead
         * costs by disabling timestamps (set this property to true).
         * 
         * Default = false
         */
        public const string DisableMessageTimestamp = "disableMessageTimestamp";

        /** 
         * Priority affects the order in which the server delivers messages to
         * consumers (higher values first).
         * The JMS specification defines ten levels of priority value, from
         * zero (lowest priority) to 9 (highest priority). The specification
         * suggests that clients consider 0–4 as gradations of normal priority,
         * and priorities 5–9 as gradations of expedited priority.
         * Programs can use this property to define a default priority for
         * messages that this producer sends. Individual sending calls can
         * override this default value.
         * 
         * Default = 4.
         */
        public const string Priority = "priority";

        /** 
         * Time-to-live (in milliseconds) determines the expiration time of a
         * message.
         * • If the time-to-live is non-zero, the expiration is the sum of that
         * time-to-live and the sending client’s current time (GMT). This
         * rule applies even within sessions with transaction semantics—
         * the timer begins with the send call, not the commit call.
         * • If the time-to-live is zero, then expiration is also zero—
         * indicating that the message never expires.
         * Programs can use this property to define a default time-to-live for
         * messages that this producer sends. Individual sending calls can
         * override this default value.
         * Whenever your application uses non-zero values for message
         * expiration or time-to-live, you must ensure that clocks are
         * synchronized among all the host computers that send and receive
         * messages. Synchronize clocks to a tolerance that is a very small
         * fraction of the smallest time-to-live.
         * 
         * Default = 0.
         */
        public const string TimeToLive = "timeToLive";
        
        #endregion

        #region MessageProtocol

        /** 
         * MessageProtocol specifies the type of ITemsMessageProtocol
         * implementation used to transform between a WCF message and an EMS message.
         *
         * This can be one of three values in the TemsMessageProtocolType enum:
         *
         *   - WCFNative - The encoded WCF message is stored in the EMS message body.
         *                 No EMS - WCF header property values transformed.
         *
         *   - TIBCOSoapOverJMS2004 - EMS - WCF header property values are transformed
         *                   according to the Soap Over JMS specifications.
         *
         *   - Custom - A custom implementation is provided.  The custom instance
         *              is set on the binding CustomMessageProtocol property.

         * Default = TemsMessageProtocolType.WCFNative.
         */
        public const string MessageProtocol = "messageProtocol";

        /** 
         * MessageType specifies the type of EMS Message to use.
         *
         * TemsMessageType.Bytes specifies that an EMS BytesMessage is used.
         * 
         * TemsMessageType.Text specifies that an EMS TextMessage is used.
         * 
         * Note: An EMS BytesMessage is always used for the
         *       BinaryMessageEncoding class and this attribute is ignored.
         *
         * The EMS TextMessage provides interoperability.  For WCF to WCF
         * applications, using the EMS BytesMessage may provide a slight
         * performance benefit since the byte[] > string > byte[] UTF encoding
         * is not required.
         *
         * Default = TemsMessageType.Text.
         */
        public const string MessageType = "messageType";

        /** 
         * ThrowOnInvalidUTF specifies if an exception is thrown for invalid
         * encoding detected by the UTF Encoder used for encoding the string
         * for an EMS TextMessage.
         *
         * true to specify that an exception (System.ArgumentException) be
         * thrown when an invalid encoding is detected.  Otherwise, the Encoder
         * does not throw an exception, and the invalid sequence is ignored.
         *
         * Note: For security reasons, setting this true is recommended to turn 
         *       on error detection.
         * 
         * Default = true.
         */
        public const string ThrowOnInvalidUTF = "throwOnInvalidUTF";

        #endregion

        #region WsdlExportExtension

        /** 
         * WsdlExtensionActive specifies if the WSDL exported by MEX should be
         * modified to include the jndi and jms elements for Soap Over JMS for 
         * all endpoints that use a Tems binding.
         *
         * Note: If this is set to false, WSDL export for Soap Over JMS can be
         *       set on individual endpoints using the endpoint
         *       behaviorConfiguration attribute:
         *
         *       <endpoint name="Sample.Service"
         *          address="net.tems://localhost:7222/queue/sample"
         *          behaviorConfiguration="TemsWsdlExport"
         *          ... />  
         *
         *      <endpointBehaviors>
         *        <behavior name="TemsWsdlExport">
         *          <TemsWsdlExportExtension />
         *        </behavior>
         *      </endpointBehaviors>
         * 
         * Default = true.
         */
        public const string WsdlExtensionActive = "wsdlExtensionActive";

        /** 
         * WsdlTypeSchemaImport specifies if the Tems WSDL exported extension
         * should include the <wsdl:types><xsd:schema ... <xsd:import> schema 
         * content expanded into the current WSDL document.
         * 
         * Default = false.
         */
        public const string WsdlTypeSchemaImport = "wsdlTypeSchemaImport";

        #endregion

        /** 
         * CustomMessageProtocolType specifies the class type that implements
         * ITemsMessageProtocol that is used for a custom message protocol.
         * 
         * Default = string.Empty.
         */
        public const string CustomMessageProtocolType = "customMessageProtocolType";

        /** 
         * WsdlExportExtensionType specifies the class type that implements
         * IWsdlExportExtension that is used when a MEX endpoint is enabled with
         * a custom message protocol.
         * 
         * Default = string.Empty.
         */
        public const string WsdlExportExtensionType = "wsdlExportExtensionType";

        #region Session_Properties

        #endregion

        /** 
         * Sets the value of the JMS_TIBCO_COMPRESS property.
         * If set to true, the EMS message will be compressed.
         * 
         * Default = false
         */
        public const string MessageCompression = "messageCompression";
        
        #region Client_Properties

        /** 
         * ClientBaseAddress specifies the EMS destination for Duplex MEP callback messages.
         * 
         * Default = "".
         */
        public const string ClientBaseAddress = "clientBaseAddress";

        /** 
         * AppHandlesMessageAcknowledge controls whether the application or TEMS handles receive message acknowledgement.
         * If set to true, the Application will handle EMS receive message acknowledgement.
         * 
         * Default = false.
         */
        public const string AppHandlesMessageAcknowledge = "appHandlesMessageAcknowledge";

        /** 
        * AppManagesConnections controls whether the application or TEMS manages EMS connections 
        * (creating and closing connections).  This allows the application to share or pool EMS connections.
        * If set to true, the Application will manage EMS connections.
        * 
        * Default = false.
        */
        public const string AppManagesConnections = "appManagesConnections";

        /** 
          * MessageSelector is a string that lets a client program specify a set of messages,
          * based on the values of message headers and properties. A selector matches a message if,
          * after substituting header and property values from the message into the selector string,
          * the string evaluates to true. Consumers are delivered only those messages that match a selector.
          * 
          * Default = "".
          */
        public const string MessageSelector = "messageSelector";

        #endregion

        /** 
         * ReplyDestCheckInterval specifies a TimeSpan that is used as an interval 
         * between checks made for reply destinations that are no longer valid
         * because the requesting client has been closed.  The default value
         * should generally not be changed unless there are specific performance
         * reasons for changing the value.
         * 
         * If a service is used by a large number of short lived clients, it
         * may be desirable to decrease the TimeSpan to quickly free EMS 
         * resources the service will no longer use.
         * 
         * If a service is used by a large number of long lived clients, it
         * may be desirable to increase the TimeSpan to reduce the processing
         * overhead of checking these more frequently than is needed.
         * 
         * Default = "00:01:00".
         */
        public const string ReplyDestCheckInterval = "replyDestCheckInterval";

        #region ConnectionFactory_Properties

        public const string ClientID = "clientID";
        public const string ConnAttemptCount = "connAttemptCount";
        public const string ConnAttemptDelay = "connAttemptDelay";
        public const string ConnAttemptTimeout = "connAttemptTimeout";
        public const string LoadBalanceMetric = "loadBalanceMetric";
        public const string ReconnAttemptCount = "reconnAttemptCount";
        public const string ReconnAttemptDelay = "reconnAttemptDelay";
        public const string ReconnAttemptTimeout = "reconnAttemptTimeout";
        public const string ServerUrl = "serverUrl";

        /**
         * Certificate Store Type used for reading SSL Certificates
         * 
         *     Enumeration options:
         * 
         *         EMSSSL_STORE_TYPE_FILE
         *         EMSSSL_STORE_TYPE_SYSTEM
         *         EMSSSL_STORE_TYPE_DEFAULT  (same as EMSSSL_STORE_TYPE_SYSTEM)
         *         
         * Default = EMSSSL_STORE_TYPE_DEFAULT.
         * 
         * If EMSSSL_STORE_TYPE_SYSTEM is set then the following properties are used to
         * configure the certificate store:
         *          certificateStoreLocation
         *          certificateStoreName
         *          certificateNameAsFullSubjectDN (must not be an empty string if configuring SSL)
         * If EMSSSL_STORE_TYPE_FILE is set then the following properties are used to
         * configure the certificate store:
         *          sslClientIdentity (must not be an empty string if configuring SSL)
         *          sslPassword
         *          
         */
        public const string CertificateStoreType = "certificateStoreType";

        /**
         * The certificate store location indicates where to lookup the certificate
         * by name. This property is used when CertificateStoreType
         * is EMSSSL_STORE_TYPE_SYSTEM
         * 
         *     Enumeration options:
         * 
         *         CurrentUser
         *         LocalMachine
         *         
         * Default = LocalMachine.
         * 
         */
        public const string CertificateStoreLocation = "certificateStoreLocation";

        /**
         * The Certificate Store Name where the client library looks for the certificates.
         * If no store name is specified, then the default store name is "My"
         * store name within this store location.  This property is used when CertificateStoreType
         * is EMSSSL_STORE_TYPE_SYSTEM
         *
         * Default = "".
         */
        public const string CertificateStoreName = "certificateStoreName";

        /**
         * The certificate name is the subject distinguished name of the certificate. During the SSL handshake,
         * the server searches for the named certificate in the store specified in CertificateStoreName
         * at the location specified by CertificateStoreLocation.
         * The search criteria to find the certificate in the store name at the store
         * location is X509FindType.FindBySubjectDistinguishedName.  A subject DN sample
         * E=client@testcompany.com, CN=client, OU=client Unit, O=Test Company, L=us-english,
         * S=California, C=US
         * 
         * Default = "".
         */
        public const string CertificateNameAsFullSubjectDN = "certificateNameAsFullSubjectDN";

        /**
         * Sets Client Identity (The client's digital certificate), the only file type
         * that is supported here is a pkcs12 or .pfx file.  NOTE: If other file format
         * are specified then a configuration exception will be thrown during the SSL
         * handshake.
         * 
         * Default = "".
        */
        public const string SSLClientIdentity = "sslClientIdentity";

        /**
         * Set the private key password.
         *
         * Default = "".
         */
        public const string SSLPassword = "sslPassword";

        public const string SSLAuthOnly = "sslAuthOnly";
        public const string SSLProxyHost = "sslProxyHost";
        public const string SSLProxyPort = "sslProxyPort";
        public const string SSLProxyAuthUsername = "sslProxyAuthUsername";
        public const string SSLProxyAuthPassword = "sslProxyAuthPassword";
        public const string SSLTrace = "sslTrace";
        public const string SSLTargetHostName = "sslTargetHostName";
        public const string Username = "username";
        public const string Password = "password";

        #endregion

        #region Config_Control

        public const string AllowAdministratedConnFactory = "allowAdministratedConnFactory";
        public const string AllowAdministratedEndpointDest = "allowAdministratedEndpointDest";
        public const string AllowAdministratedCallbackDest = "allowAdministratedCallbackDest";
        public const string AllowBindingChanges = "allowBindingChanges";
        public const string AllowCustomMessageProtocol = "allowCustomMessageProtocol";

        #endregion
    }

    static class TemsConfigurationDescriptions
    {
        #region Base_Properties

        /**
         * The maximum size (in bytes) of the buffer pool.
         * 
         * Default = 524,288 bytes (512 * 1024).
         */
        public const string MaxBufferPoolSize = "The maximum size (in bytes) of the buffer pool.\n\nDefault = 524,288 bytes (512 * 1024).";

        /**
         * The maximum allowable message size (in bytes) that can be received.
         * 
         * Default = 65,536 bytes (64 * 1024).
         */
        public const string MaxReceivedMessageSize = "The maximum allowable message size (in bytes) that can be received.\n\nDefault = 65,536 bytes (64 * 1024).";

        #endregion

        #region EMS_Session_Properties

        /**
         * When true, the session has transaction semantics, and
         * AcknowledgeMode is irrelevant.
         * When false, it has non-transaction semantics.
         * 
         * Default = false.
         */
        //public const string SessionTransacted = "sessionTransacted";

        /**
         * This mode governs message acknowledgement and redelivery for
         * consumers associated with the session.
         * 
         *     Enumeration options:
         *     
         *         AutoAcknowledge
         *         ClientAcknowledge
         *         DupsOkAcknowledge
         *         ExplicitClientAcknowledge
         *         ExplicitClientDupsOkAcknowledge
         *         NoAcknowledge
         *         
         * Default = AutoAcknowledge
         */
        public const string SessionAcknowledgeMode = "This mode governs message acknowledgement and redelivery for consumers associated with the session.\nEnumeration options:\n\n    AutoAcknowledge\n    ClientAcknowledge\n    DupsOkAcknowledge\n    ExplicitClientAcknowledge\n    ExplicitClientDupsOkAcknowledge\n    NoAcknowledge\n        \nDefault = AutoAcknowledge";

        #endregion

        #region EMS_MessageProducer_Properties

        /**
         * Delivery mode instructs the server concerning persistent storage.
         * Programs can use this property to define a default delivery mode
         * for messages that this producer sends. Individual sending calls can
         * override this default value.
         * 
         *     Enumeration options:
         * 
         *         NonPersistent
         *         Persistent
         *         ReliableDelivery
         *         
         * Default = Persistent.
         */

        public const string MessageDeliveryMode = "Delivery mode instructs the server concerning persistent storage.  Programs can use this property to define a default delivery mode for messages that this producer sends. Individual sending calls can override this default value.\nEnumeration options:\n\n    NonPersistent\n    Persistent\n    ReliableDelivery\n    \nDefault = Persistent.";

        /**
         * Applications that do not require message IDs can reduce overhead
         * costs by disabling IDs (set this property to true).
         * 
         * Default = false.
         */
        public const string DisableMessageID = "Applications that do not require message IDs can reduce overhead costs by disabling IDs (set this property to true).\n\nDefault = false.";

        /**
         * Applications that do not require timestamps can reduce overhead
         * costs by disabling timestamps (set this property to true).
         * 
         * Default = false
         */
        public const string DisableMessageTimestamp = "Applications that do not require timestamps can reduce overhead costs by disabling timestamps (set this property to true).\n\nDefault = false";

        /** 
         * Priority affects the order in which the server delivers messages to
         * consumers (higher values first).
         * The JMS specification defines ten levels of priority value, from
         * zero (lowest priority) to 9 (highest priority). The specification
         * suggests that clients consider 0–4 as gradations of normal priority,
         * and priorities 5–9 as gradations of expedited priority.
         * Programs can use this property to define a default priority for
         * messages that this producer sends. Individual sending calls can
         * override this default value.
         * 
         * Default = 4.
         */
        public const string Priority = "Priority affects the order in which the server delivers messages to consumers (higher values first).  The JMS specification defines ten levels of priority value, from zero (lowest priority) to 9 (highest priority). The specification suggests that clients consider 0–4 as gradations of normal priority, and priorities 5–9 as gradations of expedited priority.  Programs can use this property to define a default priority for messages that this producer sends. Individual sending calls can override this default value.\n\nDefault = 4.";

        /** 
         * Time-to-live (in milliseconds) determines the expiration time of a
         * message.
         * • If the time-to-live is non-zero, the expiration is the sum of that
         * time-to-live and the sending client’s current time (GMT). This
         * rule applies even within sessions with transaction semantics—
         * the timer begins with the send call, not the commit call.
         * • If the time-to-live is zero, then expiration is also zero—
         * indicating that the message never expires.
         * Programs can use this property to define a default time-to-live for
         * messages that this producer sends. Individual sending calls can
         * override this default value.
         * Whenever your application uses non-zero values for message
         * expiration or time-to-live, you must ensure that clocks are
         * synchronized among all the host computers that send and receive
         * messages. Synchronize clocks to a tolerance that is a very small
         * fraction of the smallest time-to-live.
         * 
         * Default = 0.
         */
        public const string TimeToLive = "Time-to-live (in milliseconds) determines the expiration time of a message.\n    If the time-to-live is non-zero, the expiration is the sum of that time-to-live and the sending client’s current time (GMT).  This rule applies even within sessions with transaction semantics— the timer begins with the send call, not the commit call.\n    If the time-to-live is zero, then expiration is also zero—indicating that the message never expires.\nPrograms can use this property to define a default time-to-live for messages that this producer sends. Individual sending calls can override this default value.  Whenever your application uses non-zero values for message expiration or time-to-live, you must ensure that clocks are synchronized among all the host computers that send and receive messages.  Synchronize clocks to a tolerance that is a very small fraction of the smallest time-to-live.\n\nDefault = 0.";

        #endregion

        #region MessageProtocol

        /** 
         * MessageProtocol specifies the type of ITemsMessageProtocol
         * implementation used to transform between a WCF message and an EMS message.
         *
         * This can be one of three values in the TemsMessageProtocolType enum:
         *
         *   - WCFNative - The encoded WCF message is stored in the EMS message body.
         *                 No EMS - WCF header property values transformed.
         *
         *   - TIBCOSoapOverJMS2004 - EMS - WCF header property values are transformed
         *                   according to the Soap Over JMS specifications.
         *
         *   - Custom - A custom implementation is provided.  The custom instance
         *              is set on the binding CustomMessageProtocol property.

         * Default = TemsMessageProtocolType.WCFNative.
         */
        public const string MessageProtocol = "MessageProtocol specifies the type of ITemsMessageProtocol\nimplementation used to transform between a WCF message and an EMS message.\n\nThis can be one of three values in the TemsMessageProtocolType enum:\n\n  - WCFNative - The encoded WCF message is stored in the EMS message body.\n                No EMS - WCF header property values transformed.\n\n  - TIBCOSoapOverJMS2004 - EMS - WCF header property values are transformed\n                  according to the Soap Over JMS specifications.\n\n  - Custom - A custom implementation is provided.  The custom instance\n             is set on the binding CustomMessageProtocol property.\n\nDefault = TemsMessageProtocolType.WCFNative.";

        /** 
         * MessageType specifies the type of EMS Message to use.
         *
         * TemsMessageType.Bytes specifies that an EMS BytesMessage is used.
         * 
         * TemsMessageType.Text specifies that an EMS TextMessage is used.
         * 
         * Note: An EMS BytesMessage is always used for the
         *       BinaryMessageEncoding class and this attribute is ignored.
         *
         * The EMS TextMessage provides interoperability.  For WCF to WCF
         * applications, using the EMS BytesMessage may provide a slight
         * performance benefit since the byte[] > string > byte[] UTF encoding
         * is not required.
         *
         * Default = TemsMessageType.Text.
         */
        public const string MessageType = "MessageType specifies the type of EMS Message to use.\n\nTemsMessageType.Bytes specifies that an EMS BytesMessage is used.\n\nTemsMessageType.Text specifies that an EMS TextMessage is used.\n\nNote: An EMS BytesMessage is always used for the\n      BinaryMessageEncoding class and this attribute is ignored.\n\nThe EMS TextMessage provides interoperability.  For WCF to WCF\napplications, using the EMS BytesMessage may provide a slight\nperformance benefit since the byte[] > string > byte[] UTF encoding\nis not required.\n\nDefault = TemsMessageType.Text.";

        /** 
         * ThrowOnInvalidUTF specifies if an exception is thrown for invalid
         * encoding detected by the UTF Encoder used for encoding the string
         * for an EMS TextMessage.
         *
         * true to specify that an exception (System.ArgumentException) be
         * thrown when an invalid encoding is detected.  Otherwise, the Encoder
         * does not throw an exception, and the invalid sequence is ignored.
         *
         * Note: For security reasons, setting this true is recommended to turn 
         *       on error detection.
         * 
         * Default = true.
         */
        public const string ThrowOnInvalidUTF = "ThrowOnInvalidUTF specifies if an exception is thrown for invalid\nencoding detected by the UTF Encoder used for encoding the string\nfor an EMS TextMessage.\n\ntrue to specify that an exception (System.ArgumentException) be\nthrown when an invalid encoding is detected.  Otherwise, the Encoder\ndoes not throw an exception, and the invalid sequence is ignored.\n\nNote: For security reasons, setting this true is recommended to turn \n      on error detection.\n\nDefault = true.";

        #endregion

        #region WsdlExportExtension

        /** 
         * WsdlExtensionActive specifies if the WSDL exported by MEX should be
         * modified to include the jndi and jms elements for Soap Over JMS for 
         * all endpoints that use a Tems binding.
         *
         * Note: If this is set to false, WSDL export for Soap Over JMS can be
         *       set on individual endpoints using the endpoint
         *       behaviorConfiguration attribute:
         *
         *       <endpoint name="Sample.Service"
         *          address="net.tems://localhost:7222/queue/sample"
         *          behaviorConfiguration="TemsWsdlExport"
         *          ... />  
         *
         *      <endpointBehaviors>
         *        <behavior name="TemsWsdlExport">
         *          <TemsWsdlExportExtension />
         *        </behavior>
         *      </endpointBehaviors>
         * 
         * Default = true.
         */
        public const string WsdlExtensionActive = "WsdlExtensionActive specifies if the WSDL exported by MEX should be\nmodified to include the jndi and jms elements for Soap Over JMS for \nall endpoints that use a Tems binding.\n\nNote: If this is set to false, WSDL export for Soap Over JMS can be\n      set on individual endpoints using the endpoint\n      behaviorConfiguration attribute:\n\n      <endpoint name=\"Sample.Service\"\n         address=\"net.tems://localhost:7222/queue/sample\"\n         behaviorConfiguration=\"TemsWsdlExport\"\n         ... />  \n\n     <endpointBehaviors>\n       <behavior name=\"TemsWsdlExport\">\n         <TemsWsdlExportExtension />\n       </behavior>\n     </endpointBehaviors>\n\nDefault = true.";

        /** 
         * WsdlTypeSchemaImport specifies if the Tems WSDL exported extension
         * should include the <wsdl:types><xsd:schema ... <xsd:import> schema 
         * content expanded into the current WSDL document.
         * 
         * Default = false.
         */
        public const string WsdlTypeSchemaImport = "WsdlTypeSchemaImport specifies if the Tems WSDL exported extension\nshould include the <wsdl:types><xsd:schema ... <xsd:import> schema \ncontent expanded into the current WSDL document.\n\nDefault = false.";

        #endregion

        /** 
         * CustomMessageProtocolType specifies the class type that implements
         * ITemsMessageProtocol that is used for a custom message protocol.
         * 
         * Default = string.Empty.
         */
        public const string CustomMessageProtocolType = "CustomMessageProtocolType specifies the class type that implements\nITemsMessageProtocol that is used for a custom message protocol.\n\nDefault = string.Empty.";

        /** 
         * WsdlExportExtensionType specifies the class type that implements
         * IWsdlExportExtension that is used when a MEX endpoint is enabled with
         * a custom message protocol.
         * 
         * Default = string.Empty.
         */
        public const string WsdlExportExtensionType = "WsdlExportExtensionType specifies the class type that implements\nIWsdlExportExtension that is used when a MEX endpoint is enabled with\na custom message protocol.\n\nDefault = string.Empty.";

        #region Session_Properties

        #endregion

        #region Client_Properties

        public const string MessageCompression = "messageCompression";

        /** 
         * ClientBaseAddress specifies the EMS destination for Duplex MEP callback messages.
         * 
         * Default = "".
         */
        public const string ClientBaseAddress = "Specifies the EMS destination for Duplex MEP callback messages.\n\nDefault = [client.endpoint.address].callback.";

        /** 
         * AppHandlesMessageAcknowledge controls whether the application or TEMS handles receive message acknowledge.
         * If set to true, the Application will handle EMS receive message acknowledgement.
         * 
         * Default = false.
         */
        public const string AppHandlesMessageAcknowledge = "Controls whether the application or TEMS handles receive message acknowledge.\n\nDefault = false.";

        /** 
        * AppManagesConnections controls whether the application or TEMS manages EMS connections 
        * (creating and closing connections).  This allows the application to share or pool EMS connections.
        * If set to true, the Application will manage EMS connections.
        * 
        * Default = false.
        */
        public const string AppManagesConnections = "Controls whether the application or TEMS manages EMS connections.\n\nDefault = false.";

        /** 
          * MessageSelector is a string that lets a client program specify a set of messages,
          * based on the values of message headers and properties. A selector matches a message if,
          * after substituting header and property values from the message into the selector string,
          * the string evaluates to true. Consumers are delivered only those messages that match a selector.
          * 
          * Default = "".
          */
        public const string MessageSelector = "Specifies the query string used so specify a set of messages that matches on the EMS message header and/or properties.\n\nDefault = string.Empty.";

        #endregion

        /** 
         * ReplyDestCheckInterval specifies a TimeSpan that is used as an interval 
         * between checks made for reply destinations that are no longer valid
         * because the requesting client has been closed.  The default value
         * should generally not be changed unless there are specific performance
         * reasons for changing the value.
         * 
         * If a service is used by a large number of short lived clients, it
         * may be desirable to decrease the TimeSpan to quickly free EMS 
         * resources the service will no longer use.
         * 
         * If a service is used by a large number of long lived clients, it
         * may be desirable to increase the TimeSpan to reduce the processing
         * overhead of checking these more frequently than is needed.
         * 
         * Default = "00:01:00".
         */
        public const string ReplyDestCheckInterval = "ReplyDestCheckInterval specifies a TimeSpan that is used as an interval \nbetween checks made for reply destinations that are no longer valid\nbecause the requesting client has been closed.  The default value\nshould generally not be changed unless there are specific performance\nreasons for changing the value.\n\nIf a service is used by a large number of short lived clients, it\nmay be desirable to decrease the TimeSpan to quickly free EMS \nresources the service will no longer use.\n\nIf a service is used by a large number of long lived clients, it\nmay be desirable to increase the TimeSpan to reduce the processing\noverhead of checking these more frequently than is needed.\n\nDefault = \"00:01:00\".";

        #region ConnectionFactory_Properties

        public const string ClientID = "clientID";
        public const string ConnAttemptCount = "connAttemptCount";
        public const string ConnAttemptDelay = "connAttemptDelay";
        public const string ConnAttemptTimeout = "connAttemptTimeout";
        public const string LoadBalanceMetric = "loadBalanceMetric";
        public const string ReconnAttemptCount = "reconnAttemptCount";
        public const string ReconnAttemptDelay = "reconnAttemptDelay";
        public const string ReconnAttemptTimeout = "reconnAttemptTimeout";
        public const string ServerUrl = "serverUrl";

        /**
         * Certificate Store Type used for reading SSL Certificates
         * 
         *     Enumeration options:
         * 
         *         EMSSSL_STORE_TYPE_FILE
         *         EMSSSL_STORE_TYPE_SYSTEM
         *         EMSSSL_STORE_TYPE_DEFAULT  (same as EMSSSL_STORE_TYPE_SYSTEM)
         *         
         * Default = EMSSSL_STORE_TYPE_DEFAULT.
         * 
         * If EMSSSL_STORE_TYPE_SYSTEM is set then the following properties are used to
         * configure the certificate store:
         *          certificateStoreLocation
         *          certificateStoreName
         *          certificateNameAsFullSubjectDN (must not be an empty string if configuring SSL)
         * If EMSSSL_STORE_TYPE_FILE is set then the following properties are used to
         * configure the certificate store:
         *          sslClientIdentity (must not be an empty string if configuring SSL)
         *          sslPassword
         *          
         */
        public const string CertificateStoreType = "Identifies the certificate store type used for reading SSL Certificates.\n \nEnumeration options:\n\n    EMSSSL_STORE_TYPE_FILE\n    EMSSSL_STORE_TYPE_System\n    EMSSSL_STORE_TYPE_Default\n    \nDefault = EMSSSL_STORE_TYPE_Default.";

        /**
         * The certificate store location indicates where to lookup the certificate
         * by name. This property is used when CertificateStoreType
         * is EMSSSL_STORE_TYPE_SYSTEM
         * 
         *     Enumeration options:
         * 
         *         CurrentUser
         *         LocalMachine
         *         
         * Default = LocalMachine.
         * 
         */
        public const string CertificateStoreLocation = "The certificate store location indicates where to lookup the certificate by name.\n \nEnumeration options:\n\n    CurrentUser\n    LocalMachine\n    \nDefault = LocalMachine.";

        /**
         * The Certificate Store Name where the client library looks for the certificates.
         * If no store name is specified, then the default store name is "My"
         * store name within this store location.  This property is used when CertificateStoreType
         * is EMSSSL_STORE_TYPE_SYSTEM
         *
         * Default = "".
         */
        public const string CertificateStoreName = "The Certificate Store Name where the client library looks for the certificates";

        /**
         * The certificate name is the subject distinguished name of the certificate. During the SSL handshake,
         * the server searches for the named certificate in the store specified in CertificateStoreName
         * at the location specified by CertificateStoreLocation.
         * The search criteria to find the certificate in the store name at the store
         * location is X509FindType.FindBySubjectDistinguishedName.  A subject DN sample
         * E=client@testcompany.com, CN=client, OU=client Unit, O=Test Company, L=us-english,
         * S=California, C=US
         * 
         * Default = "".
         */
        public const string CertificateNameAsFullSubjectDN = "The certificate name is the subject distinguished name of the certificate.";

        /**
         * Sets Client Identity (The client's digital certificate), the only file type
         * that is supported here is a .pkcs12 or .pfx file.  NOTE: If other file format
         * are specified then a configuration exception will be thrown during the SSL
         * handshake.
         * 
         * Default = "".
        */
        public const string SSLClientIdentity = "Sets Client Identity (The client's digital certificate), the only file type that is supported here is a .pkcs12 or .pfx file.";

        /**
         * Set the private key password.
         *
         * Default = "".
         */
        public const string SSLPassword = "Sets the private key password";

        public const string SSLAuthOnly = "sslAuthOnly";
        public const string SSLProxyHost = "sslProxyHost";
        public const string SSLProxyPort = "sslProxyPort";
        public const string SSLProxyAuthUsername = "sslProxyAuthUsername";
        public const string SSLProxyAuthPassword = "sslProxyAuthPassword";
        public const string SSLTrace = "sslTrace";
        public const string SSLTargetHostName = "sslTargetHostName";
        public const string Username = "username";
        public const string Password = "password";

        #endregion

        #region Config_Control

        /**
         * Controls whether or not to allow the application to set an Administrated
         * ConnectionFactory to use for creating a Connection for a channel.
         *
         * Default = true.
         */
        public const string AllowAdministratedConnFactory = "Controls whether or not to allow the application to set an Administrated ConnectionFactory to use for creating a Connection for a channel.\n\nDefault = true.";

        /**
         * Controls whether or not to allow the application to set an Administrated
         * Destination to use for the Destination of a channel.
         *
         * Default = true.
         */
        public const string AllowAdministratedEndpointDest = "Controls whether or not to allow the application to set an Administrated Destination to use for the Destination of a channel.\n\nDefault = true.";

        /**
         * Controls whether or not to allow the application to set an Administrated
         * Destination to use for the Callback Destination of a channel.
         *
         * Default = true.
         */
        public const string AllowAdministratedCallbackDest = "Controls whether or not to allow the application to set an Administrated Destination to use for the Callback Destination of a channel.\n\nDefault = true.";

        /**
         * Determines if TemsTransportExtensionElement binding properties can be changed programatically after app.config values have been applied.
         * 
         * Default = true.
         */
        public const string AllowBindingChanges = "Determines if TemsTransportExtensionElement binding properties can be changed programatically after app.config values have been applied.\n\nDefault = true.";

        /**
         * Controls whether or not to allow the application to set a CustomMessageProtocol.
         * 
         * Default = true.
         */
        public const string AllowCustomMessageProtocol = "Controls whether or not to allow the application to set a CustomMessageProtocol.\n\nDefault = true.";
        
        #endregion
    }

    static class TemsConfigurationDefaults
    {
        #region Base_Properties

        // Default values below are from TransportElement properties.
        internal const long MaxBufferPoolSize = 512 * 1024;
        internal const int MaxReceivedMessageSize = 64 * 1024;

        #endregion

        #region EMS_Session_Properties

        //internal const bool SessionTransacted = false;
        internal const SessionMode SessionAcknowledgeMode = SessionMode.AutoAcknowledge;

        #endregion

        #region EMS_MessageProducer_Properties

        internal const MessageDeliveryMode MessageDeliveryMode = TIBCO.EMS.MessageDeliveryMode.Persistent;
        internal const bool DisableMessageID = false;
        internal const bool DisableMessageTimestamp = false;
        internal const int Priority = 4;
        internal const long TimeToLive = 0;

        #endregion

        #region MessageProtocol

        internal const TemsMessageProtocolType MessageProtocol = TemsMessageProtocolType.WCFNative;
        internal const TemsMessageType MessageType = TemsMessageType.Text;
        internal const bool ThrowOnInvalidUTF = true;

        #endregion

        #region WsdlExportExtension
        
        internal const bool WsdlExtensionActive = true;
        internal const bool WsdlTypeSchemaImport = false;

        #endregion

        internal const string CustomMessageProtocolType = "";
        internal const string WsdlExportExtensionType = "";

        #region Session_Properties

        #endregion

        internal const bool MessageCompression = false;
        
        #region Client_Properties

        /** 
         * ClientBaseAddress specifies the EMS destination for Duplex MEP callback messages.
         * 
         * Default = "".
         */
        internal const string ClientBaseAddress = "";

        /** 
         * AppHandlesMessageAcknowledge controls whether the application or TEMS handles receive message acknowledge.
         * If set to true, the Application will handle EMS receive message acknowledgement.
         * 
         * Default = false.
         */
        public const bool AppHandlesMessageAcknowledge = false;

        /** 
        * AppManagesConnections controls whether the application or TEMS manages EMS connections 
        * (creating and closing connections).  This allows the application to share or pool EMS connections.
        * If set to true, the Application will manage EMS connections.
        * 
        * Default = false.
        */
        public const bool AppManagesConnections = false;

        /** 
         * MessageSelector is a string that lets a client program specify a set of messages,
         * based on the values of message headers and properties. A selector matches a message if,
         * after substituting header and property values from the message into the selector string,
         * the string evaluates to true. Consumers are delivered only those messages that match a selector.
         * 
         * Default = "".
         */
        public const string MessageSelector = "";

        internal const string ReplyDestCheckInterval = "00:01:00";

        #endregion

        #region ConnectionFactory_Properties

        internal const string ClientID = "";
        internal const int ConnAttemptCount = -1;
        internal const int ConnAttemptDelay = -1;
        internal const int ConnAttemptTimeout = -1;
        internal const FactoryLoadBalanceMetric LoadBalanceMetric = FactoryLoadBalanceMetric.Connections;
        internal const int ReconnAttemptCount = -1;
        internal const int ReconnAttemptDelay = -1;
        internal const int ReconnAttemptTimeout = -1;
        internal const string ServerUrl = "";
        internal const EMSSSLStoreType CertificateStoreType = EMSSSLStoreType.EMSSSL_STORE_TYPE_DEFAULT;
        internal const StoreLocation CertificateStoreLocation = StoreLocation.LocalMachine;
        internal const string CertificateStoreName = "";
        internal const string CertificateNameAsFullSubjectDN = "";
        internal const string SSLClientIdentity = "";
        internal const string SSLPassword = "";
        internal const bool SSLAuthOnly = false;
        internal const string SSLProxyHost = "";
        internal const int SSLProxyPort = -1;
        internal const string SSLProxyAuthUsername = "";
        internal const string SSLProxyAuthPassword = "";
        internal const bool SSLTrace = false;
        internal const string SSLTargetHostName = "";
        internal const string Username = "";
        internal const string Password = "";

        #endregion

        #region Config_Control

        internal const bool AllowAdministratedConnFactory = true;
        internal const bool AllowAdministratedEndpointDest = true;
        internal const bool AllowAdministratedCallbackDest = true;
        internal const bool AllowBindingChanges = true;

        internal const bool AllowCustomMessageProtocol = true;

        #endregion
    }
}
