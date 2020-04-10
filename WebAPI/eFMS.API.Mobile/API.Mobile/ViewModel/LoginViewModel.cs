using API.Mobile.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Mobile.ViewModel
{
    public class LoginViewModel
    {
        [DisplayName("Staff Id")]
        [RequiredEx]
        public string StaffId { get; set; }
        [DisplayName("Password")]
        [RequiredEx]
        public string Password { get; set; }
        public string Token { get; set; }
    }
}
