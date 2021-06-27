/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Collections;
using System.Diagnostics;

namespace com.tibco.wcf.tems
{
    public class TemsTraceSwitch : TraceSwitch
    {
        private bool showThreadId;
        private bool showDateTime;
        private bool dateTimeUtc;
        private string dateTimeFormat;
        
        public TemsTraceSwitch(string displayName, string description) :
            base(displayName, description)
        {
            showThreadId = true;
            showDateTime = true;
            dateTimeUtc = true;
            dateTimeFormat = "o";
            Initialize();
        }

        public TemsTraceSwitch(string displayName,
                                string description,
                                string switchValueDefault) :
            base(displayName, description, switchValueDefault)
        {
            showThreadId = true;
            showDateTime = true;
            dateTimeUtc = true;
            dateTimeFormat = "o";
            Initialize();
        }

        public TemsTraceSwitch(string displayName, 
                                string description,
                                string switchValueDefault,
                                bool showThreadIdDefault,
                                bool showDateTimeDefault,
                                bool dateTimeUtcDefault,
                                string dateTimeFormatDefault) :
            base(displayName, description, switchValueDefault)
        {
            showThreadId = showThreadIdDefault;
            showDateTime = showDateTimeDefault;
            dateTimeUtc = dateTimeUtcDefault;
            dateTimeFormat = dateTimeFormatDefault;
            Initialize();
        }

        private void Initialize()
        {
            foreach (DictionaryEntry de in Attributes)
            {
                string attribute = de.Key.ToString();

                if (attribute.Equals("showThreadId", StringComparison.OrdinalIgnoreCase))
                {
                    showThreadId = Boolean.Parse((string)de.Value);
                }
                else if (attribute.Equals("showDateTime", StringComparison.OrdinalIgnoreCase))
                {
                    showDateTime = Boolean.Parse((string)de.Value);
                }
                else if (attribute.Equals("dateTimeUtc", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeUtc = Boolean.Parse((string)de.Value);
                }
                else if (attribute.Equals("dateTimeFormat", StringComparison.OrdinalIgnoreCase))
                {
                    dateTimeFormat = (string)de.Value;
                }
            }
        }

        protected override string[] GetSupportedAttributes()
        {
            return new string[] { "showThreadId", "showDateTime", "dateTimeUtc", "dateTimeFormat" };
        }

        /** <summary>
          *     Specifies if the current threadId should be included as a prefix to the Trace msg.
          * 
          *     Default = true
          * </summary>
          **/ 
        public bool ShowThreadId
        {
            get { return showThreadId; }
            set { showThreadId = value; }
        }

        /**  <summary>
          * Specifies if DateTime should be included as a prefix to the Trace msg.
          * 
          * Default = true
          * </summary>
        **/
        public bool ShowDateTime
        {
            get { return showDateTime; }
            set { showDateTime = value; }
        }

        /**  <summary>
          * Specifies if DateTime should be converted to Coordinated Universal Time (UTC)
          * using the DateTime ToUniversalTime() method.
          * 
          * Default = true
          * </summary>
        **/
        public bool DateTimeUtc
        {
            get { return dateTimeUtc; }
            set { dateTimeUtc = value; }
        }

        /**  <summary>
          * Specifies the format that is used to display a DateTime prefix:
          *     DateTime.Now.ToString(DateTimeFormat, DateTimeFormatProvider)
          *     
          * The default uses the Standard format string "o" which has the format:
          *     yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzz
          * 
          * Default = "o"
          * </summary>
        **/
        public string DateTimeFormat
        {
            get { return dateTimeFormat; }
            set { dateTimeFormat = value; }
        }
    }
}
