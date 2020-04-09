using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class DocumentFileUploadModel
    {
        public List<IFormFile> Files { get; set; }
        public string FolderName { get; set; }
        public Guid JobId { get; set; }
    }
}
