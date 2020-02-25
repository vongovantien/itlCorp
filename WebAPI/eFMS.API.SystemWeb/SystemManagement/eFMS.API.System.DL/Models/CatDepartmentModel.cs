using eFMS.API.System.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models
{
    public class CatDepartmentModel: CatDepartment
    {
        public Guid? CompanyId { get; set; }
        public string OfficeName { get; set; }
        public string CompanyName { get; set; }
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
    }
}
