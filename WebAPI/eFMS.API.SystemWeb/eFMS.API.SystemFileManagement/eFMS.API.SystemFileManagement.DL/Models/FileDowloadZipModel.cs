using eFMS.API.SystemFileManagement.Service.Models;
using eFMS.API.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class FileDowloadZipModel
    {
        public string FolderName { get; set; }
        public string ObjectId { get; set; }
        public string ChillId { get; set; }
        public string FileName { get; set; }
    }
}
