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
    public interface ICatChargeDefaultAccountService:IRepositoryBaseCache<CatChargeDefaultAccount,CatChargeDefaultAccountModel>
    {  
        List<CatChargeDefaultAccountImportModel> CheckValidImport(List<CatChargeDefaultAccountImportModel> list);
        HandleState Import(List<CatChargeDefaultAccountImportModel> data);
    }
}
