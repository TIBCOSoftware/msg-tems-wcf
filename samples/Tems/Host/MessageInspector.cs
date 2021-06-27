/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using com.tibco.wcf.tems;
using System.Diagnostics;

namespace com.tibco.sample.host
{
    class MessageInspector : IDispatchMessageInspector
    {
        #region IDispatchMessageInspector Members

        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
        {
            // Note that the string concatenation is done regardless of whether or not the Console is active with
            // the line that is commented out below.  String concatenation is very expensive and needs to be avoided
            // wherever performance is critical.
            //TemsTrace.WriteLine(TraceLevel.Verbose, "Message inspector previewing inbound Request message:\n\n    " + request);

            // By using this format overload, no string processing is done if the Console is not active.
            
            //TemsTrace.WriteLine(TraceLevel.Verbose, "\n\n>>>>>>>>>>\nMessage inspector previewing inbound Request message:\n\n    {0}", request);
            return null;
        }

        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            // Note that the string concatenation is done regardless of whether or not the Console is active with
            // the line that is commented out below.  String concatenation is very expensive and needs to be avoided
            // wherever performance is critical.
            //TemsTrace.WriteLine(TraceLevel.Verbose, "Message inspector previewing outbound Reply message:\n\n    " + reply);

            // By using this format overload, no string processing is done if the Console is not active.
            
            //TemsTrace.WriteLine(TraceLevel.Verbose, "\n\n<<<<<<<<<<\nMessage inspector previewing outbound Reply message:\n\n    {0}", reply);
        }

        #endregion
    }
}
