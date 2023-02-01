﻿using eFMS.API.Common.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace eFMS.API.Report.Helpers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    public class FileHelper: ControllerBase
    {
        private string preUpper(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "";
            }
            var nameSplit=fileName.Split(' ');
            string nameResult1="";
            string nameResult = "";
            foreach (var name in nameSplit)
            {
                var isWhiteSpace = string.IsNullOrWhiteSpace(name);
                if (isWhiteSpace)
                {
                    nameResult1 += "";
                }
                else
                {
                    var upper = name[0].ToString().ToUpper() + name.Substring(1);
                    nameResult1 += upper;
                }
                
            }
            var nameSplit2= nameResult1.Split('_');
            foreach (var name in nameSplit2)
            {
                name[0].ToString().ToUpper();
                nameResult += name;
            }
            return nameResult.Replace(".xlsx","");
        }

        public FileContentResult ExportExcel(string refNo,Stream stream, string fileName)
        {
            var buffer = stream as MemoryStream;
            var dateCurr = DateTime.Now.ToString("ddMMyy");
            fileName = preUpper(fileName);
            if (!string.IsNullOrEmpty(refNo))
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
    }
}
