using eFMS.API.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatPotentialEditModel
    {
        public CatPotentialModel Potential { get; set; }
        public PermissionAllowBase Permission { get; set; }
    }
}
