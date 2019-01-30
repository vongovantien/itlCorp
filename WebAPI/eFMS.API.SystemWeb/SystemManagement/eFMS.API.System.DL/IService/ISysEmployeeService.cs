using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.IService
{
    public interface ISysEmployeeService : IRepositoryBase<SysEmployee, SysEmployeeModel>
    {
        List<EmployeeViewModel> Query(EmployeeCriteria employee);
        HandleState AddEmployee(SysEmployeeModel sysEmployee);
        HandleState UpdateEmployee(SysEmployeeModel sysEmployee);
        HandleState DeleteEmployee(string id);
    }
}
