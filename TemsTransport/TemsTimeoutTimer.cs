/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;

namespace com.tibco.wcf.tems
{
    public struct TemsTimeoutTimer
    {
        TimeSpan timeout;
        DateTime startTime;
        
        internal TemsTimeoutTimer(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                string msg = "TimeSpan timeout cannot be negative.";
                throw new ArgumentOutOfRangeException("timeout", timeout, msg);
            }

            this.timeout = timeout;
            startTime = DateTime.UtcNow;
        }

        internal void Reset()
        {
            startTime = DateTime.UtcNow;
        }

        internal TimeSpan Elapsed()
        {
            return DateTime.UtcNow - startTime;
        }

        internal TimeSpan CurrentTimeout()
        {
            return CurrentTimeout(false);
        }

        internal TimeSpan CurrentTimeout(bool throwIfExpired)
        {
            TimeSpan current = timeout - Elapsed();

            if (current <= TimeSpan.Zero && throwIfExpired)
            {
                ThrowTimedOut();
            }

            return current;
        }

        internal bool IsExpired()
        {
            return CurrentTimeout() < TimeSpan.Zero;
        }

        internal void ThrowIfExpired()
        {
            if (IsExpired())
            {
                ThrowTimedOut();
            }
        }

        internal long MillisecondsToExpire()
        {
            return MillisecondsToExpire(false);
        }
        
        internal long MillisecondsToExpire(bool throwIfExpired)
        {
            long toExpire = (long)CurrentTimeout().TotalMilliseconds;

            if (toExpire < 1 && throwIfExpired)
            {
                ThrowTimedOut();
            }

            return toExpire;
        }

        internal int IntMillisecondsToExpire()
        {
            return IntMillisecondsToExpire(false);
        }

        internal int IntMillisecondsToExpire(bool throwIfExpired)
        {
            int retval;

            if (timeout == TimeSpan.MaxValue)
            {
                retval = -1;
            }
            else
            {
                long ms = MillisecondsToExpire(throwIfExpired);

                if (ms > Int32.MaxValue)
                {
                    ms = Int32.MaxValue;
                }

                retval = (int) ms;
            }
            return retval;
        }
        
        internal TimeSpan Timeout
        {
            get { return timeout; }
        }

        internal void ThrowTimedOut()
        {
            string msg = String.Format("Tems operation has timed out on timeout value: {0}.", timeout);
            TemsTrace.WriteLine(System.Diagnostics.TraceLevel.Info, msg);
            throw new TimeoutException(msg);
        }

        public override string ToString()
        {
            return String.Format("TemsTimeoutTimer {0} / {1}", timeout, CurrentTimeout());
        }
    }
}
