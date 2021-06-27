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
    public class TemsChannelFactory<TChannel> : ChannelFactoryBase<TChannel>, ITemsChannelManager
    {
        private TemsTransportBindingElement bindingElement;
        private BindingContext bindingContext;
        private MessageEncoderFactory encoderFactory;
        private BufferManager bufferManager;

        private delegate void OnOpenDelegate(TimeSpan timeout);
        private OnOpenDelegate onOpenDel;
        private Type channelType;

        public TemsChannelFactory(TemsTransportBindingElement bindingElement, BindingContext bindingContext)
            : base(bindingContext.Binding)
        {
            onOpenDel = (t) => OnOpen(t);
            this.bindingElement = bindingElement;
            this.bindingContext = bindingContext;
            this.bufferManager = BufferManager.CreateBufferManager(bindingElement.MaxBufferPoolSize, (int)bindingElement.MaxReceivedMessageSize);

            MessageEncodingBindingElement encodingBindingElement = bindingContext.BindingParameters.Remove<MessageEncodingBindingElement>();

            if (encodingBindingElement != null)
            {
                encoderFactory = encodingBindingElement.CreateMessageEncoderFactory();
            }
            else
            {
                // See TemsTransportBindingElement T GetProperty<T>(BindingContext context).
                encoderFactory = new BinaryMessageEncodingBindingElement().CreateMessageEncoderFactory();
            }

            //this.uri = new Uri(bindingContext.ListenUriBaseAddress, bindingContext.ListenUriRelativeAddress);
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

        /*
         * For all session channels created below: If the request for a session
         * Destination times out then the session is aborted and a
         * System.ServiceModel.CommunicationException is thrown.
         * A client attempting to create this channel should catch and handle 
         * the exception.
         *
         */
        protected override TChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            TemsTrace.WriteLine(TraceLevel.Info, "TemsChannelFactory.OnCreateChannel called, typeof(TChannel) = {0}", typeof(TChannel));
            TChannel channel = default(TChannel);

            if (channelType == typeof(IRequestChannel))
            {
                channel = (TChannel)(IRequestChannel)new TemsRequestChannel(this, address, via);
            }
            else if (channelType == typeof(IOutputChannel))
            {
                channel = (TChannel)(IOutputChannel)new TemsOutputChannel(this, address, via);
            }
            else if (channelType == typeof(IDuplexChannel))
            {
                channel = (TChannel)(IDuplexChannel)new TemsDuplexChannel(this, address, via);
            }
            else if (channelType == typeof(IRequestSessionChannel))
            {
                TemsRequestSessionChannel sessionChannel = new TemsRequestSessionChannel(this, address, via);
                channel = (TChannel)(IRequestSessionChannel)sessionChannel;
            }
            else if (channelType == typeof(IOutputSessionChannel))
            {
                TemsOutputSessionChannel sessionChannel = new TemsOutputSessionChannel(this, address, via);
                channel = (TChannel)(IOutputSessionChannel)sessionChannel;
            }
            else if (channelType == typeof(IDuplexSessionChannel))
            {
                TemsDuplexSessionChannel sessionChannel = new TemsDuplexSessionChannel(this, address, via);
                channel = (TChannel)(IDuplexSessionChannel)sessionChannel;
            }

            return channel;
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return onOpenDel.BeginInvoke(timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            onOpenDel.EndInvoke(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsChannelFactory.OnOpen called.");
        }
    }
}
