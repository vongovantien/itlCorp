using eFMS.API.Common.Models;
using eFMS.API.Setting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class SetTariffModel : SetTariff
    {
        public PermissionAllowBase Permission { get; set; }
        public string UserCreatedName { get; set; }
        public string UserModifieddName { get; set; }
    }
}
