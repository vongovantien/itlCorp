using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class FileCoppyModel
    {
        public string srcBucket { get; set; }
        public string srcKey { get; set; }
        public string destBucket { get; set; }
        public string destKey { get; set; }
        public int? Type { get; set; }

    }
}
