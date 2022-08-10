using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.ForPartner.DL.Models.Receivable;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IAccAccountReceivableService : IRepositoryBase<AccAccountReceivable, AccAccountReceivableModel>
    {
        List<ObjectReceivableModel> GetListObjectReceivableBySurchargeIds(List<Guid> Ids);
        List<ObjectReceivableModel> GetListObjectReceivableBySurcharges(IQueryable<CsShipmentSurcharge> surcharges);
    }
}
