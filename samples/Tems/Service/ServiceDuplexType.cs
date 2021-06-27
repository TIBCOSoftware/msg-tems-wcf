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
    public class ServiceDuplexType : IServiceDuplex
    {
        private ServiceDuplex service;

        #region Constructors
        public ServiceDuplexType()
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "Service object created: {0}", this.GetHashCode().ToString());
            service = new ServiceDuplex();
        }

        #endregion

        #region IServiceDuplex Members

        public void ServiceMethodOne(string key)
        {
#if ManualAcknowledgeSample
            // need context for ManualAcknowledgeSample
            service.msgProperties = OperationContext.Current.IncomingMessageProperties;
#endif
            service.ServiceMethodOne(key);
        }

        public void ServiceMethodTwo(string key)
        {
#if ManualAcknowledgeSample
            // need context for ManualAcknowledgeSample
            service.msgProperties = OperationContext.Current.IncomingMessageProperties;
#endif
            service.ServiceMethodTwo(key);
        }

        public void ServiceMethodThree(string key)
        {
#if ManualAcknowledgeSample
            // need context for ManualAcknowledgeSample
            service.msgProperties = OperationContext.Current.IncomingMessageProperties;
#endif
            service.ServiceMethodThree(key);
        }
        #endregion
    }
}
