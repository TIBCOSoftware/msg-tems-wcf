/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.ServiceModel;

namespace TIBCO.WAS.Samples
{
    // The service contract is defined in generatedClient.cs, generated from the service by the svcutil tool.

    // Client implementation code.
    class Client
    {
        static void Main()
        {
            CalculatorClient client = null;
            try
            {
                // Create a client
                client = new CalculatorClient();

                // Call the Add service operation.
                double value1 = 100.00D;
                double value2 = 15.99D;
                double result = client.Add(value1, value2);
                Console.WriteLine("Add({0},{1}) = {2}", value1, value2, result);

                // Call the Subtract service operation.
                value1 = 145.00D;
                value2 = 76.54D;
                result = client.Subtract(value1, value2);
                Console.WriteLine("Subtract({0},{1}) = {2}", value1, value2, result);

                // Call the Multiply service operation.
                value1 = 9.00D;
                value2 = 81.25D;
                result = client.Multiply(value1, value2);
                Console.WriteLine("Multiply({0},{1}) = {2}", value1, value2, result);

                // Call the Divide service operation.
                value1 = 22.00D;
                value2 = 7.00D;
                result = client.Divide(value1, value2);
                Console.WriteLine("Divide({0},{1}) = {2}", value1, value2, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Basic Client unexpected exception = " + ex.Message);
            }
            finally
            {
                // Closing the client gracefully closes the connection and cleans up resources
                if (client != null)
                    client.Close();
            }

            Console.WriteLine();
            Console.WriteLine("Press <ENTER> to terminate client.");
            Console.ReadLine();
        }
    }
}
