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
            if (listService.Count() > 0)
            {
                foreach (var item in listService)
                {
                    switch (item)
                    {
                        case "AE":
                            ContractServicesName += "Air Export; ";
                            break;
                        case "AI":
                            ContractServicesName += "Air Import; ";
                            break;
                        case "SCE":
                            ContractServicesName += "Sea Consol Export; ";
                            break;
                        case "SCI":
                            ContractServicesName += "Sea Consol Import; ";
                            break;
                        case "SFE":
                            ContractServicesName += "Sea FCL Export; ";
                            break;
                        case "SLE":
                            ContractServicesName += "Sea LCL Export; ";
                            break;
                        case "SLI":
                            ContractServicesName += "Sea LCL Import; ";
                            break;
                        case "CL":
                            ContractServicesName += "Custom Logistic; ";
                            break;
                        case "IT":
                            ContractServicesName += "Trucking; ";
                            break;
                        case "SFI":
                            ContractServicesName += "Sea FCL Import; ";
                            break;
                        case "TKI":
                            ContractServicesName += "Inland Trucking; ";
                            break;
                        default:
                            ContractServicesName = "Air Export; Air Import; Sea Consol Export; Sea Consol Import; Sea FCL Export; Sea LCL Export; Sea LCL Import; Custom Logistic; Trucking  ";
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
}
