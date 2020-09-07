namespace eFMS.API.ForPartner.DL.Common
{
    public enum TransactionTypeEnum
    {
        InlandTrucking = 1,
        AirExport = 2,
        AirImport = 3,
        SeaConsolExport = 4,
        SeaConsolImport = 5,
        SeaFCLExport = 6,
        SeaFCLImport = 7,
        SeaLCLExport = 8,
        SeaLCLImport = 9
    }

    public static class TermData
    {
        public static readonly string Canceled = "Canceled";

        public static readonly string InSchedule = "InSchedule";
        public static readonly string Processing = "Processing";
        public static readonly string Done = "Done";
        public static readonly string Overdue = "Overdued";
        public static readonly string Pending = "Pending";
        public static readonly string Deleted = "Deleted";
        public static readonly string Warning = "Warning";
        public static readonly string Finish = "Finish";

        public static readonly string InlandTrucking = "InlandTrucking";
        public static readonly string AirExport  = "AirExport";
        public static readonly string AirImport  = "AirImport";
        public static readonly string SeaConsolExport = "SeaConsolExport";
        public static readonly string SeaConsolImport = "SeaConsolImport";
        public static readonly string SeaFCLExport = "SeaFCLExport";
        public static readonly string SeaFCLImport = "SeaFCLImport";
        public static readonly string SeaLCLExport = "SeaLCLExport";
        public static readonly string SeaLCLImport = "SeaLCLImport";
    }
}
