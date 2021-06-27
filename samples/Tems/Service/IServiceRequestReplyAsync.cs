/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.ServiceModel;
using System;

namespace com.tibco.sample.service
{
    [ServiceContract]
    public interface IServiceRequestReplyAsync
    {
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginServiceMethodAsync(string key, AsyncCallback callback, object state);

        string EndServiceMethodAsync(IAsyncResult result);

        // When a synchronous version of the asynchronous method is available, the service will
        // call the synchronous method by default.
        //[OperationContract]
        //string ServiceMethodAsync(string key);
    }
}
