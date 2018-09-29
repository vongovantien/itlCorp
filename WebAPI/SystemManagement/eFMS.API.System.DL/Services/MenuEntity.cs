
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public class MenuEntity
    {
        public string ID { set; get; }
        public string ParentID { set; get; }
        public string Name_VN { set; get; }
        public string Name_EN { set; get; }
        public string Description { set; get; }
        public string AssemplyName { set; get; }
        public string Icon { set; get; }
        public int? Sequence { set; get; }
        public string Arguments { set; get; }
        public string ForWorkPlace { set; get; }
        public string ForServiceType { set; get; }
        public bool Inactive { set; get; }
        public DateTime ?InactiveOn { set; get; }
        public string Class { set; get; }
        public string Button { set; get; }
        public string ButtionID { set; get; }
    }
}