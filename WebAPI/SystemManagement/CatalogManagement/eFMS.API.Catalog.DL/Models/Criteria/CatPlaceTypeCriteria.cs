using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalog.DL.Models.Criteria
{
    public class CatPlaceTypeCriteria
    {
        public string Id { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public bool? Inactive { get; set; }
    }
}
