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
using System.Threading;
using TIBCO.EMS;

namespace com.tibco.wcf.tems
{
    public class TemsChannelTransport : IExceptionListener
    {
        private const string RequestSessionProperty = "JMS_TIBCO_TEMS_SESSION_NEW";
        private const string NotifyCloseSessionProperty = "JMS_TIBCO_TEMS_SESSION_CLOSE";
        internal const string TemsMessageIdProperty = "JMS_TIBCO_TEMS_MESSAGEID";
        internal static readonly string WcfToProperty = "JMS_TIBCO_TEMS_WCF_TO";
        private static System.Collections.Queue sessionRequestMessageQueue;
        static TemsChannelTransport()
        {
            sessionRequestMessageQueue = System.Collections.Queue.Synchronized(new System.Collections.Queue());
        }

        public const string TOPIC = "topic/";
        public const string QUEUE = "queue/";

        internal bool isClient;
        private TemsTransportBindingElement bindingElement;
        internal Uri uri;
        private string serverUrl;
        private string destType;
        private string destAddr;
        private bool isTopic;
        private bool isClosed;

        private static int receiveCount;
        private static int sessionsCount;

        private Connection connection;
        private Session session;
        private Hashtable replyMessageProducers;
        private Hashtable messageIdCorrelations;
        private Destination requestDestination;
        private Destination replyDestination;
        
        internal Destination RequestDestination
        {
            get { return requestDestination; }
        }

        internal Destination ReplyDestination
        {
            get { return replyDestination; }
        }

        private EndpointAddress replyToAddress;
        private MessageProducer requestMessageProducer;
        private MessageProducer duplexSessionCallbackMessageProducer;
        private string channelSessionId;
        private MessageConsumer requestMessageConsumer;
        private MessageConsumer replyMessageConsumer;

        // clientBaseAddress specifies the EMS destination for Duplex MEP callback messages.
        private EndpointAddress clientBaseAddress;

        private TemsChannelBase channel;
        private Object receiveLock;
        internal bool useBytesMessage;
        internal System.Text.Encoding utf8Encoding;
        private TemsTimeoutTimer replyProducerCheckTimer;
        
        internal TemsChannelTransport(TemsChannelBase channel, bool isClient, Uri uri)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsChannelTransport constructed, channel = {0}, isClient = {1}, uri = {2}", channel, isClient, uri);
            this.channel = channel;
            this.isClient = isClient;
            this.uri = uri;
            bindingElement = isClient && channel.bindingElement.connection != null ? 
                                (TemsTransportBindingElement)channel.bindingElement.Clone() : channel.bindingElement;

            /** 
             * Note: An administrated callback Destination or non-temporary Destination cannot be used
             * with TemsDuplexSessionChannel because this Destination is used to receive the
             * result of the session Destination request and it also needs to support the
             * sessionful guarantees.
             **/ 

            if (channel is TemsDuplexChannel && !(channel is TemsDuplexSessionChannel))            
            {
                this.clientBaseAddress = ((TemsDuplexChannel)channel).ClientBaseAddress;
            }

            serverUrl = "tcp" + Uri.SchemeDelimiter + uri.Authority;

            /**
             * Note: To enable reconnection behavior and fault tolerance, the serverURL parameter must
             * be a comma-separated list of two or more URLs. In a situation with only one server,
             * you may supply two copies of that server’s URL to enable client reconnection
             * (for example, tcp://localhost:7222,tcp://localhost:7222).
             **/

            if (bindingElement.ReconnAttemptCount > 0)
            {
                serverUrl += "," + serverUrl;
            }

            string[] segments; ;
            segments = uri.Segments;
            destAddr = segments[segments.Length - 1];
            destType = segments[segments.Length - 2];

            this.isTopic = destType.Equals(TemsChannelTransport.TOPIC,
                                                StringComparison.OrdinalIgnoreCase);

            replyMessageProducers = new Hashtable();
            messageIdCorrelations = new Hashtable();

            InitializeSession();
            receiveLock = new Object();

            /**
             * Note:  An EMS BytesMessage is only used if one of the two conditions below is true.
             * Otherwise an EMS TextMessage is used.
             **/ 
            useBytesMessage = channel.messageEncoder.GetType().Name.Equals("BinaryMessageEncoder") || 
                                bindingElement.MessageType == TemsMessageType.Bytes;
            utf8Encoding = new System.Text.UTF8Encoding(true, bindingElement.ThrowOnInvalidUTF);
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsChannelTransport useBytesMessage = {0}", useBytesMessage);
        }
        
        private void InitializeSession()
        {
            if (bindingElement.connection != null)
            {
                connection = bindingElement.connection;
            }
            else
            {
                if (bindingElement.AppManagesConnections)
                {
                    throw new Exception("AppManagesConnections is true but Connection is Null on the TEMS binding");
                }
                else
                {
                    CreateConnection();
                    bindingElement.connection = connection;
                }
            }

            CreateSession();

            if (bindingElement.IsPropertySet("EndpointDestination"))
            {
                requestDestination = bindingElement.EndpointDestination;
            }

            if (requestDestination == null)
            {
                requestDestination = isTopic ?
                                    (Destination)session.CreateTopic(destAddr) :
                                    (Destination)session.CreateQueue(destAddr);
            }
            
            if (isClient)
            {
                requestMessageProducer = CreateProducer(requestDestination);
                TemsTrace.WriteLine(TraceLevel.Verbose, "TemsChannelTransport channel type: {0}", channel.GetType());
                /**
                 * A reply Destination is not created for a Datagram MEP if it is sessionless.
                 * The reply Destination is required for a sessionful Datagram to establish the
                 * session Destination, but is closed immediately after the session Destination is created.
                 **/ 

                if (!(channel is TemsOutputChannel) || (channel is TemsOutputSessionChannel))
                {
                    TemsTrace.WriteLine(TraceLevel.Verbose, "TemsChannelTransport channel type (is not TemsOutputChannel):  {0}", channel.GetType());
                    CreateReplyDestination();
                    replyMessageConsumer = session.CreateConsumer(replyDestination);
                }
            }
            else
            {
                if (bindingElement.MessageSelector == String.Empty)
                    requestMessageConsumer = session.CreateConsumer(requestDestination);
                else
                    requestMessageConsumer = session.CreateConsumer(requestDestination, bindingElement.MessageSelector);

                replyProducerCheckTimer = new TemsTimeoutTimer(bindingElement.ReplyDestCheckInterval);

                if (clientBaseAddress != null)
                {
                    if (bindingElement.IsPropertySet("CallbackDestination"))
                    {
                        replyDestination = bindingElement.CallbackDestination;
                    }

                    if (replyDestination != null)
                    {
                        MessageProducer replyMessageProd = CreateProducer(replyDestination);
                        string destAddress = AddressForDestination(replyDestination);
                        Uri callbackUri = new Uri(destAddress);
                        replyToAddress = new EndpointAddress(callbackUri);
                        clientBaseAddress = replyToAddress;
                        ((TemsDuplexChannel)channel).ClientBaseAddress = clientBaseAddress;
                        ((TemsDuplexChannel)channel).remoteAddress = clientBaseAddress;
                        string[] segments = callbackUri.Segments;
                        string replyDestName = segments[segments.Length - 1];
                        string type = segments[segments.Length - 2];
                        string replyDestAddress = type + replyDestName;
                        replyMessageProducers.Add(replyDestAddress, replyMessageProd);
                    }
                }
            }
        }

        /** <summary>
         *  Creates the Connection that will be used for this transport channel.
         *  
         *  If the application has set the bindingElement ConnectionFactory property then this is
         *  used to create the connection.  
         *  
         *  Otherwise a new ConnectionFactory is created.  When a new ConnectionFactory is
         *  created, it is configured from the properties set on the TemsTransportBindingElement.
         *  </summary>
         **/  
        private void CreateConnection()
        {
            if (connection == null)
            {
                ConnectionFactory factory;

                if (bindingElement.IsPropertySet("ConnectionFactory"))
                {
                    factory = bindingElement.ConnectionFactory;
                }
                else
                {
                    if (bindingElement.ServerUrl.Length == 0)
                    {
                        factory = new TIBCO.EMS.ConnectionFactory(serverUrl);
                    }
                    else
                    {
                        factory = new TIBCO.EMS.ConnectionFactory();
                    }

                    bindingElement.InitializeConnectionFactory(factory);
                }
                
                TemsTrace.WriteLine(TraceLevel.Info, "ConnectionFactory: {0}", factory.ToString());
                connection = factory.CreateConnection();
                TemsTrace.WriteLine(TraceLevel.Verbose, "EMS connection created to serverUrl = {0}", serverUrl);
                connection.ExceptionListener = this;
                connection.Start();
            }
        }

