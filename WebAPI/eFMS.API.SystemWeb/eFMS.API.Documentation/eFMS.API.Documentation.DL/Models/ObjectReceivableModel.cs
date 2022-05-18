using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.ForPartner.DL.Models.Receivable
{
    public class ObjectReceivableModel
    {
        public Guid? SurchargeId { get; set; }
        public string PartnerId { get; set; }
        public Guid? Office { get; set; }
        public string Service { get; set; }
        public string SalesmanId { get; set; }
    }

    public class CalculatorReceivableNotAuthorizeModel
    {
        public List<ObjectReceivableModel> ObjectReceivable { get; set; }
        public string UserID { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid OfficeID { get; set; }
        public Guid CompanyID { get; set; }
        public string Action { get; set; }
    }

    public class CalculatorReceivableModel
    {
        public List<ObjectReceivableModel> ObjectReceivable { get; set; }
    }
}
