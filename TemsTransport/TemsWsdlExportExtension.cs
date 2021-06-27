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
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Xml.Schema;

namespace com.tibco.wcf.tems
{
    public class TemsWsdlExportExtension : BehaviorExtensionElement, IWsdlExportExtension, IEndpointBehavior
    {
        private TemsTransportBindingElement binding;

        private bool isEndpointBehavior;

        public TemsWsdlExportExtension()
        {
            isEndpointBehavior = true;
        }

        public TemsWsdlExportExtension(TemsTransportBindingElement binding)
        {
            this.binding = binding;
            isEndpointBehavior = false;
        }

        #region IWsdlExportExtension Members

        public void ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "ExportContract called");
        }

        public void ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            if (binding == null)
            {
                System.ServiceModel.Channels.Binding endpointBinding = context.Endpoint.Binding;
                BindingElementCollection elements = ((CustomBinding)endpointBinding).Elements;
                BindingElement transportElement = elements[elements.Count - 1];

                if (transportElement is TemsTransportBindingElement)
                {
                    binding = (TemsTransportBindingElement)elements[elements.Count - 1];
                }
            }

            if (binding != null && !(isEndpointBehavior && binding.WsdlExtensionActive))
            {
                if (binding.WsdlTypeSchemaImport)
                {
                    ImportSchemas(exporter);
                }

                IWsdlExportExtension extension = null;

                if (binding.MessageProtocol == TemsMessageProtocolType.TIBCOSoapOverJMS2004)
                {
                    extension = new TemsWsdlExportExtensionSoapOverJMS();
                }
                else if (binding.AllowCustomMessageProtocol && 
                            binding.MessageProtocol == TemsMessageProtocolType.Custom)
                {
                    if (binding.WsdlExportExtensionType.Length > 0)
                    {
                        Type type = Type.GetType(binding.WsdlExportExtensionType);

                        if (type == null)
                        {
                            TemsTrace.WriteLine(TraceLevel.Info, "The TemsTransportBindingElement WsdlExportExtensionType property is not a valid type.");
                        }
                        else if (typeof(IWsdlExportExtension).IsAssignableFrom(type))
                        {
                            extension = (IWsdlExportExtension)type.GetConstructor(new Type[0]).Invoke(new object[0]);
                        }
                        else
                        {
                            TemsTrace.WriteLine(TraceLevel.Info, "The TemsTransportBindingElement WsdlExportExtensionType property is not set to an implementation of IWsdlExportExtension.");
                        }
                    }
                }

                if (extension != null)
                {
                    extension.ExportEndpoint(exporter, context);
                }
            }
        }

        #endregion

        #region Import Schema Methods

        /** <summary>
          * Includes the <wsdl:types><xsd:schema ... <xsd:import> schema 
          * content expanded into the current WSDL document.
          * </summary>
          * <param name="exporter">The WsdlExporter i.e. that is passed into the IWsdlExportExtension methods.</param>
          **/
        private void ImportSchemas(WsdlExporter exporter)
        {
            XmlSchemaSet schemaSet = exporter.GeneratedXmlSchemas;

            foreach (System.Web.Services.Description.ServiceDescription wsdl in exporter.GeneratedWsdlDocuments)
            {
                List<XmlSchema> importsList = new List<XmlSchema>();

                foreach (XmlSchema schema in wsdl.Types.Schemas)
                {
                    AddImportedSchemas(schema, schemaSet, importsList);
                }

                wsdl.Types.Schemas.Clear();

                foreach (XmlSchema schema in importsList)
                {
                    RemoveXsdImports(schema);
                    wsdl.Types.Schemas.Add(schema);
                }
            }
        }

        private void AddImportedSchemas(XmlSchema schema, XmlSchemaSet schemaSet, List<XmlSchema> importsList)
        {
            foreach (XmlSchemaImport import in schema.Includes)
            {
                ICollection realSchemas = schemaSet.Schemas(import.Namespace);

                foreach (XmlSchema ixsd in realSchemas)
                {
                    if (!importsList.Contains(ixsd))
                    {
                        importsList.Add(ixsd);
                        AddImportedSchemas(ixsd, schemaSet, importsList);
                    }
                }
            }
        }

        private void RemoveXsdImports(XmlSchema schema)
        {
            for (int i = 0; i < schema.Includes.Count; i++)
            {
                if (schema.Includes[i] is XmlSchemaImport)
                    schema.Includes.RemoveAt(i--);
            }
        }

        #endregion

        #region IEndpointBehavior Members

        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion

        public override Type BehaviorType
        {
            get { return typeof(TemsWsdlExportExtension); }
        }

        protected override object CreateBehavior()
        {
            return new TemsWsdlExportExtension();
        }
    }
}
