using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatChartOfAccountsService : RepositoryBaseCache<CatChartOfAccounts, CatChartOfAccountsModel>, ICatChartOfAccountsService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatChartOfAccounts> chartRepository;
        public CatChartOfAccountsService(IContextBase<CatChartOfAccounts> repository,
        ICacheServiceBase<CatChartOfAccounts> cacheService,
        IMapper mapper,
        IStringLocalizer<LanguageSub> localizer,
        ICurrentUser user,
        IContextBase<CatChartOfAccounts> chartRepo) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            chartRepository = chartRepo;
        }

        public IQueryable<CatChartOfAccountsModel> Paging(CatChartOfAccountsCriteria criteria, int page, int size, out int rowsCount)
        {

        }
        
        public IQueryable<CatChartOfAccountsModel> Query(CatChartOfAccountsCriteria criteria)
        {
            var data = chartRepository.Get();
            if(criteria.All == null)
            {
                data = data.Where(x => (x.AccountCode ?? "").IndexOf(criteria.AccountCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                        && (x.AccountNameLocal ?? "").IndexOf(criteria.AccountNameLocal ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                        && (x.AccountNameEn ?? "").IndexOf(criteria.AccountNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                        && (x.Active == criteria.Active || criteria.Active == null)

                );
            }
        }
    }
}
