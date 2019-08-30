using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.NoSql;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Shipment.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using SystemManagementAPI.Infrastructure.Middlewares;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class OpsTransactionController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IOpsTransactionService transactionService;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject IStringLocalizer</param>
        /// <param name="user">inject ICurrentUser</param>
        /// <param name="service">inject IOpsTransactionService</param>
        public OpsTransactionController(IStringLocalizer<LanguageSub> localizer, ICurrentUser user, IOpsTransactionService service, IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            currentUser = user;
            transactionService = service;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// get the list of countries by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost("Query")]
        public IActionResult Query(OpsTransactionCriteria criteria)
        {
            var results = transactionService.Query(criteria);
            return Ok(results);
        }

        /// <summary>
        /// get data detail by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get(Guid id)
        {
            var result = transactionService.GetDetails(id); //transactionService.First(x => x.Id == id);
            return Ok(result);
        }

        /// <summary>
        /// get and paging the list of countries by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost("Paging")]
        public IActionResult Paging(OpsTransactionCriteria criteria, int page, int size)
        {
            var data = transactionService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// add new job
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("Add")]
        public IActionResult Add(OpsTransactionModel model)
        {
            var existedMessage = transactionService.CheckExist(model);
            if (existedMessage != null)
            {
                return Ok(new ResultHandle { Status = false, Message = existedMessage });
            }
            model.Hblid = Guid.NewGuid();
            var hs = transactionService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model.Id };
            if (!hs.Success)
            {
                return BadRequest(result);
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
        public IActionResult Update(OpsTransactionModel model)
        {
            var existedMessage = transactionService.CheckExist(model);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }

            model.ModifiedDate = DateTime.Now;
            model.UserModified = currentUser.UserID;
            var hs = transactionService.Update(model,x=>x.Id==model.Id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of item that want to delete</param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        [Route("Delete/{id}")]
        public IActionResult Delete(Guid id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            if (transactionService.CheckAllowDelete(id) == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.MSG_NOT_ALLOW_DELETED].Value });
            }
            var hs = transactionService.SoftDeleteJob(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// check an item that is allowed delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("CheckAllowDelete/{id}")]
        public IActionResult CheckAllowDelete(Guid id)
        {
            return Ok(transactionService.CheckAllowDelete(id));
        }

        /// <summary>
        /// convert a custom clearance to a job
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("ConvertClearanceToJob")]
        [Authorize]
        public IActionResult ConvertClearanceToJob(OpsTransactionClearanceModel model)
        {
            var hs = transactionService.ConvertClearanceToJob(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = message };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// convert multi clearances to multi jobs
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [HttpPost("ConvertExistedClearancesToJobs")]
        [Authorize]
        public IActionResult ConvertExistedClearancesToJobs([FromBody]List<OpsTransactionClearanceModel> list)
        {
            HandleState hs = transactionService.ConvertExistedClearancesToJobs(list);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = message };
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
        [HttpGet("DownloadContainerExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            string fileName = Templates.Container.ExelImportFileName + Templates.ExelImportEx;
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

        /// <summary>
        /// read data from file excel
        /// </summary>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UploadFileContainer")]
        public IActionResult UploadFileContainer(IFormFile uploadedFile)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.NOT_FOUND_DATA_EXCEL].Value });
                List<CsMawbcontainerImportModel> list = new List<CsMawbcontainerImportModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    var container = new CsMawbcontainerImportModel
                    {
                        IsValid = true,
                        ContainerTypeName = worksheet.Cells[row, 1].Value == null ? string.Empty : worksheet.Cells[row, 1].Value.ToString().Trim(),
                        Quantity = worksheet.Cells[row, 2].Value == null ? null : (int?)worksheet.Cells[row, 2].Value,
                        ContainerNo = worksheet.Cells[row, 3].Value == null ? string.Empty : worksheet.Cells[row, 3].Value.ToString().Trim(),
                        SealNo = worksheet.Cells[row, 4].Value == null ? string.Empty : worksheet.Cells[row, 4].Value.ToString().Trim(),
                        Gw = worksheet.Cells[row, 5].Value == null ? null : (decimal?)worksheet.Cells[row, 5].Value,
                        Cbm = worksheet.Cells[row, 6].Value == null ? null : (decimal?)worksheet.Cells[row, 6].Value,
                        Nw = worksheet.Cells[row, 7].Value == null ? null : (decimal?)worksheet.Cells[row, 7].Value,
                        PackageQuantity = worksheet.Cells[row, 8].Value == null ? null : (short?)worksheet.Cells[row, 8].Value,
                        PackageTypeName = worksheet.Cells[row, 9].Value == null ? string.Empty : worksheet.Cells[row, 9].Value.ToString().Trim(),
                        MarkNo = worksheet.Cells[row, 10].Value == null ? string.Empty : worksheet.Cells[row, 10].Value.ToString().Trim(),
                        Description = worksheet.Cells[row, 11].Value == null ? string.Empty : worksheet.Cells[row, 11].Value.ToString().Trim(),
                        CommodityName = worksheet.Cells[row, 12].Value == null ? string.Empty : worksheet.Cells[row, 12].Value.ToString().Trim(),
                        UnitOfMeasureName = worksheet.Cells[row, 13].Value == null ? string.Empty : worksheet.Cells[row, 13].Value.ToString().Trim()
                    };
                    list.Add(container);
                }
                var data = transactionService.CheckValidContainerImport(list);
                var totalValidRows = list.Count(x => x.IsValid == true);
                var results = new { list, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// import list container
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Importcontainer")]
        [Authorize]
        public IActionResult Importcontainer([FromBody] List<CsMawbcontainerImportModel> data)
        {
            var result = transactionService.Importcontainer(data);
            if (result.Success)
            {
                return Ok(result);
            }
            return Ok(new ResultHandle { Status = false, Message = result.Exception.Message });
        }
    }
}
