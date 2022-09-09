﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.ForPartner.DL.Models.Receivable;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
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
        private readonly IAccAccountReceivableService AccAccountReceivableService;
        private readonly IOptions<ApiServiceUrl> apiServiceUrl;
        private readonly ICheckPointService checkPointService;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject IStringLocalizer</param>
        /// <param name="service">inject ICsTransactionService</param>
        /// <param name="user">inject ICurrentUser</param>
        /// <param name="serviceSurcharge">inject ICsShipmentSurchargeService</param>
        /// <param name="AccAccountReceivaService"></param>
        /// <param name="imageService"></param>
        public CsTransactionController(IStringLocalizer<DocumentationLanguageSub> localizer,
            ICsTransactionService service,
            ICurrentUser user,
            ICsShipmentSurchargeService serviceSurcharge,
            IAccAccountReceivableService AccAccountReceivaService,
            IOptions<ApiServiceUrl> serviceUrl,
            ICheckPointService checkPoint,
            ISysImageService imageService)
        {
            stringLocalizer = localizer;
            csTransactionService = service;
            currentUser = user;
            surchargeService = serviceSurcharge;
            sysImageService = imageService;
            AccAccountReceivableService = AccAccountReceivaService;
            apiServiceUrl = serviceUrl;
            checkPointService = checkPoint;
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
            var shiments = csTransactionService.Query(criteria);
            var data = csTransactionService.TakeShipments(shiments);
            return Ok(data);
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
            if (result == 403 || result == 0) return Ok(false);
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
            if(statusCode == 0)
            {
                return Ok();
            }
            var data = csTransactionService.GetDetails(id);//csTransactionService.GetById(id);
            return Ok(data);
        }

        /// <summary>
        /// Get Air/Sea job no and hblId
        /// </summary>
        /// <param name="mblNo">mbl no's ops</param>
        /// <param name="hblNo">hbl no's ops</param>
        /// <param name="serviceName">product service</param>
        /// <param name="serviceMode">service mode</param>
        /// <returns></returns>
        [HttpGet("GetLinkASInfomation")]
        [Authorize]
        public IActionResult GetLinkASInfomation(string jobNo, string mblNo, string hblNo, string serviceName, string serviceMode)
        {
            var data = csTransactionService.GetLinkASInfomation(jobNo, mblNo, hblNo, serviceName, serviceMode);
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
            currentUser.Action = "AddCsTransaction";

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
            currentUser.Action = "UpdateCsTransaction";
            var currentJob = csTransactionService.Get(x => x.Id == model.Id).FirstOrDefault();

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

            if (currentJob.ColoaderId != model.ColoaderId)
            {
                bool checkExistRefundFee = surchargeService.CheckExistRefundFee(model.Id, TermData.CsTransition);
                if (checkExistRefundFee == true)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[DocumentationLanguageSub.MSG_REFUND_FEE_EXISTED] });
                }
            }

            // Remove check etd, eta #15850
            //string msgCheckUpdateEtdEta = CheckUpdateEtdEta(model, out string  type);
            //if(msgCheckUpdateEtdEta.Length > 0)
            //{
            //    return BadRequest(new ResultHandle { Status = false, Message = msgCheckUpdateEtdEta, Data = new { errorCode = type } });
            //}

            // Is Service Date change month
            var msgCheckUpdateServiceDate = CheckUpdateServiceDate(model);
            if (!string.IsNullOrEmpty(msgCheckUpdateServiceDate))
            {
                return BadRequest(new ResultHandle { Status = false, Message = msgCheckUpdateServiceDate, Data = new { errorCode = "ServiceDate" } });
            }

            string msgCheckUpdateMawb = CheckHasMBLUpdatePermitted(model);
            if (msgCheckUpdateMawb.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = msgCheckUpdateMawb });
            }

            if (model.NoProfit == true)
            {
                var allowCheckNoProfit = checkPointService.AllowCheckNoProfitShipment(model.JobNo, model.NoProfit);
                if (!allowCheckNoProfit)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Shipment " + model.JobNo + " have profit, you can not check No Profit." });
                }
            }
            else
            {
                var allowUnCheckNoProfit = checkPointService.AllowUnCheckNoProfitShipment(model.JobNo, model.NoProfit);
                if (!allowUnCheckNoProfit)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Can not remove No Profit. " + model.JobNo + " already has Advance/Settlement." });
                }
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
        public async Task<IActionResult> UploadMultiFiles(List<IFormFile> files, [Required]Guid jobId, bool? isTemp)
        {
            DocumentFileUploadModel model = new DocumentFileUploadModel
            {
                Files = files,
                FolderName = "Shipment",
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
        public async Task<IActionResult> DeleteAttachedFile([Required]Guid id)
        {
            HandleState hs = await sysImageService.DeleteFile(id);
            if (hs.Success)
            {
                return Ok(new ResultHandle { Message = "Delete file Successfully", Status = true });
            }
            return BadRequest(hs);
        }

        [Authorize]
        [HttpDelete("DeleteFileTempPreAlert/{jobId}")]
        public async Task<IActionResult> DeleteFileTempPreAlert([Required]Guid jobId)
        {
            HandleState hs = await sysImageService.DeleteFileTempPreAlert(jobId);
            if (hs.Success)
            {
                return Ok(new ResultHandle { Message = "Delete file Successfully" });
            }
            return BadRequest(hs);
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
            if (!csTransactionService.CheckAllowDelete(id))
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[DocumentationLanguageSub.MSG_NOT_ALLOW_DELETED].Value });
            }
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

            var hs = csTransactionService.SoftDeleteJob(id, out List<ObjectReceivableModel> modelReceivableList);
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
            else
            {
                Response.OnCompleted(async () =>
                {
                    if (modelReceivableList.Count > 0)
                    {
                        await CalculatorReceivable(modelReceivableList);
                    }
                });
            }
            return Ok(result);
        }
        #endregion -- DELETE --

        /// <summary>
        /// import transaction - save duplicate job
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
            if (model.NoProfit == true)
            {
                var jobNo = csTransactionService.First(x => x.Id == model.Id).JobNo;
                string jobOrgNo = string.Empty;
                var allowCheckNoProfit = checkPointService.AllowCheckNoProfitShipmentDuplicate(jobNo, model.NoProfit, false, out jobOrgNo);
                if (!allowCheckNoProfit)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Shipment " + jobNo + " have profit, check No Profit with this Duplicate job is invalid." });
                }
            }

            model.UserCreated = currentUser.UserID;
            var result = csTransactionService.ImportCSTransaction(model, out List<Guid> surchargeIds);

            if (!result.Status)
            {
                return BadRequest(new ResultHandle { Status = false, Message = result.Message });
            }
            else if (surchargeIds.Count > 0)
            {

                Response.OnCompleted(async () =>
                {
                    List<ObjectReceivableModel> modelReceivableList = AccAccountReceivableService.GetListObjectReceivableBySurchargeIds(surchargeIds);
                    if(modelReceivableList.Count > 0)
                    {
                        await CalculatorReceivable(modelReceivableList);
                    }
                });
            }
            return Ok(result);
        }

        private async Task<HandleState> CalculatorReceivable(List<ObjectReceivableModel> model)
        {
            Uri urlAccounting = new Uri(apiServiceUrl.Value.ApiUrlAccounting);
            string accessToken = Request.Headers["Authorization"].ToString();

            HttpResponseMessage resquest = await HttpClientService.PutAPI(urlAccounting + "/api/v1/e/AccountReceivable/CalculateDebitAmount", model, accessToken);
            var response = await resquest.Content.ReadAsAsync<HandleState>();
            return response;
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
        [Authorize]
        public IActionResult PreviewSIFPLsheet(Guid jobId, Guid hblId, string currency)
        {
            var result = csTransactionService.PreviewSIFFormPLsheet(jobId, hblId, currency);
            return Ok(result);
        }

        /// <summary>
        /// Sync HBL with id
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("SyncHBLByShipment/{id}")]
        public async Task<IActionResult> SyncHBL(Guid id, CsTransactionSyncHBLCriteria model)
        {
            currentUser.Action = "SyncHBLByShipment";

            if (!ModelState.IsValid) return BadRequest();
            var result = await csTransactionService.SyncHouseBills(id, model);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        [Route("SyncShipmentByAirWayBill/{id}")]
        public IActionResult SyncShipmentByAirWayBill(Guid id, csTransactionSyncAirWayBill model)
        {
            currentUser.Action = "SyncShipmentByAirWayBill";

            if (!ModelState.IsValid) return BadRequest();
            HandleState hs = csTransactionService.SyncShipmentByAirWayBill(id, model);

            string message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = null };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("LockCsTransaction/{id}")]
        [Authorize]
        public IActionResult LockCsTransaction(Guid id)
        {
            currentUser.Action = "LockCsTransaction";

            HandleState hs = csTransactionService.LockCsTransaction(id);

            string message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = null };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// preview air document release
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("PreviewShipmentCoverPage")]
        public IActionResult PreviewShipmentCoverPage(Guid id)
        {
            var result = csTransactionService.PreviewShipmentCoverPage(id);
            return Ok(result);
        }


        [HttpPost("DowloadAllFileAttached")]
        //[Authorize]
        public async Task<ActionResult> DowloadAllFileAttached(FileDowloadZipModel m)
        {
            //Return memoryStream res.message
            var res = await csTransactionService.CreateFileZip(m);
            if (res.Success)
                return File((MemoryStream)res.Message, "application/zip", m.FileName);
            return BadRequest(res);
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
                    && x.TransactionType.Contains(model.TransactionType.Substring(0, 1))
                    && x.OfficeId == currentUser.OfficeID
                    && x.CurrentStatus != TermData.Canceled))
                    {
                        message = stringLocalizer[DocumentationLanguageSub.MSG_MAWB_EXISTED, model.Mawb].Value;
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
                        && x.OfficeId == currentUser.OfficeID
                        && x.Id != id
                        && x.CurrentStatus != TermData.Canceled))
                    {
                        message = stringLocalizer[DocumentationLanguageSub.MSG_MAWB_EXISTED, model.Mawb].Value;
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

        private string CheckHasMBLUpdatePermitted(CsTransactionEditModel model)
        {
            string errorMsg = string.Empty;
            string mblNo = string.Empty;
            List<string> advs = new List<string>();

            int statusCode = csTransactionService.CheckUpdateMBL(model, out mblNo, out advs);
            if (statusCode == 1)
            {
                errorMsg = String.Format("MBL {0} has Charges List that Synced to accounting system, Please you check Again!", mblNo);
            }

            if (statusCode == 2)
            {
                errorMsg = String.Format("MBL {0} has  Advances {1} that Synced to accounting system, Please you check Again!", mblNo, string.Join(", ", advs.ToArray()));
            }

            return errorMsg;
        }

        private string CheckUpdateEtdEta(CsTransactionEditModel model, out string type)
        {
            string errorMsg = string.Empty;
            string _typeError = string.Empty;
            var currentJob = csTransactionService.Get(x => x.Id == model.Id).FirstOrDefault();

            switch (model.TransactionTypeEnum)
            {
                case TransactionTypeEnum.SeaFCLImport:
                case TransactionTypeEnum.SeaLCLImport:
                case TransactionTypeEnum.SeaConsolImport:
                case TransactionTypeEnum.AirImport:
                    if (model.Eta.HasValue)
                    {
                        _typeError = "eta";
                        errorMsg = model.Eta.Value.Month != currentJob.Eta.Value.Month ? stringLocalizer[DocumentationLanguageSub.MSG_ETA_CANNOT_CHANGE_MONTH].Value : errorMsg;
                    }
                    break;
                case TransactionTypeEnum.SeaFCLExport:
                case TransactionTypeEnum.SeaLCLExport:
                case TransactionTypeEnum.SeaConsolExport:
                case TransactionTypeEnum.AirExport:
                    if (model.Etd.HasValue)
                    {
                        _typeError = "etd";
                        errorMsg = model.Etd.Value.Month != currentJob.Etd.Value.Month ? stringLocalizer[DocumentationLanguageSub.MSG_ETD_CANNOT_CHANGE_MONTH].Value : errorMsg;
                    }
                    break;
            }

            type = _typeError;
            return errorMsg;
        }

        /// <summary>
        /// Check if Service date update with another month
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string CheckUpdateServiceDate(CsTransactionEditModel model)
        {
            string errorMsg = string.Empty;
            var currentJob = csTransactionService.Get(x => x.Id == model.Id).FirstOrDefault();
            if (model.ServiceDate.HasValue)
            {
                errorMsg = model.ServiceDate.Value.Month != currentJob.ServiceDate.Value.Month ? stringLocalizer[DocumentationLanguageSub.MSG_SERVICE_DATE_CANNOT_CHANGE_MONTH].Value : errorMsg;
            }
            return errorMsg;
        }
        #endregion -- METHOD PRIVATE --
    }
}
