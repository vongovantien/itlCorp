using eFMS.API.Log.DL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Log.Models
{
    public class CategoryCriteria
    {
        public CategoryTable TableType { get; set; }
        public string Query { get; set; }
    }
}
