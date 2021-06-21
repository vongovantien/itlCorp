using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatBankService: IRepositoryBaseCache<CatBank,CatBankModel>
    {
        IQueryable<CatBankModel> GetAll();
        IQueryable<CatBankModel> Paging(CatBankCriteria criteria, int pageNumber, int pageSize, out int rowsCount);
        HandleState Update(CatBankModel model);
        HandleState Delete(Guid id);
        IQueryable<CatBankModel> Query(CatBankCriteria criteria);
        CatBankModel GetDetail(Guid id);
        List<CatBankImportModel> CheckValidImport(List<CatBankImportModel> list);
        HandleState Import(List<CatBankImportModel> data);
    }
}
