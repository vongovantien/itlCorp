using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Catalogue.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
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
    public class CatCommodityGroupController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatCommodityGroupService catComonityGroupService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface ICatCommodityGroupService</param>
        /// <param name="iMapper">inject interface IMapper</param>
        /// <param name="user"></param>
        public CatCommodityGroupController(IStringLocalizer<LanguageSub> localizer, ICatCommodityGroupService service, IMapper iMapper,
            ICurrentUser user,
            IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            catComonityGroupService = service;
            mapper = iMapper;
            currentUser = user;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// get the list of commodities by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Query")]
        public IActionResult Get(CatCommodityGroupCriteria criteria)
        {
            var results = catComonityGroupService.Query(criteria);
            return Ok(results);
        }

        /// <summary>
        /// get and paging the list of commodities by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        public IActionResult Get(CatCommodityGroupCriteria criteria, int page, int size)
        {
            var data = catComonityGroupService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// get all commodities by current language
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetByLanguage")]
        public IActionResult GetByLanguage()
        {
            var results = catComonityGroupService.GetByLanguage();
            return Ok(results);
        }

        /// <summary>
        /// get commodity by id
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult Get(short id)
        {
            var data = catComonityGroupService.First(x => x.Id == id);
            return Ok(data);
        }

        /// <summary>
        /// add new commodity
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Post(CatCommodityGroupEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var checkExistMessage = CheckExist(0, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var catCommodityGroup = mapper.Map<CatCommodityGroupModel>(model);
            catCommodityGroup.UserCreated = currentUser.UserID;
            catCommodityGroup.DatetimeCreated = catCommodityGroup.DatetimeModified = DateTime.Now;
            catCommodityGroup.Active = true;
            var hs = catComonityGroupService.Add(catCommodityGroup);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            catComonityGroupService.ClearCache();
            catComonityGroupService.Get();
            return Ok(result);
        }

        /// <summary>
        /// update an existed item
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <param name="model">object to update</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Put(short id, CatCommodityGroupEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var commonityGroup = mapper.Map<CatCommodityGroupModel>(model);
            commonityGroup.UserModified = currentUser.UserID;
            commonityGroup.DatetimeModified = DateTime.Now;
            commonityGroup.Id = id;
            if (commonityGroup.Active == false)
            {
                commonityGroup.InactiveOn = DateTime.Now;
            }
            var hs = catComonityGroupService.Update(commonityGroup, x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            catComonityGroupService.ClearCache();
            catComonityGroupService.Get();
            return Ok(result);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of data that need to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(short id)
        {
            //ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = catComonityGroupService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            catComonityGroupService.ClearCache();
            catComonityGroupService.Get();
            return Ok(result);
        }
        private string CheckExist(short id, CatCommodityGroupEditModel model)
        {
            string message = string.Empty;
            if (id == 0)
            {
                if (catComonityGroupService.Any(x => x.GroupNameEn == model.GroupNameEn && x.GroupNameVn == model.GroupNameVn))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            else
            {
                if (catComonityGroupService.Any(x => x.GroupNameEn == model.GroupNameEn && x.GroupNameVn == model.GroupNameVn && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            return message;
        }

        /// <summary>
        /// read commodity group data from file excel
        /// </summary>
        /// <param name="uploadedFile">file to read data</param>
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
                if (worksheet.Cells[1, 1].Value?.ToString() != "English Name")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'English Name'" });
                }
                if (worksheet.Cells[1, 2].Value?.ToString() != "Local Name")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 2 must have header is 'Local Name'" });
                }
                if (worksheet.Cells[1, 3].Value?.ToString() != "Status")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 3 must have header is 'Status'" });
                }
                List<CommodityGroupImportModel> list = new List<CommodityGroupImportModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    var commodityGroup = new CommodityGroupImportModel
                    {
                        IsValid = true,
                        GroupNameEn = worksheet.Cells[row, 1].Value?.ToString(),
                        GroupNameVn = worksheet.Cells[row, 2].Value?.ToString(),
                        Status = worksheet.Cells[row, 3].Value?.ToString()
                    };
                    list.Add(commodityGroup);
                }
                var data = catComonityGroupService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = "Cannot upload, file not found !" });
        }


        /// <summary>
        /// import list commodity groups into database
        /// </summary>
        /// <param name="data">list of data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("import")]
        [Authorize]
        public IActionResult Import([FromBody] List<CommodityGroupImportModel> data)
        {
            var result = catComonityGroupService.Import(data);
            return Ok(result);
        }

        /// <summary>
        /// download exel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("downloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            try
            {
                string fileName = Templates.CatCommodityGroup.ExcelImportFileName + Templates.ExcelImportEx;
                string templateName = _hostingEnvironment.ContentRootPath;
                var result = await new FileHelper().ExportExcel(templateName, fileName);
                if (result != null)
                {
                    return result;
                }
                else
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "File not found !" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "File not found !" });
            }
        }

    }
}