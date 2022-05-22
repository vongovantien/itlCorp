
using eFMS.API.Documentation.DL.Models;
using ITL.NetCore.Common;
using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsLinkChargeService
    {
        HandleState UpdateChargeLinkFee(List<CsShipmentSurchargeModel> list);
        HandleState RevertChargeLinkFee(List<CsShipmentSurchargeModel> list);
        CsLinkChargeModel DetailByChargeOrgId(Guid chargeId);
        HandleState LinkFeeJob(List<OpsTransactionModel> list);
    }
}
