/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Security.Cryptography.X509Certificates;
using TIBCO.EMS;

namespace com.tibco.wcf.tems
{
    public class TemsTransportExtensionElement : BindingElementExtensionElement
    {
        public override Type BindingElementType
        {
            get
            {
                return typeof(TemsTransportBindingElement);
            }
        }

        protected override BindingElement CreateBindingElement()
        {
            TemsTransportBindingElement bindingElement = new TemsTransportBindingElement();
            ApplyConfiguration(bindingElement);

            return bindingElement;
        }

        public BindingElement CreateDefaultBindingElement()
        {
            return this.CreateBindingElement();
        }

        #region Configuration_Properties

        #region Base_Property_Accessors

        [ConfigurationProperty(TemsConfigurationStrings.MaxBufferPoolSize, DefaultValue = TemsConfigurationDefaults.MaxBufferPoolSize)]
        [LongValidator(MinValue = 0)]
        public long MaxBufferPoolSize
        {
            get { return (long)base[TemsConfigurationStrings.MaxBufferPoolSize]; }
            set { base[TemsConfigurationStrings.MaxBufferPoolSize] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.MaxReceivedMessageSize, DefaultValue = TemsConfigurationDefaults.MaxReceivedMessageSize)]
        [IntegerValidator(MinValue = 1)]
        public int MaxReceivedMessageSize
        {
            get { return (int)base[TemsConfigurationStrings.MaxReceivedMessageSize]; }
            set { base[TemsConfigurationStrings.MaxReceivedMessageSize] = value; }
        } 
        
        #endregion

        #region EMS_Session_Property_Accessors

        [ConfigurationProperty(TemsConfigurationStrings.SessionAcknowledgeMode, DefaultValue = TemsConfigurationDefaults.SessionAcknowledgeMode)]
        public TIBCO.EMS.SessionMode SessionAcknowledgeMode
        {
            get { return (TIBCO.EMS.SessionMode)base[TemsConfigurationStrings.SessionAcknowledgeMode]; }
            set { base[TemsConfigurationStrings.SessionAcknowledgeMode] = value; }
        }
 
        #endregion

        #region EMS_MessageProducer_Property_Accessors

        [ConfigurationProperty(TemsConfigurationStrings.MessageDeliveryMode, DefaultValue = TemsConfigurationDefaults.MessageDeliveryMode)]
        public MessageDeliveryMode MessageDeliveryMode
        {
            get { return (MessageDeliveryMode)base[TemsConfigurationStrings.MessageDeliveryMode]; }
            set { base[TemsConfigurationStrings.MessageDeliveryMode] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.DisableMessageID, DefaultValue = TemsConfigurationDefaults.DisableMessageID)]
        public bool DisableMessageID
        {
            get { return (bool)base[TemsConfigurationStrings.DisableMessageID]; }
            set { base[TemsConfigurationStrings.DisableMessageID] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.DisableMessageTimestamp, DefaultValue = TemsConfigurationDefaults.DisableMessageTimestamp)]
        public bool DisableMessageTimestamp
        {
            get { return (bool)base[TemsConfigurationStrings.DisableMessageTimestamp]; }
            set { base[TemsConfigurationStrings.DisableMessageTimestamp] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.Priority, DefaultValue = TemsConfigurationDefaults.Priority)]
        [IntegerValidator(MinValue = 0, MaxValue = 9)]
        public int Priority
        {
            get { return (int)base[TemsConfigurationStrings.Priority]; }
            set { base[TemsConfigurationStrings.Priority] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.TimeToLive, DefaultValue = TemsConfigurationDefaults.TimeToLive)]
        [LongValidator(MinValue = 0)]
        public long TimeToLive
        {
            get { return (long)base[TemsConfigurationStrings.TimeToLive]; }
            set { base[TemsConfigurationStrings.TimeToLive] = value; }
        } 
        
        #endregion

        #region MessageProtocol_Property_Accessors

        [ConfigurationProperty(TemsConfigurationStrings.MessageProtocol, DefaultValue = TemsConfigurationDefaults.MessageProtocol)]
        public TemsMessageProtocolType MessageProtocol
        {
            get { return (TemsMessageProtocolType)base[TemsConfigurationStrings.MessageProtocol]; }
            set { base[TemsConfigurationStrings.MessageProtocol] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.MessageType, DefaultValue = TemsConfigurationDefaults.MessageType)]
        public TemsMessageType MessageType
        {
            get { return (TemsMessageType)base[TemsConfigurationStrings.MessageType]; }
            set { base[TemsConfigurationStrings.MessageType] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.ThrowOnInvalidUTF, DefaultValue = TemsConfigurationDefaults.ThrowOnInvalidUTF)]
        public bool ThrowOnInvalidUTF
        {
            get { return (bool)base[TemsConfigurationStrings.ThrowOnInvalidUTF]; }
            set { base[TemsConfigurationStrings.ThrowOnInvalidUTF] = value; }
        }

