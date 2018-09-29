using AutoMapper;
using SystemManagement.DL.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using SystemManagementAPI.Service.Models;
using SystemManagement.DL.IService;

namespace SystemManagement.DL.Services
{
    public class CatDepartmentService: RepositoryBase<CatDepartment, CatDepartmentModel>, ICatDepartmentService
    {
        public CatDepartmentService(IContextBase<CatDepartment> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}
