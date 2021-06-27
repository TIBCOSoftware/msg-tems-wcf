/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceProcess;
using System.Xml;
using System.Web.Configuration;

using com.tibco.wcf.tems;
using com.tibco.wcf.tems.ActivatorService.Hosting;
using com.tibco.wcf.tems.ActivatorService.Activation;
using WebAdmin = Microsoft.Web.Administration;
using SMConfig = System.ServiceModel.Configuration;

namespace com.tibco.wcf.tems.ActivatorService
{
    class NetTemsActivator : ServiceBase
    {
        static internal string sLog = "Application";

        static void Main()
        {
            try
            {
               ServiceBase.Run(new NetTemsActivator()); // Comment out this line for debug

               /* TemsListenerAdapter.Start();// Use this code for debug
                Console.WriteLine("Press Enter to stop TEMS Activator Service");
                Console.ReadLine();
                TemsListenerAdapter.Stop();*/
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("NetTemsActivator: Main Exception: {0}", ex.Message);

                if (ex.InnerException != null)
                {
                    errMsg = errMsg + ", Inner Exception = " + ex.InnerException.Message;
                }
                EventLog.WriteEntry(WasTemsInfo.ServiceName, errMsg + "\n stack = " + ex.StackTrace, EventLogEntryType.Error);
            }
        }

        public NetTemsActivator()
        {
            ServiceName = WasTemsInfo.ServiceName;
        }

        //Start the Windows service.
        protected override void OnStart(string[] args) 
        {
            TemsListenerAdapter.Start();
        }

        // Stop the Windows service.
        protected override void OnStop()
        {
            TemsListenerAdapter.Stop();
        }
    }

    [RunInstaller(true)]
    public class NetTemsActivatorInstaller : Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller service;
        const string ListenerAdapterPath = "system.applicationHost/listenerAdapters";
        const string ProtocolsPath = "system.web/protocols";
        const string ServiceModelPath = "system.serviceModel/serviceHostingEnvironment";

        public NetTemsActivatorInstaller()
        {
            string[] depends = new string[1];
            process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.NetworkService;
            service = new ServiceInstaller();
            service.ServiceName = WasTemsInfo.ServiceName;
            service.DisplayName = "Net.Tems Listener Adapter";
            service.Description = "Receives activation requests over the net.tems protocol and passes them to the Windows Process Activation Service.";
            depends[0] = "Windows Process Activation Service";
            service.ServicesDependedOn = depends;
            service.StartType = ServiceStartMode.Automatic;
            Installers.Add(process);
            Installers.Add(service);
        }
        
        public override void Install(IDictionary mySavedState)
        {
            base.Install(mySavedState);
            Dictionary<string, string> protocolAttribs = new Dictionary<string, string>();
            Dictionary<string, string> transportAttribs = new Dictionary<string, string>();

            if (!IsListenerAdapterInstalled)
            {
                // Register Net.Tems Listener Adapter in %windir%\system32\inetsrv\ApplicationHost.config
                WebAdmin.ServerManager sm = new WebAdmin.ServerManager();
                WebAdmin.Configuration wasConfiguration = sm.GetApplicationHostConfiguration();
                WebAdmin.ConfigurationSection section = wasConfiguration.GetSection(ListenerAdapterPath);
                WebAdmin.ConfigurationElementCollection listenerAdaptersCollection = section.GetCollection();
                WebAdmin.ConfigurationElement element = listenerAdaptersCollection.CreateElement();
                element.GetAttribute("name").Value = WasTemsInfo.TemsScheme;
                SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);
                element.GetAttribute("identity").Value = sid.Value;
                // element.GetAttribute("identity").Value = WindowsIdentity.GetCurrent().User.Value; // For debug
                listenerAdaptersCollection.Add(element);
                sm.CommitChanges();
                wasConfiguration = null;
                sm = null;

            }

            // Register Net.Tems info %windir%\C:\Windows\Microsoft.NET\Frameworkxx\vx.x.xxxx\CONFIG\web.config
            Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration(null); // will open web.config associated with installutil.exe dir
            ProtocolsSection peSection = (ProtocolsSection)rootWebConfig.GetSection(ProtocolsPath);
            ProtocolElement pe = new ProtocolElement(WasTemsInfo.TemsScheme);

            pe.ProcessHandlerType = typeof(TemsProcessProtocolHandler).AssemblyQualifiedName;
            pe.AppDomainHandlerType = typeof(TemsAppDomainProtocolHandler).AssemblyQualifiedName;
            pe.Validate = false;

            SMConfig.ServiceHostingEnvironmentSection smeSection = (SMConfig.ServiceHostingEnvironmentSection)rootWebConfig.GetSection("system.serviceModel/serviceHostingEnvironment");
            string tcTypeName = typeof(HostedTemsTransportConfiguration).AssemblyQualifiedName;
            SMConfig.TransportConfigurationTypeElement tce = new SMConfig.TransportConfigurationTypeElement(WasTemsInfo.TemsScheme, tcTypeName);

            if (!IsProtocolHandlerInstalled)
            {
                peSection.Protocols.Add(pe);
                smeSection.TransportConfigurationTypes.Add(tce);
                rootWebConfig.Save();
            }

            protocolAttribs.Add("name", "net.tems");
            protocolAttribs.Add("processHandlerType", pe.ProcessHandlerType);
            protocolAttribs.Add("appDomainHandlerType", pe.AppDomainHandlerType);
            protocolAttribs.Add("validate", pe.Validate.ToString().ToLower());

            transportAttribs.Add("name", "net.tems");
            transportAttribs.Add("transportConfigurationType", tce.TransportConfigurationType);

            InstallAllWebConfigs(protocolAttribs, transportAttribs, rootWebConfig.FilePath);
        }

        private void InstallAllWebConfigs(Dictionary<string, string> protocolAttribs, Dictionary<string, string> transportAttribs, string defFile)
        {
            // Locate root web.config files for installed CLR versions
            string clrRootPath = Path.Combine(Environment.GetEnvironmentVariable("windir"), "Microsoft.Net");
            string[] webConfigPaths = Directory.GetDirectories(clrRootPath, "config", SearchOption.AllDirectories);
            string fileName = "web.config";
            XmlDocument doc = null;


            // Update each of the web.config files
            foreach (string configPath in webConfigPaths)
            {
                bool isConfigured = false;
                string webConfigFile = Path.Combine(configPath, fileName);

                if (!defFile.Equals(webConfigFile, StringComparison.OrdinalIgnoreCase))
                { 
                    // If here then updated root web.config if elements do not already exist
                    doc = new XmlDocument();
                    doc.Load(webConfigFile);
                    XmlNode protocolsNode = doc.SelectSingleNode("/configuration/system.web/protocols");
                    XmlNode cfgNode = doc.SelectSingleNode("/configuration");  // Expect to be there if web.config exists

                    if (cfgNode == null) 
                        Console.WriteLine("Install error: configuration element missing in web.config");

                    if (protocolsNode == null)
                    {
                        XmlNode webNode = doc.SelectSingleNode("/configuration/system.web");
                        if (webNode == null)
                        {
                            webNode = doc.CreateNode(XmlNodeType.Element, "system.web", "");
                            cfgNode.AppendChild(webNode);
                        }
                        protocolsNode = doc.CreateNode(XmlNodeType.Element, "protocols", "");
                        webNode.AppendChild(protocolsNode);
                    }

                    XmlNode transportNode = doc.SelectSingleNode("/configuration/system.serviceModel/serviceHostingEnvironment");

                    if (transportNode == null)
                    {
                        XmlNode smNode = doc.SelectSingleNode("/configuration/system.serviceModel");

                        if (smNode == null)
                        {
                            smNode = doc.CreateNode(XmlNodeType.Element, "system.serviceModel", "");
                            cfgNode.AppendChild(smNode);
                        }

                        transportNode = doc.CreateNode(XmlNodeType.Element, "serviceHostingEnvironment", "");
                        smNode.AppendChild(transportNode);
                    }

                    XmlElement ele = doc.CreateElement("add");

                    // See if net.tems elements already exist
                    foreach (XmlNode node in protocolsNode.ChildNodes)
                    {
                        if (node.Attributes[0].Value.Equals("net.tems"))
                        {
                            isConfigured = true;
                            break;
                        }
                    }

                    if (!isConfigured)
                    {
                        foreach (KeyValuePair<string, string> entry in protocolAttribs)
                        {
                            XmlAttribute attrib = doc.CreateAttribute(entry.Key);
                            attrib.Value = entry.Value;
                            ele.Attributes.Append(attrib);
                        }

                        protocolsNode.AppendChild(ele);

                        ele = doc.CreateElement("add");

                        foreach (KeyValuePair<string, string> entry in transportAttribs)
                        {
                            XmlAttribute attrib = doc.CreateAttribute(entry.Key);
                            attrib.Value = entry.Value;
                            ele.Attributes.Append(attrib);
                        }
                        transportNode.AppendChild(ele);
                        doc.Save(webConfigFile);
                    }
                }
            }
        }

        // Override 'Uninstall' method of Installer class.
        public override void Uninstall(IDictionary mySavedState)
        {
            base.Uninstall(mySavedState);
            Console.WriteLine("The Uninstall method of 'NetTemsActivatorInstaller' has been called");

            if (IsListenerAdapterInstalled)
            {
                WebAdmin.ServerManager sm = new WebAdmin.ServerManager();
                WebAdmin.Configuration wasConfiguration = sm.GetApplicationHostConfiguration();
                WebAdmin.ConfigurationSection section = wasConfiguration.GetSection(ListenerAdapterPath);
                WebAdmin.ConfigurationElementCollection listenerAdaptersCollection = section.GetCollection();

                for (int i = 0; i < listenerAdaptersCollection.Count; i++)
                {
                    WebAdmin.ConfigurationElement element = listenerAdaptersCollection[i];

                    if (string.Compare((string)element.GetAttribute("name").Value,
                        WasTemsInfo.TemsScheme, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        listenerAdaptersCollection.RemoveAt(i);
                    }
                }

                sm.CommitChanges();
                wasConfiguration = null;
                sm = null;
            }

            Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration(null);
            ProtocolsSection peSection = (ProtocolsSection)rootWebConfig.GetSection(ProtocolsPath);

            SMConfig.ServiceHostingEnvironmentSection smeSection = (SMConfig.ServiceHostingEnvironmentSection)rootWebConfig.GetSection("system.serviceModel/serviceHostingEnvironment");
            string tcTypeName = typeof(HostedTemsTransportConfiguration).AssemblyQualifiedName;
            SMConfig.TransportConfigurationTypeElement tce = new SMConfig.TransportConfigurationTypeElement(WasTemsInfo.TemsScheme, tcTypeName);

            if (IsProtocolHandlerInstalled)
            {
                peSection.Protocols.Remove(WasTemsInfo.TemsScheme);
                smeSection.TransportConfigurationTypes.Remove(tce);
                rootWebConfig.Save();
            }

            UninstallAllWebConfigs(rootWebConfig.FilePath);
        }

        private void UninstallAllWebConfigs(string defFile)
        {
            // locate root web.config files for installed CLR versions
            string clrRootPath = Path.Combine(Environment.GetEnvironmentVariable("windir"), "Microsoft.Net");
            string[] webConfigPaths = Directory.GetDirectories(clrRootPath, "config", SearchOption.AllDirectories);
            string fileName = "web.config";
            XmlDocument doc = null;

            // update each of the web.config files
            foreach (string configPath in webConfigPaths)
            {
                string webConfigFile = Path.Combine(configPath, fileName);
                bool isConfigured = false;

                if (!defFile.Equals(webConfigFile, StringComparison.OrdinalIgnoreCase))
                {
                    // if here then remove net.tems elements from this root web.config 
                    doc = new XmlDocument();
                    doc.Load(webConfigFile);
                    XmlNode protocolsNode = doc.SelectSingleNode("/configuration/system.web/protocols");
                    XmlNode transportNode = doc.SelectSingleNode("/configuration/system.serviceModel/serviceHostingEnvironment");
                    XmlElement ele = doc.CreateElement("add");

                    // see if net.tems elements already exist
                    foreach (XmlNode node in protocolsNode.ChildNodes)
                    {
                        if (node.Attributes[0].Value.Equals("net.tems"))
                        {
                            protocolsNode.RemoveChild(node);
                            isConfigured = true;
                            break;
                        }
                    }

                    foreach (XmlNode node in transportNode.ChildNodes)
                    {
                        if (node.Attributes[0].Value.Equals("net.tems"))
                        {
                            transportNode.RemoveChild(node);
                            break;
                        }
                    }

                    if (isConfigured)  // if true net.tems elements were removed
                        doc.Save(webConfigFile);
                }
            }
        }

        private  bool IsListenerAdapterInstalled
        {           
            get
            {
                bool isInstalled = false;

                using (WebAdmin.ServerManager sm = new WebAdmin.ServerManager())
                {
                    WebAdmin.Configuration wasConfiguration = sm.GetApplicationHostConfiguration();
                    WebAdmin.ConfigurationSection section = wasConfiguration.GetSection(ListenerAdapterPath);
                    WebAdmin.ConfigurationElementCollection listenerAdaptersCollection = section.GetCollection();

                    foreach (WebAdmin.ConfigurationElement e in listenerAdaptersCollection)
                    {
                        if (string.Compare((string)e.GetAttribute("name").Value, WasTemsInfo.TemsScheme, true) == 0)
                        {
                            // Already installed.
                            isInstalled = true;
                            break;
                        }
                    }

                    wasConfiguration = null;
                }

                return isInstalled;
            }
        }

        private bool IsProtocolHandlerInstalled
        {
            get
            {
                bool isInstalled = false;
                Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration(null);
                ProtocolsSection section = (ProtocolsSection)rootWebConfig.GetSection(ProtocolsPath);

                foreach (ProtocolElement protocol in section.Protocols)
                {
                    if (string.Compare(protocol.Name, WasTemsInfo.TemsScheme, true) == 0)
                    {
                        // Already installed.
                        isInstalled = true;
                        break;
                    }
                }

                return isInstalled;
            }
        }
    }
}
