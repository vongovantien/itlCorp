using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Helpers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    public class FileHelper: ControllerBase
    {
        public FileContentResult ExportExcel(Stream stream, string fileName)
        {
            var buffer = stream as MemoryStream;
            return File(
                buffer.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
    }
}
