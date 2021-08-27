using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.IService
{
    public interface ISysEmailSettingService : IRepositoryBase<SysEmailSetting, SysEmailSettingModel>
    {
        HandleState AddEmailSetting(SysEmailSettingModel SysEmailSetting);

        IQueryable<SysEmailSetting> GetEmailSettings();

        IQueryable<SysEmailSetting> GetEmailSettingsByDebtId(int id);

        SysEmailSettingModel GetEmailSettingById(int id);

        HandleState UpdateEmailInfo(SysEmailSettingModel model);

        HandleState DeleteEmailSetting(int id);
        bool CheckExistsEmailInDept(SysEmailSettingModel model);
    }
}
