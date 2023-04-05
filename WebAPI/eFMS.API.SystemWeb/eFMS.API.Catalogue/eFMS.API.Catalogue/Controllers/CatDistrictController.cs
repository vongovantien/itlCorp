using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatDistrictController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatDistrictService catDistrictService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;
        public CatDistrictController(IStringLocalizer<LanguageSub> localizer, ICatDistrictService service,
           IMapper imapper,
           ICurrentUser user)

        {
            stringLocalizer = localizer;
            catDistrictService = service;
            mapper = imapper;
            currentUser = user;
        }
        /// <summary>
        /// get and paging the list of districts by cities
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("paging")]
        public IActionResult Get(CatDistrictCriteria criteria, int page, int size)
        {
            var data = catDistrictService.GetDistricts(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }
        /// <summary>
        /// get the list of districts
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost]
        [Route("query")]
        public IActionResult Get(CatDistrictCriteria criteria)
        {
            var data = catDistrictService.Query(criteria);
            return Ok(data);
        }

        /// <summary>
        /// get district by id
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet]
        [Route("getById/{id}")]
        public IActionResult Get(int id)
        {
            var result = catDistrictService.Get(x => x.Id == id).FirstOrDefault();
            return Ok(result);
        }
        /// <summary>
        /// add new district
        /// </summary>
        /// <param name="catDistrict">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("addNew")]
        [Authorize]
        public IActionResult Add(CatDistrictModel catDistrict)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(0, catDistrict);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }

            var hs = catDistrictService.Add(catDistrict);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
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
            var data = catDistrictService.Get();
            return Ok(data);
        }
        /// <summary>
        /// get data by city
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getDistrictByCity")]
        public IActionResult GetDistrictByCity(string codeCity)
        {
            var data = catDistrictService.GetDistrictsByCity(codeCity);
            return Ok(data);
        }
        /// <summary>
        /// get all district by current language
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetByLanguage")]
        public IActionResult GetByLanguage()
        {
            var results = catDistrictService.GetByLanguage();
            return Ok(results);
        }

        /// <summary>
        /// update an existed district
        /// </summary>
        /// <param name="catDistrict">object to update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("update")]
        [Authorize]
        public IActionResult Update(CatDistrictModel catDistrict)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(catDistrict.Id, catDistrict);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }

            var hs = catDistrictService.Update(catDistrict);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            catDistrictService.ClearCache();
            return Ok(result);
        }

        /// <summary>
        /// delete an existed district
        /// </summary>
        /// <param name="id">id of data that need to delete</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("delete/{id}")]
        [Authorize]
        public IActionResult Delete(short id)
        {
            var hs = catDistrictService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// read standard charge data from file excel 
        /// </summary>
        /// <param name="uploadedFile">file to read data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("uploadFile")]
        public IActionResult UpLoadFile(IFormFile uploadedFile)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int ColCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.NOT_FOUND_DATA_EXCEL].Value });
                if (worksheet.Cells[1, 1].Value?.ToString() != "District Code")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'District Code' " });
                }
                if (worksheet.Cells[1, 2].Value?.ToString() != "English Name")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'English Name' " });
                }
                if (worksheet.Cells[1, 3].Value?.ToString() != "Local Name")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Local Name' " });
                }
                if (worksheet.Cells[1, 4].Value?.ToString() != "Country")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Country' " });
                }
                if (worksheet.Cells[1, 5].Value?.ToString() != "Province")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Province' " });
                }
                if (worksheet.Cells[1, 5].Value?.ToString() != "Inactive")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Inactive' " });
                }
                List<CatDistrictModel> list = new List<CatDistrictModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    var country = new CatDistrictModel
                    {
                        IsValid = true,
                        Code = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        NameEn = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        NameVn = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                        CodeCountry = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                        CodeCity = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                        Status = worksheet.Cells[row, 6].Value?.ToString().Trim().ToLower()
                    };
                    list.Add(country);
                }

                var data = catDistrictService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// import list districts
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Import")]
        [Authorize]
        public IActionResult Import([FromBody] List<CatDistrictModel> data)
        {
            var result = catDistrictService.Import(data);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(new ResultHandle { Status = false, Message = result.Exception.Message });
            }
        }

        private string CheckExist(int id, CatDistrictModel model)
        {
            string message = string.Empty;
            if (id == 0)
            {
                if (catDistrictService.Any(x => x.Code.ToLower() == model.Code.ToLower() && x.CodeCity.ToLower() == model.CodeCity.ToLower()))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            else
            {
                if (catDistrictService.Any(x => x.Code.ToLower() == model.Code.ToLower() && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            return message;
        }

    }
}

