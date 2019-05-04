using eFMS.API.Catalogue.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatCommodityModel: CatCommodity
    {
        public string CommodityGroupNameVn { get; set; }
        public string CommodityGroupNameEn { get; set; }
    }
}
