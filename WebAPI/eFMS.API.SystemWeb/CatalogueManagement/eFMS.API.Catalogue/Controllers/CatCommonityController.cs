using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Catalogue.Models;
using eFMS.API.Catalogue.Service.Helpers;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

namespace eFMS.API.Catalogue.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatCommonityController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatCommodityService catComonityService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;
        private string templateName = "ImportTemplate.xlsx";
        public CatCommonityController(IStringLocalizer<LanguageSub> localizer, ICatCommodityService service, IMapper iMapper, ICurrentUser user)
        {
            stringLocalizer = localizer;
            catComonityService = service;
            mapper = iMapper;
            currentUser = user;
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Get(CatCommodityCriteria criteria)
        {
            var results = catComonityService.Query(criteria);
            return Ok(results);
        }

        [HttpPost]
        [Route("Paging")]
        public IActionResult Get(CatCommodityCriteria criteria, int page, int size)
        {
            var data = catComonityService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult Get(short id)
        {
            var data = catComonityService.First(x => x.Id == id);
            return Ok(data);
        }

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
            commonity.UserCreated = currentUser.UserID;
            commonity.DatetimeCreated = DateTime.Now;
            commonity.Inactive = false;
            var hs = catComonityService.Add(commonity);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
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
            var commonity = mapper.Map<CatCommodityModel>(model);
            commonity.UserModified = currentUser.UserID;
            commonity.DatetimeModified = DateTime.Now;
            commonity.Id = id;
            if (commonity.Inactive == true)
            {
                commonity.InactiveOn = DateTime.Now;
            }
            var hs = catComonityService.Update(commonity, x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(short id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = catComonityService.Delete(x => x.Id == id);
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
                if (catComonityService.Any(x => x.CommodityNameEn == model.CommodityNameEn || x.CommodityNameVn == model.CommodityNameVn))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            else
            {
                if (catComonityService.Any(x => (x.CommodityNameEn == model.CommodityNameEn || x.CommodityNameVn == model.CommodityNameVn) && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            return message;
        }

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
                if (worksheet.Cells[1, 3].Value?.ToString() != "Commodity Group ID")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 3 must have header is 'Commodity Group ID'" });
                }
                if (worksheet.Cells[1, 4].Value?.ToString() != "Status")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 4 must have header is 'Status'" });
                }
                List<CommodityImportModel> list = new List<CommodityImportModel>();
                for(int row = 2; row <= rowCount; row++)
                {
                    var commodity = new CommodityImportModel
                    {
                        IsValid = true,
                        CommodityNameEn = worksheet.Cells[row, 1].Value?.ToString(),
                        CommodityNameVn = worksheet.Cells[row, 2].Value?.ToString(),
                        CommodityGroupId = worksheet.Cells[row, 3].Value == null ? (short?)null : Convert.ToInt16(worksheet.Cells[row, 3].Value),
                        Status = worksheet.Cells[row,4].Value?.ToString()
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


        [HttpPost]
        [Route("import")]
        [Authorize]
        public IActionResult Import([FromBody] List<CommodityImportModel> data)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
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



        [HttpGet("downloadExcel")]
        public async Task<ActionResult> DownloadExcel(CatPlaceTypeEnum type)
        {

            try
            {
                templateName = "Commodity" + templateName;
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
            catch (Exception ex)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
            }


        }

    }
}