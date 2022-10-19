﻿using System;

namespace eFMS.API.Provider.Models
{
    public class CatDepartmentApiModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string DeptName { get; set; }
        public string Description { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
