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
    class TemsRequestSessionChannel : TemsRequestChannel, IRequestSessionChannel
    {

        private TemsOutputSession session;

        public TemsRequestSessionChannel(ChannelManagerBase channelManager,
                                        EndpointAddress remoteAddress,
                                        Uri via)
            : base(channelManager, remoteAddress, via)
        {            
            string sessionId = msgTransport.RequestSessionDestination(bindingContext.Binding.OpenTimeout);

            if (sessionId != null)
            {
                session = new TemsOutputSession(sessionId);
                TemsTrace.WriteLine(TraceLevel.Info, "TemsRequestSessionChannel constructed.");
            }
            else
            {
                TemsTrace.WriteLine(TraceLevel.Error, "TemsRequestSessionChannel sessionId is null.");
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

            TemsTrace.WriteLine(TraceLevel.Info, "TemsReplySessionChannel.OnAbort() called.");
            base.OnAbort();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TemsTimeoutTimer timer = new TemsTimeoutTimer(timeout);

            if (Session != null && Session.Id.Length != 0)
            {
                msgTransport.NotifyCloseSession(timeout);
            }

            TemsTrace.WriteLine(TraceLevel.Info, "TemsReplySessionChannel.OnClose() called.");
            base.OnClose(timer.CurrentTimeout(true));
        }
    }
}
