using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.Common
{
    public static class ConvertServicesName
    {
        public static string GetServicesName(string ServiceName)
        {
            string ContractServicesName = string.Empty;
            var ContractServiceArr = ServiceName.Split(";").ToArray();
            List<String> listService = new List<string>();
            foreach (var item in ContractServiceArr)
            {
                if (!String.IsNullOrEmpty(item))
                {
                    listService.Add(item);
                }
            }
            if (listService.Count > 0)
            {
                foreach (var item in listService)
                {
                    switch (item)
                    {
                        case "AE":
                            ContractServicesName += TypeData.AE;
                            break;
                        case "AI":
                            ContractServicesName += TypeData.AI;
                            break;
                        case "SCE":
                            ContractServicesName += TypeData.SCE;
                            break;
                        case "SCI":
                            ContractServicesName += TypeData.SCI;
                            break;
                        case "SFE":
                            ContractServicesName += TypeData.SFE;
                            break;
                        case "SLE":
                            ContractServicesName += TypeData.SLE;
                            break;
                        case "SLI":
                            ContractServicesName += TypeData.SLI;
                            break;
                        case "CL":
                            ContractServicesName += TypeData.CL;
                            break;
                        case "IT":
                        case "TK":
                            ContractServicesName += TypeData.IT;
                            break;
                        case "SFI":
                            ContractServicesName += TypeData.SFI;
                            break;
                        default:
                            ContractServicesName = TypeData.AI + TypeData.AE + TypeData.SCE + TypeData.SCI + TypeData.SFE + TypeData.SFI + TypeData.SLE + TypeData.SLI + TypeData.CL + TypeData.IT;
                            break;
                    }
                }

            }
            if (!string.IsNullOrEmpty(ContractServicesName))
            {
                ContractServicesName = ContractServicesName.Remove(ContractServicesName.Length - 2);
            }
            return ContractServicesName;
        }
    }
    public static class TypeData
    {
        public static readonly string CL = "Custom Logistic; ";
        public static readonly string IT = "Trucking; ";
        public static readonly string AE = "Air Export; ";
        public static readonly string AI = "Air Import; ";
        public static readonly string SFE = "Sea FCL Export; ";
        public static readonly string SCI = "Sea Consol Import; ";
        public static readonly string SCE = "Sea Consol Export; ";
        public static readonly string SFI = "Sea FCL Import; ";
        public static readonly string SLE = "Sea LCL Export; ";
        public static readonly string SLI = "Sea LCL Import; ";
    }
}
