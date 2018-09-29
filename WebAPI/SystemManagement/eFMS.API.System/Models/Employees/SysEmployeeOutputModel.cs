using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemManagement.DL.Helpers.PagingPrams;
using SystemManagement.DL.Models;

namespace SystemManagementAPI.Models.Employees
{
    public class SysEmployeeOutputModel
    {
        public PagingHeader Paging { get; set; }
        public List<LinkInfo> Links { get; set; }
        public List<SysEmployeeModel> Items { get; set; }
    }

}
