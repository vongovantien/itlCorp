using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Models;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.System.DL.IService
{
    public interface ISysSettingFlowService : IRepositoryBase<SysSettingFlow, SysSettingFlowModel>
    {
        SysSettingFlowViewModel GetByOfficeId(Guid officeId);

        HandleState UpdateSettingFlow(SysSettingFlowEditModel model);


    }
}
