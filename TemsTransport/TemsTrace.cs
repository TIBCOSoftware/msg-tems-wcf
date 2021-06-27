/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Diagnostics;

namespace com.tibco.wcf.tems
{
    public class TemsTrace
    {
        private static TemsTraceSwitch traceSwitch;
        private static IFormatProvider dateTimeFormatProvider;
        private static int maxThreadId;

        static TemsTrace()
        {
            traceSwitch = new TemsTraceSwitch("TemsTraceSwitch", "TemsTransport Trace Settings");
            /** Note: If the "TemsTraceSwitch" element is present, it sets the Level.  Otherwise the value 
             *  of the legacy switch element: "TemsTraceLevel" is used if present.
             *  The only way to detect that the "TemsTraceSwitch" element is present
             *        based on the new TemsTraceSwitch instance is:
             *          1) The Level is not "Off" (i.e. a change from default has occurred)
             *          2) A custom attribute is set.
             **/          
            if (traceSwitch.Attributes.Count == 0 && traceSwitch.Level == TraceLevel.Off)
            {
                /** Test if the "TemsTraceSwitch" element is present and set to:
                 *     <add name="TemsTraceSwitch" value="Off"
                 *  by creating a test instance with a default value of "Info".
                 *  If the value does not change to TraceLevel.Off the "TemsTraceSwitch" element
                 *  is not present.
                 **/  
                TemsTraceSwitch test = new TemsTraceSwitch("TemsTraceSwitch", "Test Trace Settings", "Info");

                if (test.Level == TraceLevel.Info)
                {
                    // If the "TemsTraceLevel" element does not exist, the traceSwitch.Level will remain
                    // at the default value of TraceLevel.Off.
                    traceSwitch.Level = new TraceSwitch("TemsTraceLevel", "TemsTransport Trace Level").Level;
                }
            }

            // Set initial ThreadId formatted for two digits.
            maxThreadId = 99;
            TemsTrace.WriteLine(TraceLevel.Off, "TemsTrace level config: {0}", traceSwitch.Level);
        }

        /** <summary>
         *  Specifies the level of trace logging to use.
         *  
         *  Default = System.Diagnostics.TraceLevel.Off
         *  </summary>
         **/  
        public static TraceLevel TraceLevel
        {
            get { return traceSwitch.Level; }

            set
            {
                traceSwitch.Level = value;
                TemsTrace.WriteLine(TraceLevel.Off, "TemsTrace level set to: {0}", traceSwitch.Level);
            }
        }

        /** <summary>
         *  Specifies if the current threadId should be included as a prefix to the Trace msg.
         *  
         *  Default = true
         *  </summary>
         **/
        public static bool ShowThreadId
        {
            get { return traceSwitch.ShowThreadId; }
            set { traceSwitch.ShowThreadId = value; }
        }

        /** <summary>
         *  Specifies if DateTime should be included as a prefix to the Trace msg.
         *  
         *  Default = true
         *  </summary>
         **/
        public static bool ShowDateTime
        {
            get { return traceSwitch.ShowDateTime; }
            set { traceSwitch.ShowDateTime = value; }
        }

        /** <summary>
         *  Specifies if DateTime should be converted to Coordinated Universal Time (UTC)
         *  using the DateTime ToUniversalTime() method.
         *  
         *  Default = true
         *  </summary>
         **/  
        public static bool DateTimeUtc
        {
            get { return traceSwitch.DateTimeUtc; }
            set { traceSwitch.DateTimeUtc = value; }
        }

        /** <summary>
         *  Specifies the format that is used to display a DateTime prefix:
         *      DateTime.Now.ToString(DateTimeFormat, DateTimeFormatProvider)
         *      
         *  The default uses the Standard format string "o" which has the format:
         *      yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzz
         *  
         *  Default = "o"
         *  </summary>
         **/
        public static string DateTimeFormat
        {
            get { return traceSwitch.DateTimeFormat; }
            set { traceSwitch.DateTimeFormat = value; }
        }

        /** <summary>
         *  Specifies the IFormatProvider used to display a DateTime prefix:
         *      DateTime.Now.ToString(DateTimeFormat, DateTimeFormatProvider)
         *  
         *  If not specified, the DateTimeFormatInfo associated with the current
         *  culture is used. 
         *  
         *  Default = null
         *  </summary>
         **/
        public static IFormatProvider DateTimeFormatProvider
        {
            get { return dateTimeFormatProvider; }
            set { dateTimeFormatProvider = value; }
        }

