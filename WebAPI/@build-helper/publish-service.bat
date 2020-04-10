FOR /f "delims=" %%a IN ('call profile\ini.bat profile\environment.ini %2 %1') DO (
    SET val=%%a
)

if not exist "%val%_%3" mkdir "%val%_%3"

call echo f | xcopy "profile\appsettings-%2.json" "..\%1\%1\appsettings.json" /f /y 
call dotnet publish "..\%1\%1"  -c Release --output="%val%_%3"
call echo f | xcopy "profile\appsettings-dev.json" "..\%1\%1\appsettings.json" /f /y