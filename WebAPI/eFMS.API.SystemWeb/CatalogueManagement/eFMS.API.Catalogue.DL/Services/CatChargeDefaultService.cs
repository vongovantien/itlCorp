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
using ITL.NetCore.Common;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatChargeDefaultService:RepositoryBase<CatChargeDefaultAccount,CatChargeDefaultAccountModel>,ICatChargeDefaultAccountService
    {
        public CatChargeDefaultService(IContextBase<CatChargeDefaultAccount> repository,IMapper mapper) : base(repository, mapper)
        {

        }

        public List<CatChargeDefaultAccountImportModel> CheckValidImport(List<CatChargeDefaultAccountImportModel> list)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var defaultAccount = dc.CatChargeDefaultAccount.ToList();
            list.ForEach(item =>
            {

            });
            return list;
        }

        public HandleState Import(List<CatChargeDefaultAccountImportModel> data)
        {
            throw new NotImplementedException();
        }

 
    }
}
