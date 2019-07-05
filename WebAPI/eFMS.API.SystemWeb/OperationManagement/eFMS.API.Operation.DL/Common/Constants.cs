using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Operation.DL.Common
{
    public class ConstantData
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }
    }
    public static class Constants
    {
        public static readonly string InSchedule = "InSchedule";
        public static readonly string Processing = "Processing";
        public static readonly string Done = "Done";
        public static readonly string Overdue = "Overdued";
        public static readonly string Pending = "Pending";
        public static readonly string Deleted = "Deleted";

        public static readonly List<ConstantData> OperationStages = new List<ConstantData>
        {
            new ConstantData { Value = InSchedule, DisplayName = InSchedule },
            new ConstantData { Value = Processing, DisplayName = Processing },
            new ConstantData { Value = Done, DisplayName = Done },
            new ConstantData { Value = Overdue, DisplayName = Overdue },
            new ConstantData { Value = Pending, DisplayName = Pending },
            new ConstantData { Value = Deleted, DisplayName = Deleted }
        };
    }
}
