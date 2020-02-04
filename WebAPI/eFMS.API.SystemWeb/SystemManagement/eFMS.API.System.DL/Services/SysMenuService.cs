using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;

namespace eFMS.API.System.DL.Services
{
    public class SysMenuService : RepositoryBase<SysMenu, SysMenuModel>, ISysMenuService
    {
        public SysMenuService(IContextBase<SysMenu> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}
