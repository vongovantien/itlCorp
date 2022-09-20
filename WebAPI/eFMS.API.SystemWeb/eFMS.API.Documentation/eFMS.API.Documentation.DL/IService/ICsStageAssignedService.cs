using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsStageAssignedService : IRepositoryBase<OpsStageAssigned, CsStageAssignedCriteria>
    {
        Task<HandleState> AddNewStageAssignedByType(CsStageAssignedCriteria criteria);
        Task<HandleState> AddNewStageAssigned(CsStageAssignedModel model);
        Task<HandleState> AddMutipleStageAssigned(List<CsStageAssignedModel> listStageAssigned);
        Task<List<CsStageAssignedModel>> SetMutipleStageAssigned(List<CatStage> listStages, Guid jobId, Guid HBLId);
    }
}
