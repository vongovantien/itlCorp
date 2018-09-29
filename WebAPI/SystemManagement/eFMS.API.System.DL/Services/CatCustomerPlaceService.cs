using AutoMapper;
using ITL.NetCore.Connection.EF;
using SystemManagement.DL.Models;
using ITL.NetCore.Connection.BL;
using SystemManagementAPI.Service.Models;
using SystemManagement.DL.Models.Views;
using System.Collections.Generic;
using ITL.NetCore.Connection;
using System.Linq;

namespace SystemManagement.DL.Services
{
    public class CatCustomerPlaceService : RepositoryBase<CatCustomerPlace, CatCustomerPlaceModel>, ICatCustomerPlaceService
    {
        IContextBase<CatCustomerPlace> _repository;
        public CatCustomerPlaceService(IContextBase<CatCustomerPlace> repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
        }

        public override string ToString()
        {
            return base.ToString();
        }
        public List<vw_customerPlace> CustomerPlace()
        {
            return _repository.DC.GetViewData<vw_customerPlace>().ToList();
        }
    }
}
