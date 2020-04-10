dotnet build "..\eFMS.API.SystemWeb\CatalogueManagement\eFMS.API.Catalogue" --configuration Release
call dotnet publish "..\eFMS.API.SystemWeb\CatalogueManagement\eFMS.API.Catalogue" -c Release --output="D:\Workspace\efms\PublishStaging"
pause