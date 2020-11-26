using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;

namespace eFMS.API.Catalogue.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatStageController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatStageService catStageService;
        private readonly ICurrentUser currentUser;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject IStringLocalizer service</param>
        /// <param name="service">inject ICatStageService service</param>
        /// <param name="user">inject ICurrentUser service</param>
        public CatStageController(IStringLocalizer<LanguageSub> localizer, ICatStageService service, ICurrentUser user, IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            catStageService = service;
            currentUser = user;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// get all
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var results = catStageService.GetAll();
            return Ok(results);
        }

        /// <summary>
        /// get and paging the list of stages by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="pageNumber">page to retrieve data</param>
        /// <param name="pageSize">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("getAll/{pageNumber}/{pageSize}")]
        public IActionResult Get(CatStageCriteria criteria,int pageNumber,int pageSize)
        {
            var data = catStageService.Paging(criteria, pageNumber, pageSize, out int rowCount);
            var result = new { data, totalItems = rowCount, pageNumber, pageSize };
            return Ok(result);
        }

        /// <summary>
        /// get the list of stages
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("query")]
        public IActionResult Get(CatStageCriteria criteria)
        {
            var data = catStageService.Query(criteria);      
            return Ok(data);
        }

        /// <summary>
        /// get stage by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("getById/{id}")]
        public IActionResult Get(int id)
        {
            //var results = catStageService.Get(x => x.Id == id).FirstOrDefault();
            var results = catStageService.GetAll().Where(x => x.Id == id).FirstOrDefault();
            return Ok(results);
        }

        /// <summary>
        /// add new stage
        /// </summary>
        /// <param name="catStageModel">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("addNew")]
        [Authorize]
        public IActionResult AddStage(CatStageModel catStageModel)
        {
            catStageModel.DatetimeCreated = catStageModel.DatetimeModified = DateTime.Now;
            catStageModel.UserCreated = currentUser.UserID;
            catStageModel.Active = true;
            var hs = catStageService.Add(catStageModel);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);           
        }

        /// <summary>
        /// update an existed item
        /// </summary>
        /// <param name="catStageModel">object to update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("update")]
        [Authorize]
        public IActionResult UpdateStage(CatStageModel catStageModel)
        {
            catStageModel.DatetimeModified = DateTime.Now;
            catStageModel.UserModified = currentUser.UserID;
            var hs = catStageService.Update(catStageModel,x=>x.Id==catStageModel.Id);
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
        [HttpDelete]
        [Route("delete/{id}")]
        [Authorize]
        public IActionResult DeleteStage(int id)
        {
            var hs = catStageService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// read data of file excel
        /// </summary>
        /// <param name="uploadedFile">file to upload</param>
        /// <returns></returns>
        [HttpPost]
        [Route("uploadFile")]
        public IActionResult UploadFile(IFormFile uploadedFile)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest();
                if(worksheet.Cells[1,1].Value.ToString()!= "DepartmentId")
                {
                    ResultHandle result = new ResultHandle { Status = false, Message = "Column 1 must have header is 'DepartmentId'" };
                    return BadRequest(result);
                }
                if (worksheet.Cells[1, 2].Value.ToString() != "Code")
                {
                    ResultHandle result = new ResultHandle { Status = false, Message = "Column 2 must have header is 'Code'" };
                    return BadRequest(result);
                }
                if (worksheet.Cells[1, 3].Value.ToString() != "StageNameVn")
                {
                    ResultHandle result = new ResultHandle { Status = false, Message = "Column 3 must have header is 'StageNameVn'" };
                    return BadRequest(result);
                }
                if (worksheet.Cells[1, 4].Value.ToString() != "StageNameEn")
                {
                    var hs = new HandleState("Column 4 must have header is 'StageNameEn'");
                    return BadRequest(hs);
                }
                if (worksheet.Cells[1, 5].Value.ToString() != "DescriptionVn")
                {
                    ResultHandle result = new ResultHandle { Status = false, Message = "Column 5 must have header is 'DescriptionVn'" };
                    return BadRequest(result);
                }
                if (worksheet.Cells[1, 6].Value.ToString() != "DescriptionEn")
                {
                    var hs = new HandleState("Column 6 must have header is 'DescriptionEn'");
                    return BadRequest(hs);
                }
                if (worksheet.Cells[1, 7].Value.ToString() != "Status")
                {
                    ResultHandle result = new ResultHandle { Status = false, Message = "Column 7 must have header is 'Status'" };
                    return BadRequest(result);
                }
                List<CatStageImportModel> list = new List<CatStageImportModel>();
                for(int row = 2; row <= rowCount; row++)
                {
                    var stage = new CatStageImportModel
                    {
                        IsValid = true,
                        DepartmentId = worksheet.Cells[row, 1].Value == null ? (int?)null : (int)Math.Ceiling((double)worksheet.Cells[row, 1].Value),
                        Code = worksheet.Cells[row, 2].Value?.ToString(),
                        StageNameVn = worksheet.Cells[row, 3].Value?.ToString(),
                        StageNameEn = worksheet.Cells[row, 4].Value?.ToString(),
                        DescriptionVn = worksheet.Cells[row, 5].Value?.ToString(),
                        DescriptionEn = worksheet.Cells[row, 6].Value?.ToString(),
                        Status = worksheet.Cells[row,7].Value?.ToString()
                    };
                    list.Add(stage);
                }
                var data = catStageService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// import list of stages
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("import")]
        [Authorize]
        public IActionResult Import([FromBody] List<CatStageImportModel> data)
        {
            // ChangeTrackerHelper.currentUser = currentUser.UserID;
            var result = catStageService.Import(data);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(new ResultHandle { Status = false, Message = result.Exception.Message });
            }
        }

        /// <summary>
        /// download a file excel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("downloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            try
            {
                string fileName = Templates.CatStage.ExcelImportFileName + Templates.ExcelImportEx;
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
            catch(Exception ex)
            {
                return BadRequest(new ResultHandle { Status = false, Message = ex.Message});
            }                
        }

    }
}