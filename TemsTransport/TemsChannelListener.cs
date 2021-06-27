/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.ServiceModel.Channels;
using com.tibco.wcf.tems.ActivatorService.Hosting;
using System.Diagnostics;

namespace com.tibco.wcf.tems
{
    public class TemsChannelListener<TChannel> : ChannelListenerBase<TChannel>, ITemsChannelManager where TChannel : class, System.ServiceModel.Channels.IChannel
    {
        private TemsTransportBindingElement bindingElement;
        private BindingContext bindingContext;
        private MessageEncoderFactory encoderFactory;
        private BufferManager bufferManager;

        private Uri uri;

        private object channelLock;
        internal TChannel currentChannel;

        private WaitAsyncResult result;

        private delegate void OnOpenDelegate(TimeSpan timeout);
        private OnOpenDelegate onOpenDel;
        private delegate void OnCloseDelegate(TimeSpan timeout);
        private OnCloseDelegate onCloseDel;
        private delegate TChannel OnAcceptChannelDelegate(TimeSpan timeout);
        private OnAcceptChannelDelegate onAcceptChannelDel;

        private Type channelType;
        
        public TemsChannelListener(TemsTransportBindingElement bindingElement, BindingContext bindingContext)
            : base(bindingContext.Binding)
        {
            onOpenDel = (t) => OnOpen(t);
            onCloseDel = (t) => OnClose(t);
            onAcceptChannelDel = (t) => OnAcceptChannel(t);
            this.bindingElement = bindingElement;
            this.bindingContext = bindingContext;
            bufferManager = BufferManager.CreateBufferManager(bindingElement.MaxBufferPoolSize, (int)bindingElement.MaxReceivedMessageSize);

            MessageEncodingBindingElement encodingBindingElement = bindingContext.BindingParameters.Remove<MessageEncodingBindingElement>();

            if (encodingBindingElement != null)
            {
                encoderFactory = encodingBindingElement.CreateMessageEncoderFactory();
            }
            else
            {
                encoderFactory = new BinaryMessageEncodingBindingElement().CreateMessageEncoderFactory();
            }

            uri = new Uri(bindingContext.ListenUriBaseAddress, bindingContext.ListenUriRelativeAddress);
            channelLock = new object();
            channelType = typeof(TChannel);
        }

        #region ITemsChannelManager Members

        public TemsTransportBindingElement BindingElement
        {
            get { return bindingElement; }
        }

        public BindingContext BindingContext
        {
            get { return bindingContext; }
        }

        public MessageEncoderFactory EncoderFactory
        {
            get { return encoderFactory; }
        }

        public BufferManager BufferManager
        {
            get { return bufferManager; }
        }

        #endregion

        protected override TChannel OnAcceptChannel(TimeSpan timeout)
        {
            if (currentChannel == null)
            {
                SetCurrentChannel();
            }

            return currentChannel;
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult retval;
            ThrowIfDisposedOrNotOpen();

            if (currentChannel != null)
            {
                result = new WaitAsyncResult(callback, state);
                retval = result;
            }
            else
            {
                IAsyncResult res = onAcceptChannelDel.BeginInvoke(timeout, callback, state);
                retval = res;
            }

            return retval;
        }

        protected override TChannel OnEndAcceptChannel(IAsyncResult result)
        {
            TChannel channel;

            if (result is WaitAsyncResult)
            {
                WaitAsyncResult.End(result);     
                
                if (State == System.ServiceModel.CommunicationState.Opened)
                {
                    SetCurrentChannel();
                }
                else
                {
                    currentChannel = null;
                }

                channel = currentChannel;
            }
            else
            {
                channel = onAcceptChannelDel.EndInvoke(result);
            }

            return channel;
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return OnBeginAcceptChannel(timeout, callback, state);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            TChannel channel = OnEndAcceptChannel(result);
            return channel != null;
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            TChannel channel = OnAcceptChannel(timeout);
            return channel != null;
        }

        public override Uri Uri
        {
            get { return uri; }
        }

        protected override void OnAbort()
        {
            CloseCleanup();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            CloseCleanup();
        }

        private void CloseCleanup()
        {
            if (bufferManager != null)
            {
                bufferManager.Clear();
            }

            currentChannel = null;

            if (result != null)
            {
                result.SetComplete();
                result = null;
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return onCloseDel.BeginInvoke(timeout, callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            onCloseDel.EndInvoke(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsChannelListener.OnOpen - TEMS Channel opened");
            if (HostedTemsTransportConfigurationImpl.transportManager != null)
               HostedTemsTransportConfigurationImpl.transportManager.Register(this);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return onOpenDel.BeginInvoke(timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            onOpenDel.EndInvoke(result);
        }

        /** 
         * <summary>
         *       Creates a new channel if the currentChannel is null.
         * </summary>
        **/
        private void SetCurrentChannel()
        {
            if (currentChannel == null)
            {
                lock (channelLock)
                {
                    if (currentChannel == null)
                    {
                        if (channelType == typeof(IReplyChannel))
                        {
                            currentChannel = (TChannel)(IReplyChannel)new TemsReplyChannel(this);
                        }
                        else if (channelType == typeof(IInputChannel))
                        {
                            currentChannel = (TChannel)(IInputChannel)new TemsInputChannel(this);
                        }
                        else if (channelType == typeof(IDuplexChannel))
                        {
                            currentChannel = (TChannel)(IDuplexChannel)new TemsDuplexChannel(this, null, uri);
                        }
                        else if (channelType == typeof(IReplySessionChannel))
                        {
                            currentChannel = (TChannel)(IReplySessionChannel)new TemsReplySessionChannel(this);
                        }
                        else if (channelType == typeof(IInputSessionChannel))
                        {
                            currentChannel = (TChannel)(IInputSessionChannel)new TemsInputSessionChannel(this);
                        }
                        else if (channelType == typeof(IDuplexSessionChannel))
                        {
                            currentChannel = (TChannel)(IDuplexSessionChannel)new TemsDuplexSessionChannel(this, null, uri);
                        }
                        if (currentChannel != null)
                        {
                            currentChannel.Closed += new EventHandler(OnChannelClosed);
                        }
                    }
                }
            }
        }

        void OnChannelClosed(object sender, EventArgs args)
        {
            TChannel channel = (TChannel)sender;

            lock (channelLock)
            {
                if (channel == currentChannel)
                {
                    currentChannel = null;

                    if (result != null)
                    {
                        if (!result.endCalled)
                        {
                            result.SetComplete();
                        }
                    }
                }
            }
        }

        internal void SessionChannelStarted(object sender)
        {
            OnChannelClosed(sender, null);
        }
    }
}
