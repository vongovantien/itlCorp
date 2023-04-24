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
    public class CatWardController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatWardService catWardService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;
        public CatWardController(IStringLocalizer<LanguageSub> localizer, ICatWardService service,
           IMapper imapper,
           ICurrentUser user)

        {
            stringLocalizer = localizer;
            catWardService = service;
            mapper = imapper;
            currentUser = user;
        }
        /// <summary>
        /// get and paging the list of wards by cities
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("paging")]
        public IActionResult Get(CatWardCriteria criteria, int page, int size)
        {
            var data = catWardService.GetWards(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }
        /// <summary>
        /// get the list of wards
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost]
        [Route("query")]
        public IActionResult Get(CatWardCriteria criteria)
        {
            var data = catWardService.Query(criteria);
            return Ok(data);
        }

        /// <summary>
        /// get ward by id
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet]
        [Route("getById/{id}")]
        public IActionResult Get(Guid id)
        {
            var result = catWardService.Get(x => x.Id == id).FirstOrDefault();
            return Ok(result);
        }
        /// <summary>
        /// add new ward
        /// </summary>
        /// <param name="catWard">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("addNew")]
        [Authorize]
        public IActionResult Add(CatWardModel catWard)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(string.Empty, catWard);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }

            var hs = catWardService.Add(catWard);
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
            var data = catWardService.Get();
            return Ok(data);
        }
        /// <summary>
        /// get data by District
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getWardByDistrict")]
        public IActionResult GetWardByDistrict(Guid districtID)
        {
            var data = catWardService.GetWardsByDistrict(districtID);
            return Ok(data);
        }
        /// <summary>
        /// get all wards by current language
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetByLanguage")]
        public IActionResult GetByLanguage()
        {
            var results = catWardService.GetByLanguage();
            return Ok(results);
        }

        /// <summary>
        /// update an existed ward
        /// </summary>
        /// <param name="catWard">object to update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("update")]
        [Authorize]
        public IActionResult Update(CatWardModel catWard)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(catWard.Id.ToString(), catWard);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }

            var hs = catWardService.Update(catWard);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            catWardService.ClearCache();
            return Ok(result);
        }

        /// <summary>
        /// delete an existed ward
        /// </summary>
        /// <param name="id">id of data that need to delete</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("delete/{id}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var hs = catWardService.Delete(id);
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
                if (worksheet.Cells[1, 1].Value?.ToString() != "Ward Code")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Code' " });
                }
                if (worksheet.Cells[1, 2].Value?.ToString() != "English Name")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'English Name' " });
                }
                if (worksheet.Cells[1, 3].Value?.ToString() != "Local Name")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Local Name' " });
                }
                if (worksheet.Cells[1, 4].Value?.ToString() != "Country Code")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Country' " });
                }
                if (worksheet.Cells[1, 5].Value?.ToString() != "Province Code")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Province' " });
                }
                if (worksheet.Cells[1, 6].Value?.ToString() != "District Code")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'District' " });
                }
                if (worksheet.Cells[1, 7].Value?.ToString() != "Inactive")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Inactive' " });
                }
                List<CatWardModel> list = new List<CatWardModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    var ward = new CatWardModel
                    {
                        IsValid = true,
                        Code = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        NameEn = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        NameVn = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                        CodeCountry = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                        CodeCity = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                        CodeDistrict = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                        Status = worksheet.Cells[row, 7].Value?.ToString().Trim().ToLower()
                    };
                    list.Add(ward);
                }

                var data = catWardService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// import list wards
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Import")]
        [Authorize]
        public IActionResult Import([FromBody] List<CatWardModel> data)
        {
            var result = catWardService.Import(data);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(new ResultHandle { Status = false, Message = result.Exception.Message });
            }
        }

        private string CheckExist(string id, CatWardModel model)
        {
            string message = string.Empty;
            if (id == string.Empty)
            {
                if (catWardService.Any(x => x.Code.ToLower() == model.Code.ToLower() && x.DistrictId.ToString().ToLower() == model.DistrictId.ToString().ToLower()
                    && x.CityId.ToString().ToLower() == model.CityId.ToString().ToLower()
                    && x.CountryId == model.CountryId))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            else
            {
                if (catWardService.Any(x => x.Code.ToLower() == model.Code.ToLower() && x.Id.ToString().ToLower() != id.ToLower()  
                    && x.DistrictId.ToString().ToLower() == model.DistrictId.ToString().ToLower() 
                    && x.CityId.ToString().ToLower() == model.CityId.ToString().ToLower()
                    && x.CountryId == model.CountryId))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            return message;
        }

    }
}
