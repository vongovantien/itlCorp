using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Models;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.DL.Models
{
    public class ModelUpdate : BaseUpdateModel
    {
        public string UserCreator { get; set; }
        public string PartnerGroup { get; set; }
        public List<CatContract> Salemans { get; set; }

    }
}
