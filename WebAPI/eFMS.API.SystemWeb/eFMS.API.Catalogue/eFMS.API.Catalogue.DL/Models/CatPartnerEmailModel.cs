using eFMS.API.Catalogue.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatPartnerEmailModel:CatPartnerEmail
    {
        public string UserCreatedName { get; set; }
        public string UserModfiedName { get; set; }
    }
}
