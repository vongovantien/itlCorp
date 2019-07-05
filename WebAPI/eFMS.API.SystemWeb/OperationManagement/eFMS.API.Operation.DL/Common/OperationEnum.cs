using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace eFMS.API.Operation.DL.Common
{
    public enum StageEnum
    {
        [Description("InSchedule")]
        InSchedule = 1,
        [Description("Processing")]
        Processing = 2,
        [Description("Done")]
        Done = 3,
        [Description("Overdued")]
        Overdue = 4,
        [Description("Pending")]
        Pending = 5,
        [Description("Deleted")]
        Deleted = 6,
        [Description("Warning")]
        Warning = 7
    }
}
