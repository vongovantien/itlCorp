namespace eFMS.API.Common.Globals
{
    public enum CatPlaceTypeEnum
    {
        BorderGate = 1,
        Branch = 2,
        Depot = 3,
        District = 4,
        Hub = 5,
        IndustrialZone = 6,
        Other = 7,
        Port = 8,
        Province = 9,
        Station = 10,
        Ward = 11,
        Warehouse = 12
    }

    public enum SearchCondition
    {
        AND,
        OR
    }

    public enum CatPartnerGroupEnum
    {
        AGENT = 1,
        CONSIGNEE = 2,
        CUSTOMER = 3,
        PAYMENTOBJECT = 4,
        PETROLSTATION = 5,
        SHIPPER = 6,
        SHIPPINGLINE = 7,
        SUPPLIER = 8,
        SUPPLIERMATERIAL = 9,
        CARRIER = 10,
        AIRSHIPSUP = 11
    }
    public enum CatChargeType
    {
        CREDIT = 1,
        DEBIT = 2,
        OBH = 3
    }
    public enum Crud
    {
        Get,
        Insert,
        Update,
        Delete
    }
    public enum ExportFormatType
    {
        //
        // Summary:
        //     No export format specified.
        NoFormat = 0,
        //
        // Summary:
        //     Export format of the report is a Crystal Report file.
        CrystalReport = 1,
        //
        // Summary:
        //     Export format of the report is a rich text file.
        RichText = 2,
        //
        // Summary:
        //     Export format of the report is a Microsoft Word file.
        WordForWindows = 3,
        //
        // Summary:
        //     Export format of the report is a Microsoft Excel file.
        Excel = 4,
        //
        // Summary:
        //     Export format of the report is a PDF file.
        PortableDocFormat = 5,
        //
        // Summary:
        //     Export format of the report is an HTML 3.2 file.
        HTML32 = 6,
        //
        // Summary:
        //     Export format of the report is an HTML 4.0 file.
        HTML40 = 7,
        //
        // Summary:
        //     Export format of the report is an Excel Record file.
        ExcelRecord = 8
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
    public enum PermissionRange
    {
        None,
        Owner,
        Group,
        Department,
        Office,
        Company,
        All
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
        reportGeneral,
        reportSale,
        reportSheetDebit,
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
        sysAuthorize,
        sysCompany,
        sysDepartment,
        sysGroup,
        sysOffice,
        sysPermission,
        ysRole,
        sysUserManagement,
        tcon1,
        tcon2,
        tool2,
        catChartOfAccounts,
        accManagement,
        settingUnlockRequest,
        commercialCustomer,
        commercialAgent,
        commercialIncoterm
    }
    public enum UnlockTypeEnum
    {
        SHIPMENT = 1,
        ADVANCE = 2,
        SETTLEMENT = 3,
        CHANGESERVICEDATE = 4
    }
}