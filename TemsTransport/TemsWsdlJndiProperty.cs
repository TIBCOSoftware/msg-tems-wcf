/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.Xml.Serialization;

namespace com.tibco.wcf.tems
{
    public class TemsWsdlJndiProperty
    {
        private string name;
        private string value;

        public TemsWsdlJndiProperty()
        {
            this.name = string.Empty;
            this.value = string.Empty;
        }

        public TemsWsdlJndiProperty(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        [XmlAttribute("name")]
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        [XmlText()]
        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
    }
}
