using eFMS.API.Setting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class EDocFile:SysImageDetail
    {
        public string DocumentType { get; set; }
        public string JobRef { get; set; }
        public string HBLNo { get; set; }
        public string Type { get; set; }

    }
}
