using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMSWindowService.Models
{
    public class ScheduleTime
    {
        public DayOfWeek DateOfWeek { get; set; }
        public string Time { get; set; }
        public DateTime DateTimeOption { get; set; }
        public int DaysFromNow { get; set; }
    }
}
