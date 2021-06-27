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
    class TemsDuplexSessionChannel : TemsDuplexChannel, IDuplexSessionChannel
    {
        private TemsDuplexSession session;
        private bool automaticInputSessionShutdown;

        public TemsDuplexSessionChannel(ChannelManagerBase channelManager,
                                        EndpointAddress remoteAddress,
                                        Uri via)
            : base(channelManager, remoteAddress, via)
        {
            if (isClient)
            {
                inputChannel = new TemsInputChannel(channelManager, this, isClient);
                outputChannel = new TemsOutputSessionChannel(channelManager, this, isClient, remoteAddress, via);
                string sessionId = ((TemsOutputSessionChannel)outputChannel).Session.Id;
                session = new TemsDuplexSession(sessionId, this);
            }
            else
            {
                inputChannel = new TemsInputSessionChannel(channelManager, this, isClient);
                outputChannel = new TemsOutputChannel(channelManager, this, isClient, null, via);
                session = new TemsDuplexSession("", this);
            }

            inputChannel.MessageProtocol = MessageProtocol;
            outputChannel.MessageProtocol = MessageProtocol;
            TemsTrace.WriteLine(TraceLevel.Info, "TemsDuplexSessionChannel constructed.");
            automaticInputSessionShutdown = true;
        }

        #region ISessionChannel<IDuplexSession> Members

        public IDuplexSession Session
        {
	        get { return session; }
        }

        #endregion

        internal void SetSessionId(string sessionId)
        {
            ((TemsInputSession)((TemsInputSessionChannel)inputChannel).Session).Id = sessionId;
            ((TemsDuplexSession)Session).Id = sessionId;
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsDuplexSessionChannel Session.Id set to: {0}", sessionId);
        }

        public bool AutomaticInputSessionShutdown
        {
            get { return automaticInputSessionShutdown; }
            set { automaticInputSessionShutdown = value; }
        }
    }
}
