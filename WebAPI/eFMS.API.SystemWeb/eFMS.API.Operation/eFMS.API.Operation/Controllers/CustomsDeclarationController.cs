using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.Operation.DL.Common;
using eFMS.API.Operation.DL.IService;
using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.DL.Models.Criteria;
using eFMS.API.Operation.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;

namespace eFMS.API.Operation.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CustomsDeclarationController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICustomsDeclarationService customsDeclarationService;
        private readonly ICurrentUser currentUser;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface ICustomsDeclarationService</param>
        public CustomsDeclarationController(IStringLocalizer<LanguageSub> localizer, ICustomsDeclarationService service, IHostingEnvironment hostingEnvironment, ICurrentUser user)
        {
            stringLocalizer = localizer;
            customsDeclarationService = service;
            currentUser = user;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// get the list of custom declarations
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var results = customsDeclarationService.GetAll();
            return Ok(results);
        }

        /// <summary>
        /// get the list of custom declarations by job no
        /// </summary>
        /// <param name="jobNo">jobNo that want to retrieve custom declarations</param>
        /// <returns></returns>
        [HttpGet("GetBy")]
        public IActionResult GetBy(string jobNo)
        {
            var results = customsDeclarationService.GetBy(jobNo);
            return Ok(results);
        }

        /// <summary>
        /// get the list of custom declarations by conditions
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("Query")]
        [AuthorizeEx(Menu.opsCustomClearance, UserPermission.AllowAccess)]
        public IActionResult Query(CustomsDeclarationCriteria criteria)
        {
            var data = customsDeclarationService.Query(criteria);
            return Ok(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keySearch"></param>
        /// <param name="customerNo"></param>
        /// <param name="imporTed"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet("CustomDeclaration")]
        public IActionResult GetCustomDeclaration(string keySearch, string customerNo,bool imporTed, int page, int size)
        {
            var data = customsDeclarationService.GetCustomDeclaration(keySearch , customerNo, imporTed, page, size, out int rowsCount);
            var result = new { data, totalItems = rowsCount, page, size };
            return Ok(result);
        }
        
        /// <summary>
        /// get and paging the list of custom declarations by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="pageNumber">page to retrieve data</param>
        /// <param name="pageSize">number items per page</param>
        /// <returns></returns>
        [HttpPost("Paging")]
        [AuthorizeEx(Menu.opsCustomClearance, UserPermission.AllowAccess)]
        public IActionResult Paging(CustomsDeclarationCriteria criteria, int pageNumber, int pageSize)
        {
            var data = customsDeclarationService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }

        /// <summary>
        /// add new custom clearance
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("Add")]
        [AuthorizeEx(Menu.opsCustomClearance, UserPermission.Add)]
        public IActionResult AddNew(CustomsDeclarationModel model)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.opsCustomClearance);
            var code = CheckForbitUpdate(_user.UserMenuPermission.Write);
            if (code == 403) return Forbid();
            var existedMessage = CheckExist(model, model.Id);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            model = GetModelAdd(model);
            var hs = customsDeclarationService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private CustomsDeclarationModel GetModelAdd(CustomsDeclarationModel model)
        {
            model.DatetimeCreated = DateTime.Now;
            model.DatetimeModified = DateTime.Now;
            model.UserCreated = model.UserModified = currentUser.UserID;
            model.Source = OperationConstants.FromEFMS;
            model.GroupId = currentUser.GroupId;
            model.DepartmentId = currentUser.DepartmentId;
            model.OfficeId = currentUser.OfficeID;
            model.CompanyId = currentUser.CompanyID;
            return model;
        }

        /// <summary>
        /// update an existed item
        /// </summary>
        /// <param name="model">object to update</param>
        /// <returns></returns>
        [Authorize]
        [HttpPut]
        [Route("Update")]
        [AuthorizeEx(Menu.opsCustomClearance, UserPermission.Update)]
        public IActionResult Update(CustomsDeclarationModel model)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.opsCustomClearance);
            var code = CheckForbitUpdate(_user.UserMenuPermission.Write);
            if (code == 403) return Forbid();
            var existedMessage = CheckExist(model, model.Id);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            model.DatetimeModified = DateTime.Now;
            model.UserModified = currentUser.UserID;
            var hs = customsDeclarationService.Update(model, x => x.Id == model.Id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private int CheckForbitUpdate(string action)
        {
            var permissionRange = PermissionExtention.GetPermissionRange(action);
            var modelCheckUpdate = new BaseUpdateModel { UserCreated = currentUser.UserID, GroupId = currentUser.GroupId, DepartmentId = currentUser.DepartmentId, OfficeId = currentUser.OfficeID, CompanyId = currentUser.CompanyID };
            return PermissionExtention.GetPermissionCommonItem(modelCheckUpdate, permissionRange, currentUser);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of existed item that want to delete</param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        [Route("Delete")]
        public IActionResult Delete(int id)
        {
            var hs = customsDeclarationService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// import custom declaration from ecus system
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost("ImportClearancesFromEcus")]
        public IActionResult ImportClearancesFromEcus()
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.opsCustomClearance);
            var code = CheckForbitUpdate(_user.UserMenuPermission.Write);
            if (code == 403) return Forbid();
            var hs = customsDeclarationService.ImportClearancesFromEcus();
            var message = hs.Message.ToString();
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = message };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// get clearance types(types, cargoTypes, routes, serviceTypes)
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetClearanceTypes")]
        public IActionResult GetClearanceTypes()
        {
            var results = customsDeclarationService.GetClearanceTypeData();
            return Ok(results);
        }

        /// <summary>
        /// add( update) job to clearances
        /// </summary>
        /// <param name="clearances">list of clearances</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdateJobToClearances")]
        public IActionResult UpdateJobToClearances(List<CustomsDeclarationModel> clearances)
        {
            if (customsDeclarationService.CheckAllowUpdate(clearances.Select(t=>t.Id).FirstOrDefault()) == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[OperationLanguageSub.MSG_NOT_ALLOW_DELETED].Value });
            }
            var result = customsDeclarationService.UpdateJobToClearances(clearances);
            return Ok(result);
        }

        private string CheckExist(CustomsDeclarationModel model, decimal id)
        {
            string message = null;
            if (id == 0)
            {
                if (customsDeclarationService.Any(x => x.ClearanceNo == model.ClearanceNo && x.ClearanceDate == model.ClearanceDate))
                {
                    message = string.Format(stringLocalizer[OperationLanguageSub.MSG_CLEARANCENO_EXISTED].Value, model.ClearanceNo);
                }
            }
            else
            {
                if (customsDeclarationService.Any(x => (x.ClearanceNo == model.ClearanceNo && x.Id != id && x.ClearanceDate == model.ClearanceDate)))
                {
                    message = string.Format(stringLocalizer[OperationLanguageSub.MSG_CLEARANCENO_EXISTED].Value, model.ClearanceNo);
                }
            }
            return message;
        }

        /// <summary>
        /// get custom declarations by id
        /// </summary>
        /// <param name="id">id that want to retrieve custom declarations</param>
        /// <returns></returns>
        [HttpGet("GetById/{id}")]
        [Authorize]
        [AuthorizeEx(Menu.opsCustomClearance, UserPermission.Detail)]
        public IActionResult GetById(int id)
        {
            var results = customsDeclarationService.GetDetail(id);
            return Ok(results);
        }

        /// <summary>
        /// get custom declarations by id
        /// </summary>
        /// <param name="id">id that want to retrieve custom declarations</param>
        /// <returns></returns>
        [HttpGet("CheckPermission/{id}")]
        [Authorize]
        public IActionResult CheckPermission(int id)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.opsCustomClearance);
            var statusCode = customsDeclarationService.CheckDetailPermission(id);
            if (statusCode == 403) return Ok(false);
            return Ok(true);
        }
        
        ///
        [HttpPost("CheckDeletePermission")]
        [Authorize]
        public IActionResult CheckDeletePermission(List<CustomsDeclarationModel> clearances)
        {
            var hs = customsDeclarationService.CheckAllowDelete(clearances);
            return Ok(hs);
        }

        /// <summary>
        /// delete multiple custom clearance
        /// </summary>
        /// <param name="listCustom"></param>
        /// <param name="customs">list of clearances selected</param>
        /// <returns></returns>
        [Authorize]
        [HttpPut]
        [Route("DeleteMultiple")]
        public IActionResult DeleteMultiple(List<CustomsDeclarationModel> listCustom)
        {
            foreach (var item in listCustom)
            {
                if (item.JobNo != null)
                {
                    return BadRequest();
                }
            }

            var hs = customsDeclarationService.DeleteMultiple(listCustom);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result;
            if (hs.Success)
            {
                result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            }
            result = new ResultHandle { Status = hs.Success, Message = message };
            return Ok(result);

        }

        /// <summary>
        /// download file excel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("DownloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            string fileName = Templates.CustomDeclaration.ExelImportFileName + Templates.ExelImportEx;
            string templateName = _hostingEnvironment.ContentRootPath;
            var result = await new FileHelper().ExportExcel(templateName, fileName);
            if (result != null)
            {
                return result;
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// read data from file excel
        /// </summary>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("uploadFile")]
        [Authorize]
        public IActionResult UploadFile(IFormFile uploadedFile)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.opsCustomClearance);
            if (_user.UserMenuPermission.Import == false) return Forbid();
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int? rowCount = worksheet.Dimension?.Rows;
                int? colCount = worksheet.Dimension?.Columns;
                if (rowCount < 2 || rowCount == null) return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.NOT_FOUND_DATA_EXCEL].Value });
                List<CustomClearanceImportModel> list = new List<CustomClearanceImportModel>();

                for (int row = 2; row < rowCount + 1; row++)
                {
                    var stage = new CustomClearanceImportModel
                    {
                        IsValid = true,
                        ClearanceNo = worksheet.Cells[row, 1].Value?.ToString(),
                        Type = worksheet.Cells[row, 2].Value?.ToString(),
                        FirstClearanceNo = worksheet.Cells[row, 3].Value?.ToString(),

                        ClearanceDateStr = worksheet.Cells[row, 4].Value?.ToString(),

                        AccountNo = worksheet.Cells[row, 5].Value?.ToString(),
                        CustomerName = worksheet.Cells[row, 6].Value?.ToString(),
                        Mblid = worksheet.Cells[row, 7].Value?.ToString(),
                        Hblid = worksheet.Cells[row, 8].Value?.ToString(),
                        Gateway = worksheet.Cells[row, 9].Value?.ToString(),

                        GrossWeightStr = worksheet.Cells[row, 10].Value?.ToString(),
                        NetWeightStr = worksheet.Cells[row, 11].Value?.ToString(),
                        CbmStr = worksheet.Cells[row, 12].Value?.ToString(),
                        QtyContStr = worksheet.Cells[row, 13].Value?.ToString(),
                        PcsStr = worksheet.Cells[row, 14].Value?.ToString(),

                        CommodityCode = worksheet.Cells[row, 15].Value?.ToString(),
                        CargoType = worksheet.Cells[row, 17].Value?.ToString(),
                        ServiceType = worksheet.Cells[row, 18].Value?.ToString(),
                        Route = worksheet.Cells[row, 19].Value?.ToString(),
                        ImportCountryCode = worksheet.Cells[row, 20].Value?.ToString(),
                        ExportCountryCode = worksheet.Cells[row, 21].Value?.ToString(),

                        Source = OperationConstants.FromEFMS,
                        DatetimeModified = DateTime.Now,
                        UserModified = currentUser.UserID,
                        DatetimeCreated = DateTime.Now,
                        UserCreated = currentUser.UserID,
                        Shipper = worksheet.Cells[row, 22].Value?.ToString(),
                        Consignee = worksheet.Cells[row, 23].Value?.ToString()
                    };
                    list.Add(stage);
                }
                var data = customsDeclarationService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// import list custom clearance
        /// </summary>
        /// <param name="data">list custom clearance</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Import")]
        [Authorize]
        public IActionResult Import([FromBody]List<CustomsDeclarationModel> data)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.opsCustomClearance);
            var code = CheckForbitUpdate(_user.UserMenuPermission.Write);
            if (code == 403) return Forbid();
            var result = customsDeclarationService.Import(data);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
            }
        }

        /// <summary>
        /// Get list custom of shipment operation (not locked)
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetCustomsShipmentNotLocked")]
        [Authorize]
        public IActionResult GetCustomsShipmentNotLocked()
        {
            var data = customsDeclarationService.GetCustomsShipmentNotLocked();
            return Ok(data);
        }

        /// <summary>
        /// Get list custom of shipment operation (not locked, shipment asign or PIC is current user) 
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetListCustomNoAsignPIC")]
        [Authorize]
        public IActionResult GetListCustomNoAsignPIC()
        {
            var data = customsDeclarationService.GetListCustomNoAsignPIC();
            return Ok(data);
        }
    }
}