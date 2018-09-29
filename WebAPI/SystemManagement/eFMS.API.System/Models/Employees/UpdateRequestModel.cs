﻿using System;
using System.ComponentModel.DataAnnotations;

namespace SystemManagementAPI.API.Models.Emploees
{
    public class UpdateRequestModel
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public Guid WorkPlaceId { get; set; }
        public string DepartmentId { get; set; }        
        public string EmployeeNameVn { get; set; }
        public string EmployeeNameEn { get; set; }
        public string Position { get; set; }
        public DateTime? Birthday { get; set; }
        public string ExtNo { get; set; }
        public string Tel { get; set; }
        public string HomePhone { get; set; }
        public string HomeAddress { get; set; }
        public string Email { get; set; }
        public string AccessDescription { get; set; }
        public byte[] Photo { get; set; }
        public string EmpPhotoSize { get; set; }
        public decimal? SaleTarget { get; set; }
        public decimal? Bonus { get; set; }
        public string SaleResource { get; set; }
        public byte[] Signature { get; set; }
        public Guid? LdapObjectGuid { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        [Required]
        public string UserModified { get; set; }
        [Required]
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
