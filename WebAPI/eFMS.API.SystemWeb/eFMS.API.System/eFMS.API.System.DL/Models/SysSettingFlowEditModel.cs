using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using System;
using System.Collections.Generic;

namespace eFMS.API.System.Models
{
    public class SysSettingFlowEditModel
    {
        public Guid OfficeId { get; set; }
        public List<SysSettingFlowModel> ApprovePayments { get; set; }
        public List<SysSettingFlowModel> UnlockShipments { get; set; }
        public List<SetLockingDateShipment> LockShipmentDate { get; set; }
        public SysSettingFlowModel AccountReceivable { get; set; }


    }
}
