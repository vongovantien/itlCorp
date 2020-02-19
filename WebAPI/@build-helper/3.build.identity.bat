for %%d in (
	..\eFMS.API.SystemWeb\IdentityServer\eFMS.IdentityServer.DAL,
	..\eFMS.API.SystemWeb\IdentityServer\eFMS.IdentityServer.DL
) do dotnet build "%%d"