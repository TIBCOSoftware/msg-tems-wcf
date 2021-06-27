/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.Web.Services.Configuration;
using System.Web.Services.Description;
using System.Xml.Serialization;

namespace com.tibco.wcf.tems
{
    [XmlFormatExtension("connectionFactory", TemsWsdlExportExtensionSoapOverJMS.JmsNamespace, typeof(Port))]
    public class TemsWsdlConnectionFactoryExtension : ServiceDescriptionFormatExtension
    {
        private string name;

        public TemsWsdlConnectionFactoryExtension()
        {
            name = string.Empty;
        }

        public TemsWsdlConnectionFactoryExtension(string name)
        {
            this.name = name;
        }

        [XmlText()]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