        #endregion

        #region WsdlExportExtension

        [ConfigurationProperty(TemsConfigurationStrings.WsdlExtensionActive, DefaultValue = TemsConfigurationDefaults.WsdlExtensionActive)]
        public bool WsdlExtensionActive
        {
            get { return (bool)base[TemsConfigurationStrings.WsdlExtensionActive]; }
            set { base[TemsConfigurationStrings.WsdlExtensionActive] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.WsdlTypeSchemaImport, DefaultValue = TemsConfigurationDefaults.WsdlTypeSchemaImport)]
        public bool WsdlTypeSchemaImport
        {
            get { return (bool)base[TemsConfigurationStrings.WsdlTypeSchemaImport]; }
            set { base[TemsConfigurationStrings.WsdlTypeSchemaImport] = value; }
        }

        #endregion

        [ConfigurationProperty(TemsConfigurationStrings.CustomMessageProtocolType, DefaultValue = TemsConfigurationDefaults.CustomMessageProtocolType)]
        public string CustomMessageProtocolType
        {
            get { return (string)base[TemsConfigurationStrings.CustomMessageProtocolType]; }
            set { base[TemsConfigurationStrings.CustomMessageProtocolType] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.WsdlExportExtensionType, DefaultValue = TemsConfigurationDefaults.WsdlExportExtensionType)]
        public string WsdlExportExtensionType
        {
            get { return (string)base[TemsConfigurationStrings.WsdlExportExtensionType]; }
            set { base[TemsConfigurationStrings.WsdlExportExtensionType] = value; }
        }

        #region Session_Property_Accessors

        #endregion

        #region Client_Property_Accessors

        [ConfigurationProperty(TemsConfigurationStrings.MessageCompression, DefaultValue = TemsConfigurationDefaults.MessageCompression)]
        public bool MessageCompression
        {
            get { return (bool)base[TemsConfigurationStrings.MessageCompression]; }
            set { base[TemsConfigurationStrings.MessageCompression] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.ClientBaseAddress, DefaultValue = TemsConfigurationDefaults.ClientBaseAddress)]
        public string ClientBaseAddress
        {
            get { return (string)base[TemsConfigurationStrings.ClientBaseAddress]; }
            set { base[TemsConfigurationStrings.ClientBaseAddress] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.AppHandlesMessageAcknowledge, DefaultValue = TemsConfigurationDefaults.AppHandlesMessageAcknowledge)]
        public bool AppHandlesMessageAcknowledge
        {
            get { return (bool)base[TemsConfigurationStrings.AppHandlesMessageAcknowledge]; }
            set { base[TemsConfigurationStrings.AppHandlesMessageAcknowledge] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.AppManagesConnections, DefaultValue = TemsConfigurationDefaults.AppManagesConnections)]
        public bool AppManagesConnections
        {
            get { return (bool)base[TemsConfigurationStrings.AppManagesConnections]; }
            set { base[TemsConfigurationStrings.AppManagesConnections] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.MessageSelector, DefaultValue = TemsConfigurationDefaults.MessageSelector)]
        public string MessageSelector
        {
            get { return (string)base[TemsConfigurationStrings.MessageSelector]; }
            set { base[TemsConfigurationStrings.MessageSelector] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.ReplyDestCheckInterval, DefaultValue = TemsConfigurationDefaults.ReplyDestCheckInterval)]
        public TimeSpan ReplyDestCheckInterval
        {
            get { return (TimeSpan)base[TemsConfigurationStrings.ReplyDestCheckInterval]; }
            set { base[TemsConfigurationStrings.ReplyDestCheckInterval] = value; }
        }
 
        #endregion

        #region ConnectionFactory_Property_Accessors

