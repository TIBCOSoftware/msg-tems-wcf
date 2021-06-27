/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Diagnostics;
using System.Xml;
using com.tibco.wcf.tems;
using TIBCO.EMS;

namespace com.tibco.sample.client
{
    public class TestMessageProtocol : TemsMessageProtocol
    {
        #region Constructors
        public TestMessageProtocol()
            : base()
        {
        }
        #endregion
        public override System.ServiceModel.Channels.Message Receive(TIBCO.EMS.Message emsMessage, TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "TestMessageProtocol.Receive called.");
            return base.Receive(emsMessage, timeout);
        }

        public override TIBCO.EMS.Message Send(System.ServiceModel.Channels.Message message, TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "TestMessageProtocol.Send called.");
            return base.Send(message, timeout);
        }

        public override System.ServiceModel.Channels.Message ReceiveTransform(TIBCO.EMS.Message emsMessage, System.ServiceModel.Channels.Message wcfMessage, TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "TestMessageProtocol.ReceiveTransform called.");
            return base.ReceiveTransform(emsMessage, wcfMessage, timeout);
        }

        public override TIBCO.EMS.Message SendTransform(TIBCO.EMS.Message emsMessage, System.ServiceModel.Channels.Message wcfMessage, TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "TestMessageProtocol.SendTransform called.");
            return base.SendTransform(emsMessage, wcfMessage, timeout);
        }
    }
}
