using eFMS.API.Common.Helpers;
using eFMS.API.ReportData.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace eFMS.API.ReportData.Helpers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    public class FileHelper: ControllerBase
    {

        public FileContentResult ExportExcel(string refNo,Stream stream, string fileName)
        {
            var buffer = stream as MemoryStream;
            var dateCurr = DateTime.Now.ToString("ddMMyy");
            if (string.IsNullOrEmpty(refNo))
            {

                return File(
                            buffer.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            fileName += "-" + dateCurr + "-" + refNo + ".xlsx"
                        );
            }
            return File(
                buffer.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName += "-" + dateCurr + ".xlsx"
            );
        }
        public FileReportUpload ReturnFormFile(string refNo, Stream buffer, string fileName)
        {
            var ms = buffer as MemoryStream;
            var dateCurr = DateTime.Now.ToString("ddMMyy");
            fileName +="-" + dateCurr + "-" + refNo + ".xlsx";
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
