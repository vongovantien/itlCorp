using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class CatDepartment
    {
        public CatDepartment()
        {
            SysGroup = new HashSet<SysGroup>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string DeptName { get; set; }
        public string DeptNameEn { get; set; }
        public string DeptNameAbbr { get; set; }
        public string Description { get; set; }
        public Guid? BranchId { get; set; }
        public string DeptType { get; set; }
        public string Email { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }

        public virtual SysOffice Branch { get; set; }
        public virtual ICollection<SysGroup> SysGroup { get; set; }
    }
}
