/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Diagnostics;
using com.tibco.wcf.tems;

namespace com.tibco.sample.custom
{
    public class SampleMessageProtocol : TemsMessageProtocol
    {
        #region Constructors
        public SampleMessageProtocol()
            : base()
        {
        }
        #endregion
        public override System.ServiceModel.Channels.Message Receive(TIBCO.EMS.Message emsMessage, TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "SampleMessageProtocol.Receive called.");
            return base.Receive(emsMessage, timeout);
        }

        public override TIBCO.EMS.Message Send(System.ServiceModel.Channels.Message message, TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "SampleMessageProtocol.Send called.");
            return base.Send(message, timeout);
        }

        public override System.ServiceModel.Channels.Message ReceiveTransform(TIBCO.EMS.Message emsMessage, System.ServiceModel.Channels.Message wcfMessage, TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "SampleMessageProtocol.ReceiveTransform called.");
            return base.ReceiveTransform(emsMessage, wcfMessage, timeout);
        }

        public override TIBCO.EMS.Message SendTransform(TIBCO.EMS.Message emsMessage, System.ServiceModel.Channels.Message wcfMessage, TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "SampleMessageProtocol.SendTransform called.");
            // Message Selector Example.  In Host.exe's app.config file, set the following:
            //                                   messageSelector="testproperty > 10"
            // uncomment line with SetCustomMessageProtocolExample() in both SampleClient.cs and SampleService.cs

            // emsMessage.SetIntProperty("testproperty", 24);  // Using request/reply example the service will get this message
            // OR
            // emsMessage.SetIntProperty("testproperty", 5);  // Using request/reply example this service will NOT get this message so no reply will be received
            return base.SendTransform(emsMessage, wcfMessage, timeout);
        }
    }
}
