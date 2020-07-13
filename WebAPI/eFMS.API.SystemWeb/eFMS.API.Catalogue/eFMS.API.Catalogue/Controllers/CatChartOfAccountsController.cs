using System;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ITL.NetCore.Connection.BL;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Common;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using eFMS.API.Common.Helpers;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Catalogue.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatChartOfAccountsController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatChartOfAccountsService catChartAccountsService;
        private readonly ICurrentUser currentUser;
        private readonly IHostingEnvironment _hostingEnvironment;

        public CatChartOfAccountsController(IStringLocalizer<LanguageSub> localizer, ICatChartOfAccountsService service, ICurrentUser user, IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            catChartAccountsService = service;
            currentUser = user;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("Query")]
        [Authorize]
        public IActionResult Query(CatChartOfAccountsCriteria criteria)
        {
            var data = catChartAccountsService.Query(criteria);
            return Ok(data);
        }

        [HttpGet("QueryActiveByCompany")]
        [Authorize]
        public IActionResult QueryActiveByCompany()
        {
            var data = catChartAccountsService.QueryActiveByCompany();
            return Ok(data);
        }

        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(CatChartOfAccountsCriteria criteria, int page, int size)
        {
            var data = catChartAccountsService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get(Guid id)
        {
            var data = catChartAccountsService.GetDetail(id);
            return Ok(data);
        }

        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Add(CatChartOfAccountsModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model.AccountCode, Guid.Empty);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var hs = catChartAccountsService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            return Ok(result);
        }

        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(CatChartOfAccounts model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model.AccountCode, model.Id);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var hs = catChartAccountsService.Update(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("downloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {

            try
            {
                string fileName = Templates.CatChartOfAccounts.ExelImportFileName + Templates.ExelImportEx;
                string templateName = _hostingEnvironment.ContentRootPath;
                var result = await new FileHelper().ExportExcel(templateName, fileName);
                if (result != null)
                {
                    return result;
                }
                else
                {
                    return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
            }
        }

        [HttpPost]
        [Route("import")]
        [Authorize]
        public IActionResult Import([FromBody] List<CatChartOfAccountsImportModel> data)
        {
            var hs = catChartAccountsService.Import(data);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Import successfully !!!" };
            if (!hs.Success)
            {
                return BadRequest(new ResultHandle { Status = false, Message = hs.Message.ToString() });
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("UpLoadFile")]
        public IActionResult UpLoadFile(IFormFile uploadedFile)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int ColCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.NOT_FOUND_DATA_EXCEL].Value });
                List<CatChartOfAccountsImportModel> list = new List<CatChartOfAccountsImportModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    var acc = new CatChartOfAccountsImportModel
                    {
                        IsValid = true,
                        AccountCode  = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        AccountNameLocal = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        AccountNameEn = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                        Status = worksheet.Cells[row, 4].Value?.ToString().Trim()
                    };
                    list.Add(acc);
                }
                var data = catChartAccountsService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });

        }

        [HttpPost]
        [Route("QueryExport")]
        [Authorize]
        public IActionResult QueryExport(CatChartOfAccountsCriteria criteria)
        {
            var results = catChartAccountsService.QueryExport(criteria);
            return Ok(results);
        }

        [HttpGet("CheckAllowDelete/{id}")]
        [Authorize]
        public IActionResult CheckAllowDelete(Guid id)
        {
            bool resultDelete = catChartAccountsService.CheckAllowDelete(id);
            return Ok(resultDelete);
        }


        [HttpGet("CheckAllowDetail/{id}")]
        [Authorize]
        public IActionResult CheckAllowDetail(Guid id)
        {
            bool resultDelete = catChartAccountsService.CheckAllowViewDetail(id);
            return Ok(resultDelete);
        }

        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var hs = catChartAccountsService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private string CheckExist(string accountNo, Guid id)
        {
            string message = string.Empty;
            if (id != Guid.Empty)
            {
                if (catChartAccountsService.Any(x => x.AccountCode != null && x.AccountCode == accountNo && x.Id != id))
                {
                    message = "Account Code is existed !";
                }
            }
            else
            {
                if (catChartAccountsService.Any(x => x.AccountCode != null && x.AccountCode == accountNo))
                {
                    message = "Account Code is existed !";
                }
            }
            return message;
        }
    }
}