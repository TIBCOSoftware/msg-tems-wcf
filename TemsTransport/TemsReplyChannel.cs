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

    class TemsReplyChannel : TemsChannelBase, IReplyChannel
    {
        private delegate RequestContext ReceiveRequestDelegate(TimeSpan timeout);
        private ReceiveRequestDelegate receiveRequestDelegate;
        private delegate bool TryReceiveRequestDelegate(TimeSpan timeout, out RequestContext context);
        private TryReceiveRequestDelegate tryReceiveRequestDelegate;
        private delegate bool WaitForRequestDelegate(TimeSpan timeout);
        private WaitForRequestDelegate waitForRequestDelegate;
        private object waitForRequestLock;
        private Message waitForRequestMessage;
        private EndpointAddress localAddress;

        internal TemsReplyChannel(ChannelManagerBase channelManager)
            : base(channelManager)
        {
            receiveRequestDelegate = t => ReceiveRequest(t);
            tryReceiveRequestDelegate = (TimeSpan t, out RequestContext rc) => TryReceiveRequest(t, out rc);
            waitForRequestDelegate = t => WaitForRequest(t);
            waitForRequestLock = new Object();
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsReplyChannel constructed");
            Uri receiveUri = ((ChannelListenerBase)Manager).Uri;
            localAddress = new EndpointAddress(receiveUri.OriginalString);
            CreateTemsChannelTransport(false, receiveUri);
        }

        #region IReplyChannel Members

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            //TemsTrace.WriteLine(TraceLevel.Verbose, "TemsReplyChannel.BeginReceiveRequest(AsyncCallback callback, object state) called");
            TimeSpan timeout = bindingContext.Binding.ReceiveTimeout;

            return BeginReceiveRequest(timeout, callback, state);
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            //TemsTrace.WriteLine(TraceLevel.Verbose, "TemsInputChannel.BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state) called");
            //TemsTrace.WriteLine(TraceLevel.Verbose, "The hash of the current thread is: {0}", Thread.CurrentThread.GetHashCode());
            return receiveRequestDelegate.BeginInvoke(timeout, callback, state);
        }
        
        public RequestContext ReceiveRequest()
        {
            return ReceiveRequest(bindingContext.Binding.ReceiveTimeout);
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            RequestContext retval = null;

            try
            {
                ThrowIfDisposedOrNotOpen();
                Message requestMessage = null;

                lock (waitForRequestLock)
                {
                    if (waitForRequestMessage != null)
                    {
                        requestMessage = waitForRequestMessage;
                        waitForRequestMessage = null;
                    }
                }

                if (requestMessage == null)
                {
                    requestMessage = EmsReceive(timeout);
                }

                ThrowIfDisposedOrNotOpen();
                retval = new TemsRequestContext(this, requestMessage);
            }
            catch (TimeoutException e)
            {
                // The calling method handles the timeout.
                throw e;
            }
            catch (Exception e)
            {
                HandleReceiveException("TemsReplyChannel.ReceiveRequest", e);
            }

            return retval;
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            //TemsTrace.WriteLine(TraceLevel.Verbose, "TemsReplyChannel.EndReceiveRequest(IAsyncResult result) called");
            return receiveRequestDelegate.EndInvoke(result);
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            //TemsTrace.WriteLine(TraceLevel.Verbose, "TemsReplyChannel.BeginTryReceiveRequest called");
            RequestContext context;

            return tryReceiveRequestDelegate.BeginInvoke(timeout, out context, callback, state);
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            context = null;
            bool retval = true;

            try
            {
                ThrowIfDisposedOrNotOpen();
                context = ReceiveRequest(timeout);
                ThrowIfDisposedOrNotOpen();
            }
            catch (TimeoutException)
            {
                retval = false;
            }
            catch (Exception e)
            {
                HandleReceiveException("TemsReplyChannel.TryReceiveRequest", e);
            }

            return retval;
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            //TemsTrace.WriteLine(TraceLevel.Verbose, "TemsReplyChannel.EndTryReceiveRequest called");
            return tryReceiveRequestDelegate.EndInvoke(out context, result);
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return waitForRequestDelegate.BeginInvoke(timeout, callback, state);
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            bool retval = true;

            try
            {
                ThrowIfDisposedOrNotOpen();
                // Synchronization on waitForRequestLock:
                //   If sessionless, all requests will be through one channel.
                //   If sessionful, each session has its own channel.

                // Note that a concurrent thread calling ReceiveRequest will be blocked on a call
                // to lock (waitForRequestLock) until EmsReceive returns.
                lock (waitForRequestLock)
                {
                    if (waitForRequestMessage == null)
                    {
                        waitForRequestMessage = EmsReceive(timeout);
                    }
                }

                ThrowIfDisposedOrNotOpen();
            }
            catch (TimeoutException)
            {
                retval = false;
            }
            catch (Exception e)
            {
                HandleReceiveException("TemsReplyChannel.WaitForRequest", e);
            }

            return retval;
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            return waitForRequestDelegate.EndInvoke(result);
        }

        public EndpointAddress LocalAddress
        {
            get { return localAddress; }
        }

        #endregion
    }
}
