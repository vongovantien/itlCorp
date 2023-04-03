using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class FileUploadModel
    {
        public List<IFormFile> Files { get; set; }
        public string FolderName { get; set; }
        public Guid Id { get; set; }
        public string Child { get; set; }
        public string ModuleName { get; set; }
    }

    public class FileReportUpload
    {
        public byte[] FileContent { get; set; }
        public string FileName { get; set; }
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
