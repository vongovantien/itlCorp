using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

                if (!file.Exists)
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
    }
}
