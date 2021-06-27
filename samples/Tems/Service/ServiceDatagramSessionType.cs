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
    public class ServiceDatagramSessionType : IServiceDatagramSession
    {
        private ServiceDatagram service;

        #region Constructors
        public ServiceDatagramSessionType()
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "Service object created: {0}", this.GetHashCode().ToString());
            service = new ServiceDatagram();
        }

        #endregion

        #region IServiceDatagramSession Members

        public void ServiceMethodDatagramInitiating(string key)
        {
#if ManualAcknowledgeSample
            // need context for ManualAcknowledgeSample
            service.msgProperties = OperationContext.Current.IncomingMessageProperties;
#endif
            service.ServiceMethodDatagram(key);
        }

        public void ServiceMethodDatagramSession(string key)
        {
#if ManualAcknowledgeSample
            // need context for ManualAcknowledgeSample
            service.msgProperties = OperationContext.Current.IncomingMessageProperties;
#endif
            service.ServiceMethodDatagram(key);
        }

        public void ServiceMethodDatagramTerminating(string key)
        {
#if ManualAcknowledgeSample
            // need context for ManualAcknowledgeSample
            service.msgProperties = OperationContext.Current.IncomingMessageProperties;
#endif
            service.ServiceMethodDatagram(key);
        }
        #endregion
    }
}
