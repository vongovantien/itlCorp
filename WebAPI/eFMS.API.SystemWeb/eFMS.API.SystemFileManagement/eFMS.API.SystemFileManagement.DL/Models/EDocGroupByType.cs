using eFMS.API.SystemFileManagement.Service.Models;
using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class EDocGroupByType
    {
        public SysAttachFileTemplate documentType { get; set; }
        public List<SysImageDetailModel> EDocs { get; set; }
    }
}
