for %%d in (	
	.\eFMS.API.Accounting.DAL,
	.\eFMS.API.Accounting.DL,
	.\eFMS.API.Accounting	
) do dotnet build "%%d"
@echo off
pause