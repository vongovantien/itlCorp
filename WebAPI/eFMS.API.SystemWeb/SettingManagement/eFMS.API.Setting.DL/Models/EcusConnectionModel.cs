using System;
using System.Collections.Generic;
using System.Text;
using eFMS.API.Setting.Service.Models;

namespace eFMS.API.Setting.DL.Models
{
    public class SetEcusConnectionModel : SetEcusconnection
    {
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }

    }
}
