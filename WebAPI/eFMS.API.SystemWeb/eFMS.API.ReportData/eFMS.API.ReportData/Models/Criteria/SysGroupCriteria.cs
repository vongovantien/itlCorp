using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models.Criteria
{
    public class SysGroupCriteria
    {
        public string All { get; set; }
        public short Id { get; set; }
        public string Code { get; set; }
        public string NameEN { get; set; }
        public string NameVN { get; set; }
        public string ShortName { get; set; }
        public string DepartmentName { get; set; }
        public int DepartmentId { get; set; }
    }
}
