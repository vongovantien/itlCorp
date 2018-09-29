using API.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Mobile.ViewModel
{
    public class JobViewModel
    {
        public List<Job> Jobs { get; set; }
        public int TotalItems { get; set; }
        public int NumberJobFinishs { get; set; }
        public int? Offset { get; set; }
        public int? Limit { get; set; }
    }
}
