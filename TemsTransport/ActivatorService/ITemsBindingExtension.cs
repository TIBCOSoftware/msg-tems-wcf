/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace com.tibco.wcf.tems.ActivatorService
{
    public interface ITemsBindingExtension
    {
        void CustomizeTemsBinding(ref TemsTransportBindingElement tbe);
    }
}
