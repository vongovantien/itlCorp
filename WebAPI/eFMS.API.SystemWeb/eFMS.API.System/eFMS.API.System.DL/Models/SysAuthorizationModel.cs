﻿using eFMS.API.System.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models
{
    public class SysAuthorizationModel : SysAuthorization
    {
        public string ServicesName { get; set; }
        public string UserNameAssign { get; set; }
        public string UserNameAssignTo { get; set; }
    }
}
