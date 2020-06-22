using eFMS.API.Setting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class UnlockRequestResult : SetUnlockRequest
    {
        public string RequesterName { get; set; }
    }
}
