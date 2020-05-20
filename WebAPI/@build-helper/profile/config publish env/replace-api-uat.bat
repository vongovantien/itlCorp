%SYSTEMROOT%\System32\inetsrv\appcmd stop apppool /apppool.name:"api-efms.itlvn.com"
move .\Accounting .\bak-versions\Accounting_bak%2
move .\publish-verions\Accounting_%1% .\Accounting
%SYSTEMROOT%\System32\inetsrv\appcmd start apppool /apppool.name:"api-efms.itlvn.com"
icacls .Accounting /grant "iis apppool\api-efms.itlvn.com":(OI)(CI)(RX,W,M) /T

%SYSTEMROOT%\System32\inetsrv\appcmd stop apppool /apppool.name:"api-efms.itlvn.com"
move .\Catalogue .\bak-versions\Catalogue_bak%2 
move .\publish-verions\Catalogue_%1% .\Catalogue 
%SYSTEMROOT%\System32\inetsrv\appcmd start apppool /apppool.name:"api-efms.itlvn.com"
icacls .Catalogue /grant "iis apppool\api-efms.itlvn.com":(OI)(CI)(RX,W,M) /T

%SYSTEMROOT%\System32\inetsrv\appcmd stop apppool /apppool.name:"api-efms.itlvn.com"
move .\Documentation .\bak-versions\Documentation_bak%2
move .\publish-verions\Documentation_%1% .\Documentation
%SYSTEMROOT%\System32\inetsrv\appcmd start apppool /apppool.name:"api-efms.itlvn.com"
icacls .Documentation /grant "iis apppool\api-efms.itlvn.com":(OI)(CI)(RX,W,M) /T

%SYSTEMROOT%\System32\inetsrv\appcmd stop apppool /apppool.name:"api-efms.itlvn.com"
move .\Export .\bak-versions\Export_bak%2 
move .\publish-verions\Export_%1% .\Export
%SYSTEMROOT%\System32\inetsrv\appcmd start apppool /apppool.name:"api-efms.itlvn.com"
icacls .Export /grant "iis apppool\api-efms.itlvn.com":(OI)(CI)(RX,W,M) /T

%SYSTEMROOT%\System32\inetsrv\appcmd stop apppool /apppool.name:"api-efms.itlvn.com"
move .\Operation .\bak-versions\Operation_bak%2 
move .\publish-verions\Operation_%1% .\Operation
%SYSTEMROOT%\System32\inetsrv\appcmd start apppool /apppool.name:"api-efms.itlvn.com"
icacls .Operation /grant "iis apppool\api-efms.itlvn.com":(OI)(CI)(RX,W,M) /T

%SYSTEMROOT%\System32\inetsrv\appcmd stop apppool /apppool.name:"api-efms.itlvn.com"
move .\Setting .\bak-versions\Setting_bak%2
move .\publish-verions\Setting_%1% .\Setting
%SYSTEMROOT%\System32\inetsrv\appcmd start apppool /apppool.name:"api-efms.itlvn.com"
icacls .Setting /grant "iis apppool\api-efms.itlvn.com":(OI)(CI)(RX,W,M) /T

%SYSTEMROOT%\System32\inetsrv\appcmd stop apppool /apppool.name:"api-efms.itlvn.com"
move .\System .\bak-versions\System_bak%2
move .\publish-verions\System_%1% .\System
%SYSTEMROOT%\System32\inetsrv\appcmd start apppool /apppool.name:"api-efms.itlvn.com"
icacls .System /grant "iis apppool\api-efms.itlvn.com":(OI)(CI)(RX,W,M) /T

exit 0