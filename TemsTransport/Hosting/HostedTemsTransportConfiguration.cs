/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.ServiceModel.Activation;
using com.tibco.wcf.tems.ActivatorService.Activation;

namespace com.tibco.wcf.tems.ActivatorService.Hosting
{
    public class HostedTemsTransportConfiguration : HostedTransportConfiguration
    {
        public HostedTemsTransportConfiguration()
        {
        }

        public override Uri[] GetBaseAddresses(string virtualPath)
        {
            WasHelper.ts.TraceInformation("HostedTemsTransportConfiguration:GetBaseAddresses virtualPath = " + virtualPath);
            
            return HostedTemsTransportConfigurationImpl.Value.GetBaseAddresses(virtualPath);
        }
    }
}
