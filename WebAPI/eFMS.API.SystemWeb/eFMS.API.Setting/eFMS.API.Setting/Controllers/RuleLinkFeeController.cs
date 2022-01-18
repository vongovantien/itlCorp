using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Setting.DL.Common;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Infrastructure.Middlewares;
using eFMS.API.Setting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Setting.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    //[Authorize]
    public class RuleLinkFeeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private ICurrentUser currentUser;
        private IRuleLinkFeeService ruleLinkFeeService;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="curUser"></param>
        public RuleLinkFeeController(IStringLocalizer<LanguageSub> localizer,
            ICurrentUser curUser, IRuleLinkFeeService service, IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            currentUser = curUser;
            ruleLinkFeeService = service;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Add New Rule Link Fee
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        public IActionResult AddNewRule(RuleLinkFeeModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var checkData = ruleLinkFeeService.CheckExistsDataRule(model);
            if (!checkData.Success) return Ok(new ResultHandle { Status = checkData.Success, Message = checkData.Exception.Message.ToString(), Data = checkData.Code });
            var hs = ruleLinkFeeService.AddNewRuleLinkFee(model);
            if (hs.Code == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }
            if (hs.Code == 412)
            {
                return BadRequest(new ResultHandle { Status = false, Message = hs.Exception.Message.ToString() });
            }

            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// get and paging the list of Rule Link Fee by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="pageNumber">page to retrieve data</param>
        /// <param name="pageSize">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(RuleLinkFeeCriteria criteria, int pageNumber, int pageSize)
        {
            var data = ruleLinkFeeService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }

        [HttpDelete]
        [Route("Delete")]
        public IActionResult DeleteRuleLinkFee(Guid id)
        {
            var data = ruleLinkFeeService.GetRuleLinkFeeById(id);
            if (data == null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND].Value });
            }
            HandleState hs = ruleLinkFeeService.DeleteRuleLinkFee(data.Id);
            if (hs.Code == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString() };
                return BadRequest(_result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Update Settlement Payment
        /// </summary>
        /// <param name="model">object to update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        public IActionResult UpdateRuleLinkFee(RuleLinkFeeModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkData = ruleLinkFeeService.CheckExistsDataRule(model);
            if (!checkData.Success) return Ok(new ResultHandle { Status = checkData.Success, Message = checkData.Exception.Message.ToString(), Data = checkData.Code });

            var hs = ruleLinkFeeService.UpdateRuleLinkFee(model);

            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("getRuleByID")]
        public IActionResult GetDetailRuleLinkFeeById(Guid id)
        {
            var rule = ruleLinkFeeService.GetRuleLinkFeeById(id);
            if (rule == null)
            {
                return BadRequest();
            }
            return Ok(rule);
        }

        /// <summary>
        /// download an excel file from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("DownloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            string fileName = "RuleLinkFeeImportTemplate.xlsx";
            string templateName = _hostingEnvironment.ContentRootPath;
            var result = await new FileHelper().ExportExcel(templateName, fileName);
            if (result != null)
            {
                return result;
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// read data from excel file
        /// </summary>
        /// <param name="uploadedFile">file upload</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpLoadFile")]
        //[Authorize]
        public IActionResult UpLoadFile(IFormFile uploadedFile)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int ColCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.NOT_FOUND_DATA_EXCEL].Value });

                List<RuleLinkFeeImportModel> list = null;
                list = ReadRuleLinkFeeFromExel(worksheet,rowCount);
                var data = ruleLinkFeeService.CheckRuleLinkFeeValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        private List<RuleLinkFeeImportModel> ReadRuleLinkFeeFromExel(ExcelWorksheet worksheet, int rowCount)
        {
            List<RuleLinkFeeImportModel> list = new List<RuleLinkFeeImportModel>();
            for (int row = 2; row <= rowCount; row++)
            {
                var rulelinkfee = new RuleLinkFeeImportModel
                {
                    IsValid = true,
                    RuleName = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                    ServiceBuying = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                    ChargeBuying = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                    PartnerBuying = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                    ServiceSelling = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                    ChargeSelling = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                    PartnerSelling = worksheet.Cells[row, 7].Value?.ToString().Trim(),
                    Status = "Active",
                };
                list.Add(rulelinkfee);
            }
            //list = list.Where(x => !string.IsNullOrEmpty(x.RuleName)
            //    || !string.IsNullOrEmpty(x.ServiceBuying)
            //    || !string.IsNullOrEmpty (x.ChargeNameBuying)
            //    || !string.IsNullOrEmpty(x.PartnerNameBuying)
            //    || !string.IsNullOrEmpty(x.ServiceSelling)
            //    || !string.IsNullOrEmpty(x.ChargeNameSelling)
            //    || !string.IsNullOrEmpty(x.PartnerNameSelling)).ToList();
            list = list.Where(x => x.RuleName != null
                || x.ServiceBuying != null
                || x.ChargeBuying != null
                || x.PartnerBuying != null
                || x.ServiceSelling != null
                || x.ChargeSelling != null).ToList();
            return list;
        }

        /// <summary>
        /// import partner data
        /// </summary>
        /// <param name="data">data to import</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Import")]
        public IActionResult Import([FromBody] List<RuleLinkFeeImportModel> data)
        {
            var hs = ruleLinkFeeService.Import(data);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Import successfully !" };
            if (!hs.Success)
            {
                return Ok(new ResultHandle { Status = hs.Success, Message = hs.Exception.Message.ToString(), Data = hs.Code });
            }
            return Ok(result);
        }

        

    }
}
