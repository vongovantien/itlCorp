FOR /f "delims=" %%a IN ('call profile\ini.bat profile\environment.ini %2 %1') DO (
    SET val=%%a
)

if not exist "%val%_%3" mkdir "%val%_%3"

call dotnet publish "..\eFMS.API.SystemWeb\%1\%1"  -c Release --output="%val%_%3"
call echo f | xcopy "profile\%2\appsettings.json" "%val%_%3\appsettings.json" /f /y 