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
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Activation;
using com.tibco.wcf.tems;

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

    #region Services
    // Service class which implements the service contract.
    public class CalculatorService : ICalculator
    {
        public double Add(double n1, double n2)
        {
            return n1 + n2;
        }

        public double Subtract(double n1, double n2)
        {
            return n1 - n2;
        }

        public double Multiply(double n1, double n2)
        {
            return n1 * n2;
        }

        public double Divide(double n1, double n2)
        {
            return n1 / n2;
        }
    }

    #endregion
    #region CustomHostFactory
    class SelfDescribingServiceHost : ServiceHost
    {
        public SelfDescribingServiceHost(Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
        }

        //Overriding ApplyConfiguration() allows us to 
        //alter the ServiceDescription prior to opening
        //the service host. 
        protected override void ApplyConfiguration()
        {
            //First, we call base.ApplyConfiguration()
            //to read any configuration that was provided for
            //the service we're hosting. After this call,
            //this.Description describes the service
            //as it was configured.
            base.ApplyConfiguration();
            //(rest of implementation elided for clarity)
        }
    }

    public class SelfDescribingServiceHostFactory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType,
         Uri[] baseAddresses)
        {
            //All the custom factory does is return a new instance
            //of our custom host class. The bulk of the custom logic should
            //live in the custom host (as opposed to the factory) 
            //for maximum
            //reuse value outside of the IIS/WAS hosting environment.
            Console.WriteLine("SelfDescribingServiceHostFactory");
            return new SelfDescribingServiceHost(serviceType,
                                                 baseAddresses);
        }
    }
    #endregion
}
