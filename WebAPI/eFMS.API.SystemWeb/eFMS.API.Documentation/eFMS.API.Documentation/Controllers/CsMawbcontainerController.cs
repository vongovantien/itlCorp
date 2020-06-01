using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using SystemManagementAPI.Infrastructure.Middlewares;

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsMawbcontainerController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsMawbcontainerService csContainerService;
        private readonly ICurrentUser currentUser;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="user"></param>
        /// <param name="hostingEnvironment"></param>
        public CsMawbcontainerController(IStringLocalizer<LanguageSub> localizer, ICsMawbcontainerService service, ICurrentUser user, IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            csContainerService = service;
            currentUser = user;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// get container by criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Query")]
        public IActionResult Query(CsMawbcontainerCriteria criteria)
        {
            return Ok(csContainerService.Query(criteria));
        }

        /// <summary>
        /// update container
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        //[Authorize]
        public IActionResult Update(CsMawbcontainerEdit model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = csContainerService.Update(model.CsMawbcontainerModels, model.MasterId, model.HousebillId);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// get list containers by jobId
        /// </summary>
        /// <param name="JobId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetHBConts")]
        public List<object> GetHBContainers(Guid JobId)
        {
            return csContainerService.ListContOfHB(JobId);
        }

        /// <summary>
        /// download file excel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("downloadFileExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            string fileName = Templates.Container.ExelImportFileName + Templates.ExelImportEx;
            string templateName = _hostingEnvironment.ContentRootPath;
            var result = await new FileHelper().ExportExcel(templateName, fileName);
            if (result != null)
            {
                return result;
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// download file excel from server
        /// </summary>
        /// <returns></returns>
        [HttpPost("downloadGoodsFileExcel")]
        public async Task<ActionResult> DownloadGoodsExcel()
        {
            string fileName = Templates.Goods.ExelImportFileName + Templates.ExelImportEx;
            string templateName = _hostingEnvironment.ContentRootPath;
            var result = await new FileHelper().ExportExcel(templateName, fileName);
            if (result != null)
            {
                return result;
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// import list container
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Import")]
        [Authorize]
        public IActionResult Importcontainer([FromBody] List<CsMawbcontainerImportModel> data)
        {
            var result = csContainerService.Importcontainer(data);
            if (result.Success)
            {
                return Ok(result);
            }
            return Ok(new ResultHandle { Status = false, Message = result.Exception.Message });
        }

        /// <summary>
        /// read data from file excel
        /// </summary>
        /// <param name="uploadedFile"></param>
        /// <param name="id"></param>
        /// <param name="isHouseBill"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UploadFile")]
        public IActionResult UploadFileContainer(IFormFile uploadedFile, [Required]Guid id, bool isHouseBill = false)
        {
            Guid? hblid = null;
            Guid? mblid = null;
            if (isHouseBill) hblid = id;
            else mblid = id;

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
                    var containerTypeName = worksheet.Cells[row, 1].Value == null ? string.Empty : worksheet.Cells[row, 1].Value.ToString().Trim();
                    if (containerTypeName.Contains("´")) containerTypeName = containerTypeName.Replace("´", "'");
                    var container = new CsMawbcontainerImportModel
                    {
                        IsValid = true,
                        ContainerTypeName = containerTypeName,
                        QuantityError = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        ContainerNo = worksheet.Cells[row, 3].Value == null ? string.Empty : worksheet.Cells[row, 3].Value.ToString().Trim(),
                        SealNo = worksheet.Cells[row, 4].Value == null ? string.Empty : worksheet.Cells[row, 4].Value.ToString().Trim(),
                        GwError = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                        CbmError = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                        NwError = worksheet.Cells[row, 7].Value?.ToString().Trim(),
                        PackageQuantityError = worksheet.Cells[row, 8].Value?.ToString().Trim(),
                        PackageTypeName = worksheet.Cells[row, 9].Value == null ? string.Empty : worksheet.Cells[row, 9].Value.ToString().Trim(),
                        MarkNo = worksheet.Cells[row, 10].Value == null ? string.Empty : worksheet.Cells[row, 10].Value.ToString().Trim(),
                        Description = worksheet.Cells[row, 11].Value == null ? string.Empty : worksheet.Cells[row, 11].Value.ToString().Trim(),
                        CommodityName = worksheet.Cells[row, 12].Value == null ? string.Empty : worksheet.Cells[row, 12].Value.ToString().Trim(),
                        UnitOfMeasureName = worksheet.Cells[row, 13].Value == null ? string.Empty : worksheet.Cells[row, 13].Value.ToString().Trim()
                    };
                    list.Add(container);
                }
                var data = csContainerService.CheckValidContainerImport(list, mblid, hblid);
                var totalValidRows = list.Count(x => x.IsValid == true);
                var duplicatedError = list.FirstOrDefault(x => x.DuplicateError != null)?.DuplicateError;
                var existedError = list.FirstOrDefault(x => x.ExistedError != null)?.ExistedError;
                var results = new { list, totalValidRows, duplicatedError, existedError };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// read data from file excel
        /// </summary>
        /// <param name="uploadedFile"></param>
        /// <param name="id"></param>
        /// <param name="isHouseBill"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UploadGoodsFile")]
        public IActionResult UploadGoodsFile(IFormFile uploadedFile, [Required]Guid id, bool isHouseBill = false)
        {
            Guid? hblid = null;
            Guid? mblid = null;
            if (isHouseBill) hblid = id;
            else mblid = id;

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
                    var containerType = worksheet.Cells[row, 1].Value == null ? string.Empty : worksheet.Cells[row, 1].Value.ToString().Trim();
                    if (containerType.Contains("´")) containerType = containerType.Replace("´", "'");
                    var container = new CsMawbcontainerImportModel
                    {
                        IsValid = true,
                        ContainerTypeName = containerType,
                        QuantityError = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        GwError = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                        CbmError = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                        PackageTypeName = worksheet.Cells[row, 5].Value == null ? string.Empty : worksheet.Cells[row, 5].Value.ToString().Trim(),
                        PackageQuantityError = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                        ContainerNo = worksheet.Cells[row, 7].Value == null ? string.Empty : worksheet.Cells[row, 7].Value.ToString().Trim(),
                        SealNo = worksheet.Cells[row, 8].Value == null ? string.Empty : worksheet.Cells[row, 8].Value.ToString().Trim(),
                        MarkNo = worksheet.Cells[row, 9].Value == null ? string.Empty : worksheet.Cells[row, 9].Value.ToString().Trim(),
                        CommodityName = worksheet.Cells[row, 10].Value == null ? string.Empty : worksheet.Cells[row, 10].Value.ToString().Trim(),
                        Description = worksheet.Cells[row, 11].Value == null ? string.Empty : worksheet.Cells[row, 11].Value.ToString().Trim()
                    };
                    list.Add(container);
                }
                var data = csContainerService.CheckValidGoodsImport(list, mblid, hblid);
                var totalValidRows = list.Count(x => x.IsValid == true);
                var duplicatedError = list.FirstOrDefault(x => x.DuplicateError != null)?.DuplicateError;
                var existedError = list.FirstOrDefault(x => x.ExistedError != null)?.ExistedError;
                var results = new { list, totalValidRows, duplicatedError, existedError };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }
    }
}
