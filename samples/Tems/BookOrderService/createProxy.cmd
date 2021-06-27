set svcutilPath="C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin\Svcutil.exe"
%svcutilPath% /t:code BookOrderService.wsdl BookOrderService.xsd
pause
