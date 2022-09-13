using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsStageAssignedService : IRepositoryBase<OpsStageAssigned, CsStageAssignedCriteria>
    {
        HandleState AddNewStageAssignedByType(CsStageAssignedCriteria criteria);
        HandleState AddNewStageAssigned(CsStageAssignedModel model);
        HandleState AddMutipleStageAssigned(List<CsStageAssignedModel> listItem, Guid jobId);
    }

}
