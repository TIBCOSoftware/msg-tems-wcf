/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Collections;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Web.Services.Description;
using System.Xml;
using TIBCO.EMS;

namespace com.tibco.wcf.tems
{
    class TemsWsdlExportExtensionSoapOverJMS : IWsdlExportExtension
    {
        public const string SoapTransport = "http://www.tibco.com/namespaces/ws/2004/soap/binding/JMS";
        public const string JmsNamespace = "http://www.tibco.com/namespaces/ws/2004/soap/binding/JMS";
        public const string JndiNamespace = "http://www.tibco.com/namespaces/ws/2004/soap/apis/jndi";

        public const string JavaProviderUrlName = "java.naming.provider.url";
        public const string JavaSecurityCredentialsName = "java.naming.security.credentials";
        public const string JavaSecurityPrincipalName = "java.naming.security.principal";
        public const string JavaSecurityProtocolName = "java.naming.security.protocol";

        private WsdlExporter exporter;
        private WsdlEndpointConversionContext context;
        private TemsTransportBindingElement binding;

        #region IWsdlExportExtension Members

        public void ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
        }

        public void ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            System.ServiceModel.Channels.Binding endpointBinding = context.Endpoint.Binding;
            BindingElementCollection elements = ((CustomBinding)endpointBinding).Elements;
            BindingElement transportElement = elements[elements.Count - 1];

            if (transportElement is TemsTransportBindingElement)
            {
                binding = (TemsTransportBindingElement)elements[elements.Count - 1];
            }

            if (binding != null)
            {
                this.exporter = exporter;
                this.context = context;
                
                AddNamespaces();
                SetBindingElements();
                SetJNDIElements();
                SetConnectionFactory();
                SetTargetAddress();
            }
        }

        #endregion

        private void AddNamespaces()
        {
            foreach (System.Web.Services.Description.ServiceDescription wsdl in exporter.GeneratedWsdlDocuments)
            {
                ((System.Web.Services.Description.DocumentableItem)(wsdl)).Namespaces.Add("jndi", JndiNamespace);
                ((System.Web.Services.Description.DocumentableItem)(wsdl)).Namespaces.Add("jms", JmsNamespace);
            }
        }

        private void SetBindingElements()
        {
            bool messageTypeSet = false;
            bool soapBindingSet = false;

            foreach (object extension in context.WsdlBinding.Extensions)
            {
                if (extension is TemsWsdlJmsBindingExtension)
                {
                    messageTypeSet = true;
                }
                else if (extension is SoapBinding)
                {
                    soapBindingSet = true;
                    ((SoapBinding)extension).Transport = SoapTransport;
                }
            }

            if (!messageTypeSet)
            {
                context.WsdlBinding.Extensions.Add(
                    new TemsWsdlJmsBindingExtension(
                        binding.MessageType == TemsMessageType.Text ?
                            TemsWsdlMessageFormatType.Text :
                            TemsWsdlMessageFormatType.Bytes));
            }

            if (!soapBindingSet)
            {
                SoapBinding soapBinding = new SoapBinding();
                soapBinding.Transport = SoapTransport;
                context.WsdlBinding.Extensions.Add(soapBinding);
            }
        }

        private void SetJNDIElements()
        {
            Hashtable bindingContextJNDI = binding.ContextJNDI;

            if (bindingContextJNDI != null)
            {
                TemsWsdlJndiContextExtension jndiContextExt = new TemsWsdlJndiContextExtension();

                MapBindingContext(bindingContextJNDI, jndiContextExt,
                                    JavaProviderUrlName, LookupContext.PROVIDER_URL);

                MapBindingContext(bindingContextJNDI, jndiContextExt,
                                    JavaSecurityCredentialsName, LookupContext.SECURITY_CREDENTIALS);

                MapBindingContext(bindingContextJNDI, jndiContextExt,
                                    JavaSecurityPrincipalName, LookupContext.SECURITY_PRINCIPAL);

                MapBindingContext(bindingContextJNDI, jndiContextExt,
                                    JavaSecurityProtocolName, LookupContext.SECURITY_PROTOCOL);

                foreach (DictionaryEntry entry in bindingContextJNDI)
                {
                    jndiContextExt.AddProperty(new TemsWsdlJndiProperty((string)entry.Key, (string)entry.Value));
                }

                context.WsdlPort.Extensions.Add(jndiContextExt);
            }
        }

        private void SetConnectionFactory()
        {
            TIBCO.EMS.ConnectionFactory cf = binding.ConnectionFactory;

            if (cf != null)
            {
                string federatedName = null;

                if (cf is TIBCO.EMS.FederatedConnectionFactory)
                {
                    federatedName = ((TIBCO.EMS.FederatedConnectionFactory)(cf)).Name;
                }
                else if (cf is TIBCO.EMS.FederatedQueueConnectionFactory)
                {
                    federatedName = ((TIBCO.EMS.FederatedQueueConnectionFactory)(cf)).Name;
                }
                else if (cf is TIBCO.EMS.FederatedTopicConnectionFactory)
                {
                    federatedName = ((TIBCO.EMS.FederatedTopicConnectionFactory)(cf)).Name;
                }

                if (federatedName != null)
                {
                    int index = federatedName.IndexOf("$factories");
                    if (index != -1)
                    {
                        context.WsdlPort.Extensions.Add(
                            new TemsWsdlConnectionFactoryExtension(
                                    federatedName.Substring(index + 11)));
                    }
                }
                else
                {
                    TemsTrace.WriteLine(TraceLevel.Error, "TemsWsdlExportExtension.ExportEndpoint error: The binding.ConnectionFactory: {0} is not of one of the expected types: FederatedConnectionFactory, FederatedQueueConnectionFactory, or FederatedTopicConnectionFactory.");
                }
            }
        }

        /** <summary>
          * If the binding.EndpointDestination is set, the type and QueueName or TopicName
          * of the Federated Destination object is used.
          * Otherwise, the type and name are extracted from the context.Endpoint.Address.Uri.
          * </summary>
          **/
        private void SetTargetAddress()
        {
            Destination endpoint = binding.EndpointDestination;
            TemsWsdlDestinationType destType;
            string destName;

            if (endpoint != null)
            {
                if (endpoint is TIBCO.EMS.Topic)
                {
                    destType = TemsWsdlDestinationType.topic;
                    destName = ((TIBCO.EMS.Topic)endpoint).TopicName;

                }
                else
                {
                    destType = TemsWsdlDestinationType.queue;
                    destName = ((TIBCO.EMS.Queue)endpoint).QueueName;

                }
            }
            else
            {
                Uri endpointUri = context.Endpoint.Address.Uri;
                string[] segments = endpointUri.Segments;
                string destTypeString = segments[segments.Length - 2];

                destType = destTypeString.Equals(TemsChannelTransport.TOPIC,
                                            StringComparison.OrdinalIgnoreCase) ?
                                TemsWsdlDestinationType.topic : TemsWsdlDestinationType.queue;
                destName = segments[segments.Length - 1];
            }

            context.WsdlPort.Extensions.Add(new TemsWsdlTargetAddressExtension(destType, destName));
        }

        /** <summary>
          * Maps the value set for a .NET context name to the required Java context name.
          * 
          * For each java / dotNet ContextName pair provided, checks if the java name is
          * set as a key in bindingContextJNDI.  If not and the dotNet name is set as a
          * key, then a property entry is added to jndiContextExt with:
          *     key   = javaContextName
          *     value = the value associated with the dotNetContextName key in bindingContextJNDI
          * 
          * Note that if a property entry is added to jndiContextExt here that the original
          * dotNetContextName key / value will also be added to jndiContextExt by the calling
          * method.
          * </summary>
          * <param name="bindingContextJNDI">Context Hashtable from the Tems binding.</param>
          * <param name="jndiContextExt">WSDL extension that writes the JNDI properties.</param>
          * <param name="javaContextName">Java context name.</param>
          * <param name="dotNetContextName">.NET context name.</param>
          **/ 
        private void MapBindingContext(Hashtable bindingContextJNDI,
                                        TemsWsdlJndiContextExtension jndiContextExt,
                                        string javaContextName, string dotNetContextName)
        {
            if (!bindingContextJNDI.ContainsKey(javaContextName) &&
                    bindingContextJNDI.ContainsKey(dotNetContextName))
            {
                jndiContextExt.AddProperty(new TemsWsdlJndiProperty(javaContextName,
                                        (string)bindingContextJNDI[dotNetContextName]));
            }
        }
    }
}
