@echo off
set IsWindows6=0
ver | findstr /c:" 6." && set IsWindows6=1

echo Create bin directory
if not exist %SystemDrive%\inetpub\wwwroot\ServiceModelSamples\bin mkdir %SystemDrive%\inetpub\wwwroot\ServiceModelSamples\bin

echo Create protocol binding for "net.tems".
if "%IsWindows6%" == "1" (
	%windir%\system32\inetsrv\AppCmd.exe set site "Default Web Site" /-bindings.[protocol='net.tems'] > NUL:
	%windir%\system32\inetsrv\AppCmd.exe set site "Default Web Site" /+bindings.[protocol='net.tems',bindingInformation='7222']
)

echo Create virtual directory.
if "%IsWindows6%" == "1" (
	%windir%\system32\inetsrv\AppCmd.exe delete app /app.name:"Default Web Site/ServiceModelSamples" > NUL:
        %windir%\system32\inetsrv\AppCmd.exe add app /site.name:"Default Web Site" /path:/ServiceModelSamples /physicalPath:%SystemDrive%\inetpub\wwwroot\ServiceModelSamples /enabledProtocols:http,net.tems
)

@echo on
