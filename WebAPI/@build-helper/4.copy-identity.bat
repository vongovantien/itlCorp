for %%d in (
	..\eFMS.API.SystemWeb\AccountingManagement\bin\netcoreapp2.2\,
	..\eFMS.API.SystemWeb\CatalogueManagement\bin\netcoreapp2.2\,
	..\eFMS.API.SystemWeb\DocumentationManagement\bin\netcoreapp2.2\,
	..\eFMS.API.SystemWeb\eFMS.API.ReportData\bin\netcoreapp2.2\,
	..\eFMS.API.SystemWeb\OperationManagement\bin\netcoreapp2.2\,	
	..\eFMS.API.SystemWeb\SettingManagement\bin\netcoreapp2.2\,
	..\eFMS.API.SystemWeb\SystemManagement\bin\netcoreapp2.2\
) do (xcopy "..\eFMS.API.SystemWeb\IdentityServer\bin\netcoreapp2.2\eFMS.IdentityServer.Service.dll" "%%d" /E /Y
xcopy "..\eFMS.API.SystemWeb\IdentityServer\bin\netcoreapp2.2\eFMS.IdentityServer.Service.pdb" "%%d" /E /Y
xcopy "..\eFMS.API.SystemWeb\IdentityServer\bin\netcoreapp2.2\eFMS.IdentityServer.DL.*" "%%d" /E /Y)
pause