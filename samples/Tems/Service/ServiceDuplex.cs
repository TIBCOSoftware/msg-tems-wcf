/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.ServiceModel;
using com.tibco.wcf.tems;
using System.Diagnostics;
using System;

namespace com.tibco.sample.service
{
    public class ServiceDuplex
    {
#if ManualAcknowledgeSample
        public System.ServiceModel.Channels.MessageProperties msgProperties;
#endif
        public ServiceDuplex()
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "ServiceDuplex instance created.");
        }

        public void ServiceMethodOne(string key)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "service.ServiceMethodOne(key) called, key = {0}", key);
            // throw new Exception("Test exception");
#if ManualAcknowledgeSample
            // ManualAcknowledgeSample
            SendAcknowledge();
#endif
            Callback.CallbackMethodOne(key.ToUpper());
        }

        public void ServiceMethodTwo(string key)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "service.ServiceMethodTwo(key) called, key = {0}", key);
            // throw new Exception("Test exception");
#if ManualAcknowledgeSample
            // ManualAcknowledgeSample
            SendAcknowledge();
#endif
            Callback.CallbackMethodTwo(key.ToUpper());
        }

        public void ServiceMethodThree(string key)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "service.ServiceMethodThree(key) called, key = {0}", key);
            // throw new Exception("Test exception");
#if ManualAcknowledgeSample
            // ManualAcknowledgeSample
            SendAcknowledge();
#endif
            Callback.CallbackMethodThree(key.ToUpper());
        }

        IServiceDuplexCallback Callback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IServiceDuplexCallback>();
            }
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
