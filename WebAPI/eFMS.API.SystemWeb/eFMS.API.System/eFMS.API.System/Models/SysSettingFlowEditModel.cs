using eFMS.API.System.DL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.System.Models
{
    public class SysSettingFlowEditModel
    {
        public Guid OfficeId { get; set; }
        public List<SysSettingFlowModel> settings { get; set; }
    }
}
