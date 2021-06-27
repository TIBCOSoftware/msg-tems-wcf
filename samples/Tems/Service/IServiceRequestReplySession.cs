/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.ServiceModel;

namespace com.tibco.sample.service
{
    [ServiceContract(SessionMode=SessionMode.Required)]
    public interface IServiceRequestReplySession
    {
        [OperationContract(IsInitiating=true)]
        string ServiceMethodRequestReplyInitiating(string key);

        [OperationContract(IsInitiating=false, IsTerminating=false)]
        string ServiceMethodRequestReplySession(string key);

        [OperationContract(IsTerminating=true)]
        string ServiceMethodRequestReplyTerminating(string key);
    }
}
