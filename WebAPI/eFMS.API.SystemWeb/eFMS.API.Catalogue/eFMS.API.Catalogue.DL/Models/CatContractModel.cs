using eFMS.API.Catalogue.Service.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatContractModel : CatContract
    {
        public string SaleServiceName { get; set; }
        public string Username { get; set; }
        public string OfficeNameEn { get; set; }
        public string OfficeNameVn { get; set; }
        public string OfficeNameAbbr{ get; set; }
        public string CompanyNameEn { get; set; }
        public string CompanyNameVn { get; set; }
        public string CompanyNameAbbr{ get; set; }
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
        public Guid? CreatorCompanyId { get; set; }
        public Guid? CreatorOfficeId { get; set; }
        public int? CreatorDepartmentId { get; set; }
        public int? CreatorGroupId { get; set; }

        //public string FolderName { get; set; }
        //public string ChildId { get; set; }
        public bool? IsRequestApproval { get; set; }
        public IFormFile File { get; set; }
        public bool? isChangeAgrmentType { get; set; }
    }
}