        private static string AddPrefix(string msg)
        {
            return String.Format("{0}{1}{2}", GetDateTime(), GetThreadId(), msg);
        }

        private static string GetThreadId()
        {
            string threadId = "";

            if (traceSwitch.ShowThreadId)
            {
                int id = System.Threading.Thread.CurrentThread.ManagedThreadId;

                if (id > maxThreadId)
                {
                    maxThreadId = id;
                }

                string filler = new string(' ', ("" + maxThreadId).Length - ("" + id).Length);
                threadId = String.Format("{0}{1} ", filler, id);
            }

            return threadId;
        }

        private static string GetDateTime()
        {
            string dateTime = "";

            if (traceSwitch.ShowDateTime)
            {
                DateTime dt = DateTime.Now;

                if (traceSwitch.DateTimeUtc)
                {
                    dt = dt.ToUniversalTime();
                }

                dateTime = String.Format("{0} ", dt.ToString(traceSwitch.DateTimeFormat, dateTimeFormatProvider));
            }

            return dateTime;
        }

        private static bool CheckLevel(TraceLevel writeLevel)
        {
            return traceSwitch.Level > TraceLevel.Off && writeLevel <= traceSwitch.Level;
        }

        #region WriteLine Methods

        public static void WriteLine(TraceLevel writeLevel, string msg)
        {
            if (CheckLevel(writeLevel))
            {
                Trace.WriteLine(AddPrefix(msg));
            }
        }

        public static void WriteLine(TraceLevel writeLevel, string format, object arg0)
        {
            if (CheckLevel(writeLevel))
            {
                Trace.WriteLine(String.Format(AddPrefix(format), arg0));
            }
        }

        public static void WriteLine(TraceLevel writeLevel, string format, object arg0, object arg1)
        {
            if (CheckLevel(writeLevel))
            {
                Trace.WriteLine(String.Format(AddPrefix(format), arg0, arg1));
            }
        }

        public static void WriteLine(TraceLevel writeLevel, string format, object arg0, object arg1, object arg2)
        {
            if (CheckLevel(writeLevel))
            {
                Trace.WriteLine(String.Format(AddPrefix(format), arg0, arg1, arg2));
            }
        }

        public static void WriteLine(TraceLevel writeLevel, string format, object arg0, object arg1, object arg2, object arg3)
        {
            if (CheckLevel(writeLevel))
            {
                Trace.WriteLine(String.Format(AddPrefix(format), arg0, arg1, arg2, arg3));
            }
        }

        #endregion

        #region Write Methods

        public static void Write(TraceLevel writeLevel, string msg)
        {
            if (CheckLevel(writeLevel))
            {
                Trace.Write(AddPrefix(msg));
            }
        }

        public static void Write(TraceLevel writeLevel, string format, object arg0)
        {
            if (CheckLevel(writeLevel))
            {
                Trace.Write(String.Format(AddPrefix(format), arg0));
            }
        }

        public static void Write(TraceLevel writeLevel, string format, object arg0, object arg1)
        {
            if (CheckLevel(writeLevel))
            {
                Trace.Write(String.Format(AddPrefix(format), arg0, arg1));
            }
        }

        public static void Write(TraceLevel writeLevel, string format, object arg0, object arg1, object arg2)
        {
            if (CheckLevel(writeLevel))
            {
                Trace.Write(String.Format(AddPrefix(format), arg0, arg1, arg2));
            }
        }

        public static void Write(TraceLevel writeLevel, string format, object arg0, object arg1, object arg2, object arg3)
        {
            if (CheckLevel(writeLevel))
            {
                Trace.Write(String.Format(AddPrefix(format), arg0, arg1, arg2, arg3));
            }
        }

        #endregion

        /*public static void LogBytes(byte[] bytes)
        {
            //string bytesAsString = new System.Text.ASCIIEncoding().GetString(bytes);
            //TemsTrace.WriteLine(TraceLevel.Error, "bytesAsString = \n\n-----\n" + bytesAsString + "\n-----");

            TemsTrace.WriteLine(TraceLevel.Verbose, "-----");
            foreach (byte b in bytes)
            {
                TemsTrace.WriteLine(TraceLevel.Verbose, "" + b);
            }
            TemsTrace.WriteLine(TraceLevel.Verbose, "\n-----");
        }
        */
    }
}
