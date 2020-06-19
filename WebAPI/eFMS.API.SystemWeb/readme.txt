Generate datacontext:

* Identity server:
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.IdentityServer.Service -Force -Tables sysUser, sysEmployee, sysUserLevel, sysBU, sysDepartment, sysGroup, sysMenu, sysRole, sysUserLog, catDepartment, sysOffice, sysUserPermission, sysUserPermissionGeneral, sysUserPermissionSpecial, sysAuthorization


* System server
 Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.System.Service -Force -Table sysUser, sysEmployee, sysUserLevel, sysOffice, sysCompany, catDepartment, sysGroup, sysMenu, sysRole,  sysImage, sysPermissionSample, sysPermissionSampleGeneral,sysPermissionSampleSpecial, sysMenu, sysPermissionSpecialAction, sysUserPermissionGeneral, sysUserPermissionSpecial, sysAuthorization, sysUserPermission,sysAuthorizedApproval

* Catalogue
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.Catalogue.Service -Force -Table catArea, catCharge, catChargeDefaultAccount, catChargeGroup,catChargeGroup, catCommodityGroup, catContainerType, catCountry, catCurrency, catCurrencyExchange, catDepartment, catPartner, catPartnerCharge, catPartnerGroup, catPlace, catPlaceType, catPlaceType, catSaleman, catServiceType, catStage, catTransportationMode, catUnit, catChartOfAccounts , csManifest, csMawbcontainer, csShipmentSurcharge, csTransaction, csTransactionDetail, opsStageAssigned, opsTransaction, sysOffice, sysUser, catCommodity,sysEmployee, sysCompany

* Operation
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.Operation.Service -Force -Table catCommodity, CsTransaction, CsTransactionDetail, CustomsDeclaration, opsStageAssigned, opsTransaction, setECUSConnection, sysEmployee, sysUser

* Documentation
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.Documentation.Service -Force -Table acctCDNote, catCharge, catCommodity,CatChargeGroup, catCountry, catCurrency, catCurrencyExchange, catDepartment, catPartner, catPlace, catSaleman, catUnit, csAirWayBill, csArrivalAndDeliveryDefault, csArrivalFrieghtCharge, csArrivalFrieghtChargeDefault, csDimensionDetail, csManifest, csMAWBContainer, csShipmentOtherCharge, csShipmentSurcharge, csShippingInstruction, csTransaction, csTransactionDetail,csBookingNote, CustomsDeclaration, opsStageAssigned, opsTransaction, sysAuthorization, sysCompany, sysEmployee, sysGroup, sysImage, sysNotification, sysOffice, sysUser, sysUserNotification, SysUserLevel

* Accounting
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.Accounting.Service -Force -Table AcctAdvancePayment, AcctAdvanceRequest, AcctApproveAdvance, AcctApproveSettlement, AcctCdnote, AcctSettlementPayment, AcctSoa,catCommodity, CatCharge,CatChargeDefaultAccount, CatCountry, CatCurrency, CatCurrencyExchange, CatDepartment, CatPartner, CatPlace, CatUnit, CsMawbcontainer, CsShipmentSurcharge, CsTransaction, CsTransactionDetail, CustomsDeclaration, OpsStageAssigned, OpsTransaction, SysCompany, SysEmployee, SysGroup, SysOffice, SysUser, SysUserLevel,catCommodityGroup, accAccountingManagement, accAccountingPayment

* Setting
Scaffold-DbContext "Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context eFMSDataContextDefault -Project eFMS.API.Setting.Service -Force