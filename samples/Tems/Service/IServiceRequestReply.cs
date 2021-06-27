/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.ServiceModel;

namespace com.tibco.sample.service
{
    [ServiceContract(ConfigurationName = "ConfigurationName.Sample.RequestReply", SessionMode = SessionMode.NotAllowed, Name = "IServiceRequestReply", Namespace = "com.tibco.sample.namespace")]
    public interface IServiceRequestReply
    {
        [OperationContract(Action = "com.tibco.sample.namespace/IServiceRequestReply/Sample.Action.Id.String")]
        string ServiceMethod(string key);
    }
}
