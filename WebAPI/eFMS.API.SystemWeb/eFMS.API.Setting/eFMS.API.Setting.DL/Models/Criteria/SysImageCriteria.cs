using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models.Criteria
{
    public class SysImageCriteria
    {
        public string Name { get; set; }
        public string Folder { get; set; }
        public string ObjectId { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
        public string DateType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
