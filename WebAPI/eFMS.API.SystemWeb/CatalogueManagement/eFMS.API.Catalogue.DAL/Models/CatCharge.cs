using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CatCharge
    {
        public Guid Id { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public string Code { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public string ChargeNameVn { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public string ChargeNameEn { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public string ServiceTypeId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public string Type { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public string CurrencyId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public double UnitPrice { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public short UnitId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public double Vat { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
