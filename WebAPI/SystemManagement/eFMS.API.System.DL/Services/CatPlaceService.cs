using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class CatPlaceService : RepositoryBase<CatPlace, CatPlaceModel>, ICatPlaceService
    {
        public CatPlaceService(IContextBase<CatPlace> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public IQueryable<CatPlaceModel> Query(CatPlaceCriteria criteria, string orderByProperty, bool isAscendingOrder)
        {
            throw new NotImplementedException();
        }
    }
}
