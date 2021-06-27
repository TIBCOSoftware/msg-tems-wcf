/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.Collections;
using System.Web.Services.Configuration;
using System.Web.Services.Description;
using System.Xml.Serialization;

namespace com.tibco.wcf.tems
{
    [XmlFormatExtension("context", TemsWsdlExportExtensionSoapOverJMS.JndiNamespace, typeof(Port))]
    public class TemsWsdlJndiContextExtension : ServiceDescriptionFormatExtension
    {
        private ArrayList propertiesList;
        
        public TemsWsdlJndiContextExtension()
        {
            propertiesList = new ArrayList();
        }

        [XmlElement("property")]
        public TemsWsdlJndiProperty[] Property
        {
            get
            {
                return (TemsWsdlJndiProperty[])this.propertiesList.ToArray(typeof(TemsWsdlJndiProperty));
            }
            set
            {
                this.propertiesList = new ArrayList(value);
            }
        }

        public void AddProperty(TemsWsdlJndiProperty property)
        {
            propertiesList.Add(property);
        }
    }
}
