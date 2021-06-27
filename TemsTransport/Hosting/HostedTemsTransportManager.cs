/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Diagnostics;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.Web.Configuration;

using com.tibco.wcf.tems.ActivatorService;
using com.tibco.wcf.tems.ActivatorService.Activation;

using WebAdmin = Microsoft.Web.Administration;

namespace com.tibco.wcf.tems.ActivatorService.Hosting
{
    class HostedTemsTransportManager 
    {
        internal bool isClosed = true;
        List<Uri> baseAddresses;
        Dictionary<string, IChannelListener> channelListeners;  // used to look up TEMS channel listener from virtual path
        Dictionary<int, string> vpToChannelIds;  // used to lookup virtual path from channel id
        Uri listenUri;

        internal HostedTemsTransportManager()
        {
            baseAddresses = new List<Uri>();
            channelListeners = new Dictionary<string, IChannelListener>(StringComparer.OrdinalIgnoreCase);
            ListenUri = UpdateBindings();
            vpToChannelIds = new Dictionary<int, string>();
        }

        internal void Open(int instanceId, string virtualPath)
        {
            // If multiple endpoints where one is not net.tems (like http) then
            // this will be called AFTER Register since service may already be started.
            WasHelper.ts.TraceInformation("HostedTemsTransportManager: OPEN - listenUri = " + ListenUri.AbsoluteUri + ", instance id = " + instanceId + ", virtual path = " + virtualPath);

            try
            {
                vpToChannelIds.Add(instanceId, virtualPath); 
                ServiceHostingEnvironment.EnsureServiceAvailable(virtualPath); // Starts TEMS service host (if not started will Register is called)
            }
            catch (Exception ex)
            {
                // Don't re-throw errors as will just cause re-start of worker thread ... for no point until error is fixed.
                string errMsg = "HostedTemsTransportManager: Open Exception =" + ex.Message;

                if (ex.InnerException != null)
                {
                    errMsg = errMsg + ", InnerException = " + ex.InnerException;
                }

                WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), errMsg + "\n" + ex.StackTrace);
                EventLog.WriteEntry(WasTemsInfo.ServiceName + "-w3wp", errMsg, EventLogEntryType.Error);
            }

            isClosed = false;
        }

        internal List<Uri> BaseAddresses
        {
            get
            {
                return baseAddresses;
            }
        }

        Uri UpdateBindings()
        {
            Uri retUri = null;
            string[] parts;
            string prefixFilter="";

            try
            {
                WasHelper.ts.TraceInformation("HostedTemsTransportManager:UpdateBindings");

                WebAdmin.ServerManager serverManager = new WebAdmin.ServerManager();
                WebAdmin.Site site = serverManager.Sites[HostingEnvironment.SiteName];
                string baseAddr = "";
                int port = 0;
                string host = "";
                string errMsg;

                Configuration rc = WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath, HostingEnvironment.SiteName);

                if (rc.HasFile)
                {
                    ServiceHostingEnvironmentSection smeSection = (ServiceHostingEnvironmentSection)rc.GetSection("system.serviceModel/serviceHostingEnvironment");

                    foreach (BaseAddressPrefixFilterElement pfe in smeSection.BaseAddressPrefixFilters)
                    {
                        if (pfe.Prefix.AbsoluteUri.IndexOf(WasTemsInfo.TemsScheme) != -1)
                        {
                            prefixFilter = pfe.Prefix.AbsoluteUri;
                            break;
                        }
                    }
                }
                else
                {
                    WasHelper.ts.TraceEvent(TraceEventType.Warning, WasHelper.GetErrSeq(), "HostedTemsTransportManager:UpdateBindings - no web.config file found at path = {0}", rc.FilePath);

                }

                foreach (WebAdmin.Binding binding in site.Bindings)
                {
                    if (string.Compare(binding.Protocol, WasTemsInfo.TemsScheme, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        WasHelper.ts.TraceInformation(" - bindinginfo = " + binding.BindingInformation + ", prefixFilter = " + prefixFilter);
                        parts = binding.BindingInformation.Split(':');

                        if (!(parts.Length == 1 || parts.Length == 2) || !int.TryParse(parts[parts.Length - 1], out port))
                        {
                            errMsg = string.Format(" - The binding information '{0}' is invalid for the protocol '{1}'",
                            binding.BindingInformation, WasTemsInfo.TemsScheme);
                            WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), errMsg);
                            throw new InvalidDataException(errMsg);
                        }

                        if (parts.Length == 2)
                            host = parts[0];
                        else
                            host = "localhost";

                        baseAddr = new UriBuilder(WasTemsInfo.TemsScheme, host, port).Uri.AbsoluteUri;

                        if (prefixFilter != string.Empty)
                        {
                            WasHelper.ts.TraceInformation(" - baseaddr = " + baseAddr + ", prefixFilter = " + prefixFilter);
                            if (baseAddr == prefixFilter)
                            {
                                break;
                            }
                        }
                        else
                            break;  // use first encountered since no prefixFilter defined.
                    }
                }

                if (baseAddr == string.Empty)
                {
                    errMsg = string.Format(" - No binding is found for the protocol '{0}'", WasTemsInfo.TemsScheme);
                    WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), errMsg);
                    throw new InvalidOperationException(errMsg);
                }

                retUri = new UriBuilder(WasTemsInfo.TemsScheme, host, port, HostingEnvironment.ApplicationVirtualPath).Uri;
                baseAddresses.Add(retUri);

            }
            catch (Exception ex)
            {
                string errMsg = "HostedTemsTransportManager: UpdateBindings Exception =" + ex.Message;

                if (ex.InnerException != null)
                {
                    errMsg = errMsg + ", InnerException = " + ex.InnerException;
                }

                WasHelper.ts.TraceEvent(TraceEventType.Error, WasHelper.GetErrSeq(), errMsg + "\n" + ex.StackTrace);
                EventLog.WriteEntry(WasTemsInfo.ServiceName + "-w3wp", errMsg, EventLogEntryType.Error);
            }

            return retUri;
        }

        internal Uri ListenUri
        {
            get
            {
                return listenUri;
            }
            set
            {
                listenUri = value;
                WasHelper.ts.TraceInformation("TemsTransportManager:Set ListenUri = " + listenUri.AbsoluteUri);
            }
        }

        internal void Register(IChannelListener channelListener)
        {
            WasHelper.ts.TraceInformation("TemsTransportManager:Register - uri " + channelListener.Uri.AbsoluteUri);

            if (channelListener.Uri.LocalPath != null)
            {
                WasHelper.ts.TraceInformation("TemsTransportManager:Register - local Path uri " + channelListener.Uri.LocalPath);
                channelListeners.Add(channelListener.Uri.LocalPath, channelListener);
            }
        }

        internal void Close(int listenerChannelId)
        {
            IChannelListener channelListener;
            string virtualPath;

            WasHelper.ts.TraceInformation("HostedTemsTransportManager:Close - listenUri = " + ListenUri.AbsoluteUri);
            isClosed = true;

            baseAddresses.Remove(ListenUri);

            if (vpToChannelIds.TryGetValue(listenerChannelId, out virtualPath))
            {
                if (channelListeners.TryGetValue(virtualPath, out channelListener))
                {
                    WasHelper.ts.TraceInformation("TemsTransportManager:Close - uri " + channelListener.Uri.AbsoluteUri);
                    channelListener.Close();
                    channelListeners.Remove(virtualPath);
                }
                vpToChannelIds.Remove(listenerChannelId);
            }
        }
    }
}
