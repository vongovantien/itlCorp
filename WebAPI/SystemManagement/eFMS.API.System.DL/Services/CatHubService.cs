using AutoMapper;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using SystemManagement.DL.Models;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public class CatHubService : RepositoryBase<CatHub, CatHubModel>, ICatHubService
    {
        public CatHubService(IContextBase<CatHub> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
