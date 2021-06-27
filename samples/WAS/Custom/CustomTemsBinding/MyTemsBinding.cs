/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Text;
using System.ServiceModel.Channels;
using com.tibco.wcf.tems;
using com.tibco.wcf.tems.ActivatorService;
using TIBCO.EMS;

namespace Tibco.WAS.Samples
{
    #region sampleInterface
    public class MyTemsBinding : ITemsBindingExtension
    {
        public void CustomizeTemsBinding(ref TemsTransportBindingElement tbe)
        {
            // Make modifications to TemsTransportBindingElement here
            tbe.MessageDeliveryMode = MessageDeliveryMode.NonPersistent;
        }
    }
    #endregion
}
