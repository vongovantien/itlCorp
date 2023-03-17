using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eFMS.API.Common.Helpers
{
    public class FileHelper : ControllerBase
    {
        readonly string pathTeamplate = Template.Resources.ExcelTemplate;
        public async Task<FileStreamResult> ExportExcel(string fileName)
        {
            try
            {
                //var path = $"D:/Workspace/efms/src/WebAPI/eFMS.API.SystemWeb/CatalogueManagement/bin/netcoreapp2.1/Resources/Files/WarehouseImportTeamplate.xlsx";
                string path = Path.Combine(pathTeamplate, fileName);
                FileInfo file = new FileInfo(path);
                var memory = new MemoryStream();

                if (file.Exists == false)
                {
                    return null;
                }
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public FileContentResult ExportExcel(string refNo, Stream stream, string fileName)
        {
            var buffer = stream as MemoryStream;
            var dateCurr = DateTime.Now.ToString("ddMMyy");
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

        public async Task<FileStreamResult> ExportExcel(string pathTeamplate, string fileName)
        {
            try
            {
                pathTeamplate = pathTeamplate + Template.Resources.ExcelInternalTemplate;
                string path = Path.Combine(pathTeamplate, fileName);
                FileInfo file = new FileInfo(path);
                var memory = new MemoryStream();

                if (file.Exists == false)
                {
                    return null;
                }
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ExcelPackage UploadExcel(IFormFile file)
        {
            ExcelPackage rs = new ExcelPackage(file.OpenReadStream());
            return rs;
        }
        public static async Task<MemoryStream> ReadFileAsync(string pathFile)
        {
            if (!System.IO.File.Exists(pathFile)) return null;
            try
            {
                FileInfo file = new FileInfo(pathFile);
                var memory = new MemoryStream();

                using (var stream = new FileStream(pathFile, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                return memory;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static string BeforeExtention(string fileName)
        {
            return Regex.Replace(StringHelper.RemoveSign4VietnameseString(fileName), @"[\\\/]+", "");
        }
        public static string RenameFileS3(string fileName)
        {
            return Regex.Replace(StringHelper.RemoveSign4VietnameseString(fileName), @"[\s#+:'*?<>|&%@$]+", "") + "_" + StringHelper.RandomString(5);
        }
        public static async Task<byte[]> DownloadFile(string url)
        {
            using (var client = new HttpClient())
            {
                using (var result = await client.GetAsync(url))
                { 
                    if (result.IsSuccessStatusCode)
                    {
                        return await result.Content.ReadAsByteArrayAsync();
                    }
                }
            }
            return null;
        }


        public static byte[] GetZipArchive(List<InMemoryFile> files)
        {
            byte[] archiveFile;
            using (var archiveStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        var zipArchiveEntry = archive.CreateEntry(file.FileName, System.IO.Compression.CompressionLevel.Fastest);

                        using (var zipStream = zipArchiveEntry.Open())
                        {
                            zipStream.Write(file.Content, 0, file.Content.Length);
                        }
                    }
                }

                archiveFile = archiveStream.ToArray();
            }

            return archiveFile;
        }

        public class InMemoryFile
        {
            public string FileName { get; set; }

            public byte[] Content { get; set; }
        }

        public static byte[] streamToByteArray(Stream input)
        {
            MemoryStream ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
