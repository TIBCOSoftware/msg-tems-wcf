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
    [XmlFormatExtension("binding", TemsWsdlExportExtensionSoapOverJMS.JmsNamespace, typeof(Binding))]
    public class TemsWsdlJmsBindingExtension : ServiceDescriptionFormatExtension
    {
        private TemsWsdlMessageFormatType messageFormat;

        public TemsWsdlJmsBindingExtension()
        {
            messageFormat = TemsWsdlMessageFormatType.Text;
        }

        public TemsWsdlJmsBindingExtension(TemsWsdlMessageFormatType messageFormat)
        {
            this.messageFormat = messageFormat;
        }

        [XmlAttribute("messageFormat")]
        public TemsWsdlMessageFormatType MessageFormat
        {
            get { return messageFormat; }
            set { messageFormat = value; }
        }
    }
}
