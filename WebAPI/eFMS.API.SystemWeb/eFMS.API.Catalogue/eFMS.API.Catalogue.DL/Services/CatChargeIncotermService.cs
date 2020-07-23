using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;


namespace eFMS.API.Catalogue.DL.Services
{
    public class CatChargeIncotermService : RepositoryBase<CatChargeIncoterm, CatChargeIncotermModel>, ICatChargeIncotermService
    {
        public CatChargeIncotermService(IContextBase<CatChargeIncoterm> repository, IMapper mapper) : base(repository, mapper)
        {

        }
    }
}
