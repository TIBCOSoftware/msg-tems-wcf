/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace com.tibco.sample.client
{
    public class Time
    {
        static DateTime startTime;
        static DateTime stopTime;

        public static void StartTimer()
        {
            startTime = DateTime.UtcNow;
        }

        public static void StopTimer()
        {
            stopTime = DateTime.UtcNow;
        }

        public static double Elapsed()
        {
            return (stopTime - startTime).TotalMilliseconds;
        }
    }
}
