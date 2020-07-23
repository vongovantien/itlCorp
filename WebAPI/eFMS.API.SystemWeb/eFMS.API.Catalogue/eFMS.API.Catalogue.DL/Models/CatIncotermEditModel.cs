using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eFMS.API.Catalogue.Models
{
    public class CatIncotermEditModel
    {
        public CatIncoterm Incoterm { get; set; } 
        public List<CatChargeIncoterm> sellings { get; set; }
        public List<CatChargeIncoterm> buyings { get; set; }
    }
}
