using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Mobile.Models
{
    public class User
    {
        public string StaffId { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string EnglishName { get; set; }
        public string Position { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ChangePasswordModel
    {
        public string UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
