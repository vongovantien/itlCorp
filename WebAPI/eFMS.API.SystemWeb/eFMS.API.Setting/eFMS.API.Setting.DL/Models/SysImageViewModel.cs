using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class SysImageViewModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Folder { get; set; }
        public string ObjectId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DateTimeCreated { get; set; }
        public string FolderName { get; set; }
    }
}
