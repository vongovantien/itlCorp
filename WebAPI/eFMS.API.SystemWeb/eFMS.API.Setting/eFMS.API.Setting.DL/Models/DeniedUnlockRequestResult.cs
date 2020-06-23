using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class DeniedUnlockRequestResult
    {
        public int No { get; set; }
        public string NameAndTimeDeny { get; set; }
        public string LevelApprove { get; set; }
        public string Comment { get; set; }
    }
}
