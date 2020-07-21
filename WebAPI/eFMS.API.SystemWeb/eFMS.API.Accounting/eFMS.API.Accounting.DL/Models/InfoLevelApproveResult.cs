using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models
{
    public class InfoLevelApproveResult
    {
        public string LevelApprove { get; set; }
        public string Role { get; set; }
        public string UserId { get; set; }
        public List<string> UserDeputies { get; set; }
        public string EmailUser { get; set; }
        public List<string> EmailDeputies { get; set; }
    }
}