        private void CreateSession()
        {
            if (session == null)
            {
                TIBCO.EMS.SessionMode sessionAcknowledgeMode = bindingElement.SessionAcknowledgeMode;

                session = connection.CreateSession(false, sessionAcknowledgeMode);
                isClosed = false;
            }
        }

        internal void Send(Message emsMessage, Uri replyUri)
        {            
            MessageProducer msgProducer;

            if (replyUri == null)
            {
                msgProducer = requestMessageProducer;
            }
            else
            {
                if (channel is TemsDuplexSessionChannel && !isClient)
                {
                    msgProducer = duplexSessionCallbackMessageProducer;
                }
                else
                {
                    msgProducer = CreateReplyMessageProducer(replyUri);
                }
            }

            emsMessage.SetBooleanProperty("JMS_TIBCO_COMPRESS", channel.bindingElement.MessageCompression);
            msgProducer.Send(emsMessage);
            
            string temsMessageId = emsMessage.GetStringProperty(TemsMessageIdProperty);

            if (temsMessageId != null)
            {
                messageIdCorrelations.Add(emsMessage.MessageID, temsMessageId);
            }
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
        internal string getCorrelatedMessageId(string emsMessageId)
        {
            string retval = null;

            if (messageIdCorrelations.ContainsKey(emsMessageId))
            {
                retval = (string) messageIdCorrelations[emsMessageId];
                messageIdCorrelations.Remove(emsMessageId);
            }

            return retval;
        }

        private Message CreateMessage(byte[] data)
        {
            Message msg;

            if (useBytesMessage)
            {
                msg = session.CreateBytesMessage();
                ((BytesMessage)msg).WriteBytes(data);
            }
            else
            {
                msg = session.CreateTextMessage(utf8Encoding.GetString(data));
            }

            return msg;
        }

        internal Session Session
        {
            get { return session; }
        }

        internal Message Receive(TemsTimeoutTimer timer)
        {
            lock (receiveLock)
            {
                return ReceiveMain(timer);
            }
        }

        private Message ReceiveMain(TemsTimeoutTimer timer)
        {            
            Message emsMessage = null;
            int rcInc = Interlocked.Increment(ref receiveCount);

            if (!isClient && rcInc % 1000 == 0)
            {
                int sc = sessionsCount;

                if (sc > 0)
                {
                    int queueCount = sessionRequestMessageQueue.Count;

                    if (queueCount > 0)
                    {
                        TemsTrace.WriteLine(TraceLevel.Info, "Msgs Received: {0}, sessionsCount: {1}, pending sessions: {2}", rcInc, sc, queueCount);
                    }
                    else
                    {
                        TemsTrace.WriteLine(TraceLevel.Info, "Msgs Received: {0}, sessionsCount: {1}", rcInc, sc);
                    }
                }
                else
                {
                    TemsTrace.WriteLine(TraceLevel.Info, "Msgs Received: {0}", rcInc);
                }
            }
             
            // Receive the message
            TemsTrace.WriteLine(TraceLevel.Verbose, "Waiting on EMS Receive call...");

            if (isClient)
            {
                emsMessage = replyMessageConsumer.Receive(timer.MillisecondsToExpire(true));
            }
            else
            {
                emsMessage = channelSessionId == null && sessionRequestMessageQueue.Count > 0 ?
                        (Message)sessionRequestMessageQueue.Dequeue() :
                        requestMessageConsumer.Receive((long)timer.MillisecondsToExpire(true));

                if (emsMessage != null)
                {
                    if (emsMessage.GetBooleanProperty(RequestSessionProperty))
                    {
                        emsMessage.Acknowledge();

                        if (channel.IsSessionful)
                        {
                            CreateSessionDestination(emsMessage.ReplyTo);
                            channel.MessageProtocol.Initialize(channel);
                            /**
                             * Test session.SessID
                             * TemsTrace.WriteLine(TraceLevel.Info, "Receiving on new session SessID: {0}, State: {1}", session.SessID, channel.State);
                             *
                             * Continue listening on the newly created session destination.
                             * Since the receive is called on a newly created session, the
                             * original timeout value is used.
                            **/
                            emsMessage = requestMessageConsumer.Receive((long)timer.Timeout.TotalMilliseconds);
                        }
                        else
                        {
                            // Send EMS message with null ReplyTo to notify client of failed session request.
                            Uri uri = new Uri(AddressForDestination(emsMessage.ReplyTo));
                            Message sessionIdMsg = CreateMessage(new Byte[0]);
                            Send(sessionIdMsg, uri);
                            throw new System.ServiceModel.CommunicationException("A sessionless endpoint received a session request message.");
                        }
                    }
                    else if (channel.IsSessionful && channelSessionId == null)
                    {
                        throw new System.ServiceModel.CommunicationException("A sessionful endpoint received a message that is not a session request message.");
                    }
                    else if (emsMessage.GetBooleanProperty(NotifyCloseSessionProperty))
                    {
                        emsMessage.Acknowledge();
                        channel.Close();
                        emsMessage = null;
                    }
                }
            }

            TemsTrace.WriteLine(TraceLevel.Verbose, "EMS Receive returned a message.");

            if (emsMessage == null)
            {
                /**
                 * Test session.SessID
                 * if (session != null)
                 * {
                 *    TemsTrace.WriteLine(TraceLevel.Verbose, "emsMessage == null with session: {0}", session.SessID);
                 * }
                 * else
                 * {
                 *     TemsTrace.WriteLine(TraceLevel.Verbose, "emsMessage == null with session: Null");
                 * }
                 **/ 
                // The msg can be null for timeout or another thread closes consumer.
                timer.ThrowIfExpired();
                bool channelClosingClosed = channel.State == CommunicationState.Closing ||
                                            channel.State == CommunicationState.Closed;
                bool sessionClosingClosed = false;

                /**
                 *  Note: If CloseOutputSession has been called on a TemsDuplexSessionChannel channel
                 *       then the input channel pending receive is interrupted by the consumer being
                 *       closed (by TemsChannelTransport.Close()).  This is not an error condition.
                 *       This condition is detected by checking the state of the outputChannel.
                 **/       

                if (channel is TemsDuplexSessionChannel && ((TemsDuplexSessionChannel)channel).isClient)
                {
                    CommunicationState outputChannelState = ((TemsDuplexSessionChannel)channel).outputChannel.State;
                    sessionClosingClosed = outputChannelState == CommunicationState.Closing ||
                                            outputChannelState == CommunicationState.Closed;
                }

                if (!(channelClosingClosed || sessionClosingClosed))
                {
                    TemsTrace.WriteLine(TraceLevel.Error, "Error: TemsChannelTransport received message is null.");
                }
            }

            return emsMessage;
        }

        public void OnException(EMSException e)
        {
            String errorCode = e.ErrorCode;

            if (errorCode.StartsWith("FT-SWITCH:"))
            {
                /**
                 * Note: For an FT-SWITCH EMSException the ErrorCode value will be in the form:
                 * errorCode: "FT-SWITCH: tcp://localhost:7224"
                 * Log the message and allow the channel to continue normally.
                 * The log message should look something like:
                 *    Connection has performed fault-tolerant switch to tcp://localhost:7224
                 **/    
                TemsTrace.WriteLine(TraceLevel.Error, e.Message);
            }
            else
            {
                HandleException("TemsChannelTransport Exception:", e);
            }
        }

        internal MessageProducer CreateReplyMessageProducer(Uri replyUri)
        {
            string[] segments = replyUri.Segments;
            string replyDestName = segments[segments.Length - 1];
            string type = segments[segments.Length - 2];
            string replyDestAddress = type + replyDestName;
            MessageProducer replyMessageProd = null;

            lock (replyMessageProducers)
            {
                if (replyMessageProducers.ContainsKey(replyDestAddress))
                {
                    replyMessageProd = (MessageProducer)replyMessageProducers[replyDestAddress];
                }
                else
                {
                    Destination replyDest = type.Equals(TOPIC, StringComparison.OrdinalIgnoreCase) ?
                                            (Destination)session.CreateTopic(replyDestName) :
                                            (Destination)session.CreateQueue(replyDestName);
                    replyMessageProd = CreateProducer(replyDest);
                    replyMessageProducers.Add(replyDestAddress, replyMessageProd);
                }

                if (replyProducerCheckTimer.IsExpired())
                {
                    replyProducerCheckTimer.Reset();
                    ArrayList removeKeys = new ArrayList();

                    foreach (DictionaryEntry item in replyMessageProducers)
                    {
                        MessageProducer producer = (MessageProducer)item.Value;
                        Destination replyDest = producer.Destination;

                        try
                        {
                            MessageProducer temp = session.CreateProducer(replyDest);
                            temp.Close();
                        }
                        catch (Exception)
                        {
                            producer.Close();
                            removeKeys.Add(item.Key);
                        }
                    }

                    if (removeKeys.Count > 0)
                    {
                        TemsTrace.WriteLine(TraceLevel.Info, "Closing {0} MessageProducer instances (the temporary reply Destination no longer exists indicating the client channel is closed).", removeKeys.Count);
                    }

                    foreach (object key in removeKeys)
                    {
                        replyMessageProducers.Remove(key);
                    }
                }
            }

            return replyMessageProd;
        }

        internal EndpointAddress ReplyToAddress
        {
            get { return replyToAddress; }
        }

        internal bool IsClosed
        {
            get { return isClosed; }
        }

        /** 
         * <summary>
         *      Creates the temporary destination for the reply for a request-reply MEP.
         *  </summary>
         **/  
        private void CreateReplyDestination()
        {
            if (replyDestination == null)
            {
                // If clientBaseAddress is set, this is a sessionless Duplex Service callback address.
                if (clientBaseAddress != null)
                {
                    if (bindingElement.IsPropertySet("CallbackDestination"))
                    {
                        replyDestination = bindingElement.CallbackDestination;
                    }

                    if (replyDestination == null)
                    {
                        string[] segments = clientBaseAddress.Uri.Segments;
                        string destAddr = segments[segments.Length - 1];
                        string type = segments[segments.Length - 2];
                        replyDestination = type.Equals(TOPIC, StringComparison.OrdinalIgnoreCase) ?
                                        (Destination)session.CreateTopic(destAddr) :
                                        (Destination)session.CreateQueue(destAddr);
                        replyToAddress = clientBaseAddress;
                    }
                    else
                    {
                        SetReplyToAddress();
                        clientBaseAddress = replyToAddress;
                        ((TemsDuplexChannel)channel).ClientBaseAddress = clientBaseAddress;
                    }
                }
                else
                {
                    replyDestination = isTopic ? 
                                        (Destination)session.CreateTemporaryTopic() : 
                                        (Destination)session.CreateTemporaryQueue();
                    SetReplyToAddress();
                }
            }
        }

        private void SetReplyToAddress()
        {
            SetReplyToAddress(replyDestination);
        }

        private void SetReplyToAddress(Destination destination)
        {
            string destAddress = AddressForDestination(destination);
            TemsTrace.WriteLine(TraceLevel.Verbose, "replyDestination address = {0}", destAddress);
            replyToAddress = new EndpointAddress(new Uri(destAddress));
        }

        private string AddressForDestination(Destination destination)
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
            
            string[] segments = uri.Segments;
            string baseUri = uri.Scheme + System.Uri.SchemeDelimiter + uri.Authority;

            for (int i = 0; i < segments.Length - 1; i++)
            {
                baseUri += segments[i];
            }

            return baseUri + destName;
        }

