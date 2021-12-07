using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatChargeService : IRepositoryBaseCache<CatCharge,CatChargeModel>
    {
        IQueryable<CatChargeModel> Paging(CatChargeCriteria criteria, int page, int size, out int rowsCount);
        IQueryable<CatChargeModel> Query(CatChargeCriteria criteria);
        IQueryable<CatChargeModel> QueryExport(CatChargeCriteria criteria);
        IQueryable<CatChargeModel> GetBy(string type);
        HandleState AddCharge(CatChargeAddOrUpdateModel model);
        HandleState UpdateCharge(CatChargeAddOrUpdateModel model);
        HandleState DeleteCharge(Guid id);
        CatChargeAddOrUpdateModel GetChargeById(Guid id);
        List<CatChargeImportModel> CheckValidImport(List<CatChargeImportModel> list);
        HandleState Import(List<CatChargeImportModel> data);
        IQueryable<CatChargeModel> GetSettlePaymentCharges(string keySearch, bool? Active, int? size);
        object GetListService();
        bool CheckAllowPermissionAction(Guid id, PermissionRange range);
        IQueryable<CatChargeModel> GetChargesWithCurrentUserService(List<string> serviceType, string type);
    }
}
