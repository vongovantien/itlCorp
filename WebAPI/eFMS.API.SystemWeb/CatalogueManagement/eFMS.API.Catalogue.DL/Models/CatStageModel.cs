using eFMS.API.Catalogue.Service.Models;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatStageModel : CatStage
    {
        public string DeptName { get; set; }
        public string DeptCode { get; set; }
    }
}
