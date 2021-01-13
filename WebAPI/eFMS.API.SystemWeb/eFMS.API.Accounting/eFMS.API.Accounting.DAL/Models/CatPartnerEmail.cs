using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class CatPartnerEmail
    {
        public Guid Id { get; set; }
        public Guid? OfficeId { get; set; }
        public string PartnerId { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
