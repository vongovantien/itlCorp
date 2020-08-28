using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models.Criteria
{
    public class CatPotentialCriteria
    {
        public string All { get; set; }
        public string NameEn { get; set; }
        public string NameLocal { get; set; }
        public string Taxcode { get; set; }
        public string Tel { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public bool? Active { get; set; }
        public string Type { get; set; }
        public string Creator { get; set; }
    }
}
