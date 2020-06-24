using eFMS.API.Catalogue.Service.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatContractModel : CatContract
    {
        public string Username { get; set; }
        public string OfficeNameEn { get; set; }
        public string OfficeNameVn { get; set; }
        public string OfficeNameAbbr{ get; set; }
        public string CompanyNameEn { get; set; }
        public string CompanyNameVn { get; set; }
        public string CompanyNameAbbr{ get; set; }
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
        //public string FolderName { get; set; }
        //public string ChildId { get; set; }
        //public ICollection<IFormFile> fileList { get; set; }
    }
}
