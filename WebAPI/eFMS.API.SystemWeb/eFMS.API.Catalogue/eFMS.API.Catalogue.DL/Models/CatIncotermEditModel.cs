using eFMS.API.Catalogue.DL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.Models
{
    public class CatIncotermEditModel: CatIncotermModel
    {
        [Required]
        public string Code { get; set; }
        [Required]
        public string NameEn { get; set; }
        public string NameLocal { get; set; }
        [Required]
        public string Service { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionLocal { get; set; }
        public string Active { get; set; }
        public List<CatChargeIncoterm> sellings { get; set; }
        public List<CatChargeIncoterm> buyings { get; set; }
    }
}
