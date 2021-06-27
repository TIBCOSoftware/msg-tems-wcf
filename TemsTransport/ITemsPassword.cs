/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;

namespace com.tibco.wcf.tems
{
    public interface ITemsPassword
    {
        string Manage(string password);
        string ManageSSL(string sslPassword);
        string ManageSSLProxyAuth(string sslProxyAuthPassword);
    }
}
