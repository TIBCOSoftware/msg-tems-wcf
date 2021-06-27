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
using TIBCO.EMS;

namespace com.tibco.wcf.tems
{
    /** <summary>
      *  Provides the base class implementation of the ITemsMessageProtocol interface.
      *  A custom implementation must extend this class.
      * </summary>
      **/
    public class TemsMessageProtocol : ITemsMessageProtocol
    {
        private TemsChannelBase channelBase;
        
        protected TemsTransportBindingElement bindingElement;
        protected BindingContext bindingContext;
        protected MessageEncoder messageEncoder;
        protected BufferManager bufferManager;
        protected Session session;
        protected Destination toDestination;
        protected Destination replyToDestination;
        protected bool isClient;
        protected string TemsMessageIdProperty;
        protected Uri uri;
        protected string baseUri;

        /** <summary>
          *     The System.Text.Encoding (System.Text.UTF8Encoding) instance
          *     configured with the bindingElement.ThrowOnInvalidUTF value.
          *     This can be used in encoding and decoding EMS TextMessages.
          * </summary>
          **/

        protected System.Text.Encoding utf8Encoding;

        /** <summary>
          *     Specifies if an EMS BytesMessage must be used.  The value is set
          *     based on the type of encoder (always true for binaryMessageEncoding)
          *     and the configuration binding messageType setting.
          * </summary>
          **/ 
        protected bool useBytesMessage;
        
