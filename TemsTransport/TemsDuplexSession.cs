/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Diagnostics;
using System.ServiceModel.Channels;

namespace com.tibco.wcf.tems
{
    class TemsDuplexSession : IDuplexSession
    {
        private string sessionId;
        private TemsDuplexSessionChannel channel;

        private delegate void CloseOutputSessionDelegate(TimeSpan timeout);
        private CloseOutputSessionDelegate del;

        public TemsDuplexSession(string id, TemsDuplexSessionChannel channel)
        {
            del = (t) => CloseOutputSession(t);
            sessionId = id;
            this.channel = channel;
        }
        
        #region ISession Members

        public string Id
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        #endregion

        #region IDuplexSession Members

        public System.IAsyncResult BeginCloseOutputSession(System.TimeSpan timeout, System.AsyncCallback callback, object state)
        {
            return del.BeginInvoke(timeout, callback, state);
        }

        public System.IAsyncResult BeginCloseOutputSession(System.AsyncCallback callback, object state)
        {
            TimeSpan timeout = channel.outputChannel.bindingContext.Binding.CloseTimeout;
            return del.BeginInvoke(timeout, callback, state);
        }

        public void CloseOutputSession(System.TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Info, "TemsDuplexSession.CloseOutputSession called.");
            channel.outputChannel.Close(timeout);
        }

        public void CloseOutputSession()
        {
            TimeSpan timeout = channel.outputChannel.bindingContext.Binding.CloseTimeout;
            CloseOutputSession(timeout);
        }

        public void EndCloseOutputSession(System.IAsyncResult result)
        {
            del.EndInvoke(result);
        }

        #endregion
    }
}
