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
using System.Transactions;

namespace com.tibco.sample.service
{
    public class ServiceDuplexTransaction
    {
#if ManualAcknowledgeSample
        public System.ServiceModel.Channels.MessageProperties msgProperties;
#endif
        public ServiceDuplexTransaction()
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "ServiceDuplexTransaction instance created.");
        }

        public void ServiceTransactionMethodOne(string key)
        {
            string transactionId = Transaction.Current.TransactionInformation.DistributedIdentifier.ToString();
            TemsTrace.WriteLine(TraceLevel.Verbose, "service.ServiceTransactionMethodOne(key) called, key = {0}, transactionId = {1}", key, transactionId);
            // throw new Exception("Test exception");
#if ManualAcknowledgeSample
            // ManualAcknowledgeSample
            SendAcknowledge();
#endif
            Callback.CallbackTransactionMethodOne(String.Format("{0}, service transactionId = {1}", key.ToUpper(), transactionId));
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public void ServiceTransactionMethodTwo(string key)
        {
            string transactionId = Transaction.Current.TransactionInformation.DistributedIdentifier.ToString();
            TemsTrace.WriteLine(TraceLevel.Verbose, "service.ServiceTransactionMethodTwo(key) called, key = {0}, transactionId = {1}", key, transactionId);
            // throw new Exception("Test exception");
#if ManualAcknowledgeSample
            // ManualAcknowledgeSample
            SendAcknowledge();
#endif
            Callback.CallbackTransactionMethodTwo(String.Format("{0}, service transactionId = {1}", key.ToUpper(), transactionId));
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public void ServiceTransactionMethodThree(string key)
        {
            string transactionId = Transaction.Current.TransactionInformation.DistributedIdentifier.ToString();
            TemsTrace.WriteLine(TraceLevel.Verbose, "service.ServiceTransactionMethodThree(key) called, key = {0}, transactionId = {1}", key, transactionId);
            // throw new Exception("Test exception");
#if ManualAcknowledgeSample
            // ManualAcknowledgeSample
            SendAcknowledge();
#endif
            Callback.CallbackTransactionMethodThree(String.Format("{0}, service transactionId = {1}", key.ToUpper(), transactionId));
        }

        IServiceDuplexTransactionCallback Callback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IServiceDuplexTransactionCallback>();
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
