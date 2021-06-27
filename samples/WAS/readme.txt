TIBCO Software, Inc.

WAS for Tems Samples

NOTE:  The Net.Tems Listener Adapter service runs under the built in account "Network Service".  Network Service
needs to have "Read & execute", "List folder contents", "Read" permissions to %windir%\system23\inetsrv\config

Basic:

This sample implements a simple client and service.  
The service implements the ICalculator (wCF contract).  
You will need to create an IIS 7 application called ServiceModelSamples on the default website.
You will also need to create a bin directory under ServiceModelSamples.
Post build events in the service project, copy the files the files to this deployed application (at \inetpub\wwwroot).
There is a SetupApp.bat under the service directory that can be run to create the ServiceModelSamples directory, bin subdirectory
and the site bindings and enable the net.tems protocol. Alternately, you can use the IIS Console and you will need to do the following: 

	1. add net.tems binding to the default website.
	    (The format of the binding information is [host]:port)
	   The host and port refer to the machine and port for the EMS server
	   (example binding  = net.tems  localhost:7222)

	2. Enable the net.tems protocol for the application.  Select the ServiceModelSamples application
           from the IIS management console, go to Advanced Settings, under Behavior\Enabled Protocols add net.tems
           (example:  Enabled protocols http,net.tems allows application to use both http and net.tems) 

Be sure the "Net.Tems Listener Adpater" Service is started.
Use the client to test the service. (The queue is specified in the .config files of both the service and client)


Multi-Endpoint:

This sample demonstrates how to implement multiple WCF contracts in one IIS application.  The service implements
three WCF contracts: IDatagramContract, ICalculatorContract, IStatusContract
 

The IDatagramContract demonstrates a one way contract using net.tems
The IcalculatorContract demonstrates a request reply contract using net.tems
The IStatusContract uses http to report on the progress of the other contracts.

Post build events in the Service project properties will copy the files to the deployed application (at \inetpub\wwwroot).  There is a
SetupApp.bat that can be run to create the TemsCalculator directory and bin subdirectory and
the site bindings and enable the net.tems protocol. Alternately, you can use the IIS Console. 

This sample client does not use a generated proxy (i.e. no generatedClient.cs using svcutil.exe).
Instead, the client code demonstartes how to create a channel factory to use for communicating to the service.

The .config files also contain all values being explicitly set for the TemsBinding element.  The values specified
are the defaults.

Finally, the web.config file has logging enabled at the verbose level for both TEMS and WAS.  You will need to create
the c:\logs directory or modify the config files to locate the logs where you desire.  You will need to create the
direcotry for the logs as the logger does not do this.  The logs can be viewed with Microsoft's WCF Trace Viewer
which can be downloaded from Microsoft (normally found in ...\Microsoft SDKs\Windows\v6.0A\Bin\SvcTraceViewer.exe)

Be sure the "Net.Tems Listener Adpater" Service is started.
Use the client to test the service. (The queue is specified in the .config files of both the service and client)


Custom:

This sample demonstrates how to implement a custom TEMS binding element.  The CustomTemsBinding.dll implements the
ITemsBindingElement interface that is used Net.Tems Listener Adapter service to customize a TEMS Binding Element.
The sample demonstrates modifying the MessageDeliveryMode on the TemsTransportBindingElement.  The client does all
configuration in code and shares the CustomTemsBinding.dll

If you will be doing SSL or creating a connection factory, you will also need to implement a custom servicehost factory.
The service.svc file and code in Service.cs shows how a custom servicehost factory is implemented.  From here you could create the custom tems binding
and add the service endpoint(s) to the servicehost factory.

The custom servicehost is not needed in this sample since the WASDestination is not used by the WCF TEMS services.

The service implements the Icalculator interface and runs in TemsCalculator on the default web site.  Similar to Multi-Endpoint,
post build events and SetupApp.bat configure the application and deploy the files.  The CustomTemsBinding.dll should be put in the GAC.

Post build events in the Service project properties will copy the files to the deployed application (at \inetpub\wwwroot).  There is a
SetupApp.bat under the service directory that can be called to create create the TemsSamples directory and bin subdirectory and
the site bindings and enable the net.tems protocol. Alternately, you can use the IIS Console. 


