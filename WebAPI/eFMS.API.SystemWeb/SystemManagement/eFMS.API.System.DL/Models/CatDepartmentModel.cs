using eFMS.API.System.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models
{
    public class CatDepartmentModel: CatDepartment
    {
        public string OfficeName { get; set; }
        public string CompanyName { get; set; }
    }
}
