using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Consts
{
    /// <summary>Supports all classes in the .NET Framework class hierarchy and provides low-level services to derived classes. This is the ultimate base class of all classes in the .NET Framework; it is the root of the type hierarchy.</summary>
    public static class ResourceConsts
    {
        public const string GROUP_NAME = "GROUP INFORMATION";
        public const string Export_Excel = "Export Excel";

        //Report Name
        public const string Standard_Report = "Standard Report";
        public const string Shipment_Overview = "Shipment Overview";
        public const string Accountant_PL_Sheet = "Accountant P/L Sheet";
        public const string Job_Profit_Analysis = "Job Profit Analysis";
        public const string Summary_Of_Costs_Incurred = "Summary Of Costs Incurred";
        public const string Summary_Of_Revenue_Incurred = "Summary Of Revenue Incurred";

        //Template folder
        public static string PathOfTemplateExcel = Path.Combine(Directory.GetCurrentDirectory(), @"FormatExcel\TemplateExport");
    }
}
