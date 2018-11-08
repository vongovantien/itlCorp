using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CatChargeDefaultAccount
    {
        public int Id { get; set; }
        public Guid ChargeId { get; set; }
        public string DebitAccountNo { get; set; }
        public decimal? DebitVat { get; set; }
        public string CreditAccountNo { get; set; }
        public decimal? CreditVat { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public string Type { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
