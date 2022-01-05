using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class CalculatorReceivableNotAuthorizeModel : CalculatorReceivableModel
    {
        public string UserID { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid OfficeID { get; set; }
        public Guid CompanyID { get; set; }
        public string Action { get; set; }
    }
}
