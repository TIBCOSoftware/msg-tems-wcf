/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.ServiceModel.Description;
using System.Web.Services.Description;
using System.Xml;

namespace com.tibco.sample.client
{
    class TestMex
    {
        public const string MexEndPoint = "net.tems://localhost:7222/queue/Tems.MEX";
        
        public static void Main(string[] args)
        {
            TestMex test = new TestMex();

            while (true)
            {
                System.Console.WriteLine("Press any key to send MEX client request:");
                System.Console.ReadKey(true);
                test.RunTest();
            }
        }

        private void RunTest()
        {
            MetadataExchangeClient mexClient = new MetadataExchangeClient();
            MetadataSet metadataset = mexClient.GetMetadata(new Uri(MexEndPoint), MetadataExchangeClientMode.MetadataExchange);

            Console.WriteLine("********************");
            //WriteMetadataSet(metadataset);
            WriteWsdl(metadataset);
            Console.WriteLine("\n********************");
        }

        private static void WriteMetadataSet(MetadataSet metadataset)
        {
            XmlWriter xmlWriter = XmlWriter.Create(Console.Out);
            metadataset.WriteTo(xmlWriter);
        }

        private static void WriteWsdl(MetadataSet metadataset)
        {
            WsdlImporter wsdlImporter = new WsdlImporter(metadataset);
            ServiceDescriptionCollection sdc = wsdlImporter.WsdlDocuments;

            foreach (System.Web.Services.Description.ServiceDescription sd in sdc)
            {
                sd.Write(Console.Out);
            }
        }
    }
}
