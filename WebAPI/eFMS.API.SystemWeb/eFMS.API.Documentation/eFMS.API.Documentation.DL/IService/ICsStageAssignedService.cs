using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsStageAssignedService : IRepositoryBase<OpsStageAssigned, OpsStageAssignedModel>
    {
        Task<HandleState> AddNewStageAssigned(CsStageAssignedModel model);
        Task<HandleState> AddNewStageAssignedByType(CsStageAssignedCriteria criteria);
        Task<HandleState> AddMultipleStageAssigned(Guid jobId, List<CsStageAssignedModel> listStageAssigned);
        Task<HandleState> SetMultipleStageAssigned(CsTransactionDetailModel currentHbl, CsTransactionModel currentJob, Guid jobId, Guid hblId, bool isHbl = false);


    }
}
