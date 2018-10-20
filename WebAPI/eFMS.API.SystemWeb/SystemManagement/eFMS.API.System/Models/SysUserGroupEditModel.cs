using eFMS.API.System.Infrastructure.AttributeEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.System.Models
{
    public class SysUserGroupEditModel
    {
        [Required(ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "EF_ANNOTATIONS_STRING_LENGTH")]
        [DisplayName("Code")]
        public string Code { get; set; }

        //[AppRequired]
        [Required(ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "EF_ANNOTATIONS_STRING_LENGTH")]
        [DisplayName("Name")]
        public string Name { get; set; }
        public string Decription { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
