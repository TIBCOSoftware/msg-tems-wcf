/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Diagnostics;
using System.Web.Hosting;
using com.tibco.wcf.tems.ActivatorService.Activation;

namespace com.tibco.wcf.tems.ActivatorService.Hosting
{
    public class TemsAppDomainProtocolHandler : AppDomainProtocolHandler
    {
        IListenerChannelCallback listenerChannelCallback;
        AppInstance appInstance;

        public override void StartListenerChannel(IListenerChannelCallback listenerChannelCallback)
        {
            this.listenerChannelCallback = listenerChannelCallback;

            WasHelper.ts.TraceInformation("TemsAppDomainProtocolHandler: StartListenerChannel, id = " + listenerChannelCallback.GetId());
            // Start the real work here
            HostedTemsTransportConfigurationImpl htc = HostedTemsTransportConfigurationImpl.Value;
            int length = listenerChannelCallback.GetBlobLength();

            if (length > 0)
            {
                byte[] blob = new byte[length];
                listenerChannelCallback.GetBlob(blob, ref length);
                appInstance = AppInstance.Deserialize(blob);

                HostedTemsTransportConfigurationImpl.transportManager.Open(listenerChannelCallback.GetId(), appInstance.VirtualPath);
            }
            else
            {
                WasHelper.ts.TraceInformation("TemsAppDomainProtocolHandler: StartListenerChannel, no BLOB id = " + listenerChannelCallback.GetId());
            }

            listenerChannelCallback.ReportStarted();
        }

        public override void StopListenerChannel(int listenerChannelId, bool immediate)
        {
            try
            {
                WasHelper.ts.TraceInformation("TemsAppDomainProtocolHandler: StopListenerChannel, id = " + listenerChannelId);

                if (HostedTemsTransportConfigurationImpl.transportManager != null)
                {
                    HostedTemsTransportConfigurationImpl.transportManager.Close(listenerChannelId);
                }

                listenerChannelCallback.ReportStopped(0);
            }
            catch (Exception ex)
            {
                string errMsg = "TemsAppDomainProtocolHandler: StopListenerChannel Exception = " + ex.Message;

                if (ex.InnerException != null)
                {
                    errMsg = errMsg + ", InnerException = " + ex.InnerException;
                }

                WasHelper.ts.TraceEvent(TraceEventType.Warning, WasHelper.GetErrSeq(), errMsg);
            }
       }

        public override void StopProtocol(bool immediate)
        {
            try
            {
                WasHelper.ts.TraceInformation("TemsAppDomainProtocolHandler: StopProtocol");

                if (HostedTemsTransportConfigurationImpl.transportManager != null)
                {
                    if (!HostedTemsTransportConfigurationImpl.transportManager.isClosed)
                        HostedTemsTransportConfigurationImpl.transportManager.Close(listenerChannelCallback.GetId());
                }

                listenerChannelCallback.ReportStopped(0);
                HostingEnvironment.UnregisterObject(this);
                WasHelper.ts.TraceInformation("TemsAppDomainProtocolHandler: StopProtocol End");
            }
            catch (Exception ex)
            {
                string errMsg = "TemsAppDomainProtocolHandler: StopProtocol Exception = " + ex.Message;

                if (ex.InnerException != null)
                {
                    errMsg = errMsg + ", InnerException = " + ex.InnerException;
                }

                WasHelper.ts.TraceEvent(TraceEventType.Warning, WasHelper.GetErrSeq(), errMsg);
            }
        }
    }
}
