using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class SysAuthorization
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string AssignTo { get; set; }
        public string Name { get; set; }
        public string Services { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