        internal void Close()
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsChannelTransport Close called.");

            /**
             * Test session.SessID
             * TemsTrace.WriteLine(TraceLevel.Info, "TemsChannelTransport Close called SessID: {0}, State: {1}", session.SessID, channel.State);
             **/ 
            if (!isClosed)
            {
                isClosed = true;
 
                /**
                 * Test session.SessID
                 * TemsTrace.WriteLine(TraceLevel.Info, "channelSessionId = {0}, sessionsCount = {1}", channelSessionId, sessionsCount);
                 **/ 
                if ((channelSessionId == null && sessionsCount == 0) || isClient)
                {
                    /**
                     * Test session.SessID
                     * TemsTrace.WriteLine(TraceLevel.Info, "Close connection with session: {0}", session.SessID);
                     **/ 
                    if (!bindingElement.AppManagesConnections)
                    {
                        connection.Close();
                        bindingElement.connection = null;
                    }

                    connection = null;
                    session = null;
                }
                else
                {
                    /**
                     * Test session.SessID
                     * TemsTrace.WriteLine(TraceLevel.Info, "Close session: {0}", session.SessID);
                     **/ 

                    session.Close();
                    session = null;

                    if (channelSessionId != null)
                    {
                        Interlocked.Decrement(ref sessionsCount);
                        /**
                         * Note: If EMS connection is not closed, the Session
                         *  temporary Destinations need to be deleted.
                         **/
                        DeleteSessionDestination();
                    }
                }
            }
        }

        /**
         * <summary>
         *  Deletes the temporary replyDestination created for a sessionful client. 
         *  </summary>
         **/  
        private void DeleteReplyDestination()
        {
            /**
             * Note: If clientBaseAddress is set, then there is no temporary destination to delete.
             * Datagram: Exception is thrown while closing the client.
             * replyDestination is null if Datagram.
             **/ 
            if (replyDestination != null && isClient && clientBaseAddress == null)
            {
                replyMessageConsumer.Close();
                if (isTopic)
                {
                    ((TemporaryTopic)replyDestination).Delete();
                }
                else
                {
                    ((TemporaryQueue)replyDestination).Delete();
                }
            }
        }

        private void DeleteSessionDestination()
        {
            /**
             * Note: The requestMessageConsumer must be closed before calling Delete().
             *       Otherwise Delete() throws an EMSException.
             **/
            if (isTopic)
            {
                if (requestDestination is TIBCO.EMS.TemporaryTopic)
                {
                    ((TemporaryTopic)requestDestination).Delete();
                }
            }
            else
            {
                if (requestDestination is TIBCO.EMS.TemporaryQueue)
                {
                    ((TemporaryQueue)requestDestination).Delete();
                }
            }
        }

        internal string RequestSessionDestination(TimeSpan timeout)
        {
            return RequestSessionDestinationMain(new TemsTimeoutTimer(timeout));
        }

        private string RequestSessionDestinationMain(TemsTimeoutTimer timer)
        {
            string retval = null;
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsChannelTransport.RequestSessionDestination called.");

            try
            {
                Message requestSessionMsg = CreateMessage(new Byte[0]);
                requestSessionMsg.ReplyTo = replyDestination;
                requestSessionMsg.SetBooleanProperty(RequestSessionProperty, true);
                requestSessionMsg.SetStringProperty(WcfToProperty, uri.OriginalString);
                requestMessageProducer.Send(requestSessionMsg);
                // Receive the result of the session request.
                Message msg = replyMessageConsumer.Receive(timer.MillisecondsToExpire());

                /** Note:  For the sessionful Datagram MEP, the reply Destination and MessageConsumer
                 * are used only to establish the session Destination and are closed here after
                 * the session Destination reply message has been received.
                 **/

                if (channel is TemsOutputSessionChannel)
                {
                    msg.Acknowledge();
                    replyMessageConsumer.Close();
                    DeleteReplyDestination();
                }
                
                if (msg == null)
                {
                    string errMsg = "TemsChannelTransport.RequestSessionDestination(): Session Destination request failed.  This may result from a timeout or a communication error.";
                    TemsTrace.WriteLine(TraceLevel.Error, errMsg);
                    channel.Abort();
                    throw new System.ServiceModel.CommunicationException(errMsg);
                }
                else
                {
                    if (!(channel is TemsOutputSessionChannel))
                    {
                        msg.Acknowledge();
                    }

                    // Swaps the request Destination and Producer for the client.
                    requestDestination = msg.ReplyTo;
                    retval = msg.CorrelationID;

                    /**
                     * Validation test.
                     * string destName = requestDestination is TIBCO.EMS.Queue ?
                     *    ((TIBCO.EMS.Queue)requestDestination).QueueName :
                     *    ((TIBCO.EMS.Topic)requestDestination).TopicName;
                     * bool validate = retval.Equals(destName);
                     **/ 
                    requestMessageProducer.Close();

                    if (requestDestination != null)
                    {
                        requestMessageProducer = CreateProducer(requestDestination);
                    }
                    else
                    {
                        throw new System.ServiceModel.CommunicationException("Session request failed.  Ensure the service endpoint is sessionful.");
                    }
                }
            }
            catch (Exception e)
            {
                HandleException("Error requesting session Destination:", e);
            }

            return retval;
        }

        /**
         * <summary>
         *  Creates a MessageProducer for the given destination and assigns the properties
         *  from the binding element.
         *  </summary>
         *  <param name="destination">The destination the producer sends messages to.</param>
         *  <returns>A configured MessageProducer.</returns>
         **/  
        private MessageProducer CreateProducer(Destination destination)
        {
            MessageProducer producer = session.CreateProducer(destination);
            producer.MsgDeliveryMode = bindingElement.MessageDeliveryMode;
            producer.DisableMessageID = bindingElement.DisableMessageID;
            producer.DisableMessageTimestamp = bindingElement.DisableMessageTimestamp;
            producer.Priority = bindingElement.Priority;
            producer.TimeToLive = bindingElement.TimeToLive;

            return producer;
        }

        private void CreateSessionDestination(Destination replyDestination)
        {
            /**
             * <summary>
             * Reads all pending requests from the requestMessageConsumer and 
             * adds these to the static sessionRequestMessageQueue before
             * closing the MessageConsumer.
             * This is needed because prefetch may be in use and this
             * MessageConsumer may have read a number of pending session requests
             * from the EMS server that it has prefetched and are locally cached.
             * These requests are retained in the order received in the static
             * sessionRequestMessageQueue.
             * </summary>
             **/ 

            while (true)
            {
                Message msg = requestMessageConsumer.ReceiveNoWait();

                if (msg == null)
                {
                    requestMessageConsumer.Close();
                    break;
                }
                else
                {
                    msg.Acknowledge();
                    sessionRequestMessageQueue.Enqueue(msg);
                }
            }

            Type channelType = channel.GetType();

            if (channelType == typeof(TemsReplySessionChannel))
            {
                TemsChannelListener<System.ServiceModel.Channels.IReplySessionChannel> channelListener = (TemsChannelListener<System.ServiceModel.Channels.IReplySessionChannel>) channel.ManagerBase;
                channelListener.SessionChannelStarted(channel);
                SetReplyToAddress(replyDestination);
            }
            else if (channelType == typeof(TemsInputSessionChannel))
            {
                TemsChannelListener<System.ServiceModel.Channels.IInputSessionChannel> channelListener = (TemsChannelListener<System.ServiceModel.Channels.IInputSessionChannel>) channel.ManagerBase;
                channelListener.SessionChannelStarted(channel);
            }
            else if (channelType == typeof(TemsDuplexSessionChannel))
            {
                TemsChannelListener<System.ServiceModel.Channels.IDuplexSessionChannel> channelListener = (TemsChannelListener<System.ServiceModel.Channels.IDuplexSessionChannel>) channel.ManagerBase;
                channelListener.SessionChannelStarted(channel);
            }
            
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsChannelTransport.CreateSessionDestination called.");

            try
            {
                if (isTopic)
                {
                    requestDestination = session.CreateTemporaryTopic();
                    channelSessionId = ((TIBCO.EMS.Topic)requestDestination).TopicName;
                }
                else
                {
                    requestDestination = session.CreateTemporaryQueue();
                    channelSessionId = ((TIBCO.EMS.Queue)requestDestination).QueueName;
                }

                /**
                 * Note: Set Session.Id before attempting to create sessionReplyProducer.  If the
                 *       replyDestination is not valid, the resulting channel Abort() call on the
                 *       session channel uses the Session.Id value as a test before calling
                 *       msgTransport.CloseSessionDestination().  If this is not called it results
                 *       in rapidly created temporary queues that are not closed until the process
                 *       terminates.
                 **/       
                if (channelType == typeof(TemsReplySessionChannel))
                {
                    ((TemsInputSession)((TemsReplySessionChannel)channel).Session).Id = channelSessionId;
                    // Test session.SessID
                    //TemsTrace.WriteLine(TraceLevel.Info, "TemsReplySessionChannel created with channelSessionId: {0}, SessID: {1}", channelSessionId, Session.SessID);
                }
                else if (channelType == typeof(TemsInputSessionChannel))
                {
                    ((TemsInputSession)((TemsInputSessionChannel)channel).Session).Id = channelSessionId;
                    // Test session.SessID
                    //TemsTrace.WriteLine(TraceLevel.Info, "TemsInputSessionChannel created with channelSessionId: {0}, SessID: {1}", channelSessionId, Session.SessID);
                }
                else if (channelType == typeof(TemsDuplexSessionChannel))
                {
                    ((TemsDuplexSessionChannel)channel).SetSessionId(channelSessionId);
                    // Test session.SessID
                    //TemsTrace.WriteLine(TraceLevel.Info, "TemsDuplexSessionChannel created with channelSessionId: {0}, SessID: {1}", channelSessionId, Session.SessID);
                }

                Interlocked.Increment(ref sessionsCount);

                if (bindingElement.MessageSelector == String.Empty)
                    requestMessageConsumer = session.CreateConsumer(requestDestination);
                else
                    requestMessageConsumer = session.CreateConsumer(requestDestination, bindingElement.MessageSelector);

                // Sends a reply EMS message to the RequestSession EMS message that triggered this call.

                MessageProducer sessionReplyProducer = CreateProducer(replyDestination);
                Message sessionIdMsg = CreateMessage(new Byte[0]);
                sessionIdMsg.ReplyTo = requestDestination;
                sessionIdMsg.CorrelationID = channelSessionId;

                sessionReplyProducer.Send(sessionIdMsg);

                if (channel is TemsDuplexSessionChannel)
                {
                    duplexSessionCallbackMessageProducer = sessionReplyProducer;
                    /**
                     * <summary>
                     * Set the actual sessionful callback address.
                     * If this does not match the request ReplyTo value then an error like this is raised:
                     * IErrorHandler HandleError called: The request message has
                     * ReplyTo='net.tems://localhost:7222/queue/$TMP$.EMS-SERVER.76C48EE6271444F.1' but
                     * IContextChannel.RemoteAddress is 'net.tems://localhost:7222/queue/Tems.DuplexTransactionEP.callback'.
                     * When ManualAddressing is false, these values must be the same, null, or
                     * EndpointAddress.AnonymousAddress because sending a reply to a different address
                     * than the original sender can create a security risk. If you want to process such
                     * messages, enable ManualAddressing.
                     * </summary>
                     **/
                    ((TemsDuplexSessionChannel)channel).outputChannel.remoteAddress = new EndpointAddress(AddressForDestination(duplexSessionCallbackMessageProducer.Destination));
                }
                else
                {
                    sessionReplyProducer.Close();
                }
            }
            catch (Exception e)
            {
                HandleException("Error creating session Destination: ", e);
            }
        }

        internal void NotifyCloseSession(TimeSpan timeout)
        {
            NotifyCloseSessionMain();
        }

        internal void NotifyCloseSession()
        {
            NotifyCloseSessionMain();
        }

        private void NotifyCloseSessionMain()
        {
            try
            {
                Message closeSessionMsg = CreateMessage(new Byte[0]);
                closeSessionMsg.SetBooleanProperty(NotifyCloseSessionProperty, true);
                requestMessageProducer.Send(closeSessionMsg);
            }
            catch (Exception e)
            {
                HandleException("Exception sending CloseSession notification: ", e);
            }
        }

        private void HandleException(string msg, Exception e)
        {
            bindingElement.connection = null;
            channel.Abort();
            string fullMsg = String.Format("{0} {1}", msg, e.Message);
            TemsTrace.WriteLine(TraceLevel.Error, fullMsg + "\n" + e.StackTrace);
            throw new System.ServiceModel.CommunicationException(fullMsg, e);
        }
    }
}