        #region Constructors
        public TemsMessageProtocol()
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "Constructor called for ITemsMessageProtocol type: {0} ", this.GetType());
        }
        #endregion

        #region ITemsMessageProtocol Members

        /**  <summary>
          *     Initializes the class from the TemsChannelBase instance.
          *     This will be called a second time for sessionful channels
          *     as the RequestDestination will change to the session Destination.
          * </summary>
          **/
        /// <param name="channelBase">The TemsChannelBase instance for the channel.</param>
        public virtual void Initialize(TemsChannelBase channelBase)
        {
            toDestination = channelBase.msgTransport.RequestDestination;

            if (this.channelBase == null)
            {
                this.channelBase = channelBase;
                bindingElement = channelBase.bindingElement;
                bindingContext = channelBase.bindingContext;
                messageEncoder = channelBase.messageEncoder;
                bufferManager = channelBase.bufferManager;
                session = channelBase.msgTransport.Session;
                replyToDestination = channelBase.msgTransport.ReplyDestination;
                isClient = channelBase.msgTransport.isClient;
                TemsMessageIdProperty = TemsChannelTransport.TemsMessageIdProperty;
                uri = channelBase.msgTransport.uri;
                string[] segments = uri.Segments;

                baseUri = uri.Scheme + System.Uri.SchemeDelimiter + uri.Authority;

                for (int i = 0; i < segments.Length - 1; i++)
                {
                    baseUri += segments[i];
                }

                utf8Encoding = channelBase.msgTransport.utf8Encoding;
                useBytesMessage = channelBase.msgTransport.useBytesMessage;
            }
        }
        
        public virtual System.ServiceModel.Channels.Message Receive(TIBCO.EMS.Message emsMessage, TimeSpan timeout)
        {
            byte[] bytes = null;

            if (emsMessage is TIBCO.EMS.BytesMessage)
            {
                if (!bindingElement.AppHandlesMessageAcknowledge)
                    emsMessage.Acknowledge();

                TIBCO.EMS.BytesMessage bm = (TIBCO.EMS.BytesMessage)emsMessage;
                bytes = new Byte[bm.BodyLength];
                int readBytesCount = bm.ReadBytes(bytes);
                TemsTrace.WriteLine(TraceLevel.Verbose, "TemsMessageProtocol.Receive readBytesCount = {0}", readBytesCount);
            }
            else if (emsMessage is TIBCO.EMS.TextMessage)
            {
                if (!bindingElement.AppHandlesMessageAcknowledge)
                    emsMessage.Acknowledge();

                TIBCO.EMS.TextMessage tm = (TIBCO.EMS.TextMessage)emsMessage;
                bytes = utf8Encoding.GetBytes(tm.Text);
                TemsTrace.WriteLine(TraceLevel.Verbose, "TemsMessageProtocol.Receive tm.Text.Length = {0}", tm.Text.Length);
            }
            else
            {
                TemsTrace.WriteLine(TraceLevel.Error, "Error: TemsChannelTransport received EMS message type: {0} which is not of type BytesMessage or TextMessage.", emsMessage.GetType());
            }

            /** 
             * <summary>
             *      The byte array buffer is copied to one taken from the bufferManager, otherwise
             *       the bufferManager.ReturnBuffer call in encoder.ReadMessage causes the error:
             *       "This buffer cannot be returned to the buffer manager because it is the wrong size."
             *       See: https://web.archive.org/web/20190902052321/https://blogs.msdn.microsoft.com/drnick/2006/05/22/using-the-buffermanager/
             *       for a note regarding this error.
             * </summary>
             **/
            int bytesSize = bytes.Length;

            if (bytesSize > (int)bindingElement.MaxReceivedMessageSize)
            {
                //Fault();
                string msg = String.Format("The received message size ({0}) exceeds the MaxReceivedMessageSize ({1}).", bytesSize, (int)bindingElement.MaxReceivedMessageSize);
                throw (new Exception(msg));
            }

            byte[] bufferBytes = bufferManager.TakeBuffer(bytesSize);

            Array.Copy(bytes, bufferBytes, bytesSize);

            ArraySegment<byte> arraySegment = new ArraySegment<byte>(bufferBytes, 0, bytesSize);
            System.ServiceModel.Channels.Message receiveMessage = messageEncoder.ReadMessage(arraySegment, bufferManager);

            if (bindingElement.AppHandlesMessageAcknowledge)
                receiveMessage.Properties.Add(TemsMessage.key, new TemsMessage(emsMessage));

            return receiveMessage;
        }

        public virtual TIBCO.EMS.Message Send(System.ServiceModel.Channels.Message message, TimeSpan timeout)
        {
            TIBCO.EMS.Message msg;
            ArraySegment<byte> requestData = messageEncoder.WriteMessage(message, (int)bindingElement.MaxReceivedMessageSize, bufferManager);
            byte[] data = new byte[requestData.Count];
            Array.Copy(requestData.Array, requestData.Offset, data, 0, requestData.Count);
            TemsTrace.WriteLine(TraceLevel.Verbose, "MessageProtocolHandlerSend data.Length = {0}", data.Length);

            if (useBytesMessage)
            {
                msg = session.CreateBytesMessage();
                ((TIBCO.EMS.BytesMessage)msg).WriteBytes(data);
            }
            else
            {
                msg = session.CreateTextMessage(utf8Encoding.GetString(data));
            }

            return msg;
        }

        public virtual System.ServiceModel.Channels.Message ReceiveTransform(TIBCO.EMS.Message emsMessage, System.ServiceModel.Channels.Message wcfMessage, TimeSpan timeout)
        {
            return wcfMessage;
        }

        public virtual TIBCO.EMS.Message SendTransform(TIBCO.EMS.Message emsMessage, System.ServiceModel.Channels.Message wcfMessage, TimeSpan timeout)
        {
            return emsMessage;
        }

        #endregion

        #region Transform helper methods

        /** <summary>
          *     Converts the provided Destination to a Tems formatted System.Uri.
          * </summary>
          * <param name="destination"></param>
          * <returns>A System.Uri representing the Destination in the expected Tems format.</returns>
         **/
        protected Uri DestinationToUri(Destination destination)
        {
            string destName;

            if (destination is TIBCO.EMS.Topic)
            {
                destName = ((TIBCO.EMS.Topic)destination).TopicName;
            }
            else
            {
                destName = ((TIBCO.EMS.Queue)destination).QueueName;
            }

            TemsTrace.WriteLine(TraceLevel.Verbose, "replyDestination name = {0}", destName);

            return new Uri(baseUri + destName);
        }

        /** <summary>
          *     Retrieves the TIBCO.EMS.Destination corresponding to the provided replyUri.
          * </summary>
          * <param name="replyUri"></param>
          * <returns>A TIBCO.EMS.Destination corresponding to the given uri.</returns>
          **/ 
        protected Destination DestinationForUri(Uri replyUri)
        {
            MessageProducer producer = channelBase.msgTransport.CreateReplyMessageProducer(replyUri);

            return producer.Destination;
        }

        /** <summary>
          *     Converts the provided Destination to a System.ServiceModel.EndpointAddress with a
          *     Tems formatted uri.
          * </summary>
          * <param name="destination"></param>
          * <returns>A System.ServiceModel.EndpointAddress representing the Destination
          * in the expected Tems format.  Returns null if Destination is null.</returns>
          **/ 
        protected EndpointAddress DestinationToEndpointAddress(Destination destination)
        {
            EndpointAddress retval = null;

            if (destination != null)
            {
                retval = new EndpointAddress(DestinationToUri(destination));
            }

            return retval;
        }

        /** <summary>
          * For a WCF client to an EMS Soap over JMS service, the original
          * WCF request message messageId cannot be set on the EMS message.
          * When an emsMessage is sent, if the TemsMessageIdProperty is set
          * this value is stored in a Hashmap using the emsMessage.MessageID
          * as the key and the value of the TemsMessageIdProperty as the value.
          * This method can be called to to retrieve the correlated WCF messageId
          * value for the given emsMessageId value.
          * </summary>
          * <param name="emsMessageId"></param>
          * <returns>The correlated WCF messageId string if emsMessageId key is found, otherwise null.</returns>
          **/ 
        protected string getCorrelatedMessageId(string emsMessageId)
        {
            return channelBase.msgTransport.getCorrelatedMessageId(emsMessageId);
        }

        #endregion
    }
}
