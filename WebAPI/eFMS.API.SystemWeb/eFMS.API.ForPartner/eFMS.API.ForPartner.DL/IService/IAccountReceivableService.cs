using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.DL.Models.Receivable;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.ForPartner.DL.IService
{
    public interface IAccountReceivableService: IRepositoryBase<AccAccountReceivable, AccAccountReceivableModel>
    {
        List<ObjectReceivableModel> GetObjectReceivableBySurchargeId(List<Guid> surchargeIds);
    }
}
