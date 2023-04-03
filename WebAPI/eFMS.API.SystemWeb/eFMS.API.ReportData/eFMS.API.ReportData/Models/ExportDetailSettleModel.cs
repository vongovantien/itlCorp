using System;

namespace eFMS.API.ReportData.Models
{
    public class ExportDetailSettleModel
    {
        public Guid SettlementId { get; set; }
        public string Lang { get; set; }
        public string Action { get; set; }
        public string AccessToken { get; set; }
    }

    public class FileUploadAttachTemplateModel
    {
        public FileReportUpload File { get; set; }
        public string FolderName { get; set; }
        public Guid Id { get; set; }
        public string Child { get; set; }
        public string ModuleName { get; set; }
    }
}
