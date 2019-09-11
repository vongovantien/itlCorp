Generate datacontext:

* Identity server:
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.IdentityServer.Service -Force -Tables sysUser, sysEmployee, sysUserGroup, sysBranch, sysBU, sysDepartment, sysGroup, sysGroupRole, sysMenu, sysRole, sysRoleMenu, sysRolePermission, sysUserRole, sysUserLog


* System server
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.System.Service -Force -Table catPlace, sysUser, sysEmployee, sysUserGroup, sysBranch, sysBU, sysDepartment, sysGroup, sysGroupRole, sysMenu, sysRole, sysRoleMenu, sysRolePermission, sysUserRole

* Catalogue

* Operation
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.Operation.Service -Force -Table catCommodity, CustomsDeclaration, opsStageAssigned, opsTransaction, setECUSConnection, sysEmployee, sysUser

* Documentation
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.Documentation.Service -Force

* Accounting
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.Accounting.Service -Force -Table acctAdvancePayment, acctAdvanceRequest, acctApproveAdvance, acctApproveSettlement, acctCDNote, acctSettlementPayment, acctSOA