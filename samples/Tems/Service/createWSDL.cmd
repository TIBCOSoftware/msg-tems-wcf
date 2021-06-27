set servicePath=D:\TIBCO\EMS\Tems\Service
set clientPath=D:\TIBCO\EMS\Tems\Client
set svcutilPath="C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin\Svcutil.exe"
cd /d %servicePath%
set serviceAssembly=D:\TIBCO\EMS\Tems\Service\bin\Debug\Service.dll
%svcutilPath% %serviceAssembly%
%svcutilPath% /t:code tempuri.org.wsdl tempuri.org.xsd
copy %servicePath%\tempuri.org.cs %clientPath%\ISimpleServiceProxy.cs
pause
