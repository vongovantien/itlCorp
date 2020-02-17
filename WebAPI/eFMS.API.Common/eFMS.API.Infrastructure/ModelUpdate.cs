using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Infrastructure
{
    public class ModelUpdate
    {
        public string UserCreated { get; set; }
        public string BillingOpsId { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
