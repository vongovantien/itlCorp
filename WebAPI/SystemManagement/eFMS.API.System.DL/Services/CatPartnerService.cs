using AutoMapper;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using SystemManagement.DL.Models;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public class CatPartnerService : RepositoryBase<CatPartner, CatPartnerModel>, ICatPartnerService
    {
        public CatPartnerService(IContextBase<CatPartner> repository, IMapper mapper) : base(repository, mapper)
        {
        }
        
    }
}
