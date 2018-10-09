using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Mobile.ViewModel
{
    public class LoginViewModel
    {
        [Required]
        public string StaffId { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
