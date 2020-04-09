using System;

namespace eFMS.API.ReportData.Models
{
    public class CatDepartmentModel
    {
        public int No { get; set; }
        //public int Id { get; set; }
        public string Code { get; set; }
        public string DeptName { get; set; }
        public string DeptNameEn { get; set; }
        public string DeptNameAbbr { get; set; }
        public string OfficeName { get; set; }
        public bool? Active { get; set; }
    }
}
