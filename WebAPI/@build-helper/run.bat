
call build-all.bat
set year=%date:~-4%
set month=%date:~3,2%
if "%month:~0,1%" == " " set month=0%month:~1,1%
set day=%date:~0,2%
if "%day:~0,1%" == " " set day=0%day:~1,1%
set hour=%time:~0,2%
if "%hour:~0,1%" == " " set hour=0%hour:~1,1%
set min=%time:~3,2%
if "%min:~0,1%" == " " set min=0%min:~1,1%
set secs=%time:~6,2%
if "%secs:~0,1%" == " " set secs=0%secs:~1,1%

set datetimef=%year%%month%%day%%hour%%min%

for %%a in (
	eFMS.API.Accounting,
eFMS.API.Catalogue,
eFMS.API.Documentation,
eFMS.API.ReportData,
eFMS.IdentityServer,
eFMS.API.Operation,
eFMS.API.Setting,
eFMS.API.System,
) do call publish-service.bat %%a %1 %2 %datetimef%