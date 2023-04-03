using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.Common
{
    public class CommonData
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }

        public static string GetServicesName(string ServiceName)
        {
            string ServicesNameDetail = string.Empty;
            var ServiceArr = ServiceName.Split(";").ToArray();
            List<String> listService = new List<string>();
            foreach (var item in ServiceArr)
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
                    ServicesNameDetail += CustomDataService.Services.Where(x => x.Value == item).FirstOrDefault().DisplayName + "; ";
                }

            }
            if (!string.IsNullOrEmpty(ServicesNameDetail))
            {
                ServicesNameDetail = ServicesNameDetail.Remove(ServicesNameDetail.Length - 2);
            }
            return ServicesNameDetail;
        }
    }

    public static class CustomDataService
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
            new CommonData { Value = "IT", DisplayName = "Inland Trucking " },
            new CommonData { Value = "TK", DisplayName = "Inland Trucking " }

        };
    }

}
