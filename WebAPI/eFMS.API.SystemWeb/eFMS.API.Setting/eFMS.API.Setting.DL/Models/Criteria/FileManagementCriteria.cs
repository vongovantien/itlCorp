using System.Collections.Generic;

namespace eFMS.API.Setting.DL.Models.Criteria
{
    public class FileManagementCriteria
    {
        public string FolderName { get; set; }
        public string FolderType { get; set; }
        public List<string> Keywords { get; set; }
    }
}
