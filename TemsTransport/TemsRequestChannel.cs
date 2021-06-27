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
using System.Xml;

namespace com.tibco.wcf.tems
{
    class TemsRequestChannel : TemsChannelBase, IRequestChannel
    {
        private EndpointAddress remoteAddress;
        private Uri via;

        private delegate Message RequestDelegate(Message message, TimeSpan timeout);
        private RequestDelegate del;

        public TemsRequestChannel(ChannelManagerBase channelManager,
                                    EndpointAddress remoteAddress,
                                    Uri via)
            : base(channelManager)
        {
            del = (m, t) => Request(m, t);
            this.remoteAddress = remoteAddress;
            this.via = via;
            CreateTemsChannelTransport(true, via);
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsRequestChannel constructed.");
        }

        #region IRequestChannel Members

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return del.BeginInvoke(message, timeout, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            TimeSpan timeout = bindingContext.Binding.ReceiveTimeout;

            return del.BeginInvoke(message, timeout, callback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            return del.EndInvoke(result);
        }

        public EndpointAddress RemoteAddress
        {
            get { return remoteAddress; }
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            ThrowIfDisposedOrNotOpen();
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsRequestChannel.Request(Message message, TimeSpan timeout) called.");

            return EmsRequest(message, timeout);
        }

        public Message Request(Message message)
        {
            return Request(message, bindingContext.Binding.ReceiveTimeout);
        }

        private Message EmsRequest(Message message, TimeSpan timeout)
        {
            TemsTimeoutTimer timer = new TemsTimeoutTimer(timeout);
            Message replyMessage = null;
            bool isOneWay = message.Headers.ReplyTo == null;
            
            remoteAddress.ApplyTo(message);

            UniqueId messageId = message.Headers.MessageId;

            // ReplyTo is not set for OneWay or if MessageId is not set (some ReliableSession messages for example).
            if (!isOneWay && messageId != null)
            {
                message.Headers.ReplyTo = msgTransport.ReplyToAddress;
            }

            EmsSend(message, null, timeout);

            if (!isOneWay && messageId != null)
            {
                //TemsTrace.WriteLine(TraceLevel.Verbose, "\n\n>>>>>>>>>>\nService contract request message:\n\n    {0}", message);
                replyMessage = EmsReceive(messageId, timer);
                //TemsTrace.WriteLine(TraceLevel.Verbose, "\n\n<<<<<<<<<<\nService contract reply message:\n\n    {0}", replyMessage);
            }
            else
            {
                //TemsTrace.WriteLine(TraceLevel.Verbose, "\n\n>>>>>>>>>>\nReliableSession request message:\n\n    {0}", message);
                replyMessage = EmsReceive(timer);
                //TemsTrace.WriteLine(TraceLevel.Verbose, "\n\n<<<<<<<<<<\nReliableSession reply message:\n\n    {0}", replyMessage);
            }
            return replyMessage;
        }

        public Uri Via
        {
            get { return via; }
        }

        #endregion
    }
}
