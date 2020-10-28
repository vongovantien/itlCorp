using eFMS.API.Documentation.DL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace eFMS.API.Documentation.DL.Common
{
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
        SeaLCLImport = 9,
        CustomLogistic = 10
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

        public static readonly string CustomLogistic = "CL";
        public static readonly string InlandTrucking = "IT";//"InlandTrucking";
        public static readonly string AirExport = "AE";//"AirExport";
        public static readonly string AirImport = "AI";//"AirImport";
        public static readonly string SeaConsolExport = "SCE";//"SeaConsolExport";
        public static readonly string SeaConsolImport = "SCI";//"SeaConsolImport";
        public static readonly string SeaFCLExport = "SFE";//"SeaFCLExport";
        public static readonly string SeaFCLImport = "SFI";//"SeaFCLImport";
        public static readonly string SeaLCLExport = "SLE";//"SeaLCLExport";
        public static readonly string SeaLCLImport = "SLI";//"SeaLCLImport";

        public static readonly string CD_NOTE_NEW = "New";

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
            new BillofLadingType { Value = "Surrendered", DisplayName = "Surrendered" }
        };
        public static readonly List<ServiceType> ServiceTypes = new List<ServiceType>
        {
            new ServiceType { Value = "FCL/FCL", DisplayName = "FCL/FCL" },
            new ServiceType { Value = "LCL/LCL", DisplayName = "LCL/LCL" },
            new ServiceType { Value = "FCL/LCL", DisplayName = "FCL/LCL" },
            new ServiceType { Value = "CY/CFS", DisplayName = "CY/CFS" },
            new ServiceType { Value = "CY/CY", DisplayName = "CY/CY" },
            new ServiceType { Value = "CFS/CY", DisplayName = "CFS/CY" },
            new ServiceType { Value = "CFS/CFS", DisplayName = "CFS/CFS" },
            new ServiceType { Value = "CY/DR", DisplayName = "CY/DR" },
            new ServiceType { Value = "DR/CY", DisplayName = "DR/CY" },
            new ServiceType { Value = "DR/DR", DisplayName = "DR/DR" },
            new ServiceType { Value = "DR/CFS", DisplayName = "DR/CFS" },
            new ServiceType { Value = "CFS/DR", DisplayName = "CFS/DR" },
        };
        public static readonly List<TypeOfMove> TypeOfMoves = new List<TypeOfMove>
        {
            new TypeOfMove { Value = "FCL/FCL-CY/CY", DisplayName = "FCL/FCL-CY/CY" },
            new TypeOfMove { Value = "LCL/LCL-CY/CY", DisplayName = "LCL/LCL-CY/CY" },
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
            new ProductService { Value = "Crossborder", DisplayName = "Cross border" },
            new ProductService { Value = "Warehouse", DisplayName = "Warehouse" },
            new ProductService { Value = "Railway", DisplayName = "Railway" },
            new ProductService { Value = "Express", DisplayName = "Express" },
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
