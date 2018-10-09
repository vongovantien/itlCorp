using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static API.Mobile.Common.StatusEnum;

namespace API.Mobile.Models
{
    public class Stage
    {
        public string Id { get; set; }
        public string JobId { get; set; }
        public string ShortName { get; set; }
        public int Duration { get; set; }
        public DateTime EndDate { get; set; }
        public StageStatus Status { get; set; }
        public string StatusName { get; set; }
        public string LastComment { get; set; }
        public int Order { get; set; }
        public string Role { get; set; }
        public bool IsCurrent { get; set; }
    }
}
