/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.Diagnostics;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using com.tibco.wcf.tems;

namespace com.tibco.sample.custom
{
    class SampleWsdlExportExtension : IWsdlExportExtension
    {
        private TemsTransportBindingElement binding;
        
        #region IWsdlExportExtension Members

        public void ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
            // This is not called by TemsWsdlExportExtension.
            // TemsWsdlExportExtension implements IWsdlExportExtension on the 
            // TemsTransportBindingElement and as an endpoint behavior.
            // Only contract and operation behaviors implementing IWsdlExportExtension get
            // the ExportContract call.
        }

        public void ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "SampleWsdlExportExtension.ExportEndpoint called.");
            System.ServiceModel.Channels.Binding endpointBinding = context.Endpoint.Binding;
            BindingElementCollection elements = ((CustomBinding)endpointBinding).Elements;
            BindingElement transportElement = elements[elements.Count - 1];
            
            if (transportElement is TemsTransportBindingElement)
            {
                binding = (TemsTransportBindingElement)elements[elements.Count - 1];
            }

            if (binding != null)
            {
                // Add custom code here.
            }
        }
        #endregion
    }
}
