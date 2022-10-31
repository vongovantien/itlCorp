using System;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class EDocAttachPreviewTemplateUploadModel
    {
        public string Url { get; set; }
        public string Module { get; set; }
        public string Folder { get; set; }
        public Guid ObjectId { get; set; }
        public Guid HblId { get; set; }
        public string TemplateCode { get; set; }
        public string TransactionType { get; set; }
    }
}
