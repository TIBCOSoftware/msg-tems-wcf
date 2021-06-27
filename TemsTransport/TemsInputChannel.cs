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
    class TemsInputChannel : TemsChannelBase, IInputChannel
    {
        private bool isClient;
        private object waitForMessageLock;
        private Message waitForMessage;
        private delegate Message ReceiveDelegate(TimeSpan timeout);
        private ReceiveDelegate receiveDelegate;
        private delegate bool TryReceiveDelegate(TimeSpan timeout, out Message message);
        private TryReceiveDelegate tryReceiveDelegate;
        private delegate bool WaitForMessageDelegate(TimeSpan timeout);
        private WaitForMessageDelegate waitForMessageDelegate;

        internal TemsInputChannel(ChannelManagerBase channelManager)
            : base(channelManager)
        {
            receiveDelegate = (t) => Receive(t);
            tryReceiveDelegate = (TimeSpan t, out Message message) => TryReceive(t, out message);
            waitForMessageDelegate = t => WaitForMessage(t);
            waitForMessageLock = new Object();
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsInputChannel constructed");
            this.isClient = false;
            Uri uri = ((ChannelListenerBase)Manager).Uri;
            CreateTemsChannelTransport(false, uri);
        }

        internal TemsInputChannel(ChannelManagerBase channelManager,
                                    TemsDuplexChannel duplexChannel,
                                    bool isClient)
            : base(channelManager)
        {
            receiveDelegate = (t) => Receive(t);
            tryReceiveDelegate = (TimeSpan t, out Message message) => TryReceive(t, out message);
            waitForMessageDelegate = t => WaitForMessage(t);
            waitForMessageLock = new Object();
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsInputChannel constructed");
            msgTransport = duplexChannel.msgTransport;
            this.isClient = isClient;
        }

        #region IInputChannel Members

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return receiveDelegate.BeginInvoke(timeout, callback, state);
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            TimeSpan timeout = bindingContext.Binding.ReceiveTimeout;

            return BeginReceive(timeout, callback, state);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Message message;

            return tryReceiveDelegate.BeginInvoke(timeout, out message, callback, state);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return waitForMessageDelegate.BeginInvoke(timeout, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            return receiveDelegate.EndInvoke(result);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            return tryReceiveDelegate.EndInvoke(out message, result);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return waitForMessageDelegate.EndInvoke(result);
        }

        public EndpointAddress LocalAddress
        {
            get { return msgTransport.ReplyToAddress; }
        }

        public Message Receive(TimeSpan timeout)
        {
            Message message = null;

            try
            {
                ThrowIfDisposedOrNotOpen();
                
                lock (waitForMessageLock)
                {
                    if (waitForMessage != null)
                    {
                        message = waitForMessage;
                        waitForMessage = null;
                    }
                }

                if (message == null)
                {
                    message = EmsReceive(timeout);
                }

                ThrowIfDisposedOrNotOpen();
            }
            catch (TimeoutException e)
            {
                // The calling method handles the timeout.
                throw e;
            }
            catch (Exception e)
            {
                HandleReceiveException("TemsInputChannel.Receive", e);
            }

            return message;
        }

        public Message Receive()
        {
            return Receive(bindingContext.Binding.ReceiveTimeout);
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            message = null;
            bool retval = true;

            try
            {
                ThrowIfDisposedOrNotOpen();
                message = Receive(timeout);
                ThrowIfDisposedOrNotOpen();
            }
            catch (TimeoutException)
            {
                retval = false;
            }
            catch (Exception e)
            {
                HandleReceiveException("TemsInputChannel.TryReceive", e);
            }

            return retval;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            bool retval = true;
            try
            {
                ThrowIfDisposedOrNotOpen();
                /**
                 * Note: regarding synchronization on waitForMessageLock.  There should be no contention
                 * between WaitForMessage and Receive on the lock because:
                 *     If sessionless, all requests will be through one channel.
                 *     If sessionful, each session has its own channel.
                 * Note that a concurrent thread calling ReceiveRequest will be blocked on a call
                 * to lock (waitForMessageLock) until EmsReceive returns.
                 **/ 
                lock (waitForMessageLock)
                {
                    if (waitForMessage == null)
                    {
                        waitForMessage = EmsReceive(timeout);
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
                HandleReceiveException("TemsInputChannel.WaitForMessage", e);
            }
            return retval;
        }

        #endregion
    }
}
