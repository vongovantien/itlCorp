using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.Infrastructure.Http;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Accounting;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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

        private readonly BravoLoginModel loginInfo;

        public AccountingController(
            IStringLocalizer<LanguageSub> localizer,
            IAccountingService service,
            IOptions<ESBUrl> appSettings,
            IActionFuncLogService actionFuncLog
            )
        {
            stringLocalizer = localizer;
            accountingService = service;
            webUrl = appSettings;
            actionFuncLogService = actionFuncLog;

            loginInfo = new BravoLoginModel
            {
                UserName = "bravo",
                Password = "br@vopro"
            };
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

        [HttpPost("GetListInvoicePaymentToSync")]
        [Authorize]
        public async Task<IActionResult> GetListInvoicePaymentToSync(List<RequestGuidListModel> request)
        {
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
                            Major = "Nghiệp Vụ Phiếu Thu"
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
                            Major = "Nghiệp Vụ Phiếu Thu"
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
        public async Task<IActionResult> GetListObhPaymentToSync(List<RequestIntListModel> request)
        {
            try
            {
                // 1. Login
                HttpResponseMessage responseFromApi = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
                BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;

                if (loginResponse.Success == "1")
                {
                    // 2. Get Data To Sync.
                    List<int> ids = request.Select(x => x.Id).ToList();

                    List<int> idsAdd = request.Where(x => x.Action == ACTION.ADD).Select(x => x.Id).ToList();
                    List<int> idsUpdate = request.Where(x => x.Action == ACTION.UPDATE).Select(x => x.Id).ToList();

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
                            Major = "Nghiệp Vụ Phiếu Thu"
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
                            Major = "Nghiệp Vụ Phiếu Thu"
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
        public async Task<IActionResult> SyncAdvanceToAccountantSystem(List<RequestGuidListModel> request)
        {
            if (!ModelState.IsValid) return BadRequest();

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
                            Major = "Nghiệp Vụ Tạm Ứng"
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    if (listUpdate.Count > 0)
                    {
                        resUpdate = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSAdvanceSyncUpdate", listUpdate, loginResponse.TokenKey);
                        responseUpdateModel = await resUpdate.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncAdvanceToAccountantSystem",
                            FuncPartner = "EFMSAdvanceSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel),
                            Major = "Nghiệp Vụ Tạm Ứng"
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
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
            catch (Exception)
            {
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
        }

        [HttpPut("SyncSettlementToAccountantSystem")]
        [Authorize]
        public async Task<IActionResult> SyncSettlementToAccountantSystem(List<RequestGuidListModel> request)
        {
            if (!ModelState.IsValid) return BadRequest();

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
                        resAdd = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncAdd", listAdd, loginResponse.TokenKey);
                        responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncSettlementToAccountantSystem",
                            FuncPartner = "EFMSVoucherDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel),
                            Major = "Nghiệp Vụ Chi Phí"
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
                            FuncLocal = "SyncSettlementToAccountantSystem",
                            FuncPartner = "EFMSVoucherDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel),
                            Major = "Nghiệp Vụ Chi Phí"
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
                        return Ok(result);
                    }
                    return BadRequest(new ResultHandle { Message = responseAddModel.Msg + "\n" + responseUpdateModel.Msg });
                }
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
            catch (Exception)
            {
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
        }

        [HttpPut("SyncVoucherToAccountantSystem")]
        [Authorize]
        public async Task<IActionResult> SyncVoucherToAccountantSystem(List<RequestGuidListModel> request)
        {
            if (!ModelState.IsValid) return BadRequest();

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
                            Major = "Nghiệp Vụ Chi Phí"
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
                            Major = "Nghiệp Vụ Chi Phí"
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
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
            catch (Exception)
            {
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
        }

        [HttpPut("SyncListCdNoteToAccountant")]
        [Authorize]
        public async Task<IActionResult> SyncListCdNoteToAccountant(List<RequestGuidTypeListModel> requests)
        {
            if (!ModelState.IsValid) return BadRequest();

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
                    //Credit Note có currency # currency local hoặc trong list charge của Credit Note có tồn tại 1 currency # currency local >> Send mail & Notification đến Department Accountant
                    accountingService.SendMailAndPushNotificationToAccountant(listAdd_NVCP_DiffCurrLocal);
                    accountingService.SendMailAndPushNotificationToAccountant(listUpdate_NVCP_DiffCurrLocal);

                    List<Guid> ids = requests.Where(w =>
                       !listAdd_NVCP_DiffCurrLocal.Select(se => se.Stt).Contains(w.Id.ToString())
                    && !listUpdate_NVCP_DiffCurrLocal.Select(se => se.Stt).Contains(w.Id.ToString())).Select(x => x.Id).ToList();

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
                            Major = "Nghiệp Vụ Hóa Đơn"
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
                            Major = "Nghiệp Vụ Hóa Đơn"
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // ADD Voucher: Chỉ send qua Bravo những Credit Note có currency là VND và currency của từng phí của Credit Note đó là VND
                    if (listAdd_NVCP_SameCurrLocal.Count > 0)
                    {
                        resAdd_NVCP = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncAdd", listAdd_NVCP_SameCurrLocal, loginResponse.TokenKey);
                        responseAddModel_NVCP = await resAdd_NVCP.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListCdNoteToAccountant",
                            FuncPartner = "EFMSVoucherDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd_NVCP_SameCurrLocal),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel_NVCP),
                            Major = "Nghiệp Vụ Chi Phí"
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // ADD Voucher: Chỉ send qua Bravo những Credit Note có currency là VND và currency của từng phí của Credit Note đó là VND
                    if (listUpdate_NVCP_SameCurrLocal.Count > 0)
                    {
                        resUpdate_NVCP = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncUpdate", listUpdate_NVCP_SameCurrLocal, loginResponse.TokenKey);
                        responseUpdateModel_NVCP = await resUpdate_NVCP.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListCdNoteToAccountant",
                            FuncPartner = "EFMSVoucherDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate_NVCP_SameCurrLocal),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel_NVCP),
                            Major = "Nghiệp Vụ Chi Phí"
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
                            result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString(), Data = ids };
                            return BadRequest(result);
                        }
                        return Ok(result);
                    }
                    else
                    {
                        var result = new ResultHandle { Status = false, Message = responseAddModel_NVHD.Msg + "\n" + responseUpdateModel_NVHD.Msg + "\n" + responseAddModel_NVCP.Msg + "\n" + responseUpdateModel_NVCP.Msg, Data = ids };
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

        [HttpPut("SyncListSoaToAccountant")]
        [Authorize]
        public async Task<IActionResult> SyncListSoaToAccountant(List<RequestIntTypeListModel> requests)
        {
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                // 1. Login
                HttpResponseMessage responseFromApi = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
                BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;

                if (loginResponse.Success == "1")
                {
                    // 2. Get Data To Sync.                    

                    List<int> IdsAdd_NVHD = requests.Where(x => x.Action == ACTION.ADD && x.Type?.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_DEBIT).Select(x => x.Id).ToList();
                    List<int> IdsUpdate_NVHD = requests.Where(x => x.Action == ACTION.UPDATE && x.Type?.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_DEBIT).Select(x => x.Id).ToList();
                    List<RequestIntTypeListModel> IdsAdd_NVCP = requests.Where(x => x.Action == ACTION.ADD && x.Type?.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_CREDIT).ToList();
                    List<RequestIntTypeListModel> IdsUpdate_NVCP = requests.Where(x => x.Action == ACTION.UPDATE && x.Type?.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_CREDIT).ToList();
                    
                    List<SyncModel> listAdd_NVHD = accountingService.GetListSoaToSync(IdsAdd_NVHD);
                    List<SyncModel> listUpdate_NVHD = accountingService.GetListSoaToSync(IdsUpdate_NVHD);
                    List<SyncCreditModel> listAdd_NVCP = accountingService.GetListSoaCreditToSync(IdsAdd_NVCP);
                    List<SyncCreditModel> listUpdate_NVCP = accountingService.GetListSoaCreditToSync(IdsUpdate_NVCP);

                    List<SyncCreditModel> listAdd_NVCP_SameCurrLocal = listAdd_NVCP.Where(x => x.CurrencyCode == AccountingConstants.CURRENCY_LOCAL && x.Details.Where(w => w.CurrencyCode == AccountingConstants.CURRENCY_LOCAL).Count() == x.Details.Count()).ToList();
                    List<SyncCreditModel> listUpdate_NVCP_SameCurrLocal = listUpdate_NVCP.Where(x => x.CurrencyCode == AccountingConstants.CURRENCY_LOCAL && x.Details.Where(w => w.CurrencyCode == AccountingConstants.CURRENCY_LOCAL).Count() == x.Details.Count()).ToList();

                    List<SyncCreditModel> listAdd_NVCP_DiffCurrLocal = listAdd_NVCP.Where(x => x.CurrencyCode != AccountingConstants.CURRENCY_LOCAL || x.Details.Any(w => w.CurrencyCode != AccountingConstants.CURRENCY_LOCAL)).ToList();
                    List<SyncCreditModel> listUpdate_NVCP_DiffCurrLocal = listUpdate_NVCP.Where(x => x.CurrencyCode != AccountingConstants.CURRENCY_LOCAL || x.Details.Any(w => w.CurrencyCode != AccountingConstants.CURRENCY_LOCAL)).ToList();
                    //SOA(Credit) có currency # currency local hoặc trong list charge của SOA có tồn tại 1 currency # currency local >> Send mail & Notification đến Department Accountant
                    accountingService.SendMailAndPushNotificationToAccountant(listAdd_NVCP_DiffCurrLocal);
                    accountingService.SendMailAndPushNotificationToAccountant(listUpdate_NVCP_DiffCurrLocal);

                    List<int> ids = requests.Where(w => 
                       !listAdd_NVCP_DiffCurrLocal.Select(se => se.Stt).Contains(w.Id.ToString()) 
                    && !listUpdate_NVCP_DiffCurrLocal.Select(se => se.Stt).Contains(w.Id.ToString())).Select(x => x.Id).ToList();

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
                            Major = "Nghiệp Vụ Hóa Đơn"
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
                            Major = "Nghiệp Vụ Hóa Đơn"
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // ADD Voucher: Chỉ send qua Bravo những SOA(credit) có currency là VND và currency của từng phí của SOA đó là VND
                    if (listAdd_NVCP_SameCurrLocal.Count > 0)
                    {
                        resAdd_NVCP = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncAdd", listAdd_NVCP_SameCurrLocal, loginResponse.TokenKey);
                        responseAddModel_NVCP = await resAdd_NVCP.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListSoaToAccountant",
                            FuncPartner = "EFMSVoucherDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd_NVCP_SameCurrLocal),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel_NVCP),
                            Major = "Nghiệp Vụ Chi Phí"
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    // UDPATE Voucher: Chỉ send qua Bravo những SOA(credit) có currency là VND và currency của từng phí của SOA đó là VND
                    if (listUpdate_NVCP_SameCurrLocal.Count > 0)
                    {
                        resUpdate_NVCP = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncUpdate", listUpdate_NVCP_SameCurrLocal, loginResponse.TokenKey);
                        responseUpdateModel_NVCP = await resUpdate_NVCP.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListSoaToAccountant",
                            FuncPartner = "EFMSVoucherDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate_NVCP_SameCurrLocal),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel_NVCP),
                            Major = "Nghiệp Vụ Chi Phí"
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
                            result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString(), Data = ids };
                            return BadRequest(result);
                        }
                        return Ok(result);
                    }
                    else
                    {
                        var result = new ResultHandle { Status = false, Message = responseAddModel_NVHD.Msg + "\n" + responseUpdateModel_NVHD.Msg + "\n" + responseAddModel_NVCP.Msg + "\n" + responseUpdateModel_NVCP.Msg, Data = ids };
                        return BadRequest(result);
                    }
                }
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
            catch (Exception)
            {
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
        public IActionResult GetListSOADebit(List<RequestIntTypeListModel> request)
        {
            List<int> Ids = request.Where(x => x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT).Select(x => x.Id).ToList();
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
                            Major = "Nghiệp Vụ Phiếu Thu"
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
                            Major = "Nghiệp Vụ Phiếu Thu"
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
        
    }     
}