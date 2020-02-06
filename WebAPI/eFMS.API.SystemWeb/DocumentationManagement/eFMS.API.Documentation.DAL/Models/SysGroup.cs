﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class SysGroup
    {
        public SysGroup()
        {
            SysGroupRole = new HashSet<SysGroupRole>();
            SysUserLevel = new HashSet<SysUserLevel>();
        }

        public short Id { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameVn { get; set; }
        public int? DepartmentId { get; set; }
        public short ParentId { get; set; }
        public string ManagerId { get; set; }
        public string ShortName { get; set; }
        public bool? IsSpecial { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }

        public virtual CatDepartment Department { get; set; }
        public virtual ICollection<SysGroupRole> SysGroupRole { get; set; }
        public virtual ICollection<SysUserLevel> SysUserLevel { get; set; }
    }
}
