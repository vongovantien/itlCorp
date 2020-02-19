using eFMS.API.Common.Models;

namespace eFMS.API.Documentation.DL.Models
{
    public class ModelUpdate: BaseUpdateModel
    {
        public string BillingOpsId { get; set; } //Sử dụng cho Service Logistic
        public string PersonInCharge { get; set; } //Sử dụng cho Sevice Documentation
        public string SaleManId { get; set; }
        //public short? GroupId { get; set; }
        //public int? DepartmentId { get; set; }
        //public Guid? OfficeId { get; set; }
        //public Guid? CompanyId { get; set; }
        
    }
}
