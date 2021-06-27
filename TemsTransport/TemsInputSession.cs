/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.ServiceModel.Channels;

namespace com.tibco.wcf.tems
{
    class TemsInputSession : IInputSession
    {
        private string sessionId;

        public TemsInputSession(string id)
        {
            sessionId = id;
        }
        
        #region ISession Members

        public string Id
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        #endregion
    }
}
