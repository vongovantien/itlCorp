﻿using eFMS.API.Setting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class SysImageModel: SysImage
    {
        public string folderName { get; set; }
        public string BillingType { get; set; }
        public string BillingNo { get; set; }
        public int DocumentType { get; set; }
    }
}
