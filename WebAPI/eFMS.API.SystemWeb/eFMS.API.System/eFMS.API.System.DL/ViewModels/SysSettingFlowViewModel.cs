using eFMS.API.System.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.ViewModels
{
    public class SysSettingFlowViewModel
    {
        public List<SysSettingFlow> Approvals { get; set; }
        public List<SysSettingFlow> Unlocks { get; set; }
        public List<SetLockingDateShipment> LockingDateShipment { get; set; }
        public SysSettingFlow Account { get; set; }

    }
}
