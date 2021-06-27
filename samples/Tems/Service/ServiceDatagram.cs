/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using com.tibco.wcf.tems;
using System.Diagnostics;

namespace com.tibco.sample.service
{
    public class ServiceDatagram
    {
#if ManualAcknowledgeSample
        public System.ServiceModel.Channels.MessageProperties msgProperties;
#endif

        public ServiceDatagram()
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "ServiceDatagram instance created.");
        }

        public void ServiceMethodDatagram(string key)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "service.ServiceMethodDatagram(key) called, key = {0}", key);
            key.ToUpper();
#if ManualAcknowledgeSample
            // ManualAcknowledgeSample
            SendAcknowledge();
#endif
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
