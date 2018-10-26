using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.ViewModels
{
    public class CatAreaViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
