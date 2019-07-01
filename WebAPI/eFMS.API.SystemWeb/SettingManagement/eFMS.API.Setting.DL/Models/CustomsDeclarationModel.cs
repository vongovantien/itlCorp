using eFMS.API.Setting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class CustomsDeclarationModel : CustomsDeclaration
    {
        public string CustomerName { get; set; }
        public string ImportCountryName { get; set; }
        public string ExportCountryName { get; set; }
        public string GatewayName { get; set; }
        public bool IsValid { get; set; }
        public string ClearanceDateStr { get; set; }
        public string GrossWeightStr {get;set;}
        public string NetWeightStr { get; set; }
        public string CbmStr { get; set; }
        public string PcsStr { get; set; }
        public string QtyContStr { get; set; }
    }
}
