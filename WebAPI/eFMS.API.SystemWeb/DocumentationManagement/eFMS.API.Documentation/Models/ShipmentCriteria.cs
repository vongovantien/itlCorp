using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.Models
{
    public class ShipmentCriteria
    {
        [Required]
        public ShipmentPropertySearch ShipmentPropertySearch { get; set; }
        [Required]
        public List<string> Keywords { get; set; }
    }
    public enum ShipmentPropertySearch
    {
        JOBID = 1,
        MBL = 2,
        HBL = 3
    }
}
