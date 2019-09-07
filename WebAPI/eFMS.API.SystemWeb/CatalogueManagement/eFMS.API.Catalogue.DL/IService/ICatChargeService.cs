using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatChargeService : IRepositoryBase<CatCharge,CatChargeModel>
    {
        List<Object> GetCharges(CatChargeCriteria criteria, int page, int size, out int rowsCount);
        List<CatCharge> Query(CatChargeCriteria criteria);
        HandleState AddCharge(CatChargeAddOrUpdateModel model);
        HandleState UpdateCharge(CatChargeAddOrUpdateModel model);
        HandleState DeleteCharge(Guid id);
        CatChargeAddOrUpdateModel GetChargeById(Guid id);
        List<CatChargeImportModel> CheckValidImport(List<CatChargeImportModel> list);
        HandleState Import(List<CatChargeImportModel> data);
        IQueryable<CatChargeModel> GetSettlePaymentCharges(string keySearch, bool? inActive, int? size);
    }
}
