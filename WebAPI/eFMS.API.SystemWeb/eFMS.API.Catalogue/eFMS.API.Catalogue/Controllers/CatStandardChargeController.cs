using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
    public class CatStandardChargeController : ControllerBase
    {
        
         private readonly IStringLocalizer stringLocalizer;
         private readonly ICatStandardChargeService catStandardChargeService;

         private readonly ICatChargeDefaultAccountService catChargeDefaultAccountService;
         private readonly IMapper mapper;
         private readonly ICurrentUser currentUser;
         private readonly IHostingEnvironment _hostingEnvironment;
         public CatStandardChargeController(ICatStandardChargeService service,
            ICatChargeDefaultAccountService catChargeDefaultAccount,
            IMapper imapper,
            ICurrentUser user,
            IHostingEnvironment hostingEnvironment
         )
         {
             catStandardChargeService = service;
             catChargeDefaultAccountService = catChargeDefaultAccount;
             mapper = imapper;
             currentUser = user;
             _hostingEnvironment = hostingEnvironment;
         }

        /// <summary>
        /// get all standard charge
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAll")]
        public IActionResult All()
        {
            var data = catStandardChargeService.Get();
            return Ok(data);
        }

        /// <summary>
        /// read standard charge data from file excel 
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
                if (rowCount < 2) return BadRequest();

                List<CatStandardChargeImportModel> list = new List<CatStandardChargeImportModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    var standardCharge = new CatStandardChargeImportModel
                    {
                        IsValid = true,
                        Id = Guid.NewGuid(),
                        ChargeId = Guid.Parse(worksheet.Cells[row, 1].Value?.ToString().Trim()),
                        Quantity = Convert.ToDecimal(worksheet.Cells[row, 2].Value?.ToString().Trim()),
                        UnitPrice = Convert.ToDecimal(worksheet.Cells[row, 3].Value?.ToString().Trim()),
                        Currency = Convert.ToDecimal(worksheet.Cells[row, 4].Value?.ToString().Trim()),
                        VatRate = Convert.ToDecimal(worksheet.Cells[row, 5].Value?.ToString().Trim()),
                        Type = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                        TransactionType = worksheet.Cells[row, 7].Value?.ToString().Trim(),
                        Service = worksheet.Cells[row, 8].Value?.ToString().Trim(),
                        ServiceType = worksheet.Cells[row, 9].Value?.ToString().Trim(),
                        Office = worksheet.Cells[row, 10].Value?.ToString().Trim(),
                        Note = worksheet.Cells[row, 11].Value?.ToString().Trim()
                    
                    };
                    list.Add(standardCharge);
                }
                return Ok();
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// import list standa charge into database
        /// </summary>
        /// <param name="data">list of data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("import")]
        [Authorize]
        public IActionResult Import([FromBody] List<CatStandardChargeImportModel> data)
        {
            var hs = catStandardChargeService.Import(data);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Import successfully!!!" };
            if (hs.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(new ResultHandle { Status = false, Message = hs.Exception.Message });
            }
        }

    }

}

