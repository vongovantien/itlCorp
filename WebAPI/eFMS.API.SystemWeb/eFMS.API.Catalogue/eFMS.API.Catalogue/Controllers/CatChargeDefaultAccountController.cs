using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace eFMS.API.Catalogue.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatChargeDefaultAccountController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatChargeDefaultAccountService catChargeDefaultAccountService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;
        private readonly IHostingEnvironment _hostingEnvironment;
        public CatChargeDefaultAccountController(IStringLocalizer<LanguageSub> localizer, ICatChargeDefaultAccountService service, IMapper imapper, ICurrentUser user,
            IHostingEnvironment hosting)
        {
            stringLocalizer = localizer;
            catChargeDefaultAccountService = service;
            mapper = imapper;
            currentUser = user;
            _hostingEnvironment = hosting;
        }

        /// <summary>
        /// get all charge default
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAll")]
        public IActionResult Get()
        {
            var results = catChargeDefaultAccountService.Get();
            return Ok(results);
        }

        /// <summary>
        /// add new charge default
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("addNew")]
        [Authorize]
        public IActionResult Add(CatChargeDefaultAccountModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(0, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }

            var catChargeDefaultAccount = mapper.Map<CatChargeDefaultAccountModel>(model);
            catChargeDefaultAccount.UserCreated = catChargeDefaultAccount.UserModified = currentUser.UserID;
            catChargeDefaultAccount.DatetimeCreated = DateTime.Now;
            catChargeDefaultAccount.Active = true;
            var hs = catChargeDefaultAccountService.Add(catChargeDefaultAccount);
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
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("update")]
        [Authorize]
        public IActionResult Update(CatChargeDefaultAccountModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model.Id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }

            var catChargeDefaultAccount = mapper.Map<CatChargeDefaultAccountModel>(model);
            catChargeDefaultAccount.UserModified = currentUser.UserID;
            catChargeDefaultAccount.DatetimeModified = DateTime.Now;
            var hs = catChargeDefaultAccountService.Update(catChargeDefaultAccount,x=>x.Id==model.Id);
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
        [HttpDelete]
        [Route("delete/{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            // ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = catChargeDefaultAccountService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private string CheckExist(int id, CatChargeDefaultAccountModel model)
        {
            string message = string.Empty;
            if (id == 0)
            {
                if (catChargeDefaultAccountService.Any(x => (x.Type.ToLower() == model.Type.ToLower())))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            else
            {
                if (catChargeDefaultAccountService.Any(x => ((x.Type.ToLower() == model.Type.ToLower())) && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            return message;
        }

        /// <summary>
        /// read charge default data from file excel 
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
                if(worksheet.Cells[1,1].Value?.ToString().Trim()!= "Charge Code")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Charge Code' " });
                }
                if(worksheet.Cells[1,2].Value?.ToString().Trim()!= "Voucher Type")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 2 must have header is 'Voucher Type'" });
                }
                if (worksheet.Cells[1, 3].Value?.ToString().Trim() != "Account Debit No.")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 3 must have header is 'Account Debit No.'" });
                }
                if (worksheet.Cells[1, 4].Value?.ToString().Trim() != "Account Credit No.")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 4 must have header is 'Account Credit No.'" });
                }
                if (worksheet.Cells[1, 5].Value?.ToString().Trim() != "Account Debit No. (VAT)")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 5 must have header is 'Account Debit No. (VAT)'" });
                }
                if (worksheet.Cells[1, 6].Value?.ToString().Trim() != "Account Credit No. (VAT)")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 6 must have header is 'Account Credit No. (VAT)'" });
                }
                if (worksheet.Cells[1, 7].Value?.ToString().Trim() != "Status")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Column 7 must have header is 'Status'" });
                }
                List<CatChargeDefaultAccountImportModel> list = new List<CatChargeDefaultAccountImportModel>();
                for(int row = 2; row <= rowCount; row++)
                {
                    var status = worksheet.Cells[row, 7].Value;
                    bool active = false;
                    DateTime? inactiveDate = null;
                    if (status != null)
                    {
                        active = status.ToString().ToLower() == "active";
                        inactiveDate = active == false ? (DateTime?)DateTime.Now : null;
                    }
                    var defaultAccount = new CatChargeDefaultAccountImportModel
                    {
                        IsValid = true,
                        ChargeCode = worksheet.Cells[row, 1].Value == null ? string.Empty : worksheet.Cells[row, 1].Value.ToString().Trim(),
                        Type = worksheet.Cells[row, 2].Value == null ? string.Empty : worksheet.Cells[row, 2].Value.ToString().Trim(),
                        DebitAccountNo = worksheet.Cells[row, 3].Value == null ? string.Empty : worksheet.Cells[row, 3].Value.ToString().Trim(),
                        CreditAccountNo = worksheet.Cells[row, 4].Value == null ? string.Empty : worksheet.Cells[row, 4].Value.ToString(),
                        DebitVat = worksheet.Cells[row, 5].Value == null? string.Empty : worksheet.Cells[row, 5].Value.ToString().Trim(),
                        CreditVat = worksheet.Cells[row, 6].Value == null ? string.Empty : worksheet.Cells[row, 6].Value.ToString().Trim(),
                        Status = worksheet.Cells[row, 7].Value == null? string.Empty: worksheet.Cells[row, 7].Value.ToString(),
                        Active = active,
                        InactiveOn = inactiveDate
                    };
                    list.Add(defaultAccount);
                }
                var data = catChargeDefaultAccountService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// import list charge default into database
        /// </summary>
        /// <param name="data">list of data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("import")]
        [Authorize]
        public IActionResult Import([FromBody] List<CatChargeDefaultAccountImportModel> data)
        {
            var hs = catChargeDefaultAccountService.Import(data);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Import successfully!!!" };
            if (hs.Success)
            {
                return Ok(result);
            }
            return BadRequest(new ResultHandle { Status = false, Message = hs.Exception.Message });
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
                string fileName = Templates.CatChargeDefaultAccount.ExcelImportFileName + Templates.ExcelImportEx;
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