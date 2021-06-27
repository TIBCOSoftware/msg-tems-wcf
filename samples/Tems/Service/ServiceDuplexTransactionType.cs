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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall,
                 ConcurrencyMode = ConcurrencyMode.Reentrant,
                 ReleaseServiceInstanceOnTransactionComplete = false)]
    public class ServiceDuplexTransactionType : IServiceDuplexTransaction
    {
        private ServiceDuplexTransaction service;

        #region Constructors
        public ServiceDuplexTransactionType()
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "Service object created: {0}", this.GetHashCode().ToString());
            service = new ServiceDuplexTransaction();
        }

        #endregion

        #region IServiceDuplexTransaction Members

        [OperationBehavior(TransactionScopeRequired = true)]
        public void ServiceTransactionMethodOne(string key)
        {
#if ManualAcknowledgeSample
            // need context for ManualAcknowledgeSample
            service.msgProperties = OperationContext.Current.IncomingMessageProperties;
#endif
            service.ServiceTransactionMethodOne(key);
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public void ServiceTransactionMethodTwo(string key)
        {
#if ManualAcknowledgeSample
            // need context for ManualAcknowledgeSample
            service.msgProperties = OperationContext.Current.IncomingMessageProperties;
#endif
            service.ServiceTransactionMethodTwo(key);
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public void ServiceTransactionMethodThree(string key)
        {
#if ManualAcknowledgeSample
            // need context for ManualAcknowledgeSample
            service.msgProperties = OperationContext.Current.IncomingMessageProperties;
#endif
            service.ServiceTransactionMethodThree(key);
        }
        #endregion
    }
}
