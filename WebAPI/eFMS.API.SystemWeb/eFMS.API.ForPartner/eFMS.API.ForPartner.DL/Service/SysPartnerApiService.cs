using AutoMapper;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;


namespace eFMS.API.ForPartner.DL.Service
{
    public class SysPartnerApiService : RepositoryBase<SysPartnerApi, SysPartnerApiModel>, ISysPartnerApiService
    {
        public SysPartnerApiService(IContextBase<SysPartnerApi> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}
