﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;

namespace eFMS.API.Catalogue.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatChargeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatChargeService catChargeService;
        private readonly ICatChargeDefaultAccountService catChargeDefaultAccountService;
        private readonly IMapper mapper;

        public CatChargeController(IStringLocalizer<LanguageSub> localizer, ICatChargeService service, ICatChargeDefaultAccountService catChargeDefaultAccount, IMapper imapper)
        {
            stringLocalizer = localizer;
            catChargeService = service;
            catChargeDefaultAccountService = catChargeDefaultAccount;
            mapper = imapper;
        }

        /// <summary>
        /// get and paging the list of charges by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="pageNumber">page to retrieve data</param>
        /// <param name="pageSize">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        public IActionResult Get(CatChargeCriteria criteria,int pageNumber,int pageSize)
        {
          var data = catChargeService.GetCharges(criteria, pageNumber, pageSize, out int rowCount);
            var result = new { data, totalItems = rowCount, pageNumber, pageSize };
            return Ok(result);
        }

        /// <summary>
        /// get the list of charges by criteria search
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Query")]
        public IActionResult Get(CatChargeCriteria criteria)
        {
            var data = catChargeService.Query(criteria);        
            return Ok(data);
        }

        /// <summary>
        /// get charge by id
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet]
        [Route("getById/{id}")]
        public IActionResult Get(Guid id)
        {
            var result = catChargeService.GetChargeById(id);
            return Ok(result);
        }

        /// <summary>
        /// get all charges
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAll")]
        public IActionResult All()
        {
            var data = catChargeService.Get();
            return Ok(data);
        }

        /// <summary>
        /// add a new charge
        /// </summary>
        /// <param name="model">object data to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("addNew")]
        [Authorize]
        public IActionResult Add(CatChargeAddOrUpdateModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(Guid.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var hs = catChargeService.AddCharge(model);
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
        /// <param name="model">object to update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("update")]
        [Authorize]
        public IActionResult Update(CatChargeAddOrUpdateModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model.Charge.Id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var hs = catChargeService.UpdateCharge(model);
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
        public IActionResult Delete(Guid id)
        {
            var hs = catChargeService.DeleteCharge(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }


        private string CheckExist(Guid id, CatChargeAddOrUpdateModel model)
        {
            string message = string.Empty;
            if (id == Guid.Empty)
            {
                if (catChargeService.Any(x => (x.Code.ToLower() == model.Charge.Code.ToLower())))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            else
            {
                if (catChargeService.Any(x => ((x.Code.ToLower() == model.Charge.Code.ToLower())) && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
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
                if (worksheet.Cells[1, 1].Value?.ToString() != "Code")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Code' " });
                }
                if (worksheet.Cells[1, 2].Value?.ToString() != "Name En")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 2 must have header is 'Name En' " });
                }
                if (worksheet.Cells[1, 3].Value?.ToString() != "Name Local")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 3 must have header is 'Name Local' " });
                }
                if (worksheet.Cells[1, 4].Value?.ToString() != "Unit")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 4 must have header is 'Unit'" });
                }
                if (worksheet.Cells[1, 5].Value?.ToString() != "Unit Price")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 5 must have header is 'Unit Price' " });
                }
                if (worksheet.Cells[1, 6].Value?.ToString() != "Currency")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 6 must have header is 'Currency' " });
                }
                if (worksheet.Cells[1, 7].Value?.ToString() != "VAT")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 7 must have header is 'VAT' " });
                }
                if (worksheet.Cells[1, 8].Value?.ToString() != "Type")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 8 must have header is 'Type' " });
                }
                if (worksheet.Cells[1, 9].Value?.ToString() != "Service")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 9 must have header is 'Service' " });
                }
                if (worksheet.Cells[1, 10].Value?.ToString() != "Status")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 10 must have header is 'Status' " });
                }
                List<CatChargeImportModel> list = new List<CatChargeImportModel>();
                for(int row = 2; row <= rowCount; row++)
                {
                    var charge = new CatChargeImportModel
                    {
                        IsValid = true,
                        Code = worksheet.Cells[row, 1].Value?.ToString(),
                        ChargeNameEn = worksheet.Cells[row, 2].Value?.ToString(),
                        ChargeNameVn = worksheet.Cells[row, 3].Value?.ToString(),
                        UnitId = worksheet.Cells[row, 4].Value == null ? (short)(-1) : Convert.ToInt16(worksheet.Cells[row, 4].Value),
                        UnitPrice = worksheet.Cells[row,5].Value == null? -1: Convert.ToDecimal(worksheet.Cells[row, 5].Value),
                        CurrencyId = worksheet.Cells[row,6].Value?.ToString(),
                        Vatrate = worksheet.Cells[row,7].Value == null ? -1 : Convert.ToDecimal(worksheet.Cells[row, 7].Value),
                        Type = worksheet.Cells[row, 8].Value?.ToString(),
                        ServiceTypeId = worksheet.Cells[row, 9].Value?.ToString(),
                        Status = worksheet.Cells[row, 10].Value?.ToString(),
                    };
                    list.Add(charge);
                }
                var data = catChargeService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        [HttpPost]
        [Route("import")]
        [Authorize]
        public IActionResult Import([FromBody] List<CatChargeImportModel> data)
        {
            var result = catChargeService.Import(data);
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
        public async Task<ActionResult> DownloadExcel()
        {

            try
            {
                string templateName = "Charge" + Templates.ExelImportEx;
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