namespace eFMS.API.Operation.DL.Common
{
    public class Templates
    {
        public static string ExelImportEx = "ImportTemplate.xlsx";
        public struct CustomDeclaration
        {
            public static string ExelImportFileName = "CustomClearance";
            public struct NameCaching
            {
                public static string ListName = "CustomClearances";
            }
        }

        public struct OpsTransaction
        {
            public struct NameCaching
            {
                public static string ListName = "OpsTransactions";
            }
        }
    }
}
