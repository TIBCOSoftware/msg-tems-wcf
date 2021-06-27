/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Web.Hosting;
using System.Diagnostics;
using System.Collections.Generic;
using com.tibco.wcf.tems.ActivatorService.Activation;

namespace com.tibco.wcf.tems.ActivatorService.Hosting
{
    public class TemsProcessProtocolHandler : ProcessProtocolHandler
    {
        IAdphManager adphManager;
        Dictionary<int, AppInstance> appInstanceTable = new Dictionary<int, AppInstance>();

        public override void StartListenerChannel(IListenerChannelCallback listenerChannelCallback, IAdphManager adphManager)
        {
            WasHelper.ts.TraceInformation("TemsProcessProtocolHandler: StartListenerChannel");
            int channelId = listenerChannelCallback.GetId();
            AppInstance appInstance;

            if (!this.appInstanceTable.TryGetValue(channelId, out appInstance))
            {
                lock (ThisLock)
                {
                    if (!this.appInstanceTable.TryGetValue(channelId, out appInstance))
                    {
                        int length = listenerChannelCallback.GetBlobLength();

                        if (length > 0)
                        {
                            byte[] blob = new byte[length];
                            listenerChannelCallback.GetBlob(blob, ref length);
                            appInstance = AppInstance.Deserialize(blob);
                            appInstanceTable.Add(channelId, appInstance);
                        }
                    }
                }
            }

            if (this.adphManager == null)
            {
                this.adphManager = adphManager;
            }

            Debug.Assert(channelId == appInstance.Id);
            this.adphManager.StartAppDomainProtocolListenerChannel(appInstance.AppKey,
                WasTemsInfo.TemsScheme, listenerChannelCallback);
        }

        public override void StopListenerChannel(int listenerChannelId, bool immediate)
        {
            WasHelper.ts.TraceInformation("TemsProcessProtocolHandler: StopListenerChannel id = " + listenerChannelId);
            AppInstance appInstance = this.appInstanceTable[listenerChannelId];

            if (appInstance != null)
            {
                this.adphManager.StopAppDomainProtocolListenerChannel(appInstance.AppKey,
                    WasTemsInfo.TemsScheme, listenerChannelId, immediate);
                lock (ThisLock)
                {
                    this.appInstanceTable.Remove(listenerChannelId);
                }
            }
        }

        public override void StopProtocol(bool immediate)
        {
            WasHelper.ts.TraceInformation("TemsProcessProtocolHandler: StopProtocol");
        }

        object ThisLock
        {
            get
            {
                return this.appInstanceTable;
            }
        }
    }
}
