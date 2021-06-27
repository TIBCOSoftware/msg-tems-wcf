@echo off
set ISVISTA=0
ver | findstr /c:" 6." && set ISVISTA=1

echo Create bin directory
if not exist %SystemDrive%\inetpub\wwwroot\TemsSamples\bin mkdir %SystemDrive%\inetpub\wwwroot\TemsCalculator\bin

echo Create protocol binding for "net.tems".
if "%ISVISTA%" == "1" (
	%windir%\system32\inetsrv\AppCmd.exe set site "Default Web Site" /-bindings.[protocol='net.tems'] > NUL:
	%windir%\system32\inetsrv\AppCmd.exe set site "Default Web Site" /+bindings.[protocol='net.tems',bindingInformation='7222']
)

echo Create virtual directory.
if "%ISVISTA%" == "1" (
	%windir%\system32\inetsrv\AppCmd.exe delete app /app.name:"Default Web Site/TemsCalculator" > NUL:
        %windir%\system32\inetsrv\AppCmd.exe add app /site.name:"Default Web Site" /path:/TemsCalculator /physicalPath:%SystemDrive%\inetpub\wwwroot\TemsCalculator /enabledProtocols:http,net.tems
)

@echo on
