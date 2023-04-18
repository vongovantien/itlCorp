﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.Infrastructure.Http;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Accounting;
using eFMS.API.Accounting.DL.Services;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using eFMS.API.Infrastructure.RabbitMQ;
using Microsoft.AspNetCore.Authentication.Twitter;

namespace eFMS.API.Accounting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AccountingController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAccountingService accountingService;
        private readonly IOptions<ESBUrl> webUrl;
        private readonly IActionFuncLogService actionFuncLogService;
        private readonly ICurrentUser currentUser;
        private readonly BravoLoginModel loginInfo;
        private readonly ISysImageService sysFileService;
        public IBackgroundTaskQueue _queue { get; }
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IRabbitBus _busControl;

        public AccountingController(
            IStringLocalizer<LanguageSub> localizer,
            IAccountingService service,
            IOptions<ESBUrl> appSettings,
            IActionFuncLogService actionFuncLog,
            ICurrentUser currUser,
            ISysImageService SysImageService,
            IBackgroundTaskQueue queue, 
            IServiceScopeFactory serviceScopeFactory,
            IRabbitBus busControl
            )
        {
            stringLocalizer = localizer;
            accountingService = service;
            webUrl = appSettings;
            actionFuncLogService = actionFuncLog;
            currentUser = currUser;
            loginInfo = new BravoLoginModel
            {
                UserName = "bravo",
                Password = "br@vopro"
            };

            sysFileService = SysImageService;
            _queue = queue;
            _serviceScopeFactory = serviceScopeFactory;
            _busControl = busControl;
        }
        [HttpPost("PublishRabbitFromAccounting")]
        public async Task<IActionResult> PublishRabbitFromAccounting(string message)
        {
            var models = new List<ObjectReceivableModel> {
                new ObjectReceivableModel
                {
                    Office = Guid.Parse("33facfe3-304e-4460-8705-a25f6292b11a"),
                    PartnerId = "15f38925-f6ce-4ebb-bee6-c81ab09da4b8",
                    Service = "CL"
                }
            };
            await _busControl.SendAsync(RabbitExchange.EFMS_Accounting, RabbitConstants.CalculatingReceivableDataPartnerQueue, models);
            return Ok(new { Messages = "Push Rabbit Success" });
        }

        [HttpGet]
        public IActionResult Get()
        {
            _queue.QueueBackgroundWorkItem(async token =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    new LogHelper("Ghi log sau mỗi 10 giây", DateTime.Now.ToString(""));
                    // TODO
                    await Task.Delay(TimeSpan.FromSeconds(10), token);
                }
            });
            return Ok("In progress..");
        }
        #region -- Test API --
        [HttpPost("GetListAdvanceToSync")]
        [Authorize]
        public IActionResult GetListAdvanceToSync(List<Guid> Ids)
        {
            var data = accountingService.GetListAdvanceToSyncBravo(Ids);
            return Ok(data);
        }
        #endregion -- Test API --
        [HttpPost("GetListVoucherToSync")]
        public IActionResult GetListVoucherToSync(List<Guid> Ids)
        {
            currentUser.Action = "GetListVoucherToSync";
            var data = accountingService.GetListVoucherToSyncBravo(Ids);
            return Ok(data);
        }

        [HttpPost("GetListSettleToSync")]
        public IActionResult GetListSettleToSync(List<Guid> Ids)
        {
            currentUser.Action = "GetListSettleToSync";
            var data = accountingService.GetListSettlementToSyncBravo(Ids);
            return Ok(data);
        }

        [HttpPost("GetListSoaCreditToSync")]
        public IActionResult GetListSoaCreditToSync(List<RequestStringTypeListModel> requests)
        {
            var models = requests.Where(x => x.Action == ACTION.ADD && x.Type?.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_CREDIT).ToList();
            List<SyncCreditModel> list = (models.Count > 0) ? accountingService.GetListSoaCreditToSync(models) : new List<SyncCreditModel>();
            return Ok(list);
        }

        [HttpPost("GetListCdNoteCredit")]
        public IActionResult GetListCdNoteCredit(List<RequestGuidTypeListModel> requests)
        {
            List<RequestGuidTypeListModel> models = requests.Where(x => x.Action == ACTION.ADD && x.Type == AccountingConstants.ACCOUNTANT_TYPE_CREDIT).ToList();
            List<SyncCreditModel> list = (models.Count > 0) ? accountingService.GetListCdNoteCreditToSync(models) : new List<SyncCreditModel>();
            return Ok(list);
        }


        [HttpPost("GetListInvoicePaymentToSync")]
        [Authorize]
        public async Task<IActionResult> GetListInvoicePaymentToSync(List<RequestGuidListModel> request)
        {
            var _startDateProgress = DateTime.Now;
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                // 1. Login
                HttpResponseMessage responseFromApi = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
                BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;

                if (loginResponse.Success == "1")
                {
                    // 2. Get Data To Sync.
                    List<Guid> ids = request.Select(x => x.Id).ToList();

                    List<Guid> idsAdd = request.Where(x => x.Action == ACTION.ADD).Select(x => x.Id).ToList();
                    List<Guid> idsUpdate = request.Where(x => x.Action == ACTION.UPDATE).Select(x => x.Id).ToList();

                    List<PaymentModel> listAdd = (idsAdd.Count > 0) ? accountingService.GetListInvoicePaymentToSync(idsAdd) : new List<PaymentModel>();
                    List<PaymentModel> listUpdate = (idsUpdate.Count > 0) ? accountingService.GetListInvoicePaymentToSync(idsUpdate) : new List<PaymentModel>();

                    HttpResponseMessage resAdd = new HttpResponseMessage();
                    HttpResponseMessage resUpdate = new HttpResponseMessage();
                    BravoResponseModel responseAddModel = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel = new BravoResponseModel();

                    // 3. Call Bravo to SYNC.
                    if (listAdd.Count > 0)
                    {
                        resAdd = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSReceiptDataSyncAdd", listAdd, loginResponse.TokenKey);
                        responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "GetListInvoicePaymentToSync",
                            FuncPartner = "EFMSReceiptDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel),
                            Major = "Nghiệp Vụ Phiếu Thu",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    if (listUpdate.Count > 0)
                    {
                        resUpdate = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSReceiptDataSyncUpdate", listUpdate, loginResponse.TokenKey);
                        responseUpdateModel = await resUpdate.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "GetListInvoicePaymentToSync",
                            FuncPartner = "EFMSReceiptDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel),
                            Major = "Nghiệp Vụ Phiếu Thu",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // 4. Update STATUS
                    if (responseAddModel.Success == "1"
                        || responseUpdateModel.Success == "1")
                    {
                        ResultHandle result = new ResultHandle { Status = true, Message = "Sync phiều thu thành công", Data = ids };
                        return Ok(result);
                    }
                    else
                    {
                        ResultHandle result = new ResultHandle { Status = false, Message = responseAddModel.Msg + "\n" + responseUpdateModel.Msg, Data = ids };
                        return BadRequest(result);
                    }
                }
                return BadRequest("Sync fail");
            }
            catch (Exception)
            {
                return BadRequest("Sync fail");
            }
        }

        [HttpPost("GetListObhPaymentToSync")]
        [Authorize]
        public async Task<IActionResult> GetListObhPaymentToSync(List<RequestStringListModel> request)
        {
            var _startDateProgress = DateTime.Now;
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                // 1. Login
                HttpResponseMessage responseFromApi = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
                BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;

                if (loginResponse.Success == "1")
                {
                    // 2. Get Data To Sync.
                    List<string> ids = request.Select(x => x.Id).ToList();

                    List<string> idsAdd = request.Where(x => x.Action == ACTION.ADD).Select(x => x.Id).ToList();
                    List<string> idsUpdate = request.Where(x => x.Action == ACTION.UPDATE).Select(x => x.Id).ToList();

                    List<PaymentModel> listAdd = accountingService.GetListObhPaymentToSync(idsAdd);
                    List<PaymentModel> listUpdate = accountingService.GetListObhPaymentToSync(idsUpdate);

                    HttpResponseMessage resAdd = new HttpResponseMessage();
                    HttpResponseMessage resUpdate = new HttpResponseMessage();
                    BravoResponseModel responseAddModel = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel = new BravoResponseModel();

                    // 3. Call Bravo to SYNC.
                    if (listAdd.Count > 0)
                    {
                        resAdd = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSReceiptDataSyncAdd", listAdd, loginResponse.TokenKey);
                        responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "GetListObhPaymentToSync",
                            FuncPartner = "EFMSReceiptDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel),
                            Major = "Nghiệp Vụ Phiếu Thu",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    if (listUpdate.Count > 0)
                    {
                        resUpdate = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSReceiptDataSyncUpdate", listUpdate, loginResponse.TokenKey);
                        responseUpdateModel = await resUpdate.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "GetListObhPaymentToSync",
                            FuncPartner = "EFMSReceiptDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel),
                            Major = "Nghiệp Vụ Phiếu Thu",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // 4. Update STATUS
                    if (responseAddModel.Success == "1"
                        || responseUpdateModel.Success == "1")
                    {
                        ResultHandle result = new ResultHandle { Status = true, Message = "Sync phiếu thu thành công", Data = ids };
                        return Ok(result);
                    }
                    else
                    {
                        ResultHandle result = new ResultHandle { Status = false, Message = responseAddModel.Msg + "\n" + responseUpdateModel.Msg, Data = ids };
                        return BadRequest(result);
                    }
                }
                return BadRequest("Sync fail");
            }
            catch (Exception)
            {
                return BadRequest("Sync fail");
            }
        }

        [HttpPut("SyncAdvanceToAccountantSystem")]
        [Authorize]
        public async Task<IActionResult> SyncAdvanceToAccountantSystem(List<RequestGuidAndFileListModel> request)
        {
            var _startDateProgress = DateTime.Now;
            if (!ModelState.IsValid) return BadRequest();
            currentUser.Action = "SyncAdvanceToAccountantSystem";
            try
            {
                // 1. LOGIN
                HttpResponseMessage responseFromApi = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
                BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;

                if (loginResponse.Success == "1")
                {
                    // 2. Get Data To Sync.
                    List<Guid> Ids = request.Select(x => x.Id).ToList();

                    // 3. Call Bravo to SYNC.
                    List<Guid> IdsAdd = request.Where(action => action.Action == ACTION.ADD).Select(x => x.Id).ToList();
                    List<Guid> IdsUpdate = request.Where(action => action.Action == ACTION.UPDATE).Select(x => x.Id).ToList();

                    List<BravoAdvanceModel> listAdd = (IdsAdd.Count > 0) ? accountingService.GetListAdvanceToSyncBravo(IdsAdd) : new List<BravoAdvanceModel>();
                    List<BravoAdvanceModel> listUpdate = (IdsUpdate.Count > 0) ? accountingService.GetListAdvanceToSyncBravo(IdsUpdate) : new List<BravoAdvanceModel>();

                   
                    
                    HttpResponseMessage resAdd = new HttpResponseMessage();
                    HttpResponseMessage resUpdate = new HttpResponseMessage();
                    BravoResponseModel responseAddModel = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel = new BravoResponseModel();

                    if (listAdd.Count > 0)
                    {
                        resAdd = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSAdvanceSyncAdd", listAdd, loginResponse.TokenKey);
                        responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncAdvanceToAccountantSystem",
                            FuncPartner = "EFMSAdvanceSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel),
                            Major = "Nghiệp Vụ Tạm Ứng",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    if (listUpdate.Count > 0)
                    {
                        //foreach (var item in listUpdate)
                        //{
                        //    string fileNameAttached = request.Where(x => x.Action == ACTION.UPDATE && item.Stt == x.Id)?.FirstOrDefault().fileName;
                        //    if (!string.IsNullOrEmpty(fileNameAttached))
                        //    {
                        //        item.AtchDocInfo.Add(new BravoAttachDoc
                        //        {
                        //            AttachDocRowId = Guid.NewGuid().ToString(),
                        //            AttachDocName = "Advance Preview Template",
                        //            AttachDocPath = fileNameAttached,
                        //            AttachDocDate = DateTime.Now
                        //        });
                        //    }
                        //}
                        resUpdate = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSAdvanceSyncUpdate", listUpdate, loginResponse.TokenKey);
                        responseUpdateModel = await resUpdate.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncAdvanceToAccountantSystem",
                            FuncPartner = "EFMSAdvanceSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel),
                            Major = "Nghiệp Vụ Tạm Ứng",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // 4. Update STATUS
                    if (responseAddModel.Success == "1" || responseUpdateModel.Success == "1")
                    {
                        HandleState hs = accountingService.SyncListAdvanceToBravo(Ids, out Ids);
                        string message = HandleError.GetMessage(hs, Crud.Update);
                        ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = Ids };
                        if (!hs.Success)
                        {
                            return BadRequest(result);
                        }

                        return Ok(result);
                    }
                    return BadRequest(new ResultHandle { Message = responseAddModel.Msg + "\n" + responseUpdateModel.Msg });

                }
                new LogHelper("eFMS_SYNC_LOG", loginResponse.ToString());
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_SYNC_LOG", ex.ToString());
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
        }

        [HttpPut("SyncSettlementToAccountantSystem")]
        [Authorize]
        public async Task<IActionResult> SyncSettlementToAccountantSystem(List<RequestGuidAndFileListModel> request)
        {
            var _startDateProgress = DateTime.Now;
            if (!ModelState.IsValid) return BadRequest();
            currentUser.Action = "SyncSettlementToAccountantSystem";

            try
            {
                // 1. Login
                HttpResponseMessage responseFromApi = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
                BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;

                if (loginResponse.Success == "1")
                {
                    // 2. Get Data To Sync.
                    List<Guid> Ids = request.Select(x => x.Id).ToList();

                    List<Guid> IdsAdd = request.Where(action => action.Action == ACTION.ADD).Select(x => x.Id).ToList();
                    List<Guid> IdsUpdate = request.Where(action => action.Action == ACTION.UPDATE).Select(x => x.Id).ToList();

                    List<BravoSettlementModel> listAdd = (IdsAdd.Count > 0) ? accountingService.GetListSettlementToSyncBravo(IdsAdd) : new List<BravoSettlementModel>();
                    List<BravoSettlementModel> listUpdate = (IdsUpdate.Count > 0) ? accountingService.GetListSettlementToSyncBravo(IdsUpdate) : new List<BravoSettlementModel>();

                    HttpResponseMessage resAdd = new HttpResponseMessage();
                    HttpResponseMessage resUpdate = new HttpResponseMessage();
                    BravoResponseModel responseAddModel = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel = new BravoResponseModel();

                    // 3. Call Bravo to SYNC.
                    if (listAdd.Count > 0)
                    {
                        //foreach (var item in listAdd)
                        //{
                        //    string fileNameAttached = request.Where(x => x.Action == ACTION.ADD && item.Stt == x.Id)?.FirstOrDefault().fileName;
                        //    if (!string.IsNullOrEmpty(fileNameAttached))
                        //    {
                        //        item.AtchDocInfo.Add(new BravoAttachDoc
                        //        {
                        //            AttachDocRowId = Guid.NewGuid().ToString(),
                        //            AttachDocName = "SM Preview Template",
                        //            AttachDocPath = fileNameAttached,
                        //            AttachDocDate = DateTime.Now
                        //        });
                        //    }
                        //}
                        resAdd = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncAdd", listAdd, loginResponse.TokenKey);
                        responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncSettlementToAccountantSystem",
                            FuncPartner = "EFMSVoucherDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel),
                            Major = "Nghiệp Vụ Chi Phí",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    if (listUpdate.Count > 0)
                    {
                        //foreach (var item in listUpdate)
                        //{
                        //    string fileNameAttached = request.Where(x => x.Action == ACTION.UPDATE && item.Stt == x.Id)?.FirstOrDefault().fileName;
                        //    if (!string.IsNullOrEmpty(fileNameAttached))
                        //    {
                        //        item.AtchDocInfo.Add(new BravoAttachDoc
                        //        {
                        //            AttachDocRowId = Guid.NewGuid().ToString(),
                        //            AttachDocName = "SM Preview Template",
                        //            AttachDocPath = fileNameAttached,
                        //            AttachDocDate = DateTime.Now
                        //        });
                        //    }
                        //}
                        resUpdate = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncUpdate", listUpdate, loginResponse.TokenKey);
                        responseUpdateModel = await resUpdate.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncSettlementToAccountantSystem",
                            FuncPartner = "EFMSVoucherDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel),
                            Major = "Nghiệp Vụ Chi Phí",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // 4. Update STATUS
                    if (responseAddModel.Success == "1" || responseUpdateModel.Success == "1")
                    {
                        HandleState hs = accountingService.SyncListSettlementToBravo(Ids, out Ids);
                        string message = HandleError.GetMessage(hs, Crud.Update);
                        ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = Ids };
                        if (!hs.Success)
                        {
                            return BadRequest(result);
                        }
                        listAdd.ForEach(async x =>
                        {
                            var modelSuccess = new
                            {
                                SettlementId = x.Stt,
                                Lang = "EN",
                                Action = "eDOC",
                                AccessToken= Request.Headers["Authorization"].ToString()
                        };
                            await _busControl.SendAsync(RabbitExchange.EFMS_ReportData, RabbitConstants.GenFileQueue, modelSuccess);
                        });
                        return Ok(result);
                    }
                    return BadRequest(new ResultHandle { Message = responseAddModel.Msg + "\n" + responseUpdateModel.Msg });
                }
                new LogHelper("eFMS_SYNC_LOG", loginResponse.ToString());
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_SYNC_LOG", ex.ToString());
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
        }

        [HttpPut("SyncVoucherToAccountantSystem")]
        [Authorize]
        public async Task<IActionResult> SyncVoucherToAccountantSystem(List<RequestGuidListModel> request)
        {
            var _startDateProgress = DateTime.Now;
            if (!ModelState.IsValid) return BadRequest();
            currentUser.Action = "SyncVoucherToAccountantSystem";

            try
            {
                // 1. Login
                HttpResponseMessage responseFromApi = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
                BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;

                if (loginResponse.Success == "1")
                {
                    // 2. Get Data To Sync.
                    List<Guid> Ids = request.Select(x => x.Id).ToList();

                    List<Guid> IdsAdd = request.Where(action => action.Action == ACTION.ADD).Select(x => x.Id).ToList();
                    List<Guid> IdsUpdate = request.Where(action => action.Action == ACTION.UPDATE).Select(x => x.Id).ToList();

                    List<BravoVoucherModel> listAdd = (IdsAdd.Count > 0) ? accountingService.GetListVoucherToSyncBravo(IdsAdd) : new List<BravoVoucherModel>();
                    List<BravoVoucherModel> listUpdate = (IdsUpdate.Count > 0) ? accountingService.GetListVoucherToSyncBravo(IdsUpdate) : new List<BravoVoucherModel>();

                    HttpResponseMessage resAdd = new HttpResponseMessage();
                    HttpResponseMessage resUpdate = new HttpResponseMessage();
                    BravoResponseModel responseAddModel = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel = new BravoResponseModel();

                    // 3. Call Bravo to SYNC.
                    if (listAdd.Count > 0)
                    {
                        resAdd = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncAdd", listAdd, loginResponse.TokenKey);
                        responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncVoucherToAccountantSystem",
                            FuncPartner = "EFMSVoucherDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel),
                            Major = "Nghiệp Vụ Chi Phí",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    if (listUpdate.Count > 0)
                    {
                        resUpdate = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncUpdate", listUpdate, loginResponse.TokenKey);
                        responseUpdateModel = await resUpdate.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncVoucherToAccountantSystem",
                            FuncPartner = "EFMSVoucherDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel),
                            Major = "Nghiệp Vụ Chi Phí",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // 4. Update STATUS
                    if (responseAddModel.Success == "1" || responseUpdateModel.Success == "1")
                    {
                        HandleState hs = accountingService.SyncListVoucherToBravo(Ids, out Ids);
                        string message = HandleError.GetMessage(hs, Crud.Update);
                        ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = Ids };
                        if (!hs.Success)
                        {
                            return BadRequest(result);
                        }
                        return Ok(result);
                    }
                    return BadRequest(new ResultHandle { Message = responseAddModel.Msg + "\n" + responseUpdateModel.Msg });
                }
                new LogHelper("eFMS_SYNC_LOG", loginResponse.ToString());
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_SYNC_LOG", ex.ToString());
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
        }

        /// <summary>
        /// Sync list CDNote to Accountant
        /// </summary>
        /// <param name="requests"></param>
        /// <returns></returns>
        [HttpPut("SyncListCdNoteToAccountant")]
        [Authorize]
        public async Task<IActionResult> SyncListCdNoteToAccountant(List<RequestGuidTypeListModel> requests)
        {
            var _startDateProgress = DateTime.Now;
            if (!ModelState.IsValid) return BadRequest();
            currentUser.Action = "SyncListCdNoteToAccountant";

            try
            {
                // 1. Login
                HttpResponseMessage responseFromApi = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
                BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;

                if (loginResponse.Success == "1")
                {
                    // 2. Get Data To Sync.                    
                    List<Guid> IdsAdd_NVHD = requests.Where(x => x.Action == ACTION.ADD && (x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type == AccountingConstants.ACCOUNTANT_TYPE_INVOICE)).Select(x => x.Id).ToList();
                    List<Guid> IdsUpdate_NVHD = requests.Where(x => x.Action == ACTION.UPDATE && (x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type == AccountingConstants.ACCOUNTANT_TYPE_INVOICE)).Select(x => x.Id).ToList();
                    List<RequestGuidTypeListModel> IdsAdd_NVCP = requests.Where(x => x.Action == ACTION.ADD && x.Type == AccountingConstants.ACCOUNTANT_TYPE_CREDIT).ToList();
                    List<RequestGuidTypeListModel> IdsUpdate_NVCP = requests.Where(x => x.Action == ACTION.UPDATE && x.Type == AccountingConstants.ACCOUNTANT_TYPE_CREDIT).ToList();

                    List<SyncModel> listAdd_NVHD = (IdsAdd_NVHD.Count > 0) ? accountingService.GetListCdNoteToSync(IdsAdd_NVHD) : new List<SyncModel>();
                    List<SyncModel> listUpdate_NVHD = (IdsUpdate_NVHD.Count > 0) ? accountingService.GetListCdNoteToSync(IdsUpdate_NVHD) : new List<SyncModel>();
                    List<SyncCreditModel> listAdd_NVCP = (IdsAdd_NVCP.Count > 0) ? accountingService.GetListCdNoteCreditToSync(IdsAdd_NVCP) : new List<SyncCreditModel>();
                    List<SyncCreditModel> listUpdate_NVCP = (IdsUpdate_NVCP.Count > 0) ? accountingService.GetListCdNoteCreditToSync(IdsUpdate_NVCP) : new List<SyncCreditModel>();

                    List<SyncCreditModel> listAdd_NVCP_SameCurrLocal = listAdd_NVCP.Where(x => x.CurrencyCode == AccountingConstants.CURRENCY_LOCAL && x.Details.Where(w => w.CurrencyCode == AccountingConstants.CURRENCY_LOCAL).Count() == x.Details.Count()).ToList();
                    List<SyncCreditModel> listUpdate_NVCP_SameCurrLocal = listUpdate_NVCP.Where(x => x.CurrencyCode == AccountingConstants.CURRENCY_LOCAL && x.Details.Where(w => w.CurrencyCode == AccountingConstants.CURRENCY_LOCAL).Count() == x.Details.Count()).ToList();

                    List<SyncCreditModel> listAdd_NVCP_DiffCurrLocal = listAdd_NVCP.Where(x => x.CurrencyCode != AccountingConstants.CURRENCY_LOCAL || x.Details.Any(w => w.CurrencyCode != AccountingConstants.CURRENCY_LOCAL)).ToList();
                    List<SyncCreditModel> listUpdate_NVCP_DiffCurrLocal = listUpdate_NVCP.Where(x => x.CurrencyCode != AccountingConstants.CURRENCY_LOCAL || x.Details.Any(w => w.CurrencyCode != AccountingConstants.CURRENCY_LOCAL)).ToList();

                    //List<Guid> ids = requests.Where(w =>
                    //   !listAdd_NVCP_DiffCurrLocal.Select(se => se.Stt).Contains(w.Id.ToString())
                    //&& !listUpdate_NVCP_DiffCurrLocal.Select(se => se.Stt).Contains(w.Id.ToString())).Select(x => x.Id).ToList();
                    List<Guid> ids = requests.Select(x => x.Id).ToList();

                    HttpResponseMessage resAdd_NVHD = new HttpResponseMessage();
                    HttpResponseMessage resUpdate_NVHD = new HttpResponseMessage();
                    BravoResponseModel responseAddModel_NVHD = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel_NVHD = new BravoResponseModel();

                    HttpResponseMessage resAdd_NVCP = new HttpResponseMessage();
                    HttpResponseMessage resUpdate_NVCP = new HttpResponseMessage();
                    BravoResponseModel responseAddModel_NVCP = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel_NVCP = new BravoResponseModel();
                    
                    // 3. Call Bravo to SYNC.
                    if (listAdd_NVHD.Count > 0)
                    {
                        resAdd_NVHD = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSInvoiceDataSyncAdd", listAdd_NVHD, loginResponse.TokenKey);
                        responseAddModel_NVHD = await resAdd_NVHD.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListCdNoteToAccountant",
                            FuncPartner = "EFMSInvoiceDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd_NVHD),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel_NVHD),
                            Major = "Nghiệp Vụ Hóa Đơn",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    if (listUpdate_NVHD.Count > 0)
                    {
                        resUpdate_NVHD = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSInvoiceDataSyncUpdate", listUpdate_NVHD, loginResponse.TokenKey);
                        responseUpdateModel_NVHD = await resUpdate_NVHD.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListCdNoteToAccountant",
                            FuncPartner = "EFMSInvoiceDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate_NVHD),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel_NVHD),
                            Major = "Nghiệp Vụ Hóa Đơn",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // [CR: #16976] => Send qua Bravo những Credit Note có [currency là VND và currency của từng phí của Credit Note đó là VND] hoặc [Currency là USD hoặc tồn tại phí currency USD]
                    // bỏ => [ADD Voucher: Chỉ send qua Bravo những Credit Note có currency là VND và currency của từng phí của Credit Note đó là VND]
                    if (listAdd_NVCP_SameCurrLocal.Count > 0 || listAdd_NVCP_DiffCurrLocal.Count > 0)
                    {
                        var listAddToSynceBravo = new List<SyncCreditModel>();
                        if(listAdd_NVCP_SameCurrLocal.Count > 0)
                        {
                            listAddToSynceBravo = listAdd_NVCP_SameCurrLocal;
                        }
                        else
                        {
                            listAddToSynceBravo = listAdd_NVCP_DiffCurrLocal;
                        }
                        resAdd_NVCP = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncAdd", listAddToSynceBravo, loginResponse.TokenKey);
                        responseAddModel_NVCP = await resAdd_NVCP.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListCdNoteToAccountant",
                            FuncPartner = "EFMSVoucherDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAddToSynceBravo),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel_NVCP),
                            Major = "Nghiệp Vụ Chi Phí",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // [CR: #16976] => Send qua Bravo những Credit Note có [currency là VND và currency của từng phí của Credit Note đó là VND] hoặc [Currency là USD hoặc tồn tại phí currency USD]
                    // bỏ => [ADD Voucher: Chỉ send qua Bravo những Credit Note có currency là VND và currency của từng phí của Credit Note đó là VND]
                    if (listUpdate_NVCP_SameCurrLocal.Count > 0 || listUpdate_NVCP_DiffCurrLocal.Count > 0)
                    {
                        var listUpdateToSynceBravo = new List<SyncCreditModel>();
                        if (listUpdate_NVCP_SameCurrLocal.Count > 0)
                        {
                            listUpdateToSynceBravo = listUpdate_NVCP_SameCurrLocal;
                        }
                        else
                        {
                            listUpdateToSynceBravo = listUpdate_NVCP_DiffCurrLocal;
                        }
                        resUpdate_NVCP = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncUpdate", listUpdateToSynceBravo, loginResponse.TokenKey);
                        responseUpdateModel_NVCP = await resUpdate_NVCP.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListCdNoteToAccountant",
                            FuncPartner = "EFMSVoucherDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdateToSynceBravo),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel_NVCP),
                            Major = "Nghiệp Vụ Chi Phí",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // 4. Update STATUS
                    if (responseAddModel_NVHD.Success == "1"
                        || responseUpdateModel_NVHD.Success == "1"
                        || responseAddModel_NVCP.Success == "1"
                        || responseUpdateModel_NVCP.Success == "1")
                    {
                        HandleState hs = accountingService.SyncListCdNoteToAccountant(ids);
                        string message = HandleError.GetMessage(hs, Crud.Update);
                        ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = ids };
                        if (!hs.Success)
                        {
                            new LogHelper("eFMS_SYNC_LOG", hs.ToString() + " ");
                            result = new ResultHandle { Status = hs.Success, Message = hs.Message?.ToString(), Data = ids };
                            return BadRequest(result);
                        }
                        else
                        {
                            // Sync & Update thành công >> Send Mail & Push Notification

                            //Debit Note / Invoice >> Send mail & Notification đến creator, current user & Department Accountant
                            accountingService.SendMailAndPushNotificationDebitToAccountant(listAdd_NVHD);
                            accountingService.SendMailAndPushNotificationDebitToAccountant(listUpdate_NVHD);
                            //Credit Note >> Send mail & Notification đến creator, current user & Department Accountant
                            accountingService.SendMailAndPushNotificationToAccountant(listAdd_NVCP);
                            accountingService.SendMailAndPushNotificationToAccountant(listUpdate_NVCP);
                        }
                        return Ok(result);
                    }
                    else
                    {
                        var result = new ResultHandle { Status = false, Message = responseAddModel_NVHD.Msg + "\n" + responseUpdateModel_NVHD.Msg + "\n" + responseAddModel_NVCP.Msg + "\n" + responseUpdateModel_NVCP.Msg, Data = ids };
                        return BadRequest(result);
                    }
                }
                new LogHelper("eFMS_SYNC_LOG", loginResponse.ToString());
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_SYNC_LOG", ex.ToString());
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
        }

        /// <summary>
        /// Sync list SOA to Accountant
        /// </summary>
        /// <param name="requests"></param>
        /// <returns></returns>
        [HttpPut("SyncListSoaToAccountant")]
        [Authorize]
        public async Task<IActionResult> SyncListSoaToAccountant(List<RequestStringTypeListModel> requests)
        {
            var _startDateProgress = DateTime.Now;
            if (!ModelState.IsValid) return BadRequest();
            currentUser.Action = "SyncListSoaToAccountant";

            try
            {
                // 1. Login
                HttpResponseMessage responseFromApi = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
                BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;

                if (loginResponse.Success == "1")
                {
                    // 2. Get Data To Sync.                    

                    List<string> IdsAdd_NVHD = requests.Where(x => x.Action == ACTION.ADD && x.Type?.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_DEBIT).Select(x => x.Id).ToList();
                    List<string> IdsUpdate_NVHD = requests.Where(x => x.Action == ACTION.UPDATE && x.Type?.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_DEBIT).Select(x => x.Id).ToList();
                    List<RequestStringTypeListModel> IdsAdd_NVCP = requests.Where(x => x.Action == ACTION.ADD && x.Type?.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_CREDIT).ToList();
                    List<RequestStringTypeListModel> IdsUpdate_NVCP = requests.Where(x => x.Action == ACTION.UPDATE && x.Type?.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_CREDIT).ToList();
                    
                    List<SyncModel> listAdd_NVHD = accountingService.GetListSoaToSync(IdsAdd_NVHD);
                    List<SyncModel> listUpdate_NVHD = accountingService.GetListSoaToSync(IdsUpdate_NVHD);
                    List<SyncCreditModel> listAdd_NVCP = accountingService.GetListSoaCreditToSync(IdsAdd_NVCP);
                    List<SyncCreditModel> listUpdate_NVCP = accountingService.GetListSoaCreditToSync(IdsUpdate_NVCP);

                    List<SyncCreditModel> listAdd_NVCP_SameCurrLocal = listAdd_NVCP.Where(x => x.CurrencyCode == AccountingConstants.CURRENCY_LOCAL && x.Details.Where(w => w.CurrencyCode == AccountingConstants.CURRENCY_LOCAL).Count() == x.Details.Count()).ToList();
                    List<SyncCreditModel> listUpdate_NVCP_SameCurrLocal = listUpdate_NVCP.Where(x => x.CurrencyCode == AccountingConstants.CURRENCY_LOCAL && x.Details.Where(w => w.CurrencyCode == AccountingConstants.CURRENCY_LOCAL).Count() == x.Details.Count()).ToList();

                    List<SyncCreditModel> listAdd_NVCP_DiffCurrLocal = listAdd_NVCP.Where(x => x.CurrencyCode != AccountingConstants.CURRENCY_LOCAL || x.Details.Any(w => w.CurrencyCode != AccountingConstants.CURRENCY_LOCAL)).ToList();
                    List<SyncCreditModel> listUpdate_NVCP_DiffCurrLocal = listUpdate_NVCP.Where(x => x.CurrencyCode != AccountingConstants.CURRENCY_LOCAL || x.Details.Any(w => w.CurrencyCode != AccountingConstants.CURRENCY_LOCAL)).ToList();
                    
                    //List<string> ids = requests.Where(w => 
                    //   !listAdd_NVCP_DiffCurrLocal.Select(se => se.Stt).Contains(w.Id.ToString()) 
                    //&& !listUpdate_NVCP_DiffCurrLocal.Select(se => se.Stt).Contains(w.Id.ToString())).Select(x => x.Id).ToList();
                    List<string> ids = requests.Select(x => x.Id).ToList();

                    HttpResponseMessage resAdd_NVHD = new HttpResponseMessage();
                    HttpResponseMessage resUpdate_NVHD = new HttpResponseMessage();
                    BravoResponseModel responseAddModel_NVHD = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel_NVHD = new BravoResponseModel();

                    HttpResponseMessage resAdd_NVCP = new HttpResponseMessage();
                    HttpResponseMessage resUpdate_NVCP = new HttpResponseMessage();
                    BravoResponseModel responseAddModel_NVCP = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel_NVCP = new BravoResponseModel();
                    
                    // 3. Call Bravo to SYNC.
                    if (listAdd_NVHD.Count > 0)
                    {
                        resAdd_NVHD = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSInvoiceDataSyncAdd", listAdd_NVHD, loginResponse.TokenKey);
                        responseAddModel_NVHD = await resAdd_NVHD.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListSoaToAccountant",
                            FuncPartner = "EFMSInvoiceDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd_NVHD),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel_NVHD),
                            Major = "Nghiệp Vụ Hóa Đơn",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    if (listUpdate_NVHD.Count > 0)
                    {
                        resUpdate_NVHD = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSInvoiceDataSyncUpdate", listUpdate_NVHD, loginResponse.TokenKey);
                        responseUpdateModel_NVHD = await resUpdate_NVHD.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListSoaToAccountant",
                            FuncPartner = "EFMSInvoiceDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate_NVHD),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel_NVHD),
                            Major = "Nghiệp Vụ Hóa Đơn",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // [CR: #16976] => ADD Voucher:Send qua Bravo những SOA(credit) có [currency là VND và currency của từng phí của SOA đó là VND] hoặc [Currency là USD hoặc tồn tại phí currency USD]
                    // bỏ => ADD Voucher: Chỉ send qua Bravo những SOA(credit) có currency là VND và currency của từng phí của SOA đó là VND
                    if (listAdd_NVCP_SameCurrLocal.Count > 0 || listAdd_NVCP_DiffCurrLocal.Count > 0)
                    {
                        var listAddToSynceBravo = new List<SyncCreditModel>();
                        if (listAdd_NVCP_SameCurrLocal.Count > 0)
                        {
                            listAddToSynceBravo = listAdd_NVCP_SameCurrLocal;
                        }
                        else
                        {
                            listAddToSynceBravo = listAdd_NVCP_DiffCurrLocal;
                        }
                        resAdd_NVCP = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncAdd", listAddToSynceBravo, loginResponse.TokenKey);
                        responseAddModel_NVCP = await resAdd_NVCP.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListSoaToAccountant",
                            FuncPartner = "EFMSVoucherDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAddToSynceBravo),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel_NVCP),
                            Major = "Nghiệp Vụ Chi Phí",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // [CR: #16976] => UDPATE Voucher: Send qua Bravo những SOA(credit) có [currency là VND và currency của từng phí của SOA đó là VND] hoặc [Currency là USD hoặc tồn tại phí currency USD]
                    // bỏ => UDPATE Voucher: Chỉ send qua Bravo những SOA(credit) có currency là VND và currency của từng phí của SOA đó là VND
                    if (listUpdate_NVCP_SameCurrLocal.Count > 0 || listUpdate_NVCP_DiffCurrLocal.Count > 0)
                    {
                        var listUpdateToSynceBravo = new List<SyncCreditModel>();
                        if (listUpdate_NVCP_SameCurrLocal.Count > 0)
                        {
                            listUpdateToSynceBravo = listUpdate_NVCP_SameCurrLocal;
                        }
                        else
                        {
                            listUpdateToSynceBravo = listUpdate_NVCP_DiffCurrLocal;
                        }
                        resUpdate_NVCP = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncUpdate", listUpdateToSynceBravo, loginResponse.TokenKey);
                        responseUpdateModel_NVCP = await resUpdate_NVCP.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListSoaToAccountant",
                            FuncPartner = "EFMSVoucherDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdateToSynceBravo),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel_NVCP),
                            Major = "Nghiệp Vụ Chi Phí",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // 4. Update STATUS
                    if (responseAddModel_NVHD.Success == "1"
                        || responseUpdateModel_NVHD.Success == "1"
                        || responseAddModel_NVCP.Success == "1"
                        || responseUpdateModel_NVCP.Success == "1")
                    {
                        HandleState hs = accountingService.SyncListSoaToAccountant(ids);
                        string message = HandleError.GetMessage(hs, Crud.Update);
                        ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = ids };
                        if (!hs.Success)
                        {
                            new LogHelper("eFMS_SYNC_LOG", result.ToString() + " ");
                            result = new ResultHandle { Status = hs.Success, Message = hs.Message?.ToString(), Data = ids };
                            return BadRequest(result);
                        }
                        else
                        {
                            // Sync & Update thành công >> Send Mail & Push Notification

                            //SOA >> Send mail & Notification đến creator, current user & Department Accountant
                            accountingService.SendMailAndPushNotificationDebitToAccountant(listAdd_NVHD);
                            accountingService.SendMailAndPushNotificationDebitToAccountant(listUpdate_NVHD);
                            //SOA >> Send mail & Notification đến creator, current user & Department Accountant
                            accountingService.SendMailAndPushNotificationToAccountant(listAdd_NVCP);
                            accountingService.SendMailAndPushNotificationToAccountant(listUpdate_NVCP);
                        }
                        return Ok(result);
                    }
                    else
                    {
                        var result = new ResultHandle { Status = false, Message = responseAddModel_NVHD.Msg + "\n" + responseUpdateModel_NVHD.Msg + "\n" + responseAddModel_NVCP.Msg + "\n" + responseUpdateModel_NVCP.Msg, Data = ids };
                        return BadRequest(result);
                    }
                }
                new LogHelper("eFMS_SYNC_LOG", loginResponse.ToString());
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_SYNC_LOG", ex.ToString());
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
        }

        /// <summary>
        /// Func Test (Get List Debit Note)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("GetListCdNoteDebit")]
        public IActionResult GetListCdNoteDebit(List<RequestGuidTypeListModel> request)
        {
            currentUser.Action = "GetListCdNoteDebit";
            List<Guid> Ids = request.Where(x => x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type == AccountingConstants.ACCOUNTANT_TYPE_INVOICE).Select(x => x.Id).ToList();
            List<SyncModel> list = (Ids.Count > 0) ? accountingService.GetListCdNoteToSync(Ids) : new List<SyncModel>();
            return Ok(list);
        }

        /// <summary>
        /// Func Test (Get List SOA Debit)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("GetListSOADebit")]
        public IActionResult GetListSOADebit(List<RequestStringTypeListModel> request)
        {
            currentUser.Action = "GetListSOADebit";
            List<string> Ids = request.Where(x => x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT).Select(x => x.Id).ToList();
            List<SyncModel> list = (Ids.Count > 0) ? accountingService.GetListSoaToSync(Ids) : new List<SyncModel>();
            return Ok(list);
        }

        /// <summary>
        /// Nghiệp vụ phiếu thu - Tạo Data Giải Lập Data Test Sync Qua Bravo
        /// </summary>
        /// <param name="paymentModels"></param>
        /// <param name="action">ADD(0) or UPDATE(1)</param>
        /// <returns></returns>
        [HttpPost("GiaLapDataPhieuThu")]
        [Authorize]
        public async Task<IActionResult> GiaLapDataPhieuThu(List<PaymentModel> paymentModels, [Required] ACTION action)
        {
            var _startDateProgress = DateTime.Now;
            if (!ModelState.IsValid) return BadRequest();

            if (paymentModels.Count == 0)
            {
                ResultHandle result = new ResultHandle { Status = false, Message = "paymentModels bắt buộc phải có data!", Data = paymentModels };
                return BadRequest(result);
            }
            
            try
            {
                // 1. Login
                HttpResponseMessage responseFromApi = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
                BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;

                if (loginResponse.Success == "1")
                {                    
                    HttpResponseMessage resAdd = new HttpResponseMessage();
                    HttpResponseMessage resUpdate = new HttpResponseMessage();
                    BravoResponseModel responseAddModel = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel = new BravoResponseModel();

                    // 3. Call Bravo to SYNC.
                    if (action == ACTION.ADD)
                    {
                        resAdd = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSReceiptDataSyncAdd", paymentModels, loginResponse.TokenKey);
                        responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "GiaLapDataPhieuThu",
                            FuncPartner = "EFMSReceiptDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(paymentModels),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel),
                            Major = "Nghiệp Vụ Phiếu Thu",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    if (action == ACTION.UPDATE)
                    {
                        resUpdate = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSReceiptDataSyncUpdate", paymentModels, loginResponse.TokenKey);
                        responseUpdateModel = await resUpdate.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "GiaLapDataPhieuThu",
                            FuncPartner = "EFMSReceiptDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(paymentModels),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel),
                            Major = "Nghiệp Vụ Phiếu Thu",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // 4. Update STATUS
                    if (responseAddModel.Success == "1"
                        || responseUpdateModel.Success == "1")
                    {
                        ResultHandle result = new ResultHandle { Status = true, Message = "Sync phiếu thu thành công", Data = paymentModels };
                        return Ok(result);
                    }
                    else
                    {
                        ResultHandle result = new ResultHandle { Status = false, Message = responseAddModel.Msg + "\n" + responseUpdateModel.Msg, Data = paymentModels };
                        return BadRequest(result);
                    }
                }
                return BadRequest("Sync fail");
            }
            catch (Exception)
            {
                return BadRequest("Sync fail");
            }
        }
        
        /// <summary>
        /// Func Test (Get List Receipt)
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPut("GetListReceipt")]
        public IActionResult GetListReceipt(List<Guid> ids)
        {
            List<AcctReceiptSyncModel> _receiptSyncs = new List<AcctReceiptSyncModel>();
            List<PaymentModel> list = new List<PaymentModel>();
            if (ids.Count > 0)
            {
                list = accountingService.GetListReceiptToAccountant(ids, out List<AcctReceiptSyncModel> receiptSyncs);
                _receiptSyncs = receiptSyncs;
            }
            return Ok(new { list, _receiptSyncs });
        }

        /// <summary>
        /// Sync list Receipt to Accountant
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("SyncListReceiptToAccountant")]
        [Authorize]
        public async Task<IActionResult> SyncListReceiptToAccountant(List<RequestGuidListModel> request)
        {
            var _startDateProgress = DateTime.Now;
            if (!ModelState.IsValid) return BadRequest();
            currentUser.Action = "SyncListReceiptToAccountant";

            try
            {
                // 1. Login
                HttpResponseMessage responseFromApi = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
                BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;

                if (loginResponse.Success == "1")
                {
                    // 2. Get Data To Sync.
                    List<Guid> ids = request.Select(x => x.Id).ToList();

                    List<Guid> idsAdd = request.Where(x => x.Action == ACTION.ADD).Select(x => x.Id).ToList();
                    List<Guid> idsUpdate = request.Where(x => x.Action == ACTION.UPDATE).Select(x => x.Id).ToList();

                    var receiptSyncs = new List<AcctReceiptSyncModel>();
                    List<PaymentModel> listAdd = new List<PaymentModel>();
                    if (idsAdd.Count > 0)
                    {
                        listAdd = accountingService.GetListReceiptToAccountant(idsAdd, out List<AcctReceiptSyncModel> addReceiptSyncs);
                        if (addReceiptSyncs.Count > 0)
                        {
                            receiptSyncs.AddRange(addReceiptSyncs);
                        }
                    }
                    List<PaymentModel> listUpdate = new List<PaymentModel>();
                    if (idsUpdate.Count > 0)
                    {
                        listUpdate = accountingService.GetListReceiptToAccountant(idsUpdate, out List<AcctReceiptSyncModel> updateReceiptSyncs);
                        if (updateReceiptSyncs.Count > 0)
                        {
                            receiptSyncs.AddRange(updateReceiptSyncs);
                        }
                    }

                    HttpResponseMessage resAdd = new HttpResponseMessage();
                    HttpResponseMessage resUpdate = new HttpResponseMessage();
                    BravoResponseModel responseAddModel = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel = new BravoResponseModel();

                    // 3. Call Bravo to SYNC.
                    if (listAdd.Count > 0)
                    {
                        resAdd = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSReceiptDataSyncAdd", listAdd, loginResponse.TokenKey);
                        responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "GetListReceiptToAccountant",
                            FuncPartner = "EFMSReceiptDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel),
                            Major = "Nghiệp Vụ Phiếu Thu",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    if (listUpdate.Count > 0)
                    {
                        resUpdate = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSReceiptDataSyncUpdate", listUpdate, loginResponse.TokenKey);
                        responseUpdateModel = await resUpdate.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "GetListReceiptToAccountant",
                            FuncPartner = "EFMSReceiptDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel),
                            Major = "Nghiệp Vụ Phiếu Thu",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // 4. Update STATUS
                    if (responseAddModel.Success == "1"
                        || responseUpdateModel.Success == "1")
                    {
                        HandleState hs = accountingService.SyncListReceiptToAccountant(ids, receiptSyncs);
                        string message = HandleError.GetMessage(hs, Crud.Update);
                        ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = ids };
                        if (!hs.Success)
                        {
                            new LogHelper("eFMS_SYNC_LOG", hs.ToString() + " ");
                            result = new ResultHandle { Status = hs.Success, Message = hs.Message?.ToString(), Data = ids };
                            return BadRequest(result);
                        }
                        return Ok(result);
                    }
                    else
                    {
                        ResultHandle result = new ResultHandle { Status = false, Message = responseAddModel.Msg + "\n" + responseUpdateModel.Msg, Data = ids };
                        return BadRequest(result);
                    }
                }
                new LogHelper("eFMS_SYNC_LOG", loginResponse.ToString());
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_SYNC_LOG", ex.ToString());
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
        }

        /// <summary>
        /// Sync list Receipt to Accountant (Option - sync đc tất cả các type trong cùng 1 phiếu thu)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("SyncListReceiptAllInToAccountant")]
        [Authorize]
        public async Task<IActionResult> SyncListReceiptAllInToAccountant(List<RequestGuidListModel> request)
        {
            var _startDateProgress = DateTime.Now;
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                // 1. Login
                HttpResponseMessage responseFromApi = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
                BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;

                if (loginResponse.Success == "1")
                {
                    // 2. Get Data To Sync.
                    List<Guid> ids = request.Select(x => x.Id).ToList();

                    List<Guid> idsAdd = request.Where(x => x.Action == ACTION.ADD).Select(x => x.Id).ToList();
                    List<Guid> idsUpdate = request.Where(x => x.Action == ACTION.UPDATE).Select(x => x.Id).ToList();

                    var receiptSyncs = new List<AcctReceiptSyncModel>();
                    List<PaymentModel> listAdd = new List<PaymentModel>();
                    if (idsAdd.Count > 0)
                    {
                        listAdd = accountingService.GetListReceiptAllInToAccountant(idsAdd, out List<AcctReceiptSyncModel> addReceiptSyncs);
                        if (addReceiptSyncs.Count > 0)
                        {
                            receiptSyncs.AddRange(addReceiptSyncs);
                        }
                    }
                    List<PaymentModel> listUpdate = new List<PaymentModel>();
                    if (idsUpdate.Count > 0)
                    {
                        listUpdate = accountingService.GetListReceiptAllInToAccountant(idsUpdate, out List<AcctReceiptSyncModel> updateReceiptSyncs);
                        if (updateReceiptSyncs.Count > 0)
                        {
                            receiptSyncs.AddRange(updateReceiptSyncs);
                        }
                    }

                    HttpResponseMessage resAdd = new HttpResponseMessage();
                    HttpResponseMessage resUpdate = new HttpResponseMessage();
                    BravoResponseModel responseAddModel = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel = new BravoResponseModel();

                    // 3. Call Bravo to SYNC.
                    if (listAdd.Count > 0)
                    {
                        resAdd = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSReceiptDataSyncAdd", listAdd, loginResponse.TokenKey);
                        responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "GetListReceiptAllInToAccountant",
                            FuncPartner = "EFMSReceiptDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel),
                            Major = "Nghiệp Vụ Phiếu Thu",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    if (listUpdate.Count > 0)
                    {
                        resUpdate = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSReceiptDataSyncUpdate", listUpdate, loginResponse.TokenKey);
                        responseUpdateModel = await resUpdate.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "GetListReceiptAllInToAccountant",
                            FuncPartner = "EFMSReceiptDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel),
                            Major = "Nghiệp Vụ Phiếu Thu",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // 4. Update STATUS
                    if (responseAddModel.Success == "1"
                        || responseUpdateModel.Success == "1")
                    {
                        HandleState hs = accountingService.SyncListReceiptToAccountant(ids, receiptSyncs);
                        string message = HandleError.GetMessage(hs, Crud.Update);
                        ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = ids };
                        if (!hs.Success)
                        {
                            result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString(), Data = ids };
                            return BadRequest(result);
                        }
                        return Ok(result);
                    }
                    else
                    {
                        ResultHandle result = new ResultHandle { Status = false, Message = responseAddModel.Msg + "\n" + responseUpdateModel.Msg, Data = ids };
                        return BadRequest(result);
                    }
                }
                new LogHelper("eFMS_SYNC_LOG", loginResponse.ToString());
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_SYNC_LOG", ex.ToString());
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
        }

        [HttpGet("CheckCdNoteSynced/{id}")]
        public IActionResult CheckCdNoteSynced(Guid id)
        {
            var result = accountingService.CheckCdNoteSynced(id);
            return Ok(result);
        }

        [HttpGet("CheckSoaSynced/{id}")]
        public IActionResult CheckSoaSynced(string id)
        {
            string messageError = accountingService.CheckSoaSynced(id);
            if(messageError.Length > 0)
            {
                var _result = new ResultHandle { Status = false, Message = messageError };
                return BadRequest(_result);
            }

            return Ok(new ResultHandle { Status = true  });
        }

        [HttpGet("CheckVoucherSynced/{id}")]
        public IActionResult CheckVoucherSynced(Guid id)
        {
            var result = accountingService.CheckVoucherSynced(id);
            return Ok(result);
        }

        [HttpPut("UploadAttachedFiles/{folder}/{id}")]
        [Authorize]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files, Guid id, string folder, string child = null)
        {
            FileUploadModel model = new FileUploadModel
            {
                Files = files,
                FolderName = folder,
                Id = id,
                Child = child

            };
            HandleState hs = await sysFileService.UploadFiles(model);
            if (hs.Success)
            {
                return Ok(new ResultHandle { Message = "Upload File Successfully", Status = true});
            }
            return BadRequest(hs);
        }

        [HttpGet("GetAttachedFiles/{folder}/{id}")]
        public IActionResult GetFiles(string folder, Guid id, string child = null)
        {
            List<SysImageModel> result = sysFileService.Get(x => x.Folder == folder && x.ObjectId == id.ToString() && x.ChildId == child).OrderByDescending(x => x.DateTimeCreated).ToList();
            return Ok(result);
        }

        [HttpDelete("DeleteAttachedFile/{folder}/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAttachedFile(string folder, Guid id)
        {
            HandleState hs = await sysFileService.DeleteFile(folder,id);
            if(hs.Success)
            {
                return Ok(new ResultHandle { Message = "Delete File Successfully", Status = true });
            }
            return BadRequest(hs);
        }

        [HttpPost("DowloadAllFileAttached")]
        //[Authorize]
        public async Task<ActionResult> DowloadAllFileAttached(FileDowloadZipModel m)
        {
            //Return memoryStream res.message
            var res = await sysFileService.CreateFileZip(m);
            if (res.Success)
                return File((MemoryStream)res.Message, "application/zip", m.FileName);
            return BadRequest(res);
        }

        [HttpPost("TestSendMail")]
        public ActionResult TestSendMail(string subject, string body, [FromBody] List<string> emails)
        {
            var listcc = new List<string> {"lynne.loc@itlvn.com","alex.phuong@itlvn.com", "paulchen.bao@itlvn.com" };
            var sendSuccess = SendMail.Send(subject, body, emails, null, listcc, null);
            return Ok( new { sendSuccess } );
        }
    }
}