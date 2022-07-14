using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models.Criteria
{
    public class SysImageCriteria
    {
        public string FolderName { get; set; }
        public List<string> KeyWorks { get; set; } = null;
    }
}
