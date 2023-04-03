using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
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
    public class CatCityController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatCityService catCityService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;
        public CatCityController(ICatCityService service,
           IMapper imapper,
           ICurrentUser user)

        {
            catCityService = service;
            mapper = imapper;
            currentUser = user;
        }
        /// <summary>
        /// get all data
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAll")]
        public IActionResult GetAll()
        {
            var data = catCityService.Get();
            return Ok(data);
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
                if (worksheet.Cells[1, 1].Value?.ToString() != "Province Code")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'City Code' " });
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
                if (worksheet.Cells[1, 5].Value?.ToString() != "Inactive")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Inactive' " });
                }
                List<CatCityModel> list = new List<CatCityModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    var country = new CatCityModel
                    {
                        IsValid = true,
                        Code = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        NameEn = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        NameVn = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                        CodeCountry = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                        Status = worksheet.Cells[row, 5].Value?.ToString().Trim().ToLower()
                    };
                    list.Add(country);
                }

                var data = catCityService.CheckValidImport(list);
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
        public IActionResult Import([FromBody] List<CatCityModel> data)
        {
            var result = catCityService.Import(data);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(new ResultHandle { Status = false, Message = result.Exception.Message });
            }
        }


    }
}
