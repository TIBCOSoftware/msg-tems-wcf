/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using com.tibco.wcf.tems.ActivatorService;
using com.tibco.wcf.tems.ActivatorService.Activation;

namespace com.tibco.wcf.tems.ActivatorService.Hosting
{
    class HostedTemsTransportConfigurationImpl
    {
        static HostedTemsTransportConfigurationImpl singleton = null;
        static object syncRoot = new object();
        static internal HostedTemsTransportManager transportManager;

        HostedTemsTransportConfigurationImpl()
        {
            WasHelper.ts.TraceInformation("HostedTemsTransportConfigurationImpl:Constructor ");
            transportManager = new HostedTemsTransportManager();
        }

        static object StaticLock
        {
            get
            {
                return syncRoot;
            }
        }

        public Uri[] GetBaseAddresses(string virtualPath)
        {
            WasHelper.ts.TraceInformation("HostedTemsTransportConfigurationImpl:GetBaseAddresses baseaddr count = " + transportManager.BaseAddresses.Count);
            Uri[] addresses = new Uri[transportManager.BaseAddresses.Count];
            for (int i = 0; i < transportManager.BaseAddresses.Count; i++)
            {
                addresses[i] = new Uri(transportManager.BaseAddresses[i], virtualPath);
                WasHelper.ts.TraceInformation("HostedTemsTransportConfigurationImpl:GetBaseAddresses baseaddr = " + transportManager.BaseAddresses[i].AbsoluteUri + ", virtual path = " + virtualPath);
            }

            return addresses;
        }

        public static HostedTemsTransportConfigurationImpl Value
        {
            get
            {
                if (singleton != null)
                {
                    return singleton;
                }

                lock (StaticLock)
                {
                    if (singleton != null)
                    {
                        return singleton;
                    }

                    WasHelper.ts.TraceInformation("HostedTemsTransportConfigurationImpl: Value");
                    singleton = new HostedTemsTransportConfigurationImpl();

                    return singleton;
                }
            }
        }
    }
}
