using eFMS.API.Setting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class SetUnlockRequestModel : SetUnlockRequest
    {
        public List<SetUnlockRequestJobModel> Jobs { get; set; }
    }
}
