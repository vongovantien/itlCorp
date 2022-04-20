using eFMS.API.ReportData.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.IO;

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
        public FileReportUpload ReturnFormFile(Stream buffer, string fileName)
        {
            var ms = buffer as MemoryStream;
            try
            {
                var file = new FileReportUpload();
                file.FileName = fileName;
                file.FileContent = ms.ToArray();
                return file;
            }
            catch (Exception e)
            {
                ms.Dispose();
                throw;
            }
            finally
            {
                ms.Dispose();
            }
        }
    }
}
