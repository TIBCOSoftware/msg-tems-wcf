/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using com.tibco.wcf.tems;
using System.Diagnostics;
using System;

namespace com.tibco.sample.service
{
    public class ServiceRequestReply
    {
#if ManualAcknowledgeSample
        public System.ServiceModel.Channels.MessageProperties msgProperties;
#endif

        public ServiceRequestReply()
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "ServiceRequestReply instance created.");
        }

        public string ServiceMethod(string key)
        {
            // TemsTrace.WriteLine(TraceLevel.Verbose, "service.ServiceMethod(key) called, key = {0}", key);
            // throw new Exception("Test exception");
#if ManualAcknowledgeSample
            // ManualAcknowledgeSample
            SendAcknowledge();
#endif

            return key.StartsWith("size=") ? 
                        new string('x', Convert.ToInt32(key.Substring(5))) : 
                        key.ToUpper();
        }

#if ManualAcknowledgeSample
        private void SendAcknowledge()
        {
            object msgProperty = null;
            TemsMessage temsMessage = null;
            if (msgProperties.TryGetValue(TemsMessage.key, out msgProperty))
            {
                temsMessage = (TemsMessage)msgProperty;
                temsMessage.Acknowledge();
            }
        }
#endif
    }
}
