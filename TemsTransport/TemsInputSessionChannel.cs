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
    class TemsInputSessionChannel : TemsInputChannel, IInputSessionChannel
    {
        private TemsInputSession session;

        internal TemsInputSessionChannel(ChannelManagerBase channelManager)
            : base(channelManager)
        {
            this.session = new TemsInputSession("");
            TemsTrace.WriteLine(TraceLevel.Info, "TemsInputSessionChannel constructed.");
        }

        internal TemsInputSessionChannel(ChannelManagerBase channelManager,
                                    TemsDuplexChannel duplexChannel,
                                    bool isClient)
            : base(channelManager, duplexChannel, isClient)
        {
            this.session = new TemsInputSession("");
            TemsTrace.WriteLine(TraceLevel.Info, "TemsInputSessionChannel constructed.");
        }

        #region ISessionChannel<IInputSession> Members

        public IInputSession Session
        {
            get { return session; }
        }

        #endregion

        protected override void OnAbort()
        {
            TemsTrace.WriteLine(TraceLevel.Info, "TemsInputSessionChannel.OnAbort() called.");
            base.OnAbort();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Info, "TemsInputSessionChannel.OnClose() called.");
            base.OnClose(timeout);
        }
    }
}
