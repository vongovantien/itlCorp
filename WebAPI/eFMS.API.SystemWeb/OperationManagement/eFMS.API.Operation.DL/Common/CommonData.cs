using System.Collections.Generic;

namespace eFMS.API.Operation.DL.Common
{
    public class CommonData
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }
    }

    public static class CustomData
    {
        public static readonly List<CommonData> Types = new List<CommonData>
        {
            // new CommonData { Value = "All", DisplayName = "All" },
            new CommonData { Value = "Export", DisplayName = "Export" },
            new CommonData { Value = "Import", DisplayName = "Import" }
        };
        public static readonly List<CommonData> ServiceTypes = new List<CommonData>
        {
            new CommonData { Value = "Air", DisplayName = "Air" },
            new CommonData { Value = "Sea", DisplayName = "Sea" },
            new CommonData { Value = "Crossborder", DisplayName = "Cross border" },
            new CommonData { Value = "Warehouse", DisplayName = "Warehouse" },
            new CommonData { Value = "Trucking", DisplayName = "Inland" },
            new CommonData { Value = "Railway", DisplayName = "Railway" },
            new CommonData { Value = "Express", DisplayName = "Express" }
        };
        public static readonly List<CommonData> Routes = new List<CommonData>
        {
            new CommonData { Value = "Red", DisplayName = "Red" },
            new CommonData { Value = "Green", DisplayName = "Green" },
            new CommonData { Value = "Yellow", DisplayName = "Yellow" }
        };
        public static readonly List<CommonData> CargoTypes = new List<CommonData> {
            new CommonData { Value = "FCL", DisplayName = "FCL" },
            new CommonData { Value = "LCL", DisplayName = "LCL" }
        };
    }
}
