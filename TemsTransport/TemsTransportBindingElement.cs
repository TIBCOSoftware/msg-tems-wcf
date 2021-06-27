/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Security.Cryptography.X509Certificates;

using TIBCO.EMS;

namespace com.tibco.wcf.tems
{
    public class TemsTransportBindingElement : TransportBindingElement, IWsdlExportExtension
    {
        private TIBCO.EMS.SessionMode sessionAcknowledgeMode;
        private MessageDeliveryMode messageDeliveryMode;
        private bool disableMessageID;
        private bool disableMessageTimestamp;
        private int priority;
        private long timeToLive;

        private TemsMessageProtocolType messageProtocol;
        private TemsMessageType messageType;
        private bool throwOnInvalidUTF;

        private bool wsdlExtensionActive;
        private bool wsdlTypeSchemaImport;

        private string customMessageProtocolType;
        private string wsdlExportExtensionType; 

        private bool messageCompression;
        private string clientBaseAddress;
        private bool appHandlesMessageAcknowledge;
        private bool appManagesConnections;
        private string messageSelector;

        private TimeSpan replyDestCheckInterval;

        private string clientID;
        private int connAttemptCount;
        private int connAttemptDelay;
        private int connAttemptTimeout;
        private FactoryLoadBalanceMetric loadBalanceMetric;
        private int reconnAttemptCount;
        private int reconnAttemptDelay;
        private int reconnAttemptTimeout;
        private string serverUrl;

        private EMSSSLStoreType certificateStoreType;
        private StoreLocation certificateStoreLocation;
        private string certificateStoreName;
        private string certificateNameAsFullSubjectDN;
        private string sslClientIdentity;
        private string sslPassword;
        private bool sslAuthOnly;
        private string sslProxyHost;
        private int sslProxyPort;
        private string sslProxyAuthUsername;
        private string sslProxyAuthPassword;
        private bool sslTrace;
        private string sslTargetHostName;
        private string username;
        private string password;

        private bool allowAdministratedConnFactory;
        private bool allowAdministratedEndpointDest;
        private bool allowAdministratedCallbackDest;
        private bool allowBindingChanges;
        private bool allowCustomMessageProtocol;

        private Object certificateStoreTypeInfo;
        private StreamWriter clientTracer;
        private EMSSSLHostNameVerifier hostNameVerifier;

        private Hashtable contextJNDI;
        private ConnectionFactory connectionFactory;
        private Destination endpointDestination;
        private Destination callbackDestination;

        private Hashtable setProperties;
        private bool isApplyConfigurationComplete;
        private bool isClone;
        private bool isCloned;

        private Guid id;

        internal Connection connection;  

        private TemsWsdlExportExtension wsdlExportExtension;
        private ITemsPassword temsPasswordImpl;

        public TemsTransportBindingElement()
        {
            setProperties = new Hashtable();
            isApplyConfigurationComplete = false;
            isClone = false;
            isCloned = false;
            id = Guid.NewGuid();
            temsPasswordImpl = null;
        }

