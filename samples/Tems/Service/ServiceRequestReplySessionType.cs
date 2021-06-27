/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using com.tibco.wcf.tems;
using System.Diagnostics;
using System.ServiceModel;
namespace com.tibco.sample.service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ServiceRequestReplySessionType : IServiceRequestReplySession
    {
        int count;

        private ServiceRequestReply service;

        #region Constructors
        public ServiceRequestReplySessionType()
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "Service object created: {0}", this.GetHashCode().ToString());
            service = new ServiceRequestReply();
        }

        #endregion

        #region IServiceRequestReplySession Members

        public string ServiceMethodRequestReplyInitiating(string key)
        {
            count++;
#if ManualAcknowledgeSample
            // need context for ManualAcknowledgeSample
            service.msgProperties = OperationContext.Current.IncomingMessageProperties;
#endif

            // Uncomment below and observe affect of changing:
            //    InstanceContextMode.PerCall
            //    InstanceContextMode.PerSession (default if not set)
            //    InstanceContextMode.Single
            //System.Console.WriteLine("count = " + count);
            return service.ServiceMethod(key);
        }

        public string ServiceMethodRequestReplySession(string key)
        {
#if ManualAcknowledgeSample
            // need context for ManualAcknowledgeSample
            service.msgProperties = OperationContext.Current.IncomingMessageProperties;
#endif
            return service.ServiceMethod(key);
        }

        public string ServiceMethodRequestReplyTerminating(string key)
        {
#if ManualAcknowledgeSample
            // need context for ManualAcknowledgeSample
            service.msgProperties = OperationContext.Current.IncomingMessageProperties;
#endif
            return service.ServiceMethod(key);
        }
        #endregion
    }
}
