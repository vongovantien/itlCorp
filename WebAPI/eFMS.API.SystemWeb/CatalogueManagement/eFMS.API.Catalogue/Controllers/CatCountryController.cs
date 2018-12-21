using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Infrastructure.Common;
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
    public class CatCountryController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatCountryService catCountryService;
        private readonly ICurrentUser currentUser;
        private string templateName = "ImportTeamplate.xlsx";
        public CatCountryController(IStringLocalizer<LanguageSub> localizer, ICatCountryService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            catCountryService = service;
            currentUser = user;
        }

        [HttpPost]
        [Route("paging/{pageNumber}/{pageSize}")]
        public IActionResult Get(CatCountryCriteria criteria,int pageNumber,int pageSize)
        {
            var data = catCountryService.GetCountries(criteria,pageNumber,pageSize, out int rowCount);
            var result = new { data, totalItems = rowCount, pageNumber, pageSize };
            return Ok(result);
        }

        [HttpPost]
        [Route("query")]
        public IActionResult Get(CatCountryCriteria criteria)
        {
            var data = catCountryService.Query(criteria);
            return Ok(data);
        }

        [HttpGet]
        [Route("getById/{id}")]
        public IActionResult Get(int id)
        {
            var result = catCountryService.Get(x => x.Id == id).FirstOrDefault();
            return Ok(result);
        }

        [HttpGet]
        [Route("getAll")]
        public IActionResult GetAll()
        {
            var data = catCountryService.Get();
            return Ok(data);
        }

        [HttpPost]
        [Route("addNew")]
        [Authorize]
        public IActionResult Add(CatCountryModel catCountry)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(0, catCountry);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            catCountry.DatetimeCreated = DateTime.Now;
            catCountry.UserCreated = currentUser.UserID;
            catCountry.Inactive = false;
            var hs = catCountryService.Add(catCountry);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut]
        [Route("update")]
        [Authorize]
        public IActionResult Upadte(CatCountryModel catCountry)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(catCountry.Id, catCountry);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            catCountry.DatetimeModified = DateTime.Now;
            catCountry.UserModified = currentUser.UserID;
            var hs = catCountryService.Update(catCountry,x=>x.Id==catCountry.Id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete/{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = catCountryService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetByLanguage")]
        public IActionResult GetByLanguage()
        {
            var results = catCountryService.GetByLanguage();
            return Ok(results);
        }

        [HttpGet("DownloadExcel")]
        public async Task<ActionResult> DownloadExcel(CatPlaceTypeEnum type)
        {
            templateName = "Country" + templateName;
            var result = await new FileHelper().ExportExcel(templateName);
            if (result != null)
                return result;
            return BadRequest();
        }

        [HttpPost]
        [Route("UpLoadFile")]
      //  [Authorize]
        public IActionResult UpLoadFile(IFormFile uploadedFile)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int ColCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest();
                List<CatCountryImportModel> list = new List<CatCountryImportModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    var country = new CatCountryImportModel
                    {
                        IsValid = true,
                        Code = worksheet.Cells[row, 1].Value?.ToString(),
                        NameEn = worksheet.Cells[row, 2].Value?.ToString(),
                        NameVn = worksheet.Cells[row, 3].Value?.ToString(),
                        Status = worksheet.Cells[row, 4].Value?.ToString()
                    };
                    list.Add(country);
                }

                var data = catCountryService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(file);
        }
        [HttpPost]
        [Route("Import")]
        [Authorize]
        public IActionResult Import(List<CatCountryImportModel> data)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var result = catCountryService.Import(data);
            return Ok(result);
        }
        private string CheckExist(int id, CatCountryModel model)
        {
            string message = string.Empty;
            if (id == 0)
            {
                if (catCountryService.Any(x => x.Code.ToLower() == model.Code.ToLower()))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            else
            {
                if (catCountryService.Any(x => x.Code.ToLower() == model.Code.ToLower() && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            return message;
        }
    }
}