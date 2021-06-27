/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.ServiceModel;

namespace com.tibco.sample.service
{
    [ServiceContract]
    public interface IServiceDatagram
    {
        [OperationContract(IsOneWay = true)]
        void ServiceMethodDatagram(string key);
    }
}
