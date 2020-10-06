for %%d in (
	..\eFMS.API.SystemWeb\eFMS.API.Accounting\bin\netcoreapp2.2\,
	..\eFMS.API.SystemWeb\eFMS.API.Catalogue\bin\netcoreapp2.2\,
	..\eFMS.API.SystemWeb\eFMS.API.Documentation\bin\netcoreapp2.2\,
	..\eFMS.API.SystemWeb\eFMS.API.ReportData\bin\netcoreapp2.2\,
	..\eFMS.API.SystemWeb\eFMS.IdentityServer\bin\netcoreapp2.2\,
	..\eFMS.API.SystemWeb\eFMS.API.Operation\bin\netcoreapp2.2\,	
	..\eFMS.API.SystemWeb\eFMS.API.Setting\bin\netcoreapp2.2\,
	..\eFMS.API.SystemWeb\eFMS.API.System\bin\netcoreapp2.2\,
	..\eFMS.API.SystemWeb\eFMS.API.ForPartner\bin\netcoreapp2.2\
) do xcopy "..\eFMS.API.Common\bin\netcoreapp2.2\eFMS.API.Infrastructure.*" "%%d" /E /Y