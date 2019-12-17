using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CatPartnerContact
    {
        public string Id { get; set; }
        public string PartnerId { get; set; }
        public string ContactNameVn { get; set; }
        public string ContactNameEn { get; set; }
        public DateTime? Birthday { get; set; }
        public string JobTitle { get; set; }
        public string CellPhone { get; set; }
        public string Email { get; set; }
        public string FieldInterested { get; set; }
        public bool IsDefault { get; set; }
        public string Notes { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string SugarId { get; set; }
    }
}
