using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class ModelUpdate : BaseUpdateModel
    {
        public string UserCreator { get; set; }
        public string PartnerGroup { get; set; }
        public List<CatSaleman> Salemans { get; set; }

    }
}
