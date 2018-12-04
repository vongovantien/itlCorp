using eFMS.API.Log.DL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Log.DL.Models.Criteria
{
    public class CategoryCriteria
    {
        public CategoryTable TableType { get; set; }
        public string Query { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
