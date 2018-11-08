using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatChargeDefaultAccountService:IRepositoryBase<CatChargeDefaultAccount,CatChargeDefaultAccountModel>
    {
        List<CatChargeDefaultAccount> Query(CatChargeDefaultAccountCriteria criteria);
        List<CatChargeDefaultAccount> Paging(CatChargeDefaultAccountCriteria criteria, int pageNumber, int pageSize, out int rowCount);
    }
}
