using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatChargeGroupService : RepositoryBase<CatChargeGroup, CatChargeGroupModel>, ICatChargeGroupService
    {
        //private readonly IContextBase<CatChargeGroup> chargeGroupRepository;
        //public CatChargeGroupService(IContextBase<CatChargeGroup> repository,
        //    ICacheServiceBase<CatChargeGroup> cacheService, IMapper mapper) : base(repository, cacheService, mapper)
        //{
        //    chargeGroupRepository = repository;
        //}
        public CatChargeGroupService(IContextBase<CatChargeGroup> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}
