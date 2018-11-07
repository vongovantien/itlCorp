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
    public interface ICatChargeService : IRepositoryBase<CatCharge,CatChargeModel>
    {
        List<Object> GetCharges(CatChargeCriteria criteria, int page, int size, out int rowsCount);
        HandleState AddCharge(CatChargeAddOrUpdateModel model);
        HandleState UpdateCharge(CatChargeAddOrUpdateModel model);

    }
}
