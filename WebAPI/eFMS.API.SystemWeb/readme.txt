Generate datacontext:

* Identity server:
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.IdentityServer.Service -Force -Tables sysUser, sysEmployee, sysUserGroup, sysBranch, sysBU, sysDepartment, sysGroup, sysGroupRole, sysMenu, sysRole, sysRoleMenu, sysRolePermission, sysUserRole


* System server
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.System.Service -Force -Table catPlace, sysUser, sysEmployee, sysUserGroup, sysBranch, sysBU, sysDepartment, sysGroup, sysGroupRole, sysMenu, sysRole, sysRoleMenu, sysRolePermission, sysUserRole

* Catalogue
