using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatPartnerChargeService : IRepositoryBase<CatPartnerCharge, CatPartnerChargeModel>
    {
        HandleState AddAndUpdate(string partnerId, List<CatPartnerChargeModel> charges);
    }
}
