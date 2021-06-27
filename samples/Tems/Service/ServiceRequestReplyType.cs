/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.Diagnostics;
using System.ServiceModel;
using com.tibco.wcf.tems;

namespace com.tibco.sample.service
{
    [ServiceBehavior(Namespace = "com.tibco.sample.namespace", InstanceContextMode=InstanceContextMode.PerSession)]
    public class ServiceRequestReplyType : IServiceRequestReply
    {
        int count;
        private ServiceRequestReply service;
    
        #region Constructors
        public ServiceRequestReplyType()
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "Service object created: {0}", this.GetHashCode().ToString());
            service = new ServiceRequestReply();
        }
    
        #endregion
    
        #region IServiceRequestReply Members
    
        public string ServiceMethod(string key)
        {
#if ManualAcknowledgeSample
            // need context for ManualAcknowledgeSample
            service.msgProperties = OperationContext.Current.IncomingMessageProperties;
#endif
            count++;
            // Uncomment below and observe affect of changing:
            //    InstanceContextMode.PerCall
            //    InstanceContextMode.PerSession (default if not set)
            //    InstanceContextMode.Single
            //System.Console.WriteLine("count = " + count);
            string retVal = service.ServiceMethod(key);
            return retVal;
        }
        #endregion
    }
}
