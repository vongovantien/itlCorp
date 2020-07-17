using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Linq;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatIncotermService : RepositoryBase<CatIncoterm, CatIncotermModel>, ICatIncotermService
    {
        public CatIncotermService(IContextBase<CatIncoterm> repository, IMapper mapper) : base(repository, mapper)
        {

        }
        public bool CheckAllowDelete(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool CheckAllowViewDetail(Guid id)
        {
            throw new NotImplementedException();
        }

        public HandleState Delete(Guid Id)
        {
            throw new NotImplementedException();
        }

        public CatChartOfAccountsModel GetDetail(Guid id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<CatIncoterm> Paging(CatIncotermCriteria criteria, int page, int size, out int rowsCount)
        {
            throw new NotImplementedException();
        }

        public IQueryable<CatIncoterm> Query(CatIncotermCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public HandleState Update(CatIncoterm model)
        {
            throw new NotImplementedException();
        }
    }
}
