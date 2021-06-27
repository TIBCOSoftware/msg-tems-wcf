@echo off
rem
rem Command file for running sample applications
rem

:Start

echo ********************************************************************************
echo                           Tems Samples Launch Utility
echo.
echo ********************************************************************************

:SetStart
set type=B
set /a clientCnt=1
set /p type=Start: [B]oth, [S]ervice, [C]lient, default=B 
if /i %type%==S (
    rem
) else (
    if /i %type%==C (
        set /p clientCnt=How may clients: default=1 
    ) else (
        if /i not %type%==B (
            echo Entry invalid.
            goto SetStart
        )
    )
)

:SetMEP
set mep=R
set /p mep=MEP: [R]eqReply, [A]AsyncRR [O]ne way, [D]uplex, [T]Trans Duplex, default=R 
if /i %mep%==R (
    rem
) else (
    if /i %mep%==A (
        rem
    ) else (
        if /i %mep%==O (
            rem
        ) else (
            if /i %mep%==D (
                rem
            ) else (
                if /i %mep%==T (
                    rem
                ) else (
                    echo Entry invalid.
                    goto SetMEP
                )
            )
        )
    )
)

:SetSessionful
set sessionful=N
set /p sessionful=Sessionful: [N]ot Sessionful, [S]essionful, default=N 
if /i %sessionful%==S (
    rem
) else (
    if /i %sessionful%==N (
        rem
    ) else (
        echo Entry invalid.
        goto SetSessionful
    )
)

:SetLogLevel
set logLevel=I
set /p logLevel=LogLevel: [I]nfo, [V]erbose, default=I 
if /i %logLevel%==I (
    set logLevel=3
) else (
    if /i %logLevel%==V (
        set logLevel=4
    ) else (
        echo Entry invalid.
        goto SetLogLevel
    )
)

if /i %type%==S (
    goto SetCommandLineParams
)

set iterations=3000
set /p iterations=Enter default number of client request iterations: default=%iterations% 

:SetCommandLineParams

if /i %sessionful%==s (
    set clientNameR=Tems.RequestReplySession
    set clientNameA=Tems.RequestReplyAsync
    set clientNameO=Tems.DatagramSession
    set clientNameD=Tems.DuplexSession
    set clientNameT=Tems.DuplexTransaction

    set serviceNameR=ServiceRequestReplySessionType
    set serviceNameA=ServiceRequestReplyAsyncType
    set serviceNameO=ServiceDatagramSessionType
    set serviceNameD=ServiceDuplexSessionType
    set serviceNameT=ServiceDuplexTransactionType

) else (
    set clientNameR=Tems.RequestReply
    set clientNameA=Tems.RequestReplyAsync
    set clientNameO=Tems.Datagram
    set clientNameD=Tems.Duplex
    set clientNameT=Tems.DuplexTransaction

    set serviceNameR=ServiceRequestReplyType
    set serviceNameA=ServiceRequestReplyAsyncType
    set serviceNameO=ServiceDatagramType
    set serviceNameD=ServiceDuplexType
    set serviceNameT=ServiceDuplexTransactionType
)

if /i %mep%==R (
    set clientName=%clientNameR%
    set serviceName=%serviceNameR%
) else (
    if /i %mep%==A (
        set clientName=%clientNameA%
        set serviceName=%serviceNameA%
    ) else (
        if /i %mep%==O (
            set clientName=%clientNameO%
            set serviceName=%serviceNameO%
        ) else (
            if /i %mep%==D (
                set clientName=%clientNameD%
                set serviceName=%serviceNameD%
            ) else (
                if /i %mep%==T (
                    set clientName=%clientNameT%
                    set serviceName=%serviceNameT%
                )
            )
        )
    )
)
echo.
echo.
if /i %type%==S (
    cd Host\bin\x64\Debug\
    echo Starting sample Service application:
    echo start Host\bin\x64\Debug\Host.exe -type %serviceName% -log %logLevel%
    start Host.exe -type %serviceName% -log %logLevel%
    cd ..\..\..\..
) else (
    if /i %type%==C (
        cd Client\bin\x64\Debug\
        echo Starting sample Client application %clientCnt% times:
        echo start Client.exe -ep %clientName% -log %logLevel% -iter %iterations%
        if %clientCnt%==1 (
           start Client.exe -ep %clientName% -log %logLevel% -iter %iterations%
        )else (
           for /l %%X in (1,1,%clientCnt%)do (start Client.exe -ep %clientName% -log %logLevel% -iter %iterations% -run)
        )
        cd ..\..\..\..
    ) else (
        cd Host\bin\x64\Debug\
        echo Starting sample Service application:
        echo start Host.exe -type %serviceName% -log %logLevel%
        start Host.exe -type %serviceName% -log %logLevel%
        echo.
        echo.
        cd ..\..\..\..
        cd Client\bin\x64\Debug\
        echo Starting sample Client application:
        echo start Client.exe -ep %clientName% -log %logLevel% -iter %iterations%
        start Client.exe -ep %clientName% -log %logLevel% -iter %iterations%
        cd ..\..\..\..
    )
)
echo.
echo.
set continue=yes
set /p continue=Press any key to continue or X to exit: 
echo.
echo.
if /i not %continue%==X (
    goto Start
)
