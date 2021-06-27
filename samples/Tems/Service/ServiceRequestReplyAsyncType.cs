/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.Threading;
using com.tibco.wcf.tems;
using System.Diagnostics;
using System.ServiceModel;
using System;

namespace com.tibco.sample.service
{
    public class ServiceRequestReplyAsyncType : IServiceRequestReplyAsync, IServiceRequestReply
    {
        private ServiceRequestReply service;
        private delegate string serviceDelegate(string key);
        private serviceDelegate del;

        #region Constructors
        public ServiceRequestReplyAsyncType()
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "Service object created: {0}", this.GetHashCode().ToString());
            service = new ServiceRequestReply();
            del = (key) => ServiceMethod(key);
        }

        #endregion

        #region IServiceRequestReply Members

        public string ServiceMethod(string key)
        {
            // TemsTrace.WriteLine(TraceLevel.Verbose, "ServiceMethod called.");
            return service.ServiceMethod(key);
        }

        #endregion

        #region IServiceRequestReplyAsync Members

        public System.IAsyncResult BeginServiceMethodAsync(string key, System.AsyncCallback callback, object state)
        {
#if ManualAcknowledgeSample
            // need context for ManualAcknowledgeSample
            service.msgProperties = OperationContext.Current.IncomingMessageProperties;
#endif
            // TemsTrace.WriteLine(TraceLevel.Info, "BeginServiceMethodAsync called.");
            System.IAsyncResult retVal = del.BeginInvoke(key, callback, state);
            return retVal;    
        }

        public string EndServiceMethodAsync(System.IAsyncResult result)
        {
            // TemsTrace.WriteLine(TraceLevel.Info, "EndServiceMethodAsync called.");
            string retVal = del.EndInvoke(result);

            return retVal;
        }

        // When a synchronous version of the asynchronous method is available, the service will
        // call the synchronous method by default.
        // public string ServiceMethodAsync(string key)
        // {
        //     //  TemsTrace.WriteLine(TraceLevel.Info, "ServiceMethodAsync called.");
        //     return ServiceMethod(key);
        // }
        #endregion
    }
}
