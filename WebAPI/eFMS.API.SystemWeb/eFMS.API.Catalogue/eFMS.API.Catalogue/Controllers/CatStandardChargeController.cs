using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
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
         private readonly IMapper mapper;
         private readonly ICurrentUser currentUser;
         public CatStandardChargeController(ICatStandardChargeService service,
            IMapper imapper,
            ICurrentUser user)
         
         {
             catStandardChargeService = service;
             mapper = imapper;
             currentUser = user;
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
        /// get standCharge by type and transactionType
        /// </summary>
        /// <param name="type">type=BUY/SELL/OBH</param>
        /// <param name="transactionType">transactionType=AI/AE/SFE/SFI/SLE/SLI/SCE/SCI</param>
        /// <returns></returns>
        [HttpGet("GetBy")]
        public IActionResult GetBy(string type, string transactionType)
        {
            var data = catStandardChargeService.GetBy(type, transactionType);
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
                        Code = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        Quantity = Convert.ToDecimal(worksheet.Cells[row, 2].Value?.ToString().Trim()),
                        UnitPrice = Convert.ToDecimal(worksheet.Cells[row, 3].Value?.ToString().Trim()),
                        CurrencyId = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                        Vatrate = Convert.ToDecimal(worksheet.Cells[row, 5].Value?.ToString().Trim()),
                        Type = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                        TransactionType = worksheet.Cells[row, 7].Value?.ToString().Trim(),
                        Service = worksheet.Cells[row, 8].Value?.ToString().Trim(),
                        ServiceType = worksheet.Cells[row, 9].Value?.ToString().Trim(),
                        Office = worksheet.Cells[row, 10].Value?.ToString().Trim(),
                        Notes = worksheet.Cells[row, 11].Value?.ToString().Trim()

                    };
                    var data = catStandardChargeService.CheckValidImport(standardCharge);
                    if(data.IsValid == true)
                    {
                        list.Add(standardCharge);
                    }    
                }
                var sc = catStandardChargeService.Import(list);
                ResultHandle result = new ResultHandle { Status = sc.Success, Message = "Import successfully !!!" };
                if (!sc.Success)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = sc.Message.ToString() });
                }
                return Ok(result); 
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// </summary>
        /// <param name="chargeId">delete data</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("delete/{chargeId}")]

        public IActionResult Delete(Guid chargeId)
        {

            var hs = catStandardChargeService.DeleteStandard(chargeId);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }


    }

}

