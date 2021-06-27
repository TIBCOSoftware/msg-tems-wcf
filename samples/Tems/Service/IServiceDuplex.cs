/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.ServiceModel;

namespace com.tibco.sample.service
{
    [ServiceContract(SessionMode = SessionMode.NotAllowed,
                        CallbackContract = typeof(IServiceDuplexCallback))]
    public interface IServiceDuplex
    {
        [OperationContract(IsOneWay = true)]
        void ServiceMethodOne(string key);

        [OperationContract(IsOneWay = true)]
        void ServiceMethodTwo(string key);

        [OperationContract(IsOneWay = true)]
        void ServiceMethodThree(string key);
    }

    public interface IServiceDuplexCallback
    {
        [OperationContract(IsOneWay = true)]
        void CallbackMethodOne(string key);

        [OperationContract(IsOneWay = true)]
        void CallbackMethodTwo(string key);

        [OperationContract(IsOneWay = true)]
        void CallbackMethodThree(string key);
    }
}