        public TemsTransportBindingElement(TemsTransportBindingElement original) : base(original)
        {
            sessionAcknowledgeMode = original.sessionAcknowledgeMode;
            messageDeliveryMode = original.messageDeliveryMode;
            disableMessageID = original.disableMessageID;
            disableMessageTimestamp = original.disableMessageTimestamp;
            priority = original.priority;
            timeToLive = original.timeToLive;

            messageProtocol = original.messageProtocol;
            messageType = original.messageType;
            throwOnInvalidUTF = original.throwOnInvalidUTF;

            wsdlExtensionActive = original.wsdlExtensionActive;
            wsdlTypeSchemaImport = original.wsdlTypeSchemaImport;

            customMessageProtocolType = original.customMessageProtocolType;
            wsdlExportExtensionType = original.wsdlExportExtensionType; 

            messageCompression = original.messageCompression;
            clientBaseAddress = original.clientBaseAddress;
            appHandlesMessageAcknowledge = original.appHandlesMessageAcknowledge;
            appManagesConnections = original.appManagesConnections;
            messageSelector = original.messageSelector;

            replyDestCheckInterval = original.replyDestCheckInterval;

            clientID = original.clientID;
            connAttemptCount = original.connAttemptCount;
            connAttemptDelay = original.connAttemptDelay;
            connAttemptTimeout = original.connAttemptTimeout;
            loadBalanceMetric = original.loadBalanceMetric;
            reconnAttemptCount = original.reconnAttemptCount;
            reconnAttemptDelay = original.reconnAttemptDelay;
            reconnAttemptTimeout = original.reconnAttemptTimeout;
            serverUrl = original.serverUrl;

            certificateStoreType = original.certificateStoreType;
            certificateStoreLocation = original.certificateStoreLocation;
            certificateStoreName = original.certificateStoreName;
            certificateNameAsFullSubjectDN = original.certificateNameAsFullSubjectDN;
            sslClientIdentity = original.sslClientIdentity;
            sslPassword = original.sslPassword;

            sslAuthOnly = original.sslAuthOnly;
            sslProxyHost = original.sslProxyHost;
            sslProxyPort = original.sslProxyPort;
            sslProxyAuthUsername = original.sslProxyAuthUsername;
            sslProxyAuthPassword = original.sslProxyAuthPassword;
            sslTrace = original.sslTrace;
            sslTargetHostName = original.sslTargetHostName;
            username = original.username;
            password = original.password;

            allowAdministratedConnFactory = original.allowAdministratedConnFactory;
            allowAdministratedEndpointDest = original.allowAdministratedEndpointDest;
            allowAdministratedCallbackDest = original.allowAdministratedCallbackDest;
            allowBindingChanges = original.allowBindingChanges;

            allowCustomMessageProtocol = original.allowCustomMessageProtocol;

            certificateStoreTypeInfo = original.certificateStoreTypeInfo;
            clientTracer = original.clientTracer;
            hostNameVerifier = original.hostNameVerifier;

            contextJNDI = original.contextJNDI;
            connectionFactory = original.connectionFactory;

            if (original.appManagesConnections)
                connection = original.connection;

            endpointDestination = original.endpointDestination;
            callbackDestination = original.callbackDestination;

            setProperties = original.setProperties;
            isApplyConfigurationComplete = original.isApplyConfigurationComplete;
            id = original.id;
            temsPasswordImpl = original.temsPasswordImpl;
            isClone = true;
        }

        internal bool IsApplyConfigurationComplete
        {
            get { return isApplyConfigurationComplete; }
        }

        internal void SetApplyConfigurationComplete()
        {
            isApplyConfigurationComplete = true;
        }

        public TIBCO.EMS.SessionMode SessionAcknowledgeMode
        {
            get { return sessionAcknowledgeMode; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.SessionAcknowledgeMode))
                {
                    SetPropertiesAdd("SessionAcknowledgeMode");
                }

