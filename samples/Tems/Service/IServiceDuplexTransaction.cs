/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.ServiceModel;

namespace com.tibco.sample.service
{
    [ServiceContract(SessionMode = SessionMode.Required, 
                        CallbackContract = typeof(IServiceDuplexTransactionCallback))]
    public interface IServiceDuplexTransaction
    {
        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Mandatory)]
        void ServiceTransactionMethodOne(string key);

        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Mandatory)]
        void ServiceTransactionMethodTwo(string key);

        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Mandatory)]
        void ServiceTransactionMethodThree(string key);
    }

    public interface IServiceDuplexTransactionCallback
    {
        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Mandatory)]
        void CallbackTransactionMethodOne(string key);

        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Mandatory)]
        void CallbackTransactionMethodTwo(string key);

        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Mandatory)]
        void CallbackTransactionMethodThree(string key);
    }
}
