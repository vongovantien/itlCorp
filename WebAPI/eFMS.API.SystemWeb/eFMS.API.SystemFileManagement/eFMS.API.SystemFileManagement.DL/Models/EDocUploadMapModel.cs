using System;
using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class EDocUploadMapModel
    {
        public string FolderName { get; set; }
        public string ModuleName { get; set; }
        public List<EDocFileMap> EDocFilesMap { get; set; }
        public Guid Id { get; set; }
    }
}
