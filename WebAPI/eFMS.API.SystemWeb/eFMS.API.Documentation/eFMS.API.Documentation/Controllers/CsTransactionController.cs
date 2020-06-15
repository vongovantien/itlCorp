using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using SystemManagementAPI.Infrastructure.Middlewares;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsTransactionController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsTransactionService csTransactionService;
        private readonly ICurrentUser currentUser;
        private readonly ICsShipmentSurchargeService surchargeService;
        private readonly ISysImageService sysImageService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject IStringLocalizer</param>
        /// <param name="service">inject ICsTransactionService</param>
        /// <param name="user">inject ICurrentUser</param>
        /// <param name="serviceSurcharge">inject ICsShipmentSurchargeService</param>
        /// <param name="imageService"></param>
        public CsTransactionController(IStringLocalizer<DocumentationLanguageSub> localizer,
            ICsTransactionService service,
            ICurrentUser user,
            ICsShipmentSurchargeService serviceSurcharge,
            ISysImageService imageService)
        {
            stringLocalizer = localizer;
            csTransactionService = service;
            currentUser = user;
            surchargeService = serviceSurcharge;
            sysImageService = imageService;
        }

        /// <summary>
        /// count job by date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet("CountJobByDate/{{date}}")]
        public IActionResult CountJob(DateTime date)
        {
            var result = csTransactionService.Count(x => x.DatetimeCreated == date);
            return Ok(result);
        }

        /// <summary>
        /// get total profit by job
        /// </summary>
        /// <param name="JobId">job id that want to get total profit</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTotalProfit")]
        public List<object> GetTotalProfit(Guid JobId)
        {
            return csTransactionService.GetListTotalHB(JobId);
        }

        #region -- LIST & PAGING --

        /// <summary>
        /// get list transactions by search condition
        /// </summary>
        /// <param name="criteria">search condition</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Query")]
        [Authorize]
        public IActionResult Query(CsTransactionCriteria criteria)
        {
            return Ok(csTransactionService.Query(criteria));
        }

        /// <summary>
        /// get and paging list transaction by search condition
        /// </summary>
        /// <param name="criteria">search condition</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(CsTransactionCriteria criteria, int page, int size)
        {
            var data = csTransactionService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }
        #endregion -- LIST & PAGING --

        /// <summary>
        /// check permission of user to view a shipment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("CheckPermission/{id}")]
        [Authorize]
        public IActionResult CheckDetailPermission(Guid id)
        {
            var result = csTransactionService.CheckDetailPermission(id);
            if (result == 403) return Ok(false);
            return Ok(true);
        }

        /// <summary>
        /// get transaction by id
        /// </summary>
        /// <param name="id">id that want to retrieve transaction</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get(Guid id)
        {
            var statusCode = csTransactionService.CheckDetailPermission(id);
            if (statusCode == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var data = csTransactionService.GetDetails(id);//csTransactionService.GetById(id);
            return Ok(data);
        }

        #region -- INSERT & UPDATE
        /// <summary>
        /// add new transaction
        /// </summary>
        /// <param name="model">model to add</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public IActionResult Post(CsTransactionEditModel model)
        {            
            if (!ModelState.IsValid) return BadRequest();
            string checkExistMessage = CheckExist(model.Id, model);

            ICurrentUser _currentUser = PermissionEx.GetUserMenuPermissionTransaction(model.TransactionType, currentUser);
            var permissionRange = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            model.UserCreated = currentUser.UserID;
            var result = csTransactionService.AddCSTransaction(model);
            return Ok(result);
        }

        /// <summary>
        /// update an existed item
        /// </summary>
        /// <param name="model">model to update</param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        public IActionResult Put(CsTransactionEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (!csTransactionService.Any(x => x.Id == model.Id))
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Not found transaction" });
            }

            string checkExistMessage = CheckExist(model.Id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            model.UserModified = currentUser.UserID;
            var hs = csTransactionService.UpdateCSTransaction(model);
            if (hs.Code == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("UploadFile")]
        public IActionResult UploadFile([FromForm]IFormFile file)
        {
            var s = JsonConvert.SerializeObject(file);
            return Ok(s);
        }

        /// <summary>
        /// attach multi files to shipment
        /// </summary>
        /// <param name="files"></param>
        /// <param name="jobId"></param>
        /// <param name="isTemp"></param>
        /// <returns></returns>
        [HttpPut("UploadMultiFiles/{jobId}/{isTemp}")]
        [Authorize]
        public async Task<IActionResult> UploadMultiFiles(List<IFormFile> files, [Required]Guid jobId,bool? isTemp)
        {
            string folderName = Request.Headers["Module"];
            DocumentFileUploadModel model = new DocumentFileUploadModel
            {
                Files = files,
                FolderName = folderName,
                JobId = jobId,
                IsTemp = isTemp
            };
            var result = await sysImageService.UploadDocumentationFiles(model);
            return Ok(result);
        }

        /// <summary>
        /// get all attached files in a shipment
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet("GetFileAttachs")]
        public IActionResult GetAttachedFiles([Required]Guid jobId)
        {
            string id = jobId.ToString();
            var results = sysImageService.Get(x => x.ObjectId == id && x.IsTemp != true);
            return Ok(results);
        }

        /// <summary>
        /// get all attached files in a shipment for pre alert
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet("GetFileAttachsPreAlert")]
        public IActionResult GetAttachedFilesPreAlert([Required]Guid jobId)
        {
            string id = jobId.ToString();
            var results = sysImageService.Get(x => x.ObjectId == id);
            return Ok(results);
        }

        [Authorize]
        [HttpPut("UpdateFilesToShipment")]
        public IActionResult UpdateFilesToShipment([FromBody]List<SysImageModel> files)
        {
            var result = sysImageService.UpdateFilesToShipment(files);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("DeleteAttachedFile/{id}")]
        public IActionResult DeleteAttachedFile([Required]Guid id)
        {
            var result = sysImageService.DeleteFile(id);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("DeleteFileTempPreAlert/{jobId}")]
        public IActionResult DeleteFileTempPreAlert([Required]Guid jobId)
        {
            var result = sysImageService.DeleteFileTempPreAlert(jobId);
            return Ok(result);
        }
        #endregion -- INSERT & UPDATE

        #region -- DELETE --
        /// <summary>
        /// check allow delete an existed item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("CheckAllowDelete/{id}")]
        public IActionResult CheckAllowDelete(Guid id)
        {
            return Ok(csTransactionService.CheckAllowDelete(id));
        }

        /// <summary>
        /// Check delete permission
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("CheckDeletePermission/{id}")]
        [Authorize]
        public IActionResult CheckDeletePermission(Guid id)
        {
            var result = csTransactionService.CheckDeletePermission(id);
            if (result == 403)
            {
                return Ok(false);
            }
            return Ok(true);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of existed data that want to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (!csTransactionService.Any(x => x.Id == id))
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Not found transaction" });
            }

            if (csTransactionService.CheckAllowDelete(id) == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[DocumentationLanguageSub.MSG_NOT_ALLOW_DELETED].Value });
            }

            var hs = csTransactionService.SoftDeleteJob(id);
            if (hs.Code == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        #endregion -- DELETE --

        /// <summary>
        /// import transaction
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("Import")]
        public IActionResult Import(CsTransactionEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            string checkExistMessage = CheckExist(Guid.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return Ok(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            model.UserCreated = currentUser.UserID;
            var result = csTransactionService.ImportCSTransaction(model);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("ImportMulti")]
        public IActionResult ImportMulti()
        {
            var s = csTransactionService.ImportMulti();
            return Ok();
        }

        /// <summary>
        /// Preview PLsheet of Sea FCL Import
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="hblId"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("PreviewSIFPLsheet")]
        public IActionResult PreviewSIFPLsheet(Guid jobId, Guid hblId, string currency)
        {
            var result = csTransactionService.PreviewSIFFormPLsheet(jobId, hblId, currency);
            return Ok(result);
        }

        /// <summary>
        /// Sync HBL with id
        /// </summary>
        [HttpPost]
        //[Authorize]
        [Route("SyncHBLByShipment/{id}")]
        public IActionResult SyncHBL(Guid id, CsTransactionSyncHBLCriteria model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var result = csTransactionService.SyncHouseBills(id, model);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        [Route("SyncShipmentByAirWayBill/{id}")]
        public IActionResult SyncShipmentByAirWayBill(Guid id, csTransactionSyncAirWayBill model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var result = csTransactionService.SyncShipmentByAirWayBill(id, model);
            return Ok(result);
        }

        [HttpPut("LockShipment/{id}")]
        [Authorize]
        public IActionResult LockShipment(Guid id)
        {
            HandleState hs = csTransactionService.LockCsTransaction(id);

            string message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = null };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        #region -- METHOD PRIVATE --
        private string CheckExist(Guid id, CsTransactionEditModel model)
        {
            model.TransactionType = DataTypeEx.GetType(model.TransactionTypeEnum);
            if (model.TransactionType == string.Empty)
                return stringLocalizer[DocumentationLanguageSub.MSG_NOT_FOUND_TRANSACTION_TYPE].Value;
            string message = string.Empty;

            //model.TransactionType = DataTypeEx.GetType(model.TransactionTypeEnum);
            //if (model.TransactionType == string.Empty)
            //    message = "Not found type transaction";
            if (id == Guid.Empty)
            {
                //Check trùng theo từng service --> change to check theo nhóm service
                if (!string.IsNullOrEmpty(model.Mawb?.Trim()))
                {
                    if (csTransactionService.Any(x => (x.Mawb ?? "").ToLower() == (model.Mawb ?? "").ToLower()
                    && x.TransactionType.Contains(model.TransactionType.Substring(0,1))
                    && x.CurrentStatus != TermData.Canceled))
                    {
                        message = stringLocalizer[DocumentationLanguageSub.MSG_MAWB_EXISTED].Value;
                    }
                }
            }
            else
            {
                //Check trùng theo từng service --> change to check theo nhóm service
                if (!string.IsNullOrEmpty(model.Mawb?.Trim()))
                {
                    if (csTransactionService.Any(x => (x.Mawb ?? "").ToLower() == (model.Mawb ?? "").ToLower()
                        && x.TransactionType.Contains(model.TransactionType.Substring(0, 1))
                        && x.Id != id
                        && x.CurrentStatus != TermData.Canceled))
                    {
                        message = stringLocalizer[DocumentationLanguageSub.MSG_MAWB_EXISTED].Value;
                    }
                }
            }

            //if (model.CsMawbcontainers == null || model.CsMawbcontainers.Count == 0)
            //{
            //    message = "Shipment container list must have at least 1 row of data!";
            //}
            if (message.Length > 0) return message;
            var resultMessage = string.Empty;

            switch (model.TransactionTypeEnum)
            {
                case TransactionTypeEnum.SeaFCLImport:
                    resultMessage = CheckExistsSIF(id, model);
                    break;
                case TransactionTypeEnum.SeaFCLExport:
                    resultMessage = CheckExistSFE(model);
                    break;
                case TransactionTypeEnum.SeaLCLImport:
                    resultMessage = CheckExistsSIF(id, model);//Sử dụng lại
                    break;
                case TransactionTypeEnum.SeaLCLExport:
                    resultMessage = CheckExistSFE(model);//Sử dụng lại
                    break;
            }

            return resultMessage;
        }

        private string CheckExistSFE(CsTransactionEditModel model)
        {
            string message = string.Empty;
            if (model.Pol == model.Pod && model.Pol != null && model.Pod != null)
            {
                message = model.Pod == model.Pol ? stringLocalizer[DocumentationLanguageSub.MSG_POD_DIFFERENT_POL].Value : message;
            }
            return message;
        }

        private string CheckExistsSIF(Guid id, CsTransactionEditModel model)
        {
            string message = string.Empty;
            if (model.Etd.HasValue)
            {
                message = model.Etd.Value.Date >= model.Eta.Value.Date ? stringLocalizer[DocumentationLanguageSub.MSG_ETD_BEFORE_ETA].Value : message;
            }

            if (model.Eta.HasValue)
            {
                if (model.Etd.HasValue)
                {
                    message = model.Eta.Value.Date <= model.Etd.Value.Date ? stringLocalizer[DocumentationLanguageSub.MSG_ETA_AFTER_ETD].Value : message;
                }
            }
            else
            {
                message = stringLocalizer[DocumentationLanguageSub.MSG_ETA_REQUIRED].Value;
            }

            // change request 14/2/19.
            // message = string.IsNullOrEmpty(model.Mbltype) ? stringLocalizer[DocumentationLanguageSub.MSG_MBL_TYPE_REQUIRED].Value : message;

            message = string.IsNullOrEmpty(model.ShipmentType) ? stringLocalizer[DocumentationLanguageSub.MSG_SHIPMENT_TYPE_REQUIRED].Value : message;

            if (model.Pol != null && model.Pol != Guid.Empty)
            {
                message = model.Pol == model.Pod ? stringLocalizer[DocumentationLanguageSub.MSG_POL_DIFFERENT_POD].Value : message;
            }

            if (model.Pod == null || model.Pod == Guid.Empty)
            {
                message = stringLocalizer[DocumentationLanguageSub.MSG_POD_REQUIRED].Value;
            }
            else
            {
                message = model.Pod == model.Pol ? stringLocalizer[DocumentationLanguageSub.MSG_POD_DIFFERENT_POL].Value : message;
            }

            if (model.DeliveryPlace != null && model.DeliveryPlace != Guid.Empty)
            {
                message = model.DeliveryPlace == model.Pol ? stringLocalizer[DocumentationLanguageSub.MSG_PODELI_DIFFERENT_POL].Value : message;
            }

            message = string.IsNullOrEmpty(model.TypeOfService) ? stringLocalizer[DocumentationLanguageSub.MSG_SERVICE_TYPE_REQUIRED].Value : message;

            message = string.IsNullOrEmpty(model.PersonIncharge) ? stringLocalizer[DocumentationLanguageSub.MSG_PERSON_IN_CHARGE_REQUIRED].Value : message;

            return message;
        }
        #endregion -- METHOD PRIVATE --
    }
}
