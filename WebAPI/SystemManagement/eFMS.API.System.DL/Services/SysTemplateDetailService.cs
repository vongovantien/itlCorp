using ITL.NetCore.Connection.BL;
using SystemManagement.DL.Models;
using SystemManagementAPI.Service.Models;
using ITL.NetCore.Connection.EF;
using AutoMapper;

namespace SystemManagement.DL.Services
{
    public  class SysTemplateDetailService : RepositoryBase<SysTemplateDetail, SysTemplateDetailModel>, ISysTemplateDetailService
    {
        ISysBaseEnumService _sysBaseEnumService;
        public SysTemplateDetailService(IContextBase<SysTemplateDetail> repository, IMapper mapper) : base(repository, mapper)
        {
        }
       
    }
}
