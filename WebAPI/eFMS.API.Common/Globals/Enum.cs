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
}