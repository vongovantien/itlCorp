using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class InfoLevelApproveResult
    {
        public string LevelApprove { get; set; }
        public string Role { get; set; }
        public string UserId { get; set; }
        public string EmailUser { get; set; }
        public List<string> EmailDeputies { get; set; }
    }
}
