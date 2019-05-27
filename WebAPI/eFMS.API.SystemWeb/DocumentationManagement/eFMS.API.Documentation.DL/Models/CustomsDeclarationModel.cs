using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CustomsDeclarationModel: CustomsDeclaration
    {
        public string Hblno { get; set; }
        public string ExportCountryName { get; set; }
        public string ImportcountryName { get; set; }
        public string CommodityCode { get; set; }
    }
}
