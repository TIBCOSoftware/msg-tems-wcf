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
                        CallbackContract = typeof(IServiceDuplexCallback))]
    public interface IServiceDuplexSession
    {
        [OperationContract(IsOneWay = true, IsInitiating = true)]
        void ServiceMethodDuplexInitiating(string key);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void ServiceMethodDuplexSession(string key);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void ServiceMethodThreeDuplexSession(string key);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void ServiceMethodDuplexTerminating(string key);
    }
}
