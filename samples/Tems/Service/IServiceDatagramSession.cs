/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.ServiceModel;

namespace com.tibco.sample.service
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IServiceDatagramSession
    {
        [OperationContract(IsOneWay = true, IsInitiating = true)]
        void ServiceMethodDatagramInitiating(string key);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void ServiceMethodDatagramSession(string key);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void ServiceMethodDatagramTerminating(string key);
    }
}
