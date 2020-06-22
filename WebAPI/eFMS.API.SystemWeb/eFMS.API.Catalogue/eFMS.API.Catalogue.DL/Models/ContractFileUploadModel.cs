
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.DL.Models
{
    public class ContractFileUploadModel
    {
        public List<IFormFile> Files { get; set; }
        public string FolderName { get; set; }
        public string PartnerId { get; set; }
        public string ChildId { get; set; }
        public bool? IsTemp { get; set; }
    }
}
