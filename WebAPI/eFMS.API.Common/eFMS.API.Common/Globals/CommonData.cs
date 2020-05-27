using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Common.Globals
{
    public class CommonData
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }
    }

    public class CommonDataEx
    {
        public int Key { get; set; }
        public string Value { get; set; }
    }

    public static class CustomData
    {
        //Define list services
        public static readonly List<CommonData> Services = new List<CommonData>
        {
            new CommonData { Value = "CL", DisplayName = "Custom Logistic" },
            new CommonData { Value = "AI", DisplayName = "Air Import" },
            new CommonData { Value = "AE", DisplayName = "Air Export" },
            new CommonData { Value = "SFE", DisplayName = "Sea FCL Export" },
            new CommonData { Value = "SLE", DisplayName = "Sea LCL Export" },
            new CommonData { Value = "SFI", DisplayName = "Sea FCL Import" },
            new CommonData { Value = "SLI", DisplayName = "Sea LCL Import" },
            new CommonData { Value = "SCE", DisplayName = "Sea Consol Export" },
            new CommonData { Value = "SCI", DisplayName = "Sea Consol Import" },
            new CommonData { Value = "IT", DisplayName = "Inland Trucking" }
        };

        //Define list department type
        public static readonly List<CommonData> DeptTypes = new List<CommonData>
        {
            new CommonData { Value = "ACCOUNTANT", DisplayName = "Accountant" },
            new CommonData { Value = "NORMAL", DisplayName = "Normal" },
        };

        public static readonly List<string> ChargeTypes = new List<string> {
            "CREDIT",
            "DEBIT",
            "OBH"
        };

        public static readonly List<CommonDataEx> NumberOfOriginBls = new List<CommonDataEx>
        {
            new CommonDataEx { Key = 0, Value = "ZERO (0)" },
            new CommonDataEx { Key = 1, Value = "ONE (1)" },
            new CommonDataEx { Key = 2, Value = "TWO (2)" },
            new CommonDataEx { Key = 3, Value = "THREE (3)" },
        };
    }
}
