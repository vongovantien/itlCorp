for %%d in (
	..\eFMS.API.SystemWeb\eFMS.IdentityServer\eFMS.IdentityServer.DAL,
	..\eFMS.API.SystemWeb\eFMS.IdentityServer\eFMS.IdentityServer.DL
) do dotnet build "%%d"