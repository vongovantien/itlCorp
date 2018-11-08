using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Catalogue.Service.ViewModels;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eFMS.API.Catalogue.DL.Common;
using System.Linq.Expressions;
using eFMS.API.Catalogue.DL.ViewModels;
using System.Threading;
using System.Globalization;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatChargeDefaultService:RepositoryBase<CatChargeDefaultAccount,CatChargeDefaultAccountModel>,ICatChargeDefaultAccountService
    {
        public CatChargeDefaultService(IContextBase<CatChargeDefaultAccount> repository,IMapper mapper) : base(repository, mapper)
        {

        }

        public List<CatChargeDefaultAccount> Paging(CatChargeDefaultAccountCriteria criteria, int pageNumber, int pageSize, out int rowCount)
        {
            throw new NotImplementedException();
        }

        public List<CatChargeDefaultAccount> Query(CatChargeDefaultAccountCriteria criteria)
        {
            throw new NotImplementedException();
        }
    }
}
