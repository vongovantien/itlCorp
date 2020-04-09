using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.Operation.DL.IService
{
    public interface IOpsStageAssignedService : IRepositoryBase<OpsStageAssigned, OpsStageAssignedModel>
    {
        OpsStageAssignedModel GetBy(Guid id);
        List<OpsStageAssignedModel> GetByJob(Guid jobId);
        HandleState Add(OpsStageAssignedEditModel model);
        List<OpsStageAssignedModel> GetNotAssigned(Guid jobId, int? departmentStage);
        HandleState AddMultipleStage(List<OpsStageAssignedEditModel> models, Guid jobId);
        HandleState Update(OpsStageAssignedEditModel model);
    }
}
