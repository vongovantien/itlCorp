using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;

namespace eFMS.API.System.DL.Services
{
    public class SysEmployeeService : RepositoryBase<SysEmployee, SysEmployeeModel>, ISysEmployeeService
    {
        public SysEmployeeService(IContextBase<SysEmployee> repository, IMapper mapper) : base(repository, mapper)
        {
            //currentUser = user;
        }

    }
}
