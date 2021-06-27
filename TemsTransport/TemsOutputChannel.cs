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
    class TemsOutputChannel : TemsChannelBase, IOutputChannel
    {
        internal EndpointAddress remoteAddress;
        Uri via;
        bool isClient;
        internal TemsDuplexChannel duplexChannel;

        private delegate void SendDelegate(Message message, TimeSpan timeout);
        private SendDelegate del;
        
        public TemsOutputChannel(ChannelManagerBase channelManager,
                                    EndpointAddress remoteAddress, 
                                    Uri via)
            : base(channelManager)
        {
            del = (m, t) => Send(m, t);
            this.remoteAddress = remoteAddress;
            this.via = via;
            this.isClient = true;

            // Note: via is the actual network (as in DNS) address of the service whereas
            //       remoteAddress is any EPR. It is possible to configure a service to
            //       listen at a physical address that is independent of the service's EPR.
            //       For example, this is useful in routing and discovery scenarios.
            //
            //       Since via is the actual network address this is the value that should
            //       be passed to the TemsChannelTransport constructor.
            CreateTemsChannelTransport(true, via);
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsOutputChannel constructed.");
        }

        public TemsOutputChannel(ChannelManagerBase channelManager,
                                    TemsDuplexChannel duplexChannel,
                                    bool isClient,
                                    EndpointAddress remoteAddress,
                                    Uri via)
            : base(channelManager)
        {
            del = (m, t) => Send(m, t);
            this.duplexChannel = duplexChannel;
            this.isClient = isClient;
            this.remoteAddress = remoteAddress;
            this.via = via;
            this.msgTransport = duplexChannel.msgTransport;
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsOutputChannel constructed.");
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Info, "TemsOutputChannel.OnClose() called.");
            // If this is the outputChannel of a TemsDuplexSessionChannel then:
            //   - If this type is TemsOutputChannel (TemsDuplexSessionChannel isClient == false)
            //     the TemsChannelBase OnClose should not be called.  This would result in the
            //     TemsChannelTransport Close being called and the connection and session being
            //     closed.  The TemsInputSessionChannel of the TemsDuplexSessionChannel controls
            //     the session and it is responsible to call TemsChannelBase OnClose.
            //   - If this type is TemsOutputSessionChannel (TemsDuplexSessionChannel isClient == true)
            //     the TemsChannelBase OnClose should be called since this is the channel that
            //     controls the session and is responsible to close the connection and session.
            //if (duplexChannel == null || this is TemsOutputSessionChannel)
            if (duplexChannel == null)
            {
                base.OnClose(timeout);
            }
        }

        #region IOutputChannel Members

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return del.BeginInvoke(message, timeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            TimeSpan timeout = bindingContext.Binding.SendTimeout;

            return del.BeginInvoke(message, timeout, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            del.EndInvoke(result);
        }

        public EndpointAddress RemoteAddress
        {
            get { return remoteAddress; }
        }

        public void Send(Message message, TimeSpan timeout)
        {
            remoteAddress.ApplyTo(message);

            if (isClient)
            {
                message.Headers.ReplyTo = msgTransport.ReplyToAddress;
                EmsSend(message, null, timeout);
            }
            else
            {
                EmsSend(message, remoteAddress.Uri, timeout);
            }
        }

        public void Send(Message message)
        {
            Send(message, bindingContext.Binding.SendTimeout);
        }

        public Uri Via
        {
            get { return via; }
        }

        #endregion
    }
}
