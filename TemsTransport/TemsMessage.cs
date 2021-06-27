/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using TIBCO.EMS;

namespace com.tibco.wcf.tems
{
    public class TemsMessage
    {
        public const string key = "TemsMessageProperty";
        private Message emsMessage;

        internal TemsMessage(Message aEMSMessage)
        {
            emsMessage = aEMSMessage;
        }

        public void Acknowledge()
        {
            emsMessage.Acknowledge();
        }
    }
}
