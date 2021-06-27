/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.ServiceModel.Channels;
using System.Diagnostics;
using System.ServiceModel;
using System.Globalization;

namespace Tibco.Samples
{
    #region Contracts
    [ServiceContract]
    public interface ICalculatorContract
    {
        [OperationContract]
        int Add(int x, int y);
    }

    [ServiceContract]
    public interface IDatagramContract
    {
        [OperationContract(IsOneWay = true)]
        void Hello(string greeting);
    }

    [ServiceContract]
    public interface IStatusContract
    {
        [OperationContract]
        void Start();

        [OperationContract]
        string GetStatus();
    }
    #endregion

    class TemsTestConsole
    {
        /// <summary>
        /// Example of using TEMS where bindings and addresses are specified in code.
        /// </summary>
        static void Main(string[] args)
        {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Console.WriteLine("Testing Tems Activation.");

            // Start status service
            Console.WriteLine("Start the status service.");
            IStatusContract status = ChannelFactoryHelper.CreateSingleChannel<IStatusContract>("status");
            status.Start();

            // Sending datagrams
            Console.WriteLine("Sending TEMS datagrams.");
            Console.Write("Type a word that you want to say to the server: \n");
            string input = Console.ReadLine();
            IDatagramContract datagram = ChannelFactoryHelper.CreateSingleChannel<IDatagramContract>("datagram");

            for (int i = 0; i < 5; i++)
            {
                string greeting = string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", input, i);
                Console.WriteLine("Sending datagram: {0}", greeting);
                datagram.Hello(greeting);
            }

            ((IChannel)datagram).Close();

            // Calling TEMS duplex contract
            Console.WriteLine("Calling TEMS duplex contract (ICalculatorContract).");

            // Create a channel.
            ICalculatorContract calculator = ChannelFactoryHelper.CreateSingleChannel<ICalculatorContract>("calculator");

            for (int i = 0; i < 10; ++i)
            {
                int zz= calculator.Add(i+5, i * 2);
                Console.WriteLine("    {0} + {1} = {2}", i+5, i * 2, zz);
            }

            ((IChannel)calculator).Close();

            // Check status service
            Console.WriteLine("Getting status and dump server traces:");
            string statusResult = status.GetStatus();
            string[] traces = statusResult.Split('|');

            for (int i = 0; i < traces.Length; i++)
            {
                Console.WriteLine("    {0}", traces[i]);
            }

            ((IChannel)status).Close();

            Console.WriteLine("Press <ENTER> to complete test.");
            Console.ReadLine();
        }
    }

    public static class ChannelFactoryHelper
    {
        static void BindLifetimes(IChannelFactory factory, IChannel channel)
        {
            channel.Closed += delegate
            {
                IAsyncResult result = factory.BeginClose(FactoryCloseCallback, factory);
                if (result.CompletedSynchronously)
                    factory.EndClose(result);
            };
        }

        static void FactoryCloseCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
                return;
            IChannelFactory factory = (IChannelFactory)result.AsyncState;
            factory.EndClose(result);
        }

        public static T CreateSingleChannel<T>(string endpointConfigurationName)
        {
            return CreateSingleChannel<T>(new ChannelFactory<T>(endpointConfigurationName));
        }

        public static T CreateSingleChannel<T>(Binding binding, EndpointAddress address)
        {
            return CreateSingleChannel<T>(new ChannelFactory<T>(binding, address));
        }

        static T CreateSingleChannel<T>(ChannelFactory<T> factory)
        {
            T channel = factory.CreateChannel();
            BindLifetimes(factory, (IChannel)channel);
            return channel;
        }
    }
}
