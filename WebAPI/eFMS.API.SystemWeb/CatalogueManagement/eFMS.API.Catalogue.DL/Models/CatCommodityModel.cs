using eFMS.API.Catalogue.Service.Models;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatCommodityModel: CatCommodity
    {
        public string CommodityGroupNameVn { get; set; }
        public string CommodityGroupNameEn { get; set; }
    }
}
