using AutoMapper;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.IdentityServer.DL.Services
{
    public class SysEmployeeService :  RepositoryBase<SysEmployee, SysEmployeeModel>, ISysEmployeeService
    {
        public SysEmployeeService(IContextBase<SysEmployee> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}
