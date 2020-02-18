using System;

namespace eFMS.API.Infrastructure.Models
{
    public class BaseUpdateModel
    {
        public string UserCreated { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
