using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using eFMS.API.Setting.Infrastructure.Middlewares;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.DL.Common;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Authorization;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Common.NoSql;
using eFMS.API.Common.Helpers;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Setting.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CustomsDeclarationController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICustomsDeclarationService customsDeclarationService;
        private readonly IDistributedCache cache;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface ICustomsDeclarationService</param>
        /// <param name="distributedCache"></param>
        public CustomsDeclarationController(IStringLocalizer<DL.Common.LanguageSub> localizer, ICustomsDeclarationService service, IDistributedCache distributedCache, ICurrentUser user)
        {
            stringLocalizer = localizer;
            customsDeclarationService = service;
            cache = distributedCache;
            currentUser = user;
        }

        /// <summary>
        /// get the list of custom declarations
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var results = customsDeclarationService.Get();
            return Ok(results);
        }

        /// <summary>
        /// get the list of custom declarations by job id
        /// </summary>
        /// <param name="jobNo">jobId that want to retrieve custom declarations</param>
        /// <returns></returns>
        [HttpGet("GetBy")]
        public IActionResult GetBy(string jobNo)
        {
            var results = customsDeclarationService.GetBy(jobNo);
            return Ok(results);
        }

        /// <summary>
        /// get the list of custom declarations by conditions
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("Query")]
        public IActionResult Query(CustomsDeclarationCriteria criteria)
        {
            var data = customsDeclarationService.Query(criteria);
            return Ok(data);
        }

        /// <summary>
        /// get and paging the list of custom declarations by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="pageNumber">page to retrieve data</param>
        /// <param name="pageSize">number items per page</param>
        /// <returns></returns>
        [HttpPost("Paging")]
        public IActionResult Paging(CustomsDeclarationCriteria criteria, int pageNumber, int pageSize)
        {
            var data = customsDeclarationService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }

        /// <summary>
        /// add new custom clearance
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("Add")]
        public IActionResult AddNew(CustomsDeclarationModel model)
        {
            var existedMessage = CheckExist(model, model.Id);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            model.DatetimeCreated = DateTime.Now;
            model.DatetimeModified = DateTime.Now;
            model.UserCreated = model.UserModified = currentUser.UserID;
            model.Source = Constants.FromEFMS;
            var hs = customsDeclarationService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            else
            {
                cache.Remove(Templates.CustomDeclaration.NameCaching.ListName);
            }
            return Ok(result);
        }

        /// <summary>
        /// update an existed item
        /// </summary>
        /// <param name="model">object to update</param>
        /// <returns></returns>
        [Authorize]
        [HttpPut]
        [Route("Update")]
        public IActionResult Update(CustomsDeclarationModel model)
        {
            var existedMessage = CheckExist(model, model.Id);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            model.DatetimeModified = DateTime.Now;
            model.UserModified = currentUser.UserID;
            var hs = customsDeclarationService.Update(model, x => x.Id == model.Id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                cache.Remove(Templates.CustomDeclaration.NameCaching.ListName);
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of existed item that want to delete</param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        [Route("Delete")]
        public IActionResult Delete(int id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = customsDeclarationService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// import custom declaration from ecus system
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost("ImportClearancesFromEcus")]
        public IActionResult ImportClearancesFromEcus()
        {
            var hs = customsDeclarationService.ImportClearancesFromEcus();
            if (hs.Success)
            {
                cache.Remove(Templates.CustomDeclaration.NameCaching.ListName);
            }
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// get clearance types(types, cargoTypes, routes, serviceTypes)
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetClearanceTypes")]
        public IActionResult GetClearanceTypes()
        {
            var results = customsDeclarationService.GetClearanceTypeData();
            return Ok(results);
        }

        /// <summary>
        /// add( update) job to clearances
        /// </summary>
        /// <param name="clearances">list of clearances</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdateJobToClearances")]
        public IActionResult UpdateJobToClearances(List<CustomsDeclarationModel> clearances)
        {
            var result = customsDeclarationService.UpdateJobToClearances(clearances);
            if (result.Success)
            {
                cache.Remove(Templates.CustomDeclaration.NameCaching.ListName);
            }
            return Ok(result);
        }

        private string CheckExist(CustomsDeclarationModel model, decimal id)
        {
            string message = null;
            if (id == 0)
            {
                if (customsDeclarationService.Any(x => x.ClearanceNo == model.ClearanceNo && x.ClearanceDate == model.ClearanceDate))
                {
                    message = stringLocalizer[LanguageSub.MSG_CLEARANCENO_EXISTED].Value;
                }
            }
            else
            {
                if (customsDeclarationService.Any(x => (x.ClearanceNo == model.ClearanceNo && x.Id != id && x.ClearanceDate == model.ClearanceDate)))
                {
                    message = stringLocalizer[LanguageSub.MSG_CLEARANCENO_EXISTED].Value;
                }
            }
            return message;
        }

        /// <summary>
        /// get custom declarations by id
        /// </summary>
        /// <param name="id">id that want to retrieve custom declarations</param>
        /// <returns></returns>
        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var results = customsDeclarationService.GetById(id);
            return Ok(results);
        }

        /// <summary>
        /// Delete multiple custom clearance
        /// </summary>
        /// <param name="customs">list of clearances selected</param>
        /// <returns></returns>
        [Authorize]
        [HttpPut]
        [Route("DeleteMultiple")]
        public IActionResult DeleteMultiple(List<CustomsDeclarationModel> customs)
        {
            var hs = customsDeclarationService.DeleteMultiple(customs);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// download file excel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("DownloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            string templateName = Templates.CustomDeclaration.ExelImportFileName + Templates.ExelImportEx;
            var result = await new FileHelper().ExportExcel(templateName);
            if (result != null)
            {
                return result;
            }
            else
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
            }
        }

        /// <summary>
        /// read data from file excel
        /// </summary>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("uploadFile")]
        public IActionResult UploadFile(IFormFile uploadedFile)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int? rowCount = worksheet.Dimension?.Rows;
                int? colCount = worksheet.Dimension?.Columns;
                if (rowCount < 2 || rowCount == null) return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.NOT_FOUND_DATA_EXCEL].Value });
                List<CustomsDeclarationModel> list = new List<CustomsDeclarationModel>();
                DateTime? dateNull = null;
                decimal? decimalNull = null;
                int? intNull = null;

                for (int row = 2; row < rowCount + 1; row++)
                {                   
                    var stage = new CustomsDeclarationModel
                    {
                        IsValid = true,
                        ClearanceNo = worksheet.Cells[row, 1]?.Value.ToString(),
                        Type = worksheet.Cells[row, 2].Value?.ToString(),
                        FirstClearanceNo = worksheet.Cells[row, 3].Value?.ToString(),
                        ClearanceDate = worksheet.Cells[row, 4].Value == null ? dateNull : Convert.ToDateTime(worksheet.Cells[row, 4].Value.ToString()),
                        PartnerTaxCode = worksheet.Cells[row, 5].Value?.ToString(),
                        CustomerName = worksheet.Cells[row, 6].Value?.ToString(),
                        Mblid = worksheet.Cells[row, 7].Value?.ToString(),
                        Hblid = worksheet.Cells[row, 8].Value?.ToString(),
                        Gateway = worksheet.Cells[row, 9].Value?.ToString(),
                        GrossWeight = worksheet.Cells[row, 10].Value == null ? decimalNull : Convert.ToDecimal(worksheet.Cells[row, 10].Value.ToString()),
                        NetWeight = worksheet.Cells[row, 11].Value == null ? decimalNull : Convert.ToDecimal(worksheet.Cells[row, 11].Value.ToString()),
                        Cbm = worksheet.Cells[row, 12].Value == null ? decimalNull : Convert.ToDecimal(worksheet.Cells[row, 12].Value.ToString()),
                        QtyCont = worksheet.Cells[row, 13].Value == null ? intNull : Convert.ToInt32(worksheet.Cells[row, 13].Value.ToString()),
                        Pcs = worksheet.Cells[row, 14].Value == null ? intNull : Convert.ToInt32(worksheet.Cells[row, 14].Value.ToString()),
                        CommodityCode = worksheet.Cells[row, 15].Value?.ToString(),
                        //CountryShipping = worksheet.Cells[row, 16].Value == null ? string.Empty : worksheet.Cells[row, 16].Value.ToString(),
                        CargoType = worksheet.Cells[row, 17].Value?.ToString(),
                        ServiceType = worksheet.Cells[row, 18].Value?.ToString(),
                        Route = worksheet.Cells[row, 19].Value?.ToString(),
                        ImportCountryCode = worksheet.Cells[row, 20].Value?.ToString(),
                        ExportCountryCode = worksheet.Cells[row, 21].Value?.ToString()
                    };
                    list.Add(stage);
                }
                var data = customsDeclarationService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }
    }
}
