using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.IO;
using System.Threading.Tasks;

namespace eFMS.API.Common.Helpers
{
    public class FileHelper: ControllerBase
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
    }
}
