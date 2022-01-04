using System.Collections.Generic;

namespace eFMS.API.Operation.DL.Common
{
    public class ConstantData
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }
    }
    public static class OperationConstants
    {
        public static readonly string InSchedule = "InSchedule";
        public static readonly string Processing = "Processing";
        public static readonly string Done = "Done";
        public static readonly string Overdue = "Overdued";
        public static readonly string Pending = "Pending";
        public static readonly string Deleted = "Deleted";
        public static readonly string Finish = "Finish";

        public const string Insert = "Insert";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string Active = "Active";
        public const string FromEFMS = "eFMS";
        public const string FromEcus = "Ecus";
        public const string FROM_REPLICATE= "Replicate";


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

    public static class ClearanceConstants
    {
        public const string Air_Service = "Air";
        public const string Sea_Service = "Sea";
        public const string Trucking_Inland_Service = "Trucking Inland";
        public const string Rail_Service = "Rail";
        public const string Warehouse_Service = "Warehouse";
        public const string Air_Service_Type = "1";
        public const string Sea_FCL_Service_Type = "2";
        public const string Sea_LCL_Service_Type = "3";
        public const string Trucking_Inland_Service_Type = "4";
        public const string Rail_Service_Type = "5";
        public const string Warehouse_Service_Type6 = "6";
        public const string Warehouse_Service_Type9 = "9";
        public const string CargoTypeFCL = "FCL";
        public const string CargoTypeLCL = "LCL";

        public const string Import_Type = "N";
        public const string Export_Type = "X";
        public const string Import_Type_Value = "Import";
        public const string Export_Type_Value = "Export";

        public const string Route_Type_Do = "do";
        public const string Route_Type_Xanh = "xanh";
        public const string Route_Type_Vang = "vang";
        public const string Route_Type_Red = "Red";
        public const string Route_Type_Green = "Green";
        public const string Route_Type_Yellow = "Yellow";
    }
}
