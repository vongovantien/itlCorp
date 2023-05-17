using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models.CataloguePartner
{
    public class BravoLoginResponseModel
    {
        public string TokenKey { get; set; }
        public string Success { get; set; }
        public string Message { get; set; }
    }

    public class BravoLoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
