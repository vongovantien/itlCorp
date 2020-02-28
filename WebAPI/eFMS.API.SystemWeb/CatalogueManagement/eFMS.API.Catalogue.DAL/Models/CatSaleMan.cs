﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CatSaleman
    {
        public Guid Id { get; set; }
        public string SaleManId { get; set; }
        public Guid? Office { get; set; }
        public Guid? Company { get; set; }
        public string Service { get; set; }
        public string PartnerId { get; set; }
        public DateTime? EffectDate { get; set; }
        public string Description { get; set; }
        public bool? Status { get; set; }
        public DateTime? CreateDate { get; set; }
        public string UserModified { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string UserCreated { get; set; }
        public string FreightPayment { get; set; }
        public short? GroupId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public int? DepartmentId { get; set; }
    }
}
