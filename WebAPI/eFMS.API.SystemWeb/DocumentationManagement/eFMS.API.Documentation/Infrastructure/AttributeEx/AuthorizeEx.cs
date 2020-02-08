using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;

namespace eFMS.API.Documentation.Infrastructure.AttributeEx
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AuthorizeExAttribute : AuthorizeAttribute
    {
        public AuthorizeExAttribute(params object[] permissions)
        {
            Roles = string.Join(",", permissions.Select(permission => string.Format("{0}", permission)));
        }
        public AuthorizeExAttribute(Menu menu, UserPermission permission)
        {
            Roles = string.Format("{0}.{1}", menu, permission);
        }
    }
    public enum UserPermission
    {
        AllowAccess = 0,
        Add = 1,
        Update = 2,
        Delete = 3,
        List = 4,
        Import = 5,
        Export = 6,
        Detail = 7
    }
    public enum Menu
    {
        acct,
        acctAP,
        acctARP,
        acctSOA,
        acctSP,
        cat,
        catCommodity,
        catCurrency,
        catCharge,
        catLocation,
        catPartnerdata,
        catPortindex,
        catStage,
        catUnit,
        catWarehouse,
        design,
        designForm,
        designTable,
        doc,
        docAirExport,
        docAirImport,
        docInlandTrucking,
        docSeaConsolExport,
        docSeaConsolImport,
        docSeaFCLExport,
        docSeaFCLImport,
        docSeaLCLExport,
        docSeaLCLImport,
        ops,
        opsAssignment,
        opsCustomClearance,
        opsJobManagement,
        opsTruckingAssignment,
        report,
        reportPerformanceReport,
        reportPL,
        reportShipmentOverview,
        setting,
        settingCatalogLogViewer,
        settingEcusConnection,
        settingExchangeRate,
        settingIDDefinition,
        settingKPI,
        settingSupplier,
        settingTariff,
        settingUnlock,
        sys,
        sysCompany,
        sysDepartment,
        sysGroup,
        sysOffice,
        sysPermission,
        ysRole,
        sysUserManagement,
        tcon1,
        tcon2,
        tool2
    }
}
