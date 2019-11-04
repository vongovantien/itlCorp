using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsArrivalFrieghtChargeService: IRepositoryBase<CsArrivalFrieghtCharge, CsArrivalFrieghtChargeModel>
    {
        HandleState UpdateArrival(CsArrivalFrieghtChargeEditModel model);
    }
}
