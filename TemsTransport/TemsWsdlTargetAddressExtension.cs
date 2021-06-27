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
    [XmlFormatExtension("targetAddress", TemsWsdlExportExtensionSoapOverJMS.JmsNamespace, typeof(Port))]
    public class TemsWsdlTargetAddressExtension : ServiceDescriptionFormatExtension
    {
        private TemsWsdlDestinationType destination;
        private string address;
        //private string selector;

        public TemsWsdlTargetAddressExtension()
        {
            address = string.Empty;
        }

        public TemsWsdlTargetAddressExtension(TemsWsdlDestinationType destination, string address)
        {
            this.destination = destination;
            this.address = address;
        }

        [XmlAttribute("destination")]
        public TemsWsdlDestinationType Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        [XmlText()]
        public string Address
        {
            get { return address; }
            set { address = value; }
        }
    }
}
