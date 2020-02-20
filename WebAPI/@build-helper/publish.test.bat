dotnet build "..\eFMS.API.SystemWeb\CatalogueManagement\eFMS.API.Catalogue" --configuration Release
call dotnet publish "..\eFMS.API.SystemWeb\CatalogueManagement\eFMS.API.Catalogue" -c Release --output="\\192.168.7.31\inetpub\efms-api\Catalogue"
pause