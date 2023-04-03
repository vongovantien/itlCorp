namespace eFMS.API.Infrastructure.RabbitMQ
{
    public class RabbitConstants
    {
        public static string CalculatingReceivableDataPartnerQueue { get; } = "Accounting.CalculatingReceivableDataPartnerQueue";
        public static string CalculatingReceivableOverDuePaymentQueue { get; } = "Accounting.CalculatingOverDuePaymentQueue";
        public static string PostAttachFileTemplateToEDocQueue { get; } = "FileManagement.PostAttachFileTemplateToEDocQueue";
        public static string GenFileQueue { get; } = "ReportData.GenFileSyncQueue";
    }

    public class RabbitExchange
    {
        public static string EFMS_Accounting { get; } = "eFMS_AccountingExchange";
        public static string EFMS_Documentation { get; } = "eFMS_DocumentationExchange";
        public static string EFMS_Operation { get; } = "eFMS_OperationExchange";
        public static string EFMS_System { get; } = "eFMS_SystemExchange";
        public static string EFMS_Setting { get; } = "eFMS_SettingExchange";
        public static string EFMS_Catalogue { get; } = "eFMS_CatalogueExchange";
        public static string EFMS_Report { get; } = "eFMS_ReportExchange";
        public static string EFMS_Mail { get; } = "eFMS_MailExchange";
        public static string EFMS_Notify { get; } = "eFMS_NotifyExchange";
        public static string EFMS_FileManagement { get; } = "eFMS_FileManagementExchange";
        public static string EFMS_ReportData { get; } = "eFMS_ReportDataExchange";
    }
}
