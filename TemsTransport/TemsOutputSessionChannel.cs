/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace com.tibco.wcf.tems
{
    class TemsOutputSessionChannel : TemsOutputChannel, IOutputSessionChannel
    {
        private IOutputSession session;
        
        public TemsOutputSessionChannel(ChannelManagerBase channelManager,
                                    EndpointAddress remoteAddress, 
                                    Uri via)
            : base(channelManager, remoteAddress, via)
        {
            string sessionId = msgTransport.RequestSessionDestination(bindingContext.Binding.OpenTimeout);

            if (sessionId != null)
            {
                session = new TemsOutputSession(sessionId);
                TemsTrace.WriteLine(TraceLevel.Info, "TemsOutputSessionChannel constructed.");
            }
            else
            {
                TemsTrace.WriteLine(TraceLevel.Error, "TemsOutputSessionChannel sessionId is null.");
                Abort();
            }
        }

        public TemsOutputSessionChannel(ChannelManagerBase channelManager,
                                    TemsDuplexChannel duplexChannel,
                                    bool isClient,
                                    EndpointAddress remoteAddress,
                                    Uri via)
            : base(channelManager, duplexChannel, isClient, remoteAddress, via)
        {
            string sessionId = msgTransport.RequestSessionDestination(bindingContext.Binding.OpenTimeout);

            if (sessionId != null)
            {
                session = new TemsOutputSession(sessionId);
                TemsTrace.WriteLine(TraceLevel.Info, "TemsOutputSessionChannel constructed.");
            }
            else
            {
                TemsTrace.WriteLine(TraceLevel.Error, "TemsOutputSessionChannel sessionId is null.");
                Abort();
            }
        }
        
        #region ISessionChannel<IOutputSession> Members

        public IOutputSession Session
        {
            get { return session; }
        }

        #endregion

        protected override void OnAbort()
        {
            if (Session != null && Session.Id.Length != 0)
            {
                msgTransport.NotifyCloseSession();
            }

            TemsTrace.WriteLine(TraceLevel.Info, "TemsOutputSessionChannel.OnAbort() called.");
            base.OnAbort();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TemsTimeoutTimer timer = new TemsTimeoutTimer(timeout);

            if (Session != null && Session.Id.Length != 0)
            {
                msgTransport.NotifyCloseSession(timeout);
            }

            TemsTrace.WriteLine(TraceLevel.Info, "TemsOutputSessionChannel.OnClose() called.");

            if (duplexChannel == null)
            {
                base.OnClose(timer.CurrentTimeout(true));
            }
            else
            {
                if (((TemsDuplexSessionChannel)duplexChannel).AutomaticInputSessionShutdown)
                {
                    duplexChannel.inputChannel.Close(timer.CurrentTimeout(true));
                }
            }
        }
    }
}
