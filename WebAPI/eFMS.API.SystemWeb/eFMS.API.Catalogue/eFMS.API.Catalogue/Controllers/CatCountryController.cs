using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
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
    public class CatCountryController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatCountryService catCountryService;
        private readonly ICurrentUser currentUser;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject IStringLocalizer</param>
        /// <param name="service">inject ICatCountryService serrvice</param>
        /// <param name="user"></param>
        public CatCountryController(IStringLocalizer<LanguageSub> localizer, ICatCountryService service, ICurrentUser user, IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            catCountryService = service;
            currentUser = user;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// get and paging the list of countries by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("paging")]
        public IActionResult Get(CatCountryCriteria criteria, int page, int size)
        {
            var data = catCountryService.GetCountries(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// get the list of countries
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost]
        [Route("query")]
        public IActionResult Get(CatCountryCriteria criteria)
        {
            var data = catCountryService.Query(criteria);
            return Ok(data);
        }

        /// <summary>
        /// get country by id
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet]
        [Route("getById/{id}")]
        public IActionResult Get(int id)
        {
            var result = catCountryService.Get(x => x.Id == id).FirstOrDefault();
            return Ok(result);
        }
        
        /// <summary>
        /// get all data
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAll")]
        public IActionResult GetAll()
        {
            var data = catCountryService.Get();
            return Ok(data);
        }

        /// <summary>
        /// add new country
        /// </summary>
        /// <param name="catCountry">object to add</param>
        /// <returns></returns>
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
            
            var hs = catCountryService.Add(catCountry);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// update an existed country
        /// </summary>
        /// <param name="catCountry">object to update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("update")]
        [Authorize]
        public IActionResult Update(CatCountryModel catCountry)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(catCountry.Id, catCountry);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }

            var hs = catCountryService.Update(catCountry);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            catCountryService.ClearCache();
            return Ok(result);
        }

        /// <summary>
        /// delete an existed country
        /// </summary>
        /// <param name="id">id of data that need to delete</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("delete/{id}")]
        [Authorize]
        public IActionResult Delete(short id)
        {
            var hs = catCountryService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// get all countries by current language
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetByLanguage")]
        public IActionResult GetByLanguage()
        {
            var results = catCountryService.GetByLanguage();
            return Ok(results);
        }

        /// <summary>
        /// download exel file from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("DownloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            string fileName = Templates.CatCountry.ExcelImportFileName + Templates.ExcelImportEx;
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
        /// read a file excel
        /// </summary>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
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
                if (worksheet.Cells[1, 1].Value?.ToString() != "Country Code")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Country Code' " });
                }
                if (worksheet.Cells[1, 2].Value?.ToString() != "English Name")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'English Name' " });
                }
                if (worksheet.Cells[1, 3].Value?.ToString() != "Local Name")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Local Name' " });
                }
                if (worksheet.Cells[1, 4].Value?.ToString() != "Inactive")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Inactive' " });
                }
                List<CatCountryImportModel> list = new List<CatCountryImportModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    var country = new CatCountryImportModel
                    {
                        IsValid = true,
                        Code = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        NameEn = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        NameVn = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                        Status = worksheet.Cells[row, 4].Value?.ToString().Trim().ToLower()
                    };
                    list.Add(country);
                }

                var data = catCountryService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// import list countries
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Import")]
        [Authorize]
        public IActionResult Import([FromBody]List<CatCountryImportModel> data)
        {
            var result = catCountryService.Import(data);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(new ResultHandle { Status = false, Message = result.Exception.Message });
            }
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