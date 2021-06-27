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
using com.tibco.wcf.tems;
using com.tibco.wcf.tems.ActivatorService.Activation;
using Tibco.WAS.Samples;

namespace Tibco.Samples
{
    #region Contracts
    [ServiceContract(Namespace = "http://Tibco.Samples")]
    public interface ICalculator
    {
        [OperationContract]
        double Add(double n1, double n2);
        [OperationContract]
        double Subtract(double n1, double n2);
        [OperationContract]
        double Multiply(double n1, double n2);
        [OperationContract]
        double Divide(double n1, double n2);
    }
    #endregion

    class Client
    {
        static void Main()
        {
            // Calling TEMS duplex contract
            Console.WriteLine("Calling TEMS duplex contract (ICalculator).");

            // Create a channel.
            string calculatorAddr = "net.tems://localhost:7222/TemsCalculator/Service.svc/customQ";
            EndpointAddress calcEndpoint = new EndpointAddress(calculatorAddr);
            TemsTransportExtensionElement temsEE = new TemsTransportExtensionElement();
            MyTemsBinding custEE = new MyTemsBinding();
            TemsTransportBindingElement temsBE = new TemsTransportBindingElement();
            temsEE.ApplyConfiguration(temsBE);
            custEE.CustomizeTemsBinding(ref temsBE); // returns tems EE with custom settings;
            CustomBinding temsBinding = new CustomBinding(new TextMessageEncodingBindingElement(), temsBE);

            ICalculator calculator = ChannelFactoryHelper.CreateSingleChannel<ICalculator>(temsBinding, calcEndpoint);

            // Call the Add service operation.
            double value1 = 100.00D;
            double value2 = 15.99D;
            double result = calculator.Add(value1, value2);
            Console.WriteLine("Add({0},{1}) = {2}", value1, value2, result);

            // Call the Subtract service operation.
            value1 = 145.00D;
            value2 = 76.54D;
            result = calculator.Subtract(value1, value2);
            Console.WriteLine("Subtract({0},{1}) = {2}", value1, value2, result);

            // Call the Multiply service operation.
            value1 = 9.00D;
            value2 = 81.25D;
            result = calculator.Multiply(value1, value2);
            Console.WriteLine("Multiply({0},{1}) = {2}", value1, value2, result);

            // Call the Divide service operation.
            value1 = 22.00D;
            value2 = 7.00D;
            result = calculator.Divide(value1, value2);
            Console.WriteLine("Divide({0},{1}) = {2}", value1, value2, result);

            // Closing the client gracefully closes the connection and cleans up resources
            ((IChannel)calculator).Close();

            Console.WriteLine();
            Console.WriteLine("Press <ENTER> to terminate client.");
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
