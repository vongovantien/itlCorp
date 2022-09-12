using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsStageAssignedService : IRepositoryBase<OpsStageAssigned, CsStageAssignedModel>
    {
        HandleState AddNewStageAssigned(CsStageAssignedModel model);
    }

}
