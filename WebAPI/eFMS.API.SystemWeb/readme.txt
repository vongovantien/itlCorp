Generate datacontext:

* Identity server:
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.IdentityServer.Service -Force -Tables sysUser, sysEmployee, sysUserLevel, sysBU, sysDepartment, sysGroup, sysMenu, sysRole, sysUserLog, catDepartment, sysOffice, sysUserPermission, sysUserPermissionGeneral, sysUserPermissionSpecial, sysAuthorization


* System server
 Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.System.Service -Force -Table catPlace, sysUser, sysEmployee, sysUserLevel, sysOffice, sysCompany, catDepartment, sysGroup, sysMenu, sysRole,  sysImage, sysPermissionSample, sysPermissionSampleGeneral,sysPermissionSampleSpecial, sysMenu, sysPermissionSpecialAction, sysUserPermissionGeneral, sysUserPermissionSpecial, sysAuthorization, sysUserPermission

* Catalogue
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.Catalogue.Service -Force 

* Operation
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.Operation.Service -Force -Table catCommodity, CsTransaction, CsTransactionDetail, CustomsDeclaration, opsStageAssigned, opsTransaction, setECUSConnection, sysEmployee, sysUser

* Documentation
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.Documentation.Service -Force

* Accounting
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.Accounting.Service -Force -Table AcctAdvancePayment, AcctAdvanceRequest, AcctApproveAdvance, AcctApproveSettlement, AcctCdnote, AcctSettlementPayment, AcctSoa, CatCharge, CatCountry, CatCurrency, CatCurrencyExchange, CatDepartment, CatPartner, CatPlace, CatUnit, CsMawbcontainer, CsShipmentSurcharge, CsTransaction, CsTransactionDetail, CustomsDeclaration, OpsStageAssigned, OpsTransaction, SysCompany, SysEmployee, SysGroup, SysOffice, SysUser, SysUserLevel

* Setting
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.Setting.Service -Force