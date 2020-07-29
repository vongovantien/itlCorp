using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eFMS.API.Catalogue.Models
{
    public class CatIncotermEditModel
    {
        public CatIncotermModel Incoterm { get; set; } 
        public List<CatChargeIncoterm> Sellings { get; set; }
        public List<CatChargeIncoterm> Buyings { get; set; }
        public PermissionAllowBase Permission { get; set; }
    }
}
