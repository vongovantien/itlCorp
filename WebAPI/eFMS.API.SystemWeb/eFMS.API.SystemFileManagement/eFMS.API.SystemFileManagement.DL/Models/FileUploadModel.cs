using eFMS.API.SystemFileManagement.Service.Models;
using eFMS.API.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc;

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
}
