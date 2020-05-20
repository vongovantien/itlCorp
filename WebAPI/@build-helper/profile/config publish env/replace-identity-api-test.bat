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

%SYSTEMROOT%\System32\inetsrv\appcmd stop apppool /apppool.name:"api-efms-identity"
move .\IdentityServer .\bak-versions\IdentityServer_bak%datetimef%
move .\publish-verions\IdentityServer_%1% .\IdentityServer
%SYSTEMROOT%\System32\inetsrv\appcmd start apppool /apppool.name:"api-efms-identity"
icacls .\IdentityServer /grant "iis apppool\api-efms-identity":(OI)(CI)(RX,W,M) /T

exit 0


