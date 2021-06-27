/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.ServiceModel.Description;

namespace com.tibco.wcf.tems
{
    public interface ITemsMessageProtocol
    {
        void Initialize(TemsChannelBase channelBase);
        System.ServiceModel.Channels.Message Receive(TIBCO.EMS.Message emsMessage, TimeSpan timeout);
        System.ServiceModel.Channels.Message ReceiveTransform(TIBCO.EMS.Message emsMessage, System.ServiceModel.Channels.Message wcfMessage, TimeSpan timeout);
        TIBCO.EMS.Message Send(System.ServiceModel.Channels.Message message, TimeSpan timeout);
        TIBCO.EMS.Message SendTransform(TIBCO.EMS.Message emsMessage, System.ServiceModel.Channels.Message wcfMessage, TimeSpan timeout);
    }
}
