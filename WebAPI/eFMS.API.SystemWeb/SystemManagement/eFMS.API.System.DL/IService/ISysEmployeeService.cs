using eFMS.API.System.DL.Models;
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
        HandleState Update(SysEmployeeModel sysEmployeeModel);
        HandleState Insert(SysEmployeeModel sysEmployeeModel);

    }
}
