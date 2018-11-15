using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCurrencyExchangeService : RepositoryBase<CatCurrencyExchange, CatCurrencyExchangeModel>, ICatCurrencyExchangeService
    {
        public CatCurrencyExchangeService(IContextBase<CatCurrencyExchange> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public List<CatCurrencyExchangeViewModel> Paging(CatCurrencyExchangeCriteria criteria, int page, int size, out int rowsCount)
        {
            throw new NotImplementedException();
        }
    }
}
