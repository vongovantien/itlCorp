using eFMSWindowService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMSWindowService.Helpers
{
    public class ScheduleSetting
    {
        /// <summary>List specific time to run service </summary>
        public static List<ScheduleTime> _scheduled { get; set; }
        public static void SetDateTimeSchedule(List<ScheduleTime> values)
        {
            foreach (var item in values)
            {
                var time = DateTime.Parse(item.Time);
                item.DateTimeOption = DateOfNextWeek(time, item.DateOfWeek);
                item.DaysFromNow = item.DateTimeOption.Subtract(DateTime.Now).Days;
            }
            _scheduled = values;
        }

        public static DateTime GetValidDate()
        {
            DateTime scheduledTime = new DateTime();
            var nearDate = _scheduled.OrderBy(x => x.DaysFromNow).ThenBy(x => x.DateTimeOption).FirstOrDefault();
            scheduledTime = nearDate.DateTimeOption;
            return scheduledTime;
        }

        /// <summary>
        /// Get date of next week with specific date
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="startOfWeek"></param>
        /// <returns></returns>
        private static DateTime DateOfNextWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            var daysToAdd = ((startOfWeek - dt.DayOfWeek + 7) % 7);
            dt = dt.AddDays(daysToAdd);
            if (dt.Subtract(DateTime.Now).TotalMinutes < 0)
            {
                return dt.AddDays(7);
            }
            else
            {
                return dt;
            }
        }

        /// <summary>
        /// Get due time from current to specific time to run
        /// </summary>
        /// <param name="scheduledTime"></param>
        /// <returns></returns>
        public static int GetDueTime(DateTime scheduledTime)
        {
            TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);

            //Get the difference in Minutes between the Scheduled and Current Time.
            int dueTime = timeSpan.TotalMilliseconds > 0 ? Convert.ToInt32(timeSpan.TotalMilliseconds) : 0;
            return dueTime;
        }

        /// <summary>
        /// Check if current datetime is suitable
        /// </summary>
        /// <param name="_dt"></param>
        /// <returns></returns>
        public static bool IsTimeToRun(DateTime _dt)
        {
            return (_dt.Subtract(DateTime.Now).TotalSeconds < 60 && _dt.Minute == DateTime.Now.Minute);
        }

    }
}
