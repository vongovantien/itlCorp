using eFMS.API.Accounting.DL.Models;
using System.Collections.Generic;
using System.ComponentModel;

namespace eFMS.API.Accounting.DL.Common
{
    public enum PaymentType {
        Invoice,
        OBH
    }
    public enum OverDueDate
    {
        All,
        Between1_15,
        Between16_30,
        Between31_60,
        Between61_90
    }
    public enum TransactionTypeEnum
    {
        InlandTrucking = 1,
        AirExport = 2,
        AirImport = 3,
        SeaConsolExport = 4,
        SeaConsolImport = 5,
        SeaFCLExport = 6,
        SeaFCLImport = 7,
        SeaLCLExport = 8,
        SeaLCLImport = 9
    }
    public enum JobStatus
    {
        [Description("Overdued")]
        Overdued = 1,
        [Description("Processing")]
        Processing = 2,
        [Description("InSchedule")]
        InSchedule = 3,
        [Description("Pending")]
        Pending = 4,
        [Description("Finish")]
        Finish = 5,
        [Description("Canceled")]
        Canceled = 6,
        [Description("Warning")]
        Warning = 7
    }
    public enum StageEnum
    {
        [Description("InSchedule")]
        InSchedule = 1,
        [Description("Processing")]
        Processing = 2,
        [Description("Done")]
        Done = 3,
        [Description("Overdued")]
        Overdue = 4,
        [Description("Pending")]
        Pending = 5,
        [Description("Deleted")]
        Deleted = 6,
        [Description("Warning")]
        Warning = 7
    }

    public enum ARTypeEnum
    {
        TrialOrOffical = 1,
        Guarantee = 2,
        Other = 3
    }

    public static class TermData
    {
        public static readonly string Canceled = "Canceled";

        public static readonly string InSchedule = "InSchedule";
        public static readonly string Processing = "Processing";
        public static readonly string Done = "Done";
        public static readonly string Overdue = "Overdued";
        public static readonly string Pending = "Pending";
        public static readonly string Deleted = "Deleted";
        public static readonly string Warning = "Warning";
        public static readonly string Finish = "Finish";

        public static readonly string InlandTrucking = "InlandTrucking";
        public static readonly string AirExport  = "AirExport";
        public static readonly string AirImport  = "AirImport";
        public static readonly string SeaConsolExport = "SeaConsolExport";
        public static readonly string SeaConsolImport = "SeaConsolImport";
        public static readonly string SeaFCLExport = "SeaFCLExport";
        public static readonly string SeaFCLImport = "SeaFCLImport";
        public static readonly string SeaLCLExport = "SeaLCLExport";
        public static readonly string SeaLCLImport = "SeaLCLImport";

        public static readonly string AR_TrialOrOffical = "AR_TrialOrOffical";
        public static readonly string AR_Guarantee = "AR_Guarantee";
        public static readonly string AR_Other = "AR_Other";

        public static readonly List<FreightTerm> FreightTerms = new List<FreightTerm>
        {
            new FreightTerm { Value = "Collect", DisplayName = "Collect" },
            new FreightTerm { Value = "Prepaid", DisplayName = "Prepaid" }
        };
        public static readonly List<ShipmentType> ShipmentTypes = new List<ShipmentType>
        {
            new ShipmentType { Value = "Freehand", DisplayName = "Freehand" },
            new ShipmentType { Value = "Nominated", DisplayName = "Nominated" }
        };
        public static readonly List<BillofLadingType> BillofLadingTypes = new List<BillofLadingType>
        {
            new BillofLadingType { Value = "Copy", DisplayName = "Copy" },
            new BillofLadingType { Value = "Original", DisplayName = "Original" },
            new BillofLadingType { Value = "Sea Waybill", DisplayName = "Sea Waybill" },
            new BillofLadingType { Value = "Surendered", DisplayName = "Surendered" }
        };
        public static readonly List<ServiceType> ServiceTypes = new List<ServiceType>
        {
            new ServiceType { Value = "FCL/FCL", DisplayName = "FCL/FCL" },
            new ServiceType { Value = "LCL/LCL", DisplayName = "LCL/LCL" },
            new ServiceType { Value = "FCL/LCL", DisplayName = "FCL/LCL" },
            new ServiceType { Value = "CY/CFS", DisplayName = "CY/CFS" },
            new ServiceType { Value = "CY/CY", DisplayName = "CY/CY" },
            new ServiceType { Value = "CFS/CY", DisplayName = "CFS/CY" },
            new ServiceType { Value = "CFS/CFS", DisplayName = "CFS/CFS" }
        };
        public static readonly List<TypeOfMove> TypeOfMoves = new List<TypeOfMove>
        {
            new TypeOfMove { Value = "FCL/FCL-CY/CY", DisplayName = "FCL/FCL-CY/CY" },
            new TypeOfMove { Value = "LCL/LCL-CFS/CFS", DisplayName = "LCL/LCL-CFS/CFS" },
            new TypeOfMove { Value = "LCL/FCL-CFS/CY", DisplayName = "LCL/FCL-CFS/CY" },
            new TypeOfMove { Value = "FCL/LCL-CY/CFS", DisplayName = "FCL/LCL-CY/CFS" }
        };
        public static readonly List<ProductService> ProductServices = new List<ProductService>
        {
            new ProductService { Value = "SeaFCL", DisplayName = "Sea FCL" },
            new ProductService { Value = "SeaLCL", DisplayName = "Sea LCL" },
            new ProductService { Value = "Air", DisplayName = "Air" },
            new ProductService { Value = "Trucking", DisplayName = "Trucking" },
            new ProductService { Value = "Warehouse", DisplayName = "Warehouse" },
            new ProductService { Value = "Other", DisplayName = "Other" }
        };
        public static readonly List<ServiceMode> ServiceModes = new List<ServiceMode>
        {
            new ServiceMode { Value = "Export", DisplayName = "Export" },
            new ServiceMode { Value = "Import", DisplayName = "Import" }
        };
        public static readonly List<ShipmentMode> ShipmentModes = new List<ShipmentMode>
        {
            new ShipmentMode { Value = "Internal", DisplayName = "Internal" },
            new ShipmentMode { Value = "External", DisplayName = "External" }
        };
    }
}
