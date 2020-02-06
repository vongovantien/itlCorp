using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Contexts;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Helpers;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatBranchService : RepositoryBase<SysBranch, CatBranchModel>, ICatBranchService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IDistributedCache cache;
        public CatBranchService(IContextBase<SysBranch> repository, IMapper mapper) : base(repository, mapper)
        {

        }
        public List<CatBranchModel> GetListBranches()
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            List<CatBranchModel> results = new List<CatBranchModel>();

            var lstBranch = DataContext.Get();
            var lstOfficeCompany = from b in dc.SysOffice
                                   join bu in dc.SysCompany on b.Buid equals bu.Id into bu2
                                   from bu in bu2.DefaultIfEmpty()
                                   select new { b, AbbrCompany = bu.BunameAbbr };

            foreach (var item in lstOfficeCompany)
            {

                var officeCompany = mapper.Map<CatBranchModel>(item.b);
                officeCompany.AbbrCompany = item.AbbrCompany;
                results.Add(officeCompany);
            }
            return results;
        }
    }
}
