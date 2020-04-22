using eFMS.API.Setting.DL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Setting.Models
{
    public class CategoryCriteria
    {
        public CategoryTable TableType { get; set; }
        public string Query { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
