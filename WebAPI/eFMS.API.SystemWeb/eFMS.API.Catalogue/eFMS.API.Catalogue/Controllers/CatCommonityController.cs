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
    public class CatCommonityController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatCommodityService catComonityService;
        private readonly IMapper mapper;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface ICatAreaService</param>
        /// <param name="iMapper">inject interface IMapper</param>
        public CatCommonityController(IStringLocalizer<LanguageSub> localizer, ICatCommodityService service, IMapper iMapper, IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            catComonityService = service;
            mapper = iMapper;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// get all data
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var results = catComonityService.GetAll();
            return Ok(results);
        }

        /// <summary>
        /// get the list of commodities
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Query")]
        public IActionResult Get(CatCommodityCriteria criteria)
        {
            var results = catComonityService.Query(criteria);
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
        public IActionResult Get(CatCommodityCriteria criteria, int page, int size)
        {
            var data = catComonityService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// get commodity by id
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult Get(short id)
        {
            var data = catComonityService.First(x => x.Id == id);
            return Ok(data);
        }

        /// <summary>
        /// add a new commodity
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Post(CatCommodityEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var checkExistMessage = CheckExist(0, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var commonity = mapper.Map<CatCommodityModel>(model);
            var hs = catComonityService.Add(commonity);
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
        /// <param name="id">id of data that need to retrieve</param>
        /// <param name="model">object to update</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Put(short id, CatCommodityEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var commodity = mapper.Map<CatCommodityModel>(model);
            commodity.Id = id;
            var hs = catComonityService.Update(commodity);
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
        /// <param name="id">id of data that need to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(short id)
        {
            var hs = catComonityService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        private string CheckExist(short id, CatCommodityEditModel model)
        {
            string message = string.Empty;
            if (id == 0)
            {
                if (catComonityService.Any(x => x.Code == model.Code))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            else
            {
                if (catComonityService.Any(x => (x.Code == model.Code) && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            return message;
        }

        /// <summary>
        /// read commodities data from file excel
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
                if (worksheet.Cells[1, 1].Value?.ToString() != "Code")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Code'" });
                }
                if (worksheet.Cells[1, 2].Value?.ToString() != "English Name")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 2 must have header is 'English Name'" });
                }
                if (worksheet.Cells[1, 3].Value?.ToString() != "Local Name")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 3 must have header is 'Local Name'" });
                }
                if (worksheet.Cells[1, 4].Value?.ToString() != "Commodity Group ID")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 4 must have header is 'Commodity Group ID'" });
                }
                if (worksheet.Cells[1, 5].Value?.ToString() != "Status")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 4 must have header is 'Status'" });
                }
                List<CommodityImportModel> list = new List<CommodityImportModel>();
                for(int row = 2; row <= rowCount; row++)
                {
                    var commodity = new CommodityImportModel
                    {
                        IsValid = true,
                        Code = worksheet.Cells[row, 1].Value?.ToString(),
                        CommodityNameEn = worksheet.Cells[row, 2].Value?.ToString(),
                        CommodityNameVn = worksheet.Cells[row, 3].Value?.ToString(),
                        CommodityGroupId = worksheet.Cells[row, 4].Value == null ? (short?)null : Convert.ToInt16(worksheet.Cells[row, 4].Value),
                        Status = worksheet.Cells[row,5].Value?.ToString()
                    };
                    list.Add(commodity);
                }
                var data = catComonityService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);

            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// import list commodities into database
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("import")]
        [Authorize]
        public IActionResult Import([FromBody] List<CommodityImportModel> data)
        {
            var result = catComonityService.Import(data);
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
        /// download exel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("downloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            try
            {
                var fileName = Templates.CatCommodity.ExcelImportFileName + Templates.ExcelImportEx;
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

    }
}