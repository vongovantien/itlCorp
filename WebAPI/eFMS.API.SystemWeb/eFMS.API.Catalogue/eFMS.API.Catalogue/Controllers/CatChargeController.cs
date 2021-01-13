using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Infrastructure.Extensions;
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
    /// 
    /// </summary>
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
        private readonly ICurrentUser currentUser;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="catChargeDefaultAccount"></param>
        /// <param name="imapper"></param>
        public CatChargeController(IStringLocalizer<LanguageSub> localizer, 
            ICatChargeService service, 
            ICatChargeDefaultAccountService catChargeDefaultAccount,
            IMapper imapper,
            ICurrentUser user,
            IHostingEnvironment hostingEnvironment
)
        {
            stringLocalizer = localizer;
            catChargeService = service;
            catChargeDefaultAccountService = catChargeDefaultAccount;
            mapper = imapper;
            currentUser = user;
            _hostingEnvironment = hostingEnvironment;
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
        [AuthorizeEx(Menu.catCharge, UserPermission.AllowAccess)]
        public IActionResult Get(CatChargeCriteria criteria,int pageNumber,int pageSize)
        {
            var data = catChargeService.Paging(criteria, pageNumber, pageSize, out int rowCount);
            var result = new { data, totalItems = rowCount, pageNumber, pageSize };
            return Ok(result);
        }

        /// <summary>
        /// get the list of charges by conditions
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

        [HttpPost]
        [Route("QueryExport")]
        public IActionResult QueryExport(CatChargeCriteria searchObject)
        {
            IQueryable<CatChargeModel> data = catChargeService.QueryExport(searchObject);
            return Ok(data);
        }

        [HttpPost("GetDefaultByChargeCodes")]
        public IActionResult GetDefaultByChargeCodes(List<string> chargeCodes)
        {
            var results = catChargeService.Get(x => chargeCodes.Contains(x.Code));
            return Ok(results);
        }

        /// <summary>
        /// get charge by type
        /// </summary>
        /// <param name="type">type=1: CREDIT, 2: DEBIT, 3: OBH</param>
        /// <returns></returns>
        [HttpGet("GetBy")]
        public IActionResult GetBy(CatChargeType type)
        {
            string chargeType = PlaceTypeEx.GetChargeType(type);
            var data = catChargeService.GetBy(chargeType);
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
            //ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catCharge);
            //PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);

            //if(permissionRange == PermissionRange.None || !catChargeService.CheckAllowPermissionAction(id, permissionRange))
            //{
            //    return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            //}

            var result = catChargeService.GetChargeById(id);
            return Ok(result);
        }

        /// <summary>
        /// get settle payment charges
        /// </summary>
        /// <param name="keySearch"></param>
        /// <param name="active"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet("SettlePaymentCharges")]
        public IActionResult GetSettlePaymentCharges(string keySearch, bool? active, int? size)
        {
            var results = catChargeService.GetSettlePaymentCharges(keySearch, active, size);
            return Ok(results);
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
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catCharge); 
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);

            if (permissionRange == PermissionRange.None)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(Guid.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var hs = catChargeService.AddCharge(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value , Data = model.Charge.Id};
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
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catCharge);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);

            if (permissionRange == PermissionRange.None || !catChargeService.CheckAllowPermissionAction(model.Charge.Id, permissionRange))
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

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

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of data that need to delete</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("delete/{id}")]
        [Authorize]

        public IActionResult Delete(Guid id)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catCharge);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);

            if(!catChargeService.CheckAllowPermissionAction(id, permissionRange))
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

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

        /// <summary>
        /// read charge data from file excel 
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
                // int colCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest();
                //if (worksheet.Cells[1, 1].Value?.ToString() != "Code")
                //{
                //    return BadRequest(new ResultHandle { Status = false, Message = "Column 1 must have header is 'Code' " });
                //}
                //if (worksheet.Cells[1, 2].Value?.ToString() != "Name En")
                //{
                //    return BadRequest(new ResultHandle { Status = false, Message = "Column 2 must have header is 'Name En' " });
                //}
                //if (worksheet.Cells[1, 3].Value?.ToString() != "Name Local")
                //{
                //    return BadRequest(new ResultHandle { Status = false, Message = "Column 3 must have header is 'Name Local' " });
                //}
                //if (worksheet.Cells[1, 4].Value?.ToString() != "Unit")
                //{
                //    return BadRequest(new ResultHandle { Status = false, Message = "Column 4 must have header is 'Unit'" });
                //}
                //if (worksheet.Cells[1, 5].Value?.ToString() != "Unit Price")
                //{
                //    return BadRequest(new ResultHandle { Status = false, Message = "Column 5 must have header is 'Unit Price' " });
                //}
                //if (worksheet.Cells[1, 6].Value?.ToString() != "Currency")
                //{
                //    return BadRequest(new ResultHandle { Status = false, Message = "Column 6 must have header is 'Currency' " });
                //}
                //if (worksheet.Cells[1, 7].Value?.ToString() != "VAT")
                //{
                //    return BadRequest(new ResultHandle { Status = false, Message = "Column 7 must have header is 'VAT' " });
                //}
                //if (worksheet.Cells[1, 8].Value?.ToString() != "Type")
                //{
                //    return BadRequest(new ResultHandle { Status = false, Message = "Column 8 must have header is 'Type' " });
                //}
                //if (worksheet.Cells[1, 9].Value?.ToString() != "Service")
                //{
                //    return BadRequest(new ResultHandle { Status = false, Message = "Column 9 must have header is 'Service' " });
                //}
                //if (worksheet.Cells[1, 10].Value?.ToString() != "Status")
                //{
                //    return BadRequest(new ResultHandle { Status = false, Message = "Column 10 must have header is 'Status' " });
                //}
                
                List<CatChargeImportModel> list = new List<CatChargeImportModel>();
                for(int row = 2; row <= rowCount; row++)
                {
                    bool active = true;
                    string status = worksheet.Cells[row, 10].Value?.ToString().Trim();
                    if(status.ToLower() != "active")
                    {
                        active = false;
                    }
                    decimal unitPrice = -1;
                    var price = worksheet.Cells[row, 5].Value;
                    if(price != null) { unitPrice = Convert.ToDecimal(price); }
                    decimal vatRate = -1;
                    var vat = worksheet.Cells[row, 7].Value;
                    if(vat != null) { vatRate = Convert.ToDecimal(vat); }

                    var charge = new CatChargeImportModel
                    {
                        IsValid = true,
                        Code = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        ChargeNameEn = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        ChargeNameVn = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                        UnitCode = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                        UnitPrice = unitPrice,
                        CurrencyId = worksheet.Cells[row,6].Value?.ToString().Trim(),
                        Vatrate = vatRate,
                        Type = worksheet.Cells[row, 8].Value?.ToString().Trim(),
                        ServiceName = worksheet.Cells[row, 9].Value?.ToString().Trim(),
                        Status = status,
                        Active = active
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

        /// <summary>
        /// import list charge into database
        /// </summary>
        /// <param name="data">list of data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("import")]
        [Authorize]
        public IActionResult Import([FromBody] List<CatChargeImportModel> data)
        {
            var hs = catChargeService.Import(data);
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
        
        /// <summary>
        /// download exel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("downloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            try
            {
                string fileName = Templates.CatCharge.ExcelImportFileName + Templates.ExcelImportEx;
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

        /// <summary>
        /// Get list service
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetListServices")]
        public IActionResult GetListServices()
        {
            var results = catChargeService.GetListService();
            return Ok(results);
        }

        [HttpGet("CheckAllowDetail/{id}")]
        [Authorize]
        public IActionResult CheckAllowDetail(Guid id)
        {
            var charge = catChargeService.First(x => x.Id == id);
            if (charge == null)
            {
                return Ok(false);
            }

            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catCharge);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);

            return Ok(catChargeService.CheckAllowPermissionAction(id, permissionRange));
        }

        [HttpGet("CheckAllowDelete/{id}")]
        [Authorize]

        public IActionResult CheckAllowDelete(Guid id)
        {
            var charge = catChargeService.First(x => x.Id == id);
            if (charge == null)
            {
                return Ok(false);
            }
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catCharge);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);

            return Ok(catChargeService.CheckAllowPermissionAction(id, permissionRange));
        }

    }
}