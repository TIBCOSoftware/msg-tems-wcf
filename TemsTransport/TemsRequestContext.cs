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
    class TemsRequestContext : RequestContext
    {
        private TemsReplyChannel replyChannel;
        private Message requestMessage;

        private delegate void ReplyDelegate(Message message, TimeSpan timeout);
        private ReplyDelegate del;
        
        internal TemsRequestContext(TemsReplyChannel replyChannel, Message requestMessage)
        {
            del = (m, t) => Reply(m, t);
            this.replyChannel = replyChannel;
            this.requestMessage = requestMessage;
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsRequestContext created. requestMessage.Headers.ReplyTo = ' {0}", requestMessage.Headers.ReplyTo); 
        }
        
        public override void Abort()
        {
            TemsTrace.WriteLine(TraceLevel.Error, "TemsRequestContext.Abort() called.");
            replyChannel.Abort();
        }

        public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return del.BeginInvoke(message, timeout, callback, state);
        }

        public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
        {
            TimeSpan timeout = replyChannel.bindingContext.Binding.ReceiveTimeout;

            return del.BeginInvoke(message, timeout, callback, state);
        }

        public override void Close(TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsRequestContext.Close(TimeSpan timeout) called.");
        }

        public override void Close()
        {
            Close(replyChannel.bindingContext.Binding.CloseTimeout);
        }

        public override void EndReply(IAsyncResult result)
        {
            del.EndInvoke(result);
        }

        public override void Reply(Message message, TimeSpan timeout)
        {
            EndpointAddress replyAddress = requestMessage.Headers.ReplyTo;

            if (replyAddress == null && replyChannel.GetType() == typeof(TemsReplySessionChannel))
            {
                replyAddress = replyChannel.msgTransport.ReplyToAddress;
            }

            if (message != null)
            {
                if (replyAddress == null)
                {
                    replyChannel.Abort();
                    throw new Exception("Request message does not contain a ReplyTo address.");
                }
                else
                {
                    replyChannel.EmsSend(message, replyAddress.Uri, timeout);
                }
            }
        }

        public override void Reply(Message message)
        {
            Reply(message, replyChannel.bindingContext.Binding.SendTimeout);
        }

        protected override void Dispose(bool disposing)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsRequestContext.Dispose called, disposing = {0}", disposing);
            base.Dispose(disposing);
        }

        void Dispose()
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsRequestContext.Dispose() called.");
            Dispose(true);
        }

        public override Message RequestMessage
        {
            get
            {
                TemsTrace.WriteLine(TraceLevel.Verbose, "TemsRequestContext.RequestMessage called.");

                return requestMessage;
            }
        }
    }
}
