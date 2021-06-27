/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Diagnostics;
using System.ServiceModel.Channels;

namespace com.tibco.wcf.tems
{
    class TemsReplySessionChannel : TemsReplyChannel, IReplySessionChannel
    {
        private TemsInputSession session;

        public TemsReplySessionChannel(ChannelManagerBase channelManager)
            : base(channelManager)
        {
            // When initially instantiated, the channel listens on the Endpoint address.
            // The session Id is created when a sessionful client channel is instantiated.
            this.session = new TemsInputSession("");
            TemsTrace.WriteLine(TraceLevel.Info, "TemsReplySessionChannel constructed.");
        }
        
        #region ISessionChannel<IInputSession> Members

        public IInputSession Session
        {
            get { return session; }
        }

        #endregion

        protected override void OnAbort()
        {
            TemsTrace.WriteLine(TraceLevel.Info, "TemsReplySessionChannel.OnAbort() called.");
            base.OnAbort();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Info, "TemsReplySessionChannel.OnClose() called.");
            base.OnClose(timeout);
        }
    }
}
