/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;
using System.Collections;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Web.Services.Description;
using System.Xml;
using TIBCO.EMS;

namespace com.tibco.wcf.tems
{
    class TemsMessageProtocolSoapOverJMS : TemsMessageProtocol
    {
        #region Constructors
        public TemsMessageProtocolSoapOverJMS()
            : base()
        {
        }
        #endregion

        public override System.ServiceModel.Channels.Message Receive(TIBCO.EMS.Message emsMessage, TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsMessageProtocolSoapOverJMS.Receive called.");

            return base.Receive(emsMessage, timeout);
        }

        public override TIBCO.EMS.Message Send(System.ServiceModel.Channels.Message message, TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsMessageProtocolSoapOverJMS.Send called.");

            return base.Send(message, timeout);
        }

        public override System.ServiceModel.Channels.Message ReceiveTransform(TIBCO.EMS.Message emsMessage, System.ServiceModel.Channels.Message wcfMessage, TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsMessageProtocolSoapOverJMS.ReceiveTransform called.");

            wcfMessage.Headers.To = DestinationToUri(emsMessage.Destination);
            wcfMessage.Headers.ReplyTo = DestinationToEndpointAddress(emsMessage.ReplyTo);
            wcfMessage.Headers.MessageId = new UniqueId(emsMessage.MessageID);

            if (emsMessage.CorrelationID != null)
            {
                string correlatedMessageId = getCorrelatedMessageId(emsMessage.CorrelationID);

                if (correlatedMessageId == null)
                {
                    correlatedMessageId = emsMessage.CorrelationID;
                }

                wcfMessage.Headers.RelatesTo = new UniqueId(correlatedMessageId);
            }

            string soapAction = emsMessage.GetStringProperty("SoapAction");

            // Remove double quotes added by BW around the Action string if present.
            if (soapAction != null && soapAction.StartsWith("\"") && soapAction.EndsWith("\""))
            {
                soapAction = soapAction.Substring(1, soapAction.Length - 2);
            }

            wcfMessage.Headers.Action = soapAction;

            Destination faultDestination = (Destination)emsMessage.GetObjectProperty("SOAPFaultAddress");
            wcfMessage.Headers.FaultTo = DestinationToEndpointAddress(faultDestination);

            return wcfMessage;
        }

        public override TIBCO.EMS.Message SendTransform(TIBCO.EMS.Message emsMessage, System.ServiceModel.Channels.Message wcfMessage, TimeSpan timeout)
        {
            TemsTrace.WriteLine(TraceLevel.Verbose, "TemsMessageProtocolSoapOverJMS.SendTransform called.");

            // Set from the JMS header values.
            if (isClient)
            {
                emsMessage.Destination = toDestination;
                emsMessage.ReplyTo = replyToDestination;
            }
            else
            {
                emsMessage.Destination = DestinationForUri(wcfMessage.Headers.To);
            }

            // Need to be able to correlate the WCF messageId with the EMS MessageID.
            // The EMS MessageID value cannot be set because it is set by the MessageProducer when
            // Send is called.  The messageIdCorrelations Hashtable is used to match the
            // EMS MessageID (key) with the WCF messageId (value).
            // The MessageId may be null for reply messages.
            if (wcfMessage.Headers.MessageId != null)
            {
                emsMessage.SetStringProperty(TemsMessageIdProperty, wcfMessage.Headers.MessageId.ToString());
            }

            if (wcfMessage.Headers.RelatesTo != null)
            {
                emsMessage.CorrelationID = wcfMessage.Headers.RelatesTo.ToString();
            }

            // Set from the JMS property values.
            emsMessage.SetStringProperty("SoapAction", wcfMessage.Headers.Action);

            if (wcfMessage.Headers.FaultTo != null)
            {
                emsMessage.SetObjectProperty("SOAPFaultAddress", DestinationForUri(wcfMessage.Headers.FaultTo.Uri));
            }

            emsMessage.SetStringProperty("Mime_Version", "1.0");

            bool isSoap11 = wcfMessage.Version.Envelope.ToString().StartsWith("Soap11");
            string contentType = isSoap11 ? "application/xml" : "application/soap+xml";
            emsMessage.SetStringProperty("Content_Type", contentType);

            return emsMessage;
        }
    }
}
