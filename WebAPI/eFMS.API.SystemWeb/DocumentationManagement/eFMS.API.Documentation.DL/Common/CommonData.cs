using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Common
{
    public class CommonData
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }
    }

    public static class CustomData
    {
        //Define list services
        public static readonly List<CommonData> Services = new List<CommonData>
        {
            new CommonData { Value = "LGT", DisplayName = "Logistic (operation)" },
            new CommonData { Value = "AI", DisplayName = "Air Import" },
            new CommonData { Value = "AE", DisplayName = "Air Export" },
            new CommonData { Value = "SFE", DisplayName = "Sea FCL Export" },
            new CommonData { Value = "SLE", DisplayName = "Sea LCL Export" },
            new CommonData { Value = "SFI", DisplayName = "Sea FCL Import" },
            new CommonData { Value = "SLI", DisplayName = "Sea LCL Import" },
            new CommonData { Value = "SCE", DisplayName = "Sea Consol Export" },
            new CommonData { Value = "SCI", DisplayName = "Sea Consol Import" },
            new CommonData { Value = "IT", DisplayName = "Inland Trucking " }
        };

        //Define list status of SOA
        public static readonly List<CommonData> StatusSoa = new List<CommonData>
        {
            new CommonData { Value = "New", DisplayName = "New" },
            new CommonData { Value = "RequestConfirmed", DisplayName = "Request Confirmed" },
            new CommonData { Value = "Confirmed", DisplayName = "Confirmed" },
            new CommonData { Value = "NeedRevise", DisplayName = "Need Revise" },
            new CommonData { Value = "Done", DisplayName = "Done" }
        };
    }
}