        [ConfigurationProperty(TemsConfigurationStrings.ClientID, DefaultValue = TemsConfigurationDefaults.ClientID)]
        public string ClientID
        {
            get { return (string)base[TemsConfigurationStrings.ClientID]; }
            set { base[TemsConfigurationStrings.ClientID] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.ConnAttemptCount, DefaultValue = TemsConfigurationDefaults.ConnAttemptCount)]
        [IntegerValidator(MinValue = -1)]
        public int ConnAttemptCount
        {
            get { return (int)base[TemsConfigurationStrings.ConnAttemptCount]; }
            set { base[TemsConfigurationStrings.ConnAttemptCount] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.ConnAttemptDelay, DefaultValue = TemsConfigurationDefaults.ConnAttemptDelay)]
        [IntegerValidator(MinValue = -1)]
        public int ConnAttemptDelay
        {
            get { return (int)base[TemsConfigurationStrings.ConnAttemptDelay]; }
            set { base[TemsConfigurationStrings.ConnAttemptDelay] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.ConnAttemptTimeout, DefaultValue = TemsConfigurationDefaults.ConnAttemptTimeout)]
        [IntegerValidator(MinValue = -1)]
        public int ConnAttemptTimeout
        {
            get { return (int)base[TemsConfigurationStrings.ConnAttemptTimeout]; }
            set { base[TemsConfigurationStrings.ConnAttemptTimeout] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.LoadBalanceMetric, DefaultValue = TemsConfigurationDefaults.LoadBalanceMetric)]
        public FactoryLoadBalanceMetric LoadBalanceMetric
        {
            get { return (FactoryLoadBalanceMetric)base[TemsConfigurationStrings.LoadBalanceMetric]; }
            set { base[TemsConfigurationStrings.LoadBalanceMetric] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.ReconnAttemptCount, DefaultValue = TemsConfigurationDefaults.ReconnAttemptCount)]
        [IntegerValidator(MinValue = -1)]
        public int ReconnAttemptCount
        {
            get { return (int)base[TemsConfigurationStrings.ReconnAttemptCount]; }
            set { base[TemsConfigurationStrings.ReconnAttemptCount] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.ReconnAttemptDelay, DefaultValue = TemsConfigurationDefaults.ReconnAttemptDelay)]
        [IntegerValidator(MinValue = -1)]
        public int ReconnAttemptDelay
        {
            get { return (int)base[TemsConfigurationStrings.ReconnAttemptDelay]; }
            set { base[TemsConfigurationStrings.ReconnAttemptDelay] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.ReconnAttemptTimeout, DefaultValue = TemsConfigurationDefaults.ReconnAttemptTimeout)]
        [IntegerValidator(MinValue = -1)]
        public int ReconnAttemptTimeout
        {
            get { return (int)base[TemsConfigurationStrings.ReconnAttemptTimeout]; }
            set { base[TemsConfigurationStrings.ReconnAttemptTimeout] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.ServerUrl, DefaultValue = TemsConfigurationDefaults.ServerUrl)]
        public string ServerUrl
        {
            get { return (string)base[TemsConfigurationStrings.ServerUrl]; }
            set { base[TemsConfigurationStrings.ServerUrl] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.CertificateStoreType, DefaultValue = TemsConfigurationDefaults.CertificateStoreType)]
        public EMSSSLStoreType CertificateStoreType
        {
            get { return (EMSSSLStoreType)base[TemsConfigurationStrings.CertificateStoreType]; }
            set { base[TemsConfigurationStrings.CertificateStoreType] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.CertificateStoreLocation, DefaultValue = TemsConfigurationDefaults.CertificateStoreLocation)]
        public StoreLocation CertificateStoreLocation
        {
            get { return (StoreLocation)base[TemsConfigurationStrings.CertificateStoreLocation]; }
            set { base[TemsConfigurationStrings.CertificateStoreLocation] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.CertificateStoreName, DefaultValue = TemsConfigurationDefaults.CertificateStoreName)]
        public string CertificateStoreName
        {
            get { return (string)base[TemsConfigurationStrings.CertificateStoreName]; }
            set { base[TemsConfigurationStrings.CertificateStoreName] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.CertificateNameAsFullSubjectDN, DefaultValue = TemsConfigurationDefaults.CertificateNameAsFullSubjectDN)]
        public string CertificateNameAsFullSubjectDN
        {
            get { return (string)base[TemsConfigurationStrings.CertificateNameAsFullSubjectDN]; }
            set { base[TemsConfigurationStrings.CertificateNameAsFullSubjectDN] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.SSLClientIdentity, DefaultValue = TemsConfigurationDefaults.SSLClientIdentity)]
        public string SSLClientIdentity
        {
            get { return (string)base[TemsConfigurationStrings.SSLClientIdentity]; }
            set { base[TemsConfigurationStrings.SSLClientIdentity] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.SSLPassword, DefaultValue = TemsConfigurationDefaults.SSLPassword)]
        public string SSLPassword
        {
            get { return (string)base[TemsConfigurationStrings.SSLPassword]; }
            set { base[TemsConfigurationStrings.SSLPassword] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.SSLAuthOnly, DefaultValue = TemsConfigurationDefaults.SSLAuthOnly)]
        public bool SSLAuthOnly
        {
            get { return (bool)base[TemsConfigurationStrings.SSLAuthOnly]; }
            set { base[TemsConfigurationStrings.SSLAuthOnly] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.SSLProxyHost, DefaultValue = TemsConfigurationDefaults.SSLProxyHost)]
        public string SSLProxyHost
        {
            get { return (string)base[TemsConfigurationStrings.SSLProxyHost]; }
            set { base[TemsConfigurationStrings.SSLProxyHost] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.SSLProxyPort, DefaultValue = TemsConfigurationDefaults.SSLProxyPort)]
        [IntegerValidator(MinValue = -1)]
        public int SSLProxyPort
        {
            get { return (int)base[TemsConfigurationStrings.SSLProxyPort]; }
            set { base[TemsConfigurationStrings.SSLProxyPort] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.SSLProxyAuthUsername, DefaultValue = TemsConfigurationDefaults.SSLProxyAuthUsername)]
        public string SSLProxyAuthUsername
        {
            get { return (string)base[TemsConfigurationStrings.SSLProxyAuthUsername]; }
            set { base[TemsConfigurationStrings.SSLProxyAuthUsername] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.SSLProxyAuthPassword, DefaultValue = TemsConfigurationDefaults.SSLProxyAuthPassword)]
        public string SSLProxyAuthPassword
        {
            get { return (string)base[TemsConfigurationStrings.SSLProxyAuthPassword]; }
            set { base[TemsConfigurationStrings.SSLProxyAuthPassword] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.SSLTrace, DefaultValue = TemsConfigurationDefaults.SSLTrace)]
        public bool SSLTrace
        {
            get { return (bool)base[TemsConfigurationStrings.SSLTrace]; }
            set { base[TemsConfigurationStrings.SSLTrace] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.SSLTargetHostName, DefaultValue = TemsConfigurationDefaults.SSLTargetHostName)]
        public string SSLTargetHostName
        {
            get { return (string)base[TemsConfigurationStrings.SSLTargetHostName]; }
            set { base[TemsConfigurationStrings.SSLTargetHostName] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.Username, DefaultValue = TemsConfigurationDefaults.Username)]
        public string Username
        {
            get { return (string)base[TemsConfigurationStrings.Username]; }
            set { base[TemsConfigurationStrings.Username] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.Password, DefaultValue = TemsConfigurationDefaults.Password)]
        public string Password
        {
            get { return (string)base[TemsConfigurationStrings.Password]; }
            set { base[TemsConfigurationStrings.Password] = value; }
        }
        #endregion

        #region Config_Control

        [ConfigurationProperty(TemsConfigurationStrings.AllowAdministratedConnFactory, DefaultValue = TemsConfigurationDefaults.AllowAdministratedConnFactory)]
        public bool AllowAdministratedConnFactory
        {
            get { return (bool)base[TemsConfigurationStrings.AllowAdministratedConnFactory]; }
            set { base[TemsConfigurationStrings.AllowAdministratedConnFactory] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.AllowAdministratedEndpointDest, DefaultValue = TemsConfigurationDefaults.AllowAdministratedEndpointDest)]
        public bool AllowAdministratedEndpointDest
        {
            get { return (bool)base[TemsConfigurationStrings.AllowAdministratedEndpointDest]; }
            set { base[TemsConfigurationStrings.AllowAdministratedEndpointDest] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.AllowAdministratedCallbackDest, DefaultValue = TemsConfigurationDefaults.AllowAdministratedCallbackDest)]
        public bool AllowAdministratedCallbackDest
        {
            get { return (bool)base[TemsConfigurationStrings.AllowAdministratedCallbackDest]; }
            set { base[TemsConfigurationStrings.AllowAdministratedCallbackDest] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.AllowBindingChanges, DefaultValue = TemsConfigurationDefaults.AllowBindingChanges)]
        public bool AllowBindingChanges
        {
            get { return (bool)base[TemsConfigurationStrings.AllowBindingChanges]; }
            set { base[TemsConfigurationStrings.AllowBindingChanges] = value; }
        }

        [ConfigurationProperty(TemsConfigurationStrings.AllowCustomMessageProtocol, DefaultValue = TemsConfigurationDefaults.AllowCustomMessageProtocol)]
        public bool AllowCustomMessageProtocol
        {
            get { return (bool)base[TemsConfigurationStrings.AllowCustomMessageProtocol]; }
            set { base[TemsConfigurationStrings.AllowCustomMessageProtocol] = value; }
        }
        
        #endregion

        #endregion

        #region Configuration_Infrastructure_overrides
        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);

            TemsTransportBindingElement temsBindingElement = (TemsTransportBindingElement)bindingElement;

            if (temsBindingElement.IsApplyConfigurationComplete)
            {
                throw new Exception("TemsTransportBindingElement.ApplyConfiguration cannot be called more than once.");
            }
            else
            {
                temsBindingElement.MaxBufferPoolSize = MaxBufferPoolSize;
                temsBindingElement.MaxReceivedMessageSize = MaxReceivedMessageSize;
                temsBindingElement.SessionAcknowledgeMode = SessionAcknowledgeMode;
                temsBindingElement.MessageDeliveryMode = MessageDeliveryMode;
                temsBindingElement.DisableMessageID = DisableMessageID;
                temsBindingElement.DisableMessageTimestamp = DisableMessageTimestamp;
                temsBindingElement.Priority = Priority;
                temsBindingElement.TimeToLive = TimeToLive;

                temsBindingElement.MessageProtocol = MessageProtocol;
                temsBindingElement.MessageType = MessageType;
                temsBindingElement.ThrowOnInvalidUTF = ThrowOnInvalidUTF;

                temsBindingElement.WsdlExtensionActive = WsdlExtensionActive;
                temsBindingElement.WsdlTypeSchemaImport = WsdlTypeSchemaImport;

                temsBindingElement.CustomMessageProtocolType = CustomMessageProtocolType;
                temsBindingElement.WsdlExportExtensionType = WsdlExportExtensionType; 

                temsBindingElement.MessageCompression = MessageCompression;
                temsBindingElement.ClientBaseAddress = ClientBaseAddress;
                temsBindingElement.AppHandlesMessageAcknowledge = AppHandlesMessageAcknowledge;
                temsBindingElement.AppManagesConnections = AppManagesConnections;
                temsBindingElement.MessageSelector = MessageSelector;
                temsBindingElement.ReplyDestCheckInterval = ReplyDestCheckInterval;

                temsBindingElement.ClientID = ClientID;
                temsBindingElement.ConnAttemptCount = ConnAttemptCount;
                temsBindingElement.ConnAttemptDelay = ConnAttemptDelay;
                temsBindingElement.ConnAttemptTimeout = ConnAttemptTimeout;
                temsBindingElement.LoadBalanceMetric = LoadBalanceMetric;
                temsBindingElement.ReconnAttemptCount = ReconnAttemptCount;
                temsBindingElement.ReconnAttemptDelay = ReconnAttemptDelay;
                temsBindingElement.ReconnAttemptTimeout = ReconnAttemptTimeout;
                temsBindingElement.ServerUrl = ServerUrl;
                temsBindingElement.CertificateStoreType = CertificateStoreType;
                temsBindingElement.CertificateStoreLocation = CertificateStoreLocation;
                temsBindingElement.CertificateStoreName = CertificateStoreName;
                temsBindingElement.CertificateNameAsFullSubjectDN = CertificateNameAsFullSubjectDN;
                temsBindingElement.SSLClientIdentity = SSLClientIdentity;
                temsBindingElement.SSLPassword = SSLPassword;
                temsBindingElement.SSLAuthOnly = SSLAuthOnly;
                temsBindingElement.SSLProxyHost = SSLProxyHost;
                temsBindingElement.SSLProxyPort = SSLProxyPort;
                temsBindingElement.SSLProxyAuthUsername = SSLProxyAuthUsername;
                temsBindingElement.SSLProxyAuthPassword = SSLProxyAuthPassword;
                temsBindingElement.SSLTrace = SSLTrace;
                temsBindingElement.SSLTargetHostName = SSLTargetHostName;
                temsBindingElement.Username = Username;
                temsBindingElement.Password = Password;

                temsBindingElement.AllowAdministratedConnFactory = AllowAdministratedConnFactory;
                temsBindingElement.AllowAdministratedEndpointDest = AllowAdministratedEndpointDest;
                temsBindingElement.AllowAdministratedCallbackDest = AllowAdministratedCallbackDest;
                temsBindingElement.AllowBindingChanges = AllowBindingChanges;
                temsBindingElement.AllowCustomMessageProtocol = AllowCustomMessageProtocol;
                temsBindingElement.SetApplyConfigurationComplete();
            }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            TemsTransportExtensionElement source = (TemsTransportExtensionElement)from;
            MaxBufferPoolSize = source.MaxBufferPoolSize;
            MaxReceivedMessageSize = source.MaxReceivedMessageSize;
            SessionAcknowledgeMode = source.SessionAcknowledgeMode;
            MessageDeliveryMode = source.MessageDeliveryMode;
            DisableMessageID = source.DisableMessageID;
            DisableMessageTimestamp = source.DisableMessageTimestamp;
            Priority = source.Priority;
            TimeToLive = source.TimeToLive;

            MessageProtocol = source.MessageProtocol;
            MessageType = source.MessageType;
            ThrowOnInvalidUTF = source.ThrowOnInvalidUTF;

            WsdlExtensionActive = source.WsdlExtensionActive;
            WsdlTypeSchemaImport = source.WsdlTypeSchemaImport;

            CustomMessageProtocolType = source.CustomMessageProtocolType;
            WsdlExportExtensionType = source.WsdlExportExtensionType; 

            MessageCompression = source.MessageCompression;
            ClientBaseAddress = source.ClientBaseAddress;
            AppHandlesMessageAcknowledge = source.AppHandlesMessageAcknowledge;
            AppManagesConnections = source.AppManagesConnections;
            MessageSelector = source.MessageSelector;

