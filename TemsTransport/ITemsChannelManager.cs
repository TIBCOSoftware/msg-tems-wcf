/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.ServiceModel.Channels;

namespace com.tibco.wcf.tems
{
    public interface ITemsChannelManager
    {
        TemsTransportBindingElement BindingElement { get; }
        BindingContext BindingContext { get; }
        MessageEncoderFactory EncoderFactory { get; }
        BufferManager BufferManager { get; }
    }
}
