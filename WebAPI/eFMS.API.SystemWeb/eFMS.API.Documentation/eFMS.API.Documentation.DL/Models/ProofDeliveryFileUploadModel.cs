
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.DL.Models
{
    public class ProofDeliveryFileUploadModel
    {
        public IFormFile Files { get; set; }
        public string FolderName { get; set; }
        public Guid HblId { get; set; }
    }
}
