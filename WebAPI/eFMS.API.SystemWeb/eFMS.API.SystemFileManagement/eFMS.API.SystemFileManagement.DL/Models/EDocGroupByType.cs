using eFMS.API.SystemFileManagement.Service.Models;
using System;
using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class EDocGroupByType
    {
        public SysAttachFileTemplate documentType { get; set; }
        public List<SysImageDetailModel> EDocs { get; set; }
    }

    public class EdocJobInfo{
        public string JobNo { get; set; }
        public string HBLNo { get; set; }
    }
}
