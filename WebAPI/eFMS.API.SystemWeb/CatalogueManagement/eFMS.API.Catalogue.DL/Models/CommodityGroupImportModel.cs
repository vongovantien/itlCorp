using eFMS.API.Catalogue.Service.Models;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CommodityGroupImportModel:CatCommodityGroup
    {
        public bool IsValid { get; set; }
        public string Status { get; set; }
    }
}
