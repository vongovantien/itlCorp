using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.IdentityServer.DL.IService
{
    public interface ISysEmployeeService : IRepositoryBase<SysEmployee, EmployeeModel>
    {
    }
}
