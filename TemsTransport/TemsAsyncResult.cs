/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Threading;

namespace com.tibco.wcf.tems
{
    /**
     * <summary>
     * A generic base class for IAsyncResult implementations
     * that wraps a ManualResetEvent.
     * </summary>
     **/
    abstract class AsyncResult : IAsyncResult
    {
        AsyncCallback callback;
        object state;
        bool completedSynchronously;
        internal bool endCalled;
        Exception exception;
        bool isCompleted;
        ManualResetEvent manualResetEvent;

        protected AsyncResult(AsyncCallback callback, object state)
        {
            this.callback = callback;
            this.state = state;
        }

        public object AsyncState
        {
            get
            {
                return state;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (manualResetEvent != null)
                {
                    return manualResetEvent;
                }

                lock (ThisLock)
                {
                    if (manualResetEvent == null)
                    {
                        manualResetEvent = new ManualResetEvent(isCompleted);
                    }
                }

                return manualResetEvent;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return completedSynchronously;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return isCompleted;
            }
        }

        object ThisLock
        {
            get { return this; }
        }


        protected void Complete(bool completedSynchronously)
        {
            this.completedSynchronously = completedSynchronously;

            if (completedSynchronously)
            {
                this.isCompleted = true;
            }
            else
            {
                lock (ThisLock)
                {
                    this.isCompleted = true;

                    if (manualResetEvent != null)
                    {
                        manualResetEvent.Set();
                    }
                }
            }

            try
            {
                if (callback != null)
                {
                    callback(this);
                }
            }
            catch (Exception unhandledException)
            {
                unhandledException = new Exception("Async callback exception", unhandledException);
                ThreadPool.QueueUserWorkItem(new WaitCallback(RaiseUnhandledException), unhandledException);
            }
        }

        protected void Complete(bool completedSynchronously, Exception exception)
        {
            this.exception = exception;
            Complete(completedSynchronously);
        }

        protected static void End(AsyncResult asyncResult)
        {
            if (asyncResult.endCalled)
            {
                throw new InvalidOperationException("Async object already ended.");
            }

            asyncResult.endCalled = true;

            if (!asyncResult.isCompleted)
            {
                using (WaitHandle waitHandle = asyncResult.AsyncWaitHandle)
                {
                    waitHandle.WaitOne();
                }
            }

            if (asyncResult.exception != null)
            {
                throw asyncResult.exception;
            }
        }

        void RaiseUnhandledException(object o)
        {
            Exception exception = (Exception)o;
            throw exception;
        }
        protected static TAsyncResult End<TAsyncResult>(IAsyncResult result)
            where TAsyncResult : AsyncResult
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            TAsyncResult asyncResult = result as TAsyncResult;

            if (asyncResult == null)
            {
                throw new ArgumentException("Invalid async result.", "result");
            }

            if (asyncResult.endCalled)
            {
                throw new InvalidOperationException("Async object already ended.");
            }

            asyncResult.endCalled = true;

            if (!asyncResult.isCompleted)
            {
                asyncResult.AsyncWaitHandle.WaitOne();
            }

            if (asyncResult.manualResetEvent != null)
            {
                asyncResult.manualResetEvent.Close();
            }

            if (asyncResult.exception != null)
            {
                throw asyncResult.exception;
            }

            return asyncResult;
        }

    }

    class WaitAsyncResult : AsyncResult
    {
        public WaitAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
        }

        public void SetComplete()
        {
            Complete(false);
        }

        public static void End(IAsyncResult result)
        {
            WaitAsyncResult completedResult = result as WaitAsyncResult;

            if (completedResult == null)
            {
                throw new ArgumentException("Invalid async result.", "result");
            }

            AsyncResult.End(completedResult);
        }
    }

   /**
    * <Summary>
    *   An AsyncResult that completes as soon as it is instantiated.
    * </Summary>
    **/
    class CompletedAsyncResult : AsyncResult
    {
        public CompletedAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
            Complete(true);
        }

        public static void End(IAsyncResult result)
        {
            CompletedAsyncResult completedResult = result as CompletedAsyncResult;
            if (completedResult == null)
            {
                throw new ArgumentException("Invalid async result.", "result");
            }

            AsyncResult.End(completedResult);
        }
     }                                                                                                                         

    /**
     * <Summary>
     *   An AsyncResult that completes as soon as it is instantiated.
     * </Summary>
     **/
    class TypedAsyncResult<T> : AsyncResult
    {
        T data;

        public TypedAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
        }

        public T Data
        {
            get { return data; }
        }

        protected void Complete(T data, bool completedSynchronously)
        {
            this.data = data;
            Complete(completedSynchronously);
        }

        public static T End(IAsyncResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            TypedAsyncResult<T> completedResult = result as TypedAsyncResult<T>;

            if (completedResult == null)
            {
                throw new ArgumentException("Invalid async result.", "result");
            }

            AsyncResult.End(completedResult);

            return completedResult.Data;
        }

        public void SetComplete(T data, bool completedSynchronously)
        {
            this.Complete(data, completedSynchronously);
        }
    }

    /**
     * <Summary>
     *   An AsyncResult that completes as soon as it is instantiated.
     * </Summary>
     **/
    class TypedCompletedAsyncResult<T> : TypedAsyncResult<T>
    {
        public TypedCompletedAsyncResult(T data, AsyncCallback callback, object state)
            : base(callback, state)
        {
            Complete(data, true);
        }

        public new static T End(IAsyncResult result)
        {
            TypedCompletedAsyncResult<T> completedResult = result as TypedCompletedAsyncResult<T>;
            if (completedResult == null)
            {
                throw new ArgumentException("Invalid async result.", "result");
            }

            return TypedAsyncResult<T>.End(completedResult);
        }
    }
}
