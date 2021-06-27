/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Select an option:\n");
            Console.WriteLine("    1 - Server RequestReply");
            Console.WriteLine("    2 - Client RequestReply\n");
            Console.WriteLine("    3 - Server Datagram");
            Console.WriteLine("    4 - Client Datagram\n");
            Console.WriteLine("    5 - Server Duplex");
            Console.WriteLine("    6 - Client Duplex\n");
            Console.WriteLine("    Add v for verbose.\n");

            string input = Console.ReadLine();
            string actor;
            string mep;
            int logLevel = input.IndexOf("v", StringComparison.OrdinalIgnoreCase) == -1 ? -2 : 10;
            string switchInput = input.Substring(0, 1);
            int iterations = 100000;

            switch (switchInput)
            {
                case "1":
                    actor = "Server";
                    mep = "RequestReply";
                    break;
                case "2":
                    actor = "Client";
                    mep = "RequestReply";
                    break;
                case "3":
                    actor = "Server";
                    mep = "Datagram";
                    break;
                case "4":
                    actor = "Client";
                    mep = "Datagram";
                    break;
                case "5":
                    actor = "Server";
                    mep = "Duplex";
                    break;
                case "6":
                    actor = "Client";
                    mep = "Duplex";
                    iterations = 10000;
                    break;
                default:
                    actor = "Server";
                    mep = "RequestReply";
                    break;
            }

            string command;
            string[] commandArgs;

            if (actor.Equals("Server"))
            {
                command = String.Format("-type Service{0}Type -log {1}", mep, logLevel);
                commandArgs = command.Split(new char[] { ' ' });
                Console.WriteLine("\nStarting {0} for {1} MEP using command:\n\n    host.exe {2}\n", actor, mep, command);
                com.tibco.test.host.TestService.Main(commandArgs);
            }
            else
            {
                command = String.Format("-ep Tems.{0} -log {1} -iter {2}", mep, logLevel, iterations);
                commandArgs = command.Split(new char[] { ' ' });
                Console.WriteLine("\nStarting {0} for {1} MEP using command:\n\n    client.exe {2}\n", actor, mep, command);
                com.tibco.test.client.TestClient.Main(commandArgs);
            }

            Console.WriteLine("Press any key to continue.");
            Console.ReadLine();
        }
    }
}
