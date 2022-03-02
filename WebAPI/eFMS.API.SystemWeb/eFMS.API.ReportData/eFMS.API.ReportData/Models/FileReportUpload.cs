using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class FileReportUpload
    {
        public byte[] FileContent { get; set; }
        public string FileName { get; set; }
    }
}
