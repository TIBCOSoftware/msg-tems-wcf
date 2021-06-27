/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Collections;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace com.tibco.wcf.tems
{
    public class TemsChannelBase : ChannelBase
    {
        internal TemsChannelTransport msgTransport;
        internal TemsTransportBindingElement bindingElement;
        internal BindingContext bindingContext;
        internal MessageEncoder messageEncoder;
        internal BufferManager bufferManager;

        protected Hashtable correlatedMessages;

        private delegate void OnOpenDelegate(TimeSpan timeout);
        private OnOpenDelegate onOpenDel;
        private delegate void OnCloseDelegate(TimeSpan timeout);
        private OnCloseDelegate onCloseDel;
        private bool isSessionful;
        private ITemsMessageProtocol messageProtocol;
        private object messageProtocolSendLock;
        private object messageProtocolReceiveLock;
        
        #region Constructors
        internal TemsChannelBase(ChannelManagerBase channelManager)
            : base(channelManager)
        {
            onOpenDel = (t) => OnOpen(t);
            onCloseDel = (t) => OnClose(t);

            isSessionful = this is IRequestSessionChannel ||
                           this is IReplySessionChannel ||
                           this is IDuplexSessionChannel ||
                           this is IInputSessionChannel ||
                           this is IOutputSessionChannel;
            
            bindingElement = ((ITemsChannelManager)channelManager).BindingElement;
            bindingContext = ((ITemsChannelManager)channelManager).BindingContext;

            // For sessionful channels the encoder is set using EncoderFactory.CreateSessionEncoder().
            this.messageEncoder = isSessionful ?
                ((ITemsChannelManager)channelManager).EncoderFactory.CreateSessionEncoder() :
                ((ITemsChannelManager)channelManager).EncoderFactory.Encoder;

            bufferManager = ((ITemsChannelManager)channelManager).BufferManager;
            correlatedMessages = new Hashtable();
            messageProtocolSendLock = new Object();
            messageProtocolReceiveLock = new Object();
        }
        #endregion

        internal ChannelManagerBase ManagerBase
        {
            get { return Manager; }
        }

        internal bool IsSessionful
        {
            get { return isSessionful; }
        }

        internal ITemsMessageProtocol MessageProtocol
        {
            get { return messageProtocol; }

            set { messageProtocol = value; }
        }

        private void CreateMessageProtocol()
        {
            if (bindingElement.MessageProtocol == TemsMessageProtocolType.WCFNative)
            {
                messageProtocol = new TemsMessageProtocol();
            }
            else if (bindingElement.MessageProtocol == TemsMessageProtocolType.TIBCOSoapOverJMS2004)
            {
                messageProtocol = new TemsMessageProtocolSoapOverJMS();
            }
            else if (bindingElement.MessageProtocol == TemsMessageProtocolType.Custom)
            {
                if (!bindingElement.AllowCustomMessageProtocol)
                {
                    throw new Exception("The TemsTransportBindingElement AllowCustomMessageProtocol is false but the MessageProtocol is set to Custom.");
                }

                if (bindingElement.CustomMessageProtocolType.Length == 0)
                {
                    throw new Exception("MessageProtocol is set to Custom but CustomMessageProtocolType is not set.");
                }

                Type type = Type.GetType(bindingElement.CustomMessageProtocolType);

                if (type == null)
                {
                    throw new Exception("MessageProtocol is set to Custom but CustomMessageProtocolType is not a valid type.");
                }

                if (!(type.IsSubclassOf(typeof(TemsMessageProtocol))))
                {
                    throw new Exception("MessageProtocol is set to Custom but CustomMessageProtocolType is not a subclass of TemsMessageProtocol.");
                }

                messageProtocol = (ITemsMessageProtocol)type.GetConstructor(new Type[0]).Invoke(new object[0]);
            }

            if (bindingElement.AppHandlesMessageAcknowledge == true)
            {
                if (!(bindingElement.SessionAcknowledgeMode == TIBCO.EMS.SessionMode.ClientAcknowledge ||
                    bindingElement.SessionAcknowledgeMode == TIBCO.EMS.SessionMode.ExplicitClientAcknowledge ||
                    bindingElement.SessionAcknowledgeMode == TIBCO.EMS.SessionMode.ExplicitClientDupsOkAcknowledge))
                {
                    throw new Exception("AppHandlesMessageAcknowledge is set to true but SessionAcknowledgeMode is not ClientAcknowledge nor ExplicitClientAcknowledge nor ExplicitClientDupsOkAcknowledge.");
                }
            }

            messageProtocol.Initialize(this);
        }

        protected void CreateTemsChannelTransport(bool isClient, Uri via)
        {
            try
            {
                msgTransport = new TemsChannelTransport(this, isClient, via);
                CreateMessageProtocol();
            }
            catch (Exception e)
            {
                Abort();
                string msg = String.Format("Exception creating TemsChannelTransport: {0}", e.Message);
                Exception exWrapper = new CommunicationException(msg, e);
                TemsTrace.WriteLine(TraceLevel.Error, msg + "\n" + e.StackTrace);
                throw exWrapper;
            }                
        }
        
        #region EMS Request Processing

        protected Message EmsReceive(UniqueId messageId, TemsTimeoutTimer timer)
        {
            Message correlatedMsg = null;
            try
            {
                bool msgFound = false;

                while (!msgFound)
                {
                    TemsTrace.WriteLine(TraceLevel.Verbose, "Request correlatedMessages");

                    lock (correlatedMessages)
                    {
                        TemsTrace.WriteLine(TraceLevel.Verbose, "Holding correlatedMessages lock");

                        if (correlatedMessages.ContainsKey(messageId))
                        {
                            correlatedMsg = (Message)correlatedMessages[messageId];
                            correlatedMessages.Remove(messageId);
                            msgFound = true;
                        }
                        else
                        {
                            Message msg = EmsReceive(timer);
                            UniqueId relatesTo = msg.Headers.RelatesTo;

                            if (messageId.Equals(relatesTo))
                            {
                                correlatedMsg = msg;
                                msgFound = true;
                            }
                            else if (relatesTo == null)
                            {
                                correlatedMsg = msg;
                                msgFound = true;
                            }
                            else
                            {
                                if (correlatedMessages.ContainsKey(relatesTo))
                                {
                                    TemsTrace.WriteLine(TraceLevel.Verbose, "correlatedMessages key value exists for msg.Headers.Action: {0}", msg.Headers.Action);
                                }
                                else
                                {
                                    correlatedMessages.Add(relatesTo, msg);
                                }
                            }
                        }

                        TemsTrace.WriteLine(TraceLevel.Verbose, "Release correlatedMessages lock");
                    }
                }
            }
            catch (TimeoutException e)
            {
                TemsTrace.WriteLine(TraceLevel.Verbose, "TemsChannelBase.EmsReceive has timed out.");
                throw e; 
            }
            catch (CommunicationException e)
            {
                throw e;    
            }
            catch (Exception e)
            {
                HandleException("Exception receiving EMS message: ", e);
            }

            return correlatedMsg;
        }

        protected Message EmsReceive(TimeSpan timeout)
        {
            return EmsReceive(new TemsTimeoutTimer(timeout));
        }

        protected Message EmsReceive(TemsTimeoutTimer timer)
        {
            Message receiveMessage = null;
            try
            {
                TIBCO.EMS.Message emsMessage = msgTransport.Receive(timer);

                if (emsMessage == null)
                {
                    if (msgTransport.IsClosed)
                    {
                        TemsTrace.WriteLine(TraceLevel.Info, "TemsChannelTransport is closed.");
                    }
                    else
                    {
                        timer.ThrowIfExpired();
                        throw new Exception("The service did not respond.");
                    }
                }
                else
                {
                    lock (messageProtocolReceiveLock)
                    {
                        receiveMessage = MessageProtocol.Receive(emsMessage, timer.CurrentTimeout(true));
                        receiveMessage = MessageProtocol.ReceiveTransform(emsMessage, receiveMessage, timer.CurrentTimeout(true));
                    }
                }
            }
            catch (TimeoutException e)
            {
                TemsTrace.WriteLine(TraceLevel.Verbose, "TemsChannelBase.EmsReceive has timed out.");
                throw e;
            }
            catch (Exception e)
            {
                HandleException("Exception receiving EMS message: ", e);
            }

            TemsTrace.WriteLine(TraceLevel.Verbose, "===<<<EmsReceive:");
            TemsTrace.WriteLine(TraceLevel.Verbose, "\n\n{0}", receiveMessage);

            return receiveMessage;
        }

        internal void EmsSend(Message message, Uri replyUri, TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, replyUri == null ? "===>>>EmsSend:" : "===>>>EmsReply:");

            try
            {
                TemsTrace.WriteLine(TraceLevel.Verbose, "\n\n{0}", message);
                TIBCO.EMS.Message emsMessage = null;

                lock (messageProtocolSendLock)
                {
                    emsMessage = MessageProtocol.Send(message, timeout);

                    if (msgTransport.isClient)
                    {
                         emsMessage.SetStringProperty(TemsChannelTransport.WcfToProperty, message.Headers.To.AbsoluteUri);
                    }

                    emsMessage = MessageProtocol.SendTransform(emsMessage, message, timeout);
                }

                msgTransport.Send(emsMessage, replyUri);
            }
            catch (TimeoutException e)
            {
                TemsTrace.WriteLine(TraceLevel.Verbose, "TemsChannelBase.EmsSendMain has timed out.");
                throw e;
            }
            catch (Exception e)
            {
                HandleException(String.Format("Exception sending {0}message:", replyUri == null ? "" : "reply "), e); 
            }
        }

        #endregion

        private void HandleException(string msg, Exception e)
        {
            if (e.GetType() == typeof(TimeoutException))
            {
                throw e;
            }

            this.bindingElement.connection = null;
            Abort();
            string fullMsg = String.Format("{0} {1}", msg, e.Message);
            TemsTrace.WriteLine(TraceLevel.Error, fullMsg + "\n" + e.StackTrace);
            throw new System.ServiceModel.CommunicationException(fullMsg, e);
        }

        protected void HandleReceiveException(string msg, Exception e)
        {
            if (!(State == CommunicationState.Closing || State == CommunicationState.Closed))
            {
                TemsTrace.WriteLine(TraceLevel.Error, "{0} exception: {1}", msg, e.Message);
            }

            if (e.GetType() == typeof(CommunicationException))
            {
                this.bindingElement.connection = null;
                throw e;
            }
        }

        #region ChannelBase overrides

        protected override void OnAbort()
        {
            TemsTrace.WriteLine(TraceLevel.Info, "TemsChannelBase.OnAbort() called.");

            if (msgTransport != null)
            {
                msgTransport.Close();
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return onCloseDel.BeginInvoke(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return onOpenDel.BeginInvoke(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            msgTransport.Close();
            TemsTrace.WriteLine(TraceLevel.Info, "TemsChannelBase.OnClose() called.");
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            onCloseDel.EndInvoke(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            onOpenDel.EndInvoke(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Info, "TemsChannelBase({0}).OnOpen called.", this.GetType().Name);
        }
        #endregion

        #region CommunicationObject overrides
        
        protected override void OnClosed()
        {
            base.OnClosed();
        }

        protected override void OnClosing()
        {
            base.OnClosing();
        }

        protected override void OnFaulted()
        {
            base.OnFaulted();
        }

        protected override void OnOpened()
        {
            base.OnOpened();
        }

        protected override void OnOpening()
        {
            base.OnOpening();
        }

        #endregion

        #region IChannel Members

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(FaultConverter))
            {
                return FaultConverter.GetDefaultFaultConverter(this.messageEncoder.MessageVersion) as T;
            }
            return base.GetProperty<T>();
        }

        #endregion
    }
}