            ReplyDestCheckInterval = source.ReplyDestCheckInterval;

            ClientID = source.ClientID;
            ConnAttemptCount = source.ConnAttemptCount;
            ConnAttemptDelay = source.ConnAttemptDelay;
            ConnAttemptTimeout = source.ConnAttemptTimeout;
            LoadBalanceMetric = source.LoadBalanceMetric;
            ReconnAttemptCount = source.ReconnAttemptCount;
            ReconnAttemptDelay = source.ReconnAttemptDelay;
            ReconnAttemptTimeout = source.ReconnAttemptTimeout;
            ServerUrl = source.ServerUrl;
            CertificateStoreType = source.CertificateStoreType;
            CertificateStoreLocation = source.CertificateStoreLocation;
            CertificateStoreName = source.CertificateStoreName;
            CertificateNameAsFullSubjectDN = source.CertificateNameAsFullSubjectDN;
            SSLClientIdentity = source.SSLClientIdentity;
            SSLPassword = source.SSLPassword;
            SSLAuthOnly = source.SSLAuthOnly;
            SSLProxyHost = source.SSLProxyHost;
            SSLProxyPort = source.SSLProxyPort;
            SSLProxyAuthUsername = source.SSLProxyAuthUsername;
            SSLProxyAuthPassword = source.SSLProxyAuthPassword;
            SSLTrace = source.SSLTrace;
            SSLTargetHostName = source.SSLTargetHostName;
            Username = source.Username;
            Password = source.Password;

