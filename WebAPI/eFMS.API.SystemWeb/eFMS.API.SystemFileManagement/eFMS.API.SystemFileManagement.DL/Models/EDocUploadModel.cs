using System;
using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class EDocUploadModel
    {
        public string FolderName { get; set; }
        public string ModuleName { get; set; }
        public List<EDocFile> EDocFiles { get; set; }
        public Guid? Id { get; set; }
    }
}
