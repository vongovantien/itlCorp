using eFMS.API.Common.Models;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class ModelUpdate: BaseUpdateModel
    {
        public string BillingOpsId { get; set; } //Sử dụng cho Service Logistic
        public string PersonInCharge { get; set; } //Sử dụng cho Sevice Documentation
        public string SaleManId { get; set; } //Sử dụng cho Logistic & Documentation        
        public string[] SalemanIds { get; set; }
        public List<string> Groups { get; set; }
        public List<string> Departments { get; set; }
    }
}