            AllowAdministratedConnFactory = source.AllowAdministratedConnFactory;
            AllowAdministratedEndpointDest = source.AllowAdministratedEndpointDest;
            AllowAdministratedCallbackDest = source.AllowAdministratedCallbackDest;
            AllowBindingChanges = source.AllowBindingChanges;
            AllowCustomMessageProtocol = source.AllowCustomMessageProtocol;
        }

        protected override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);

            TemsTransportBindingElement temsBindingElement = (TemsTransportBindingElement)bindingElement;
            MaxBufferPoolSize = temsBindingElement.MaxBufferPoolSize;
            MaxReceivedMessageSize = (int)temsBindingElement.MaxReceivedMessageSize;
            SessionAcknowledgeMode = temsBindingElement.SessionAcknowledgeMode;
            MessageDeliveryMode = temsBindingElement.MessageDeliveryMode;
            DisableMessageID = temsBindingElement.DisableMessageID;
            DisableMessageTimestamp = temsBindingElement.DisableMessageTimestamp;
            Priority = temsBindingElement.Priority;
            TimeToLive = temsBindingElement.TimeToLive;

            MessageProtocol = temsBindingElement.MessageProtocol;
            MessageType = temsBindingElement.MessageType;
            ThrowOnInvalidUTF = temsBindingElement.ThrowOnInvalidUTF;

            WsdlExtensionActive = temsBindingElement.WsdlExtensionActive;
            WsdlTypeSchemaImport = temsBindingElement.WsdlTypeSchemaImport;

            CustomMessageProtocolType = temsBindingElement.CustomMessageProtocolType;
            WsdlExportExtensionType = temsBindingElement.WsdlExportExtensionType;    

            MessageCompression = temsBindingElement.MessageCompression;
            ClientBaseAddress = temsBindingElement.ClientBaseAddress;
            AppHandlesMessageAcknowledge = temsBindingElement.AppHandlesMessageAcknowledge;
            AppManagesConnections = temsBindingElement.AppManagesConnections;
            MessageSelector = temsBindingElement.MessageSelector;

            ReplyDestCheckInterval = temsBindingElement.ReplyDestCheckInterval;

            ClientID = temsBindingElement.ClientID;
            ConnAttemptCount = temsBindingElement.ConnAttemptCount;
            ConnAttemptDelay = temsBindingElement.ConnAttemptDelay;
            ConnAttemptTimeout = temsBindingElement.ConnAttemptTimeout;
            LoadBalanceMetric = temsBindingElement.LoadBalanceMetric;
            ReconnAttemptCount = temsBindingElement.ReconnAttemptCount;
            ReconnAttemptDelay = temsBindingElement.ReconnAttemptDelay;
            ReconnAttemptTimeout = temsBindingElement.ReconnAttemptTimeout;
            ServerUrl = temsBindingElement.ServerUrl;
            CertificateStoreType = temsBindingElement.CertificateStoreType;
            CertificateStoreLocation = temsBindingElement.CertificateStoreLocation;
            CertificateStoreName = temsBindingElement.CertificateStoreName;
            CertificateNameAsFullSubjectDN = temsBindingElement.CertificateNameAsFullSubjectDN;
            SSLClientIdentity = temsBindingElement.SSLClientIdentity;
            SSLPassword = temsBindingElement.SSLPassword;
            SSLAuthOnly = temsBindingElement.SSLAuthOnly;
            SSLProxyHost = temsBindingElement.SSLProxyHost;
            SSLProxyPort = temsBindingElement.SSLProxyPort;
            SSLProxyAuthUsername = temsBindingElement.SSLProxyAuthUsername;
            SSLProxyAuthPassword = temsBindingElement.SSLProxyAuthPassword;
            SSLTrace = temsBindingElement.SSLTrace;
            SSLTargetHostName = temsBindingElement.SSLTargetHostName;
            Username = temsBindingElement.Username;
            Password = temsBindingElement.Password;

            AllowAdministratedConnFactory = temsBindingElement.AllowAdministratedConnFactory;
            AllowAdministratedEndpointDest = temsBindingElement.AllowAdministratedEndpointDest;
            AllowAdministratedCallbackDest = temsBindingElement.AllowAdministratedCallbackDest;
            AllowBindingChanges = temsBindingElement.AllowBindingChanges;
            AllowCustomMessageProtocol = temsBindingElement.AllowCustomMessageProtocol;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                ConfigurationPropertyCollection properties = base.Properties;
                
                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.MaxBufferPoolSize,
                    typeof(long), TemsConfigurationDefaults.MaxBufferPoolSize, null, new LongValidator(0, Int64.MaxValue), ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.MaxBufferPoolSize));
                
                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.MaxReceivedMessageSize,
                    typeof(int), TemsConfigurationDefaults.MaxReceivedMessageSize, null, new IntegerValidator(1, Int32.MaxValue), ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.MaxReceivedMessageSize));
                
                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.SessionAcknowledgeMode,
                    typeof(TIBCO.EMS.SessionMode), TemsConfigurationDefaults.SessionAcknowledgeMode, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.SessionAcknowledgeMode));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.MessageDeliveryMode,
                    typeof(MessageDeliveryMode), TemsConfigurationDefaults.MessageDeliveryMode, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.MessageDeliveryMode));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.DisableMessageID,
                    typeof(Boolean), TemsConfigurationDefaults.DisableMessageID, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.DisableMessageID));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.DisableMessageTimestamp,
                    typeof(Boolean), TemsConfigurationDefaults.DisableMessageTimestamp, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.DisableMessageTimestamp));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.Priority,
                    typeof(int), TemsConfigurationDefaults.Priority, null, new IntegerValidator(0, 9), ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.Priority));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.TimeToLive,
                    typeof(long), TemsConfigurationDefaults.TimeToLive, null, new LongValidator(0, Int64.MaxValue), ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.TimeToLive));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.MessageProtocol,
                    typeof(TemsMessageProtocolType), TemsConfigurationDefaults.MessageProtocol, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.MessageProtocol));
                
                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.MessageType,
                    typeof(TemsMessageType), TemsConfigurationDefaults.MessageType, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.MessageType));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.ThrowOnInvalidUTF,
                    typeof(Boolean), TemsConfigurationDefaults.ThrowOnInvalidUTF, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.ThrowOnInvalidUTF));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.WsdlExtensionActive,
                    typeof(Boolean), TemsConfigurationDefaults.WsdlExtensionActive, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.WsdlExtensionActive));


                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.WsdlTypeSchemaImport,
                    typeof(Boolean), TemsConfigurationDefaults.WsdlTypeSchemaImport, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.WsdlTypeSchemaImport));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.CustomMessageProtocolType,
                    typeof(string), TemsConfigurationDefaults.CustomMessageProtocolType, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.CustomMessageProtocolType));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.WsdlExportExtensionType,
                    typeof(string), TemsConfigurationDefaults.WsdlExportExtensionType, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.WsdlExportExtensionType));                

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.MessageCompression,
                    typeof(Boolean), TemsConfigurationDefaults.MessageCompression, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.MessageCompression));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.ClientBaseAddress,
                    typeof(string), TemsConfigurationDefaults.ClientBaseAddress, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.ClientBaseAddress));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.AppManagesConnections,
                    typeof(Boolean), TemsConfigurationDefaults.AppManagesConnections, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.AppManagesConnections));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.AppHandlesMessageAcknowledge,
                    typeof(Boolean), TemsConfigurationDefaults.AppHandlesMessageAcknowledge, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.AppHandlesMessageAcknowledge));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.MessageSelector,
                    typeof(string), TemsConfigurationDefaults.MessageSelector, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.MessageSelector));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.ReplyDestCheckInterval,
                    typeof(TimeSpan), TemsConfigurationDefaults.ReplyDestCheckInterval, new TimeSpanConverter(), new PositiveTimeSpanValidator(), ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.ReplyDestCheckInterval));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.ClientID,
                    typeof(string), TemsConfigurationDefaults.ClientID, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.ClientID));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.ConnAttemptCount,
                    typeof(int), TemsConfigurationDefaults.ConnAttemptCount, null, new IntegerValidator(-1, Int32.MaxValue), ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.ConnAttemptCount));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.ConnAttemptDelay,
                    typeof(int), TemsConfigurationDefaults.ConnAttemptDelay, null, new IntegerValidator(-1, Int32.MaxValue), ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.ConnAttemptDelay));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.ConnAttemptTimeout,
                    typeof(int), TemsConfigurationDefaults.ConnAttemptTimeout, null, new IntegerValidator(-1, Int32.MaxValue), ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.ConnAttemptTimeout));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.LoadBalanceMetric,
                    typeof(FactoryLoadBalanceMetric), TemsConfigurationDefaults.LoadBalanceMetric, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.LoadBalanceMetric));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.ReconnAttemptCount,
                    typeof(int), TemsConfigurationDefaults.ReconnAttemptCount, null, new IntegerValidator(-1, Int32.MaxValue), ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.ReconnAttemptCount));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.ReconnAttemptDelay,
                    typeof(int), TemsConfigurationDefaults.ReconnAttemptDelay, null, new IntegerValidator(-1, Int32.MaxValue), ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.ReconnAttemptDelay));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.ReconnAttemptTimeout,
                    typeof(int), TemsConfigurationDefaults.ReconnAttemptTimeout, null, new IntegerValidator(-1, Int32.MaxValue), ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.ReconnAttemptTimeout));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.ServerUrl,
                    typeof(string), TemsConfigurationDefaults.ServerUrl, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.ServerUrl));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.CertificateStoreType,
                    typeof(EMSSSLStoreType), TemsConfigurationDefaults.CertificateStoreType, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.CertificateStoreType));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.CertificateStoreLocation,
                    typeof(StoreLocation), TemsConfigurationDefaults.CertificateStoreLocation, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.CertificateStoreLocation));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.CertificateStoreName,
                    typeof(string), TemsConfigurationDefaults.CertificateStoreName, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.CertificateStoreName));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.CertificateNameAsFullSubjectDN,
                    typeof(string), TemsConfigurationDefaults.CertificateNameAsFullSubjectDN, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.CertificateNameAsFullSubjectDN));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.SSLClientIdentity,
                    typeof(string), TemsConfigurationDefaults.SSLClientIdentity, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.SSLClientIdentity));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.SSLPassword,
                    typeof(string), TemsConfigurationDefaults.SSLPassword, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.SSLPassword));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.SSLAuthOnly,
                    typeof(bool), TemsConfigurationDefaults.SSLAuthOnly, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.SSLAuthOnly));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.SSLProxyHost,
                    typeof(string), TemsConfigurationDefaults.SSLProxyHost, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.SSLProxyHost));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.SSLProxyPort,
                    typeof(int), TemsConfigurationDefaults.SSLProxyPort, null, new IntegerValidator(-1, Int32.MaxValue), ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.SSLProxyPort));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.SSLProxyAuthUsername,
                    typeof(string), TemsConfigurationDefaults.SSLProxyAuthUsername, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.SSLProxyAuthUsername));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.SSLProxyAuthPassword,
                    typeof(string), TemsConfigurationDefaults.SSLProxyAuthPassword, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.SSLProxyAuthPassword));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.SSLTrace,
                    typeof(bool), TemsConfigurationDefaults.SSLTrace, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.SSLTrace));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.SSLTargetHostName,
                    typeof(string), TemsConfigurationDefaults.SSLTargetHostName, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.SSLTargetHostName));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.Username,
                    typeof(string), TemsConfigurationDefaults.Username, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.Username));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.Password,
                    typeof(string), TemsConfigurationDefaults.Password, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.Password));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.AllowAdministratedConnFactory,
                    typeof(bool), TemsConfigurationDefaults.AllowAdministratedConnFactory, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.AllowAdministratedConnFactory));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.AllowAdministratedEndpointDest,
                    typeof(bool), TemsConfigurationDefaults.AllowAdministratedEndpointDest, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.AllowAdministratedEndpointDest));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.AllowAdministratedCallbackDest,
                    typeof(bool), TemsConfigurationDefaults.AllowAdministratedCallbackDest, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.AllowAdministratedCallbackDest));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.AllowBindingChanges,
                    typeof(bool), TemsConfigurationDefaults.AllowBindingChanges, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.AllowBindingChanges));

                properties.Add(new ConfigurationProperty(TemsConfigurationStrings.AllowCustomMessageProtocol,
                    typeof(bool), TemsConfigurationDefaults.AllowCustomMessageProtocol, null, null, ConfigurationPropertyOptions.None, TemsConfigurationDescriptions.AllowCustomMessageProtocol));
                
                return properties;
            }
        }
        #endregion
    }
}
