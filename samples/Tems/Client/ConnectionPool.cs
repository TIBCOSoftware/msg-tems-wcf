/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using com.tibco.wcf.tems;
using TIBCO.EMS;

namespace com.tibco.sample.client
{
    public class ConnectionPool : IExceptionListener
    {
        private List<Connection> connectionPool = null;
        ConnectionFactory factory = null;

        public ConnectionPool()
        {
            connectionPool = new List<Connection>();
        }

        public void Initialize(ConnectionFactory aFactory, int size)
        {
            Connection connection;

            factory = aFactory;  // save in case need to create connections later

            // Create "pool" of connections
            for (int i = 0; i < size; i++)
            {
                connection = factory.CreateConnection();
                // set the exception listener
                connection.ExceptionListener = this;
                connectionPool.Add(connection);
            }
        }

        public Connection GetConnection()
        {
            Connection connection;
            lock (connectionPool)
            {
                if (connectionPool.Count > 0)
                {
                    connection = connectionPool[0];
                    connectionPool.RemoveAt(0);
                }
                else
                { // create another connection as pool is currently empty
                    connection = factory.CreateConnection();
                    connection.ExceptionListener = this;
                }
            }
            connection.Start();
            return connection;
        }

        public void ReturnConnection(Connection connection)
        {
            connection.Stop();
            lock (connectionPool)
            {
                connectionPool.Add(connection);
            }
        }

        public void Release()
        {
            // free connections in Pool
            foreach (Connection connection in connectionPool)
            {
                connection.Close();
            }
            connectionPool.Clear();
            connectionPool = null;
        }

        #region IExceptionListener Members

        public void OnException(EMSException ex)
        {
            string errMsg = String.Format("ManageConnectionsExample OnException: {0}", ex.Message);
            if (ex.InnerException != null)
            {
                errMsg = errMsg + ", Inner Exception = " + ex.InnerException.Message;
            }
            TemsTrace.WriteLine(TraceLevel.Error, errMsg);
        }

        #endregion
    }
}
