using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.Infrastructure.Middlewares;
using eFMS.API.SystemFileManagement.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eFMS.API.SystemFileManagement.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AttachFileTemplateController : ControllerBase
    {
        private readonly IAttachFileTemplateService AttachFilteTemplateService;
        private readonly IStringLocalizer stringLocalizer;

        public AttachFileTemplateController(
            IStringLocalizer<LanguageSub> localizer,
            IAttachFileTemplateService AttachFilteTemplate)
        {
            stringLocalizer = localizer;
            AttachFilteTemplateService = AttachFilteTemplate;
        }

        [HttpPost]
        [Route("import")]
        public async Task<IActionResult> UploadFile(IFormFile uploadedFile)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                if (rowCount < 2) return BadRequest();

                List<SysAttachFileTemplate> list = new List<SysAttachFileTemplate>();
                for (int row = 2; row <= rowCount; row++)
                {
                    var st = worksheet.Cells[row, 13].Value;
                    var template = new SysAttachFileTemplate
                    {
                        NameVn = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        NameEn = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        Code = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                        Active = worksheet.Cells[row, 4].Value?.ToString().Trim() == "TRUE" ? true : false,
                        Required = worksheet.Cells[row, 5].Value?.ToString().Trim() == "TRUE" ? true : false,
                        TransactionType = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                        Service = worksheet.Cells[row, 7].Value?.ToString().Trim(),
                        ServiceType = worksheet.Cells[row, 8].Value?.ToString().Trim(),
                        PreFix = worksheet.Cells[row, 9].Value?.ToString().Trim(),
                        SubFix = worksheet.Cells[row, 10].Value?.ToString().Trim(),
                        Type = worksheet.Cells[row, 11].Value?.ToString().Trim(),
                        StorageFollowing = worksheet.Cells[row, 12].Value?.ToString().Trim(),
                        //StorageTime = (int)worksheet.Cells[row, 13].Value,
                        StorageType = worksheet.Cells[row, 14].Value?.ToString().Trim(),
                        AccountingType = worksheet.Cells[row, 15].Value?.ToString().Trim(),
                        PartnerType = worksheet.Cells[row, 16].Value?.ToString().Trim(),
                    };
                    list.Add(template);
                }
                var data = await AttachFilteTemplateService.Import(list);
                if (data.Success)
                {
                    return Ok(new ResultHandle { Message = "Import successful" });
                }
                return BadRequest(new ResultHandle { Message = data.Message.ToString() });
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        [HttpGet("GetDocumentType")]
        public async Task<IActionResult> GetDocumentTypeAsync(string transactionType, string billingId)
        {
            var result = await AttachFilteTemplateService.GetDocumentType(transactionType, billingId);
            if (result == null)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}