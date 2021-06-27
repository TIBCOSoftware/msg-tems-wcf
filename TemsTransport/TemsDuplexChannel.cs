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
    class TemsDuplexChannel : TemsChannelBase, IDuplexChannel
    {
        internal EndpointAddress remoteAddress;
        private Uri via;
        internal TemsInputChannel inputChannel;
        internal TemsOutputChannel outputChannel;
        internal bool isClient;
        // clientBaseAddress specifies the EMS destination for Duplex MEP callback messages.
        private EndpointAddress clientBaseAddress;
        
        public TemsDuplexChannel(ChannelManagerBase channelManager,
                                    EndpointAddress remoteAddress, 
                                    Uri via)
            : base(channelManager)
        {
            this.remoteAddress = remoteAddress != null ? remoteAddress : ClientBaseAddress;
            this.via = via;
            isClient = remoteAddress != null;
            CreateTemsChannelTransport(isClient, via);
            TemsTrace.WriteLine(TraceLevel.Verbose, "clientBaseAddress = {0}", clientBaseAddress);

            /** 
             * Note: If this is sessionful, the sessionful version of the input and output channel is
             * created in the sessionful extension of this class.
             **/ 

            if (!(this is TemsDuplexSessionChannel))
            {
                inputChannel = new TemsInputChannel(channelManager, this, isClient);
                outputChannel = new TemsOutputChannel(channelManager, this, isClient, this.remoteAddress, via);
                inputChannel.MessageProtocol = MessageProtocol;
                outputChannel.MessageProtocol = MessageProtocol;
            }

            TemsTrace.WriteLine(TraceLevel.Info, "TemsDuplexChannel constructed.");
        }

        internal EndpointAddress ClientBaseAddress
        {
            get
            {
                // Lazy initialize.
                if (clientBaseAddress == null)
                {
                    string baseUri;
                    string baseAddress = bindingElement.ClientBaseAddress;
                    if (baseAddress.Length > 0)
                    {
                        if (baseAddress.StartsWith(bindingElement.Scheme))
                        {
                            // The full uri path is specified in the binding attribute.
                            baseUri = baseAddress;
                        }
                        else
                        {
                            Uri uri = via != null ? via : ((ChannelListenerBase)Manager).Uri;
                            // The relative uri path is specified in the binding attribute.
                            string segmentDelimiter = baseAddress.StartsWith("/") ? "" : "/";
                            baseUri = uri.Scheme + Uri.SchemeDelimiter + uri.Authority + segmentDelimiter + baseAddress;
                        }
                    }
                    else
                    {
                        // The uri path is not specified in the binding attribute.  Use the default.
                        Uri uri = via != null ? via : ((ChannelListenerBase)Manager).Uri;
                        baseUri = uri.ToString() + ".callback";
                    }
                    clientBaseAddress = new EndpointAddress(baseUri);
                }

                return clientBaseAddress;
            }

            set { clientBaseAddress = value; }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            outputChannel.Open(timeout);
            inputChannel.Open(timeout);
            base.OnOpen(timeout);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            /** Note: The output channel needs to be closed first because for the sessionful Duplex
              * closing the output channel sends a NotifyCloseSession which depends on the
              * EMS transport session, which is closed when the input channel is closed.
              **/ 
            outputChannel.Close(timeout);
            inputChannel.Close(timeout);
            base.OnClose(timeout);
        }

        #region IInputChannel Members

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return inputChannel.BeginReceive(timeout, callback, state);
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return inputChannel.BeginReceive(callback, state);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return inputChannel.BeginTryReceive(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return inputChannel.BeginWaitForMessage(timeout, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            return inputChannel.EndReceive(result);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            return inputChannel.EndTryReceive(result, out message);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return inputChannel.EndWaitForMessage(result);
        }

        public EndpointAddress LocalAddress
        {
            get { return inputChannel.LocalAddress; }
        }

        public Message Receive(TimeSpan timeout)
        {
            return inputChannel.Receive(timeout);
        }

        public Message Receive()
        {
            return inputChannel.Receive();
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            return inputChannel.TryReceive(timeout, out message);
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return inputChannel.WaitForMessage(timeout);
        }

        #endregion

        #region IOutputChannel Members

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return outputChannel.BeginSend(message, callback, state);
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return outputChannel.BeginSend(message, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            outputChannel.EndSend(result);
        }

        public EndpointAddress RemoteAddress
        {
            get { return outputChannel.RemoteAddress; }
        }

        public void Send(Message message, TimeSpan timeout)
        {
            outputChannel.Send(message, timeout);
        }

        public void Send(Message message)
        {
            outputChannel.Send(message);
        }

        public Uri Via
        {
            get { return outputChannel.Via; }
        }

        #endregion
    }
}