                sessionAcknowledgeMode = value;
            }
        }

        public MessageDeliveryMode MessageDeliveryMode
        {
            get { return messageDeliveryMode; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.MessageDeliveryMode))
                {
                    SetPropertiesAdd("MessageDeliveryMode");
                }

                messageDeliveryMode = value;
            }
        }

        public bool DisableMessageID
        {
            get { return disableMessageID; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.DisableMessageID))
                {
                    SetPropertiesAdd("DisableMessageID");
                }

                disableMessageID = value;
            }
        }

        public bool DisableMessageTimestamp
        {
            get { return disableMessageTimestamp; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.DisableMessageTimestamp))
                {
                    SetPropertiesAdd("DisableMessageTimestamp");
                }

                disableMessageTimestamp = value;
            }
        }

        public int Priority
        {
            get { return priority; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.Priority))
                {
                    SetPropertiesAdd("Priority");
                }

                priority = value;
            }
        }

        public long TimeToLive
        {
            get { return timeToLive; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.TimeToLive))
                {
                    SetPropertiesAdd("TimeToLive");
                }

                timeToLive = value;
            }
        }

        public TemsMessageProtocolType MessageProtocol
        {
            get { return messageProtocol; }
            set
            {
                if (isApplyConfigurationComplete && allowCustomMessageProtocol == false && value == TemsMessageProtocolType.Custom)
                {
                    throw new Exception("The TemsTransportBindingElement MessageProtocol property cannot be set to Custom because AllowCustomMessageProtocol is false.");
                }

                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.MessageProtocol))
                {
                    SetPropertiesAdd("MessageProtocol");
                }

                messageProtocol = value;
            }
        }

        public TemsMessageType MessageType
        {
            get { return messageType; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.MessageType))
                {
                    SetPropertiesAdd("MessageType");
                }

                messageType = value;
            }
        }

        public bool ThrowOnInvalidUTF
        {
            get { return throwOnInvalidUTF; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.ThrowOnInvalidUTF))
                {
                    SetPropertiesAdd("ThrowOnInvalidUTF");
                }

                throwOnInvalidUTF = value;
            }
        }

        public bool WsdlExtensionActive
        {
            get { return wsdlExtensionActive; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.WsdlExtensionActive))
                {
                    SetPropertiesAdd("WsdlExtensionActive");
                }

                wsdlExtensionActive = value;
            }
        }

        public bool WsdlTypeSchemaImport
        {
            get { return wsdlTypeSchemaImport; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.WsdlTypeSchemaImport))
                {
                    SetPropertiesAdd("WsdlTypeSchemaImport");
                }

                wsdlTypeSchemaImport = value;
            }
        }

        public string CustomMessageProtocolType
        {
            get { return customMessageProtocolType; }
            set
            {
                if (isApplyConfigurationComplete && !allowCustomMessageProtocol)
                {
                    throw new Exception("The TemsTransportBindingElement CustomMessageProtocolType property cannot be set because AllowCustomMessageProtocol=\"false\".");
                }

                if (isApplyConfigurationComplete && value != TemsConfigurationDefaults.CustomMessageProtocolType && MessageProtocol != TemsMessageProtocolType.Custom)
                {
                    throw new Exception("The TemsTransportBindingElement CustomMessageProtocolType property cannot be set because MessageProtocolType is not set to \"Custom\".");
                }

                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.CustomMessageProtocolType))
                {
                    SetPropertiesAdd("CustomMessageProtocolType");
                }

                customMessageProtocolType = value;
            }
        }

        public string WsdlExportExtensionType
        {
            get { return wsdlExportExtensionType; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.WsdlExportExtensionType))
                {
                    SetPropertiesAdd("WsdlExportExtensionType");
                }

                wsdlExportExtensionType = value;
            }
        } 

        public bool MessageCompression
        {
            get { return messageCompression; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.MessageCompression))
                {
                    SetPropertiesAdd("MessageCompression");
                }

                messageCompression = value;
            }
        }

        public string ClientBaseAddress
        {
            get { return clientBaseAddress; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.ClientBaseAddress)))
                {
                    SetPropertiesAdd("ClientBaseAddress");
                }

                clientBaseAddress = value;
            }
        }

        public bool AppHandlesMessageAcknowledge
        {
            get { return appHandlesMessageAcknowledge; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.AppHandlesMessageAcknowledge))
                {
                    SetPropertiesAdd("AppHandlesMessageAcknowledge");
                }

                appHandlesMessageAcknowledge = value;
            }
        }

        public bool AppManagesConnections
        {
            get { return appManagesConnections; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.AppManagesConnections))
                {
                    SetPropertiesAdd("AppManagesConnections");
                }

                appManagesConnections = value;
            }
        }

        public string MessageSelector
        {
            get { return messageSelector; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.MessageSelector)))
                {
                    SetPropertiesAdd("MessageSelector");
                }

                messageSelector = value;
            }
        }

        public TimeSpan ReplyDestCheckInterval
        {
            get { return replyDestCheckInterval; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.ReplyDestCheckInterval)))
                {
                    SetPropertiesAdd("ReplyDestCheckInterval");
                }

                replyDestCheckInterval = value;
            }
        }

        public string ClientID
        {
            get { return clientID; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.ClientID)))
                {
                    SetPropertiesAdd("ClientID");
                }

                clientID = value;
            }
        }
        public int ConnAttemptCount
        {
            get { return connAttemptCount; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.ConnAttemptCount))
                {
                    SetPropertiesAdd("ConnAttemptCount");
                }

                connAttemptCount = value;
            }
        }
        public int ConnAttemptDelay
        {
            get { return connAttemptDelay; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.ConnAttemptDelay))
                {
                    SetPropertiesAdd("ConnAttemptDelay");
                }

                connAttemptDelay = value;
            }
        }
        public int ConnAttemptTimeout
        {
            get { return connAttemptTimeout; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.ConnAttemptTimeout))
                {
                    SetPropertiesAdd("ConnAttemptTimeout");
                }

                connAttemptTimeout = value;
            }
        }
        public FactoryLoadBalanceMetric LoadBalanceMetric
        {
            get { return loadBalanceMetric; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.LoadBalanceMetric))
                {
                    SetPropertiesAdd("LoadBalanceMetric");
                }

                loadBalanceMetric = value;
            }
        }
        public int ReconnAttemptCount
        {
            get { return reconnAttemptCount; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.ReconnAttemptCount))
                {
                    SetPropertiesAdd("ReconnAttemptCount");
                }

                reconnAttemptCount = value;
            }
        }
        public int ReconnAttemptDelay
        {
            get { return reconnAttemptDelay; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.ReconnAttemptDelay))
                {
                    SetPropertiesAdd("ReconnAttemptDelay");
                }

                reconnAttemptDelay = value;
            }
        }
        public int ReconnAttemptTimeout
        {
            get { return reconnAttemptTimeout; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.ReconnAttemptTimeout))
                {
                    SetPropertiesAdd("ReconnAttemptTimeout");
                }

                reconnAttemptTimeout = value;
            }
        }
        public string ServerUrl
        {
            get { return serverUrl; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.ServerUrl)))
                {
                    SetPropertiesAdd("ServerUrl");
                }

                serverUrl = value;
            }
        }

        public EMSSSLStoreType CertificateStoreType
        {
            get { return certificateStoreType; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.CertificateStoreType)))
                {
                    SetPropertiesAdd("CertificateStoreType", true);
                }

                certificateStoreType = value;
            }
        }
        public StoreLocation CertificateStoreLocation
        {
            get { return certificateStoreLocation; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.CertificateStoreLocation)))
                {
                    SetPropertiesAdd("CertificateStoreLocation", true);
                }

                certificateStoreLocation = value;
            }
        }
        public string CertificateStoreName
        {
            get { return certificateStoreName; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.CertificateStoreName)))
                {
                    SetPropertiesAdd("CertificateStoreName");
                }

                certificateStoreName = value;
            }
        }
        public string CertificateNameAsFullSubjectDN
        {
            get { return certificateNameAsFullSubjectDN; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.CertificateNameAsFullSubjectDN)))
                {
                    SetPropertiesAdd("CertificateNameAsFullSubjectDN");
                }

                certificateNameAsFullSubjectDN = value;
            }
        }
        public string SSLClientIdentity
        {
            get { return sslClientIdentity; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.SSLClientIdentity)))
                {
                    SetPropertiesAdd("SSLClientIdentity");
                }

                sslClientIdentity = value;
            }
        }
        public string SSLPassword
        {
            get { return sslPassword; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.SSLPassword)))
                {
                    SetPropertiesAdd("SSLPassword");
                }

                sslPassword = value;
            }
        }

        public bool SSLAuthOnly
        {
            get { return sslAuthOnly; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.SSLAuthOnly))
                {
                    SetPropertiesAdd("SSLAuthOnly");
                }

                sslAuthOnly = value;
            }
        }
        public string SSLProxyHost
        {
            get { return sslProxyHost; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.SSLProxyHost)))
                {
                    SetPropertiesAdd("SSLProxyHost");
                }

                sslProxyHost = value;
            }
        }
        public int SSLProxyPort
        {
            get { return sslProxyPort; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.SSLProxyPort))
                {
                    SetPropertiesAdd("SSLProxyPort");
                }

                sslProxyPort = value;
            }
        }
        public string SSLProxyAuthUsername
        {
            get { return sslProxyAuthUsername; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.SSLProxyAuthUsername)))
                {
                    SetPropertiesAdd("SSLProxyAuthUsername");
                }

                sslProxyAuthUsername = value;
            }
        }
        public string SSLProxyAuthPassword
        {
            get { return sslProxyAuthPassword; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.SSLProxyAuthPassword)))
                {
                    SetPropertiesAdd("SSLProxyAuthPassword");
                }

                sslProxyAuthPassword = value;
            }
        }
        public bool SSLTrace
        {
            get { return sslTrace; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.SSLTrace))
                {
                    SetPropertiesAdd("SSLTrace");
                }

                sslTrace = value;
            }
        }
        public string SSLTargetHostName
        {
            get { return sslTargetHostName; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.SSLTargetHostName)))
                {
                    SetPropertiesAdd("SSLTargetHostName");
                }

                sslTargetHostName = value;
            }
        }
        public string Username
        {
            get { return username; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && !value.Equals(TemsConfigurationDefaults.Username)))
                {
                    SetPropertiesAdd("Username");
                }

                username = value;
            }
        }
        public string Password
        {
            get { return password; }
            set
            {
                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && ! value.Equals(TemsConfigurationDefaults.Password)))
                {
                    SetPropertiesAdd("Password");
                }

                password = value;
            }
        }

        public bool AllowAdministratedConnFactory
        {
            get { return allowAdministratedConnFactory; }
            set
            {
                if (isApplyConfigurationComplete && allowAdministratedConnFactory == false && value == true)
                {
                    throw new Exception("The TemsTransportBindingElement AllowAdministratedConnFactory property cannot be changes from false to true.");
                }

                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.AllowAdministratedConnFactory))
                {
                    SetPropertiesAdd("AllowAdministratedConnFactory");
                }

                allowAdministratedConnFactory = value;
            }
        }
        public bool AllowAdministratedEndpointDest
        {
            get { return allowAdministratedEndpointDest; }
            set
            {
                if (isApplyConfigurationComplete && allowAdministratedEndpointDest == false && value == true)
                {
                    throw new Exception("The TemsTransportBindingElement AllowAdministratedEndpointDest property cannot be changes from false to true.");
                }

                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.AllowAdministratedEndpointDest))
                {
                    SetPropertiesAdd("AllowAdministratedEndpointDest");
                }

                allowAdministratedEndpointDest = value;
            }
        }
        public bool AllowAdministratedCallbackDest
        {
            get { return allowAdministratedCallbackDest; }
            set
            {
                if (isApplyConfigurationComplete && allowAdministratedCallbackDest == false && value == true)
                {
                    throw new Exception("The TemsTransportBindingElement AllowAdministratedCallbackDest property cannot be changes from false to true.");
                }

                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.AllowAdministratedCallbackDest))
                {
                    SetPropertiesAdd("AllowAdministratedCallbackDest");
                }

                allowAdministratedCallbackDest = value;
            }
        }
        public bool AllowBindingChanges
        {
            get { return allowBindingChanges; }
            set
            {
                if (isApplyConfigurationComplete && allowBindingChanges == false && value == true)
                {
                    throw new Exception("The TemsTransportBindingElement AllowBindingChanges property cannot be changes from false to true.");
                }

                if (isApplyConfigurationComplete || 
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.AllowBindingChanges))
                {
                    SetPropertiesAdd("AllowBindingChanges");
                }

                allowBindingChanges = value;
            }
        }
        public bool AllowCustomMessageProtocol
        {
            get { return allowCustomMessageProtocol; }
            set
            {
                if (isApplyConfigurationComplete && allowCustomMessageProtocol == false && value == true)
                {
                    throw new Exception("The TemsTransportBindingElement AllowCustomMessageProtocol property cannot be changes from false to true.");
                }

                if (isApplyConfigurationComplete ||
                    (!isApplyConfigurationComplete && value != TemsConfigurationDefaults.AllowCustomMessageProtocol))
                {
                    SetPropertiesAdd("AllowCustomMessageProtocol");
                }

                allowCustomMessageProtocol = value;
            }
        }


        public Object CertificateStoreTypeInfo
        {
            get { return certificateStoreTypeInfo; }
            set
            {
                SetPropertiesAdd("CertificateStoreTypeInfo", true);
                certificateStoreTypeInfo = value;
            }
        }
        public StreamWriter ClientTracer
        {
            get { return clientTracer; }
            set
            {
                SetPropertiesAdd("ClientTracer", true);
                clientTracer = value;
            }
        }
        public EMSSSLHostNameVerifier HostNameVerifier
        {
            get { return hostNameVerifier; }
            set
            {
                SetPropertiesAdd("HostNameVerifier", true);
                hostNameVerifier = value;
            }
        }

        public Hashtable ContextJNDI
        {
            get { return contextJNDI; }
            set
            {
                SetPropertiesAdd("ContextJNDI", true);
                contextJNDI = value;
            }
        }
        public ConnectionFactory ConnectionFactory
        {
            get { return connectionFactory; }
            set
            {
                if (!allowAdministratedConnFactory)
                {
                    throw new Exception("The TemsTransportBindingElement ConnectionFactory property cannot be set because AllowAdministratedConnFactory=\"false\".");
                }

                SetPropertiesAdd("ConnectionFactory", true);
                connectionFactory = value;
            }
        }
        public Destination EndpointDestination
        {
            get { return endpointDestination; }
            set
            {
                if (!allowAdministratedEndpointDest)
                {
                    throw new Exception("The TemsTransportBindingElement EndpointDestination property cannot be set because AllowAdministratedEndpointDest=\"false\".");
                }

                SetPropertiesAdd("EndpointDestination", true);
                endpointDestination = value;
            }
        }
        public Destination CallbackDestination
        {
            get { return callbackDestination; }
            set
            {
                if (!allowAdministratedCallbackDest)
                {
                    throw new Exception("The TemsTransportBindingElement CallbackDestination property cannot be set because AllowAdministratedCallbackDest=\"false\".");
                }

                SetPropertiesAdd("CallbackDestination", true);
                callbackDestination = value;
            }
        }
        private TemsWsdlExportExtension WsdlExportExtension
        {
            get
            {
                if (wsdlExportExtension == null)
                {
                    wsdlExportExtension = new TemsWsdlExportExtension(this);
                }

                return wsdlExportExtension;
            }
        }

        public bool IsPropertySet(string property)
        {
            return setProperties.ContainsKey(property);
        }

        private void SetPropertiesAdd(string property)
        {
            SetPropertiesAdd(property, false);
        }

        private void SetPropertiesAdd(string property, bool overrideAllowBindingChanges)
        {
            if (isClone || isCloned)
            {
                throw new Exception("A cloned TemsTransportBindingElement cannot be modified.");
            }

            if (isApplyConfigurationComplete && !allowBindingChanges && !overrideAllowBindingChanges)
            {
                throw new Exception("The TemsTransportBindingElement properties cannot be changed because allowBindingChanges=\"false\".");
            }

            if (!setProperties.ContainsKey(property))
            {
                setProperties.Add(property, null);
            }
        }

        internal void InitializeConnectionFactory(ConnectionFactory factory)
        {
            if (setProperties.Contains("CertificateStoreTypeInfo"))
            {   
                factory.SetCertificateStoreType(certificateStoreType, certificateStoreTypeInfo);
            }
            else
            {
                if (CertificateStoreType == EMSSSLStoreType.EMSSSL_STORE_TYPE_FILE)
                {
                    if (setProperties.Contains("SSLClientIdentity"))
                    {
                        EMSSSLFileStoreInfo storeInfo = new EMSSSLFileStoreInfo();
                        storeInfo.SetSSLClientIdentity(sslClientIdentity);
                        string tmp = sslPassword;

                        if (temsPasswordImpl != null)
                            tmp = temsPasswordImpl.ManageSSL(sslPassword);

                        storeInfo.SetSSLPassword(tmp.ToCharArray());
                        factory.SetCertificateStoreType(EMSSSLStoreType.EMSSSL_STORE_TYPE_FILE, storeInfo);
                        certificateStoreTypeInfo = storeInfo;
                    }
                }
                else
                {
                    if (setProperties.Contains("CertificateNameAsFullSubjectDN"))
                    {
                        EMSSSLSystemStoreInfo storeInfo = new EMSSSLSystemStoreInfo();
                        storeInfo.SetCertificateNameAsFullSubjectDN(certificateNameAsFullSubjectDN);
                        storeInfo.SetCertificateStoreLocation(certificateStoreLocation);

                        if (setProperties.Contains("CertificateStoreName"))
                        {
                            storeInfo.SetCertificateStoreName(certificateStoreName);
                        }

                        factory.SetCertificateStoreType(EMSSSLStoreType.EMSSSL_STORE_TYPE_SYSTEM, storeInfo);
                        certificateStoreTypeInfo = storeInfo;
                    }
                }
            }
            
            if (setProperties.Contains("ClientID"))
                factory.SetClientID(clientID);

            if (setProperties.Contains("ClientTracer"))
                factory.SetClientTracer(clientTracer);

            if (setProperties.Contains("ConnAttemptCount"))
                factory.SetConnAttemptCount(connAttemptCount);

            if (setProperties.Contains("ConnAttemptDelay"))
                factory.SetConnAttemptDelay(connAttemptDelay);

            if (setProperties.Contains("ConnAttemptTimeout"))
                factory.SetConnAttemptTimeout(connAttemptTimeout);

            if (setProperties.Contains("HostNameVerifier"))
                factory.SetHostNameVerifier(hostNameVerifier);

            if (setProperties.Contains("LoadBalanceMetric"))
                factory.SetMetric((int)loadBalanceMetric);

            if (setProperties.Contains("ReconnAttemptCount"))
                factory.SetReconnAttemptCount(reconnAttemptCount);

            if (setProperties.Contains("ReconnAttemptDelay"))
                factory.SetReconnAttemptDelay(reconnAttemptDelay);

            if (setProperties.Contains("ReconnAttemptTimeout"))
                factory.SetReconnAttemptTimeout(reconnAttemptTimeout);

            if (setProperties.Contains("ServerUrl"))
                factory.SetServerUrl(serverUrl);

            if (setProperties.Contains("SSLAuthOnly"))
                factory.SetSSLAuthOnly(sslAuthOnly);

            if (setProperties.Contains("SSLProxyHost") && setProperties.Contains("SSLProxyPort"))
                factory.SetSSLProxy(sslProxyHost, sslProxyPort);

            if (setProperties.Contains("SSLProxyAuthUsername") && setProperties.Contains("SSLProxyAuthPassword"))
            {
                string tmp = sslProxyAuthPassword;

                if (temsPasswordImpl != null)
                    tmp = temsPasswordImpl.ManageSSLProxyAuth(sslProxyAuthPassword);

                factory.SetSSLProxyAuth(sslProxyAuthUsername, tmp);
            }

            if (setProperties.Contains("SSLTrace"))
                factory.SetSSLTrace(sslTrace);

            if (setProperties.Contains("SSLTargetHostName"))
                factory.SetTargetHostName(sslTargetHostName);

            if (setProperties.Contains("Username"))
                factory.SetUserName(username);

            if (setProperties.Contains("Password"))
            {
                string tmp = password;

                if (temsPasswordImpl != null)
                    tmp = temsPasswordImpl.Manage(password);

                factory.SetUserPassword(tmp);
            }
        }

        public ITemsPassword TemsPasswordImpl
        {
            get {return temsPasswordImpl; }
            set { temsPasswordImpl = value; }
        }

        public override string Scheme
        {
            get { return "net.tems"; }
        }

        public Guid Id
        {
            get { return id; }
        }

        public Connection Connection
        { 
            get
            {
                if (!appManagesConnections)
                    throw new Exception("AppManagesConnections is false so cannot get Connection");

                return connection; 
            }
            set
            {
                if (!appManagesConnections)
                    throw new Exception("AppManagesConnections is false so cannot set Connection");

                connection = value; 
            }
        }

       
        public override BindingElement Clone()
        {
            isCloned = true;
            
            return new TemsTransportBindingElement(this);
        }
        
        public override T GetProperty<T>(BindingContext context)
        {           
            if (context == null)
                throw new ArgumentNullException("context");

            return context.GetInnerProperty<T>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            Boolean retval = false;
            Type type = typeof(TChannel);

            if (type == typeof(IOutputChannel) ||
                type == typeof(IOutputSessionChannel) ||
                type == typeof(IRequestChannel) ||
                type == typeof(IRequestSessionChannel) ||
                type == typeof(IDuplexChannel) ||
                type == typeof(IDuplexSessionChannel))
            {
                retval = true;
            }

            return retval;
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            Boolean retval = false;
            Type type = typeof(TChannel);

            if (type == typeof(IInputChannel) ||
                type == typeof(IInputSessionChannel) ||
                type == typeof(IReplyChannel) ||
                type == typeof(IReplySessionChannel) ||
                type == typeof(IDuplexChannel) ||
                type == typeof(IDuplexSessionChannel))
            {
                retval = true;
            }

            return retval;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            return (IChannelFactory<TChannel>)(object)new TemsChannelFactory<TChannel>(this, context);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            return (IChannelListener<TChannel>)(object)new TemsChannelListener<TChannel>(this, context);
        }

        #region IWsdlExportExtension Members

        public void ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
            if (WsdlExtensionActive)
            {
                WsdlExportExtension.ExportContract(exporter, context);
            }
        }

        public void ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            if (WsdlExtensionActive)
            {
                WsdlExportExtension.ExportEndpoint(exporter, context);
            }
        }
        #endregion
    }
}
