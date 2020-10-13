using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
                        resAdd = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSInvoiceDataSyncAdd", listAdd, loginResponse.TokenKey);
                        responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncSettlementToAccountantSystem",
                            FuncPartner = "EFMSInvoiceDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel),
                            Major = "Nghiệp Vụ Chi Phí"
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    if (listUpdate.Count > 0)
                    {
                        resUpdate = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSInvoiceDataSyncUpdate", listUpdate, loginResponse.TokenKey);
                        responseUpdateModel = await resUpdate.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncSettlementToAccountantSystem",
                            FuncPartner = "EFMSInvoiceDataSyncUpdate",
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
                    List<BravoVoucherModel> list = accountingService.GetListVoucherToSyncBravo(Ids);

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
        public async Task<IActionResult> SyncListCdNoteToAccountant(List<RequestGuidTypeListModel> request)
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
                    List<Guid> ids = request.Select(x => x.Id).ToList();

                    List<Guid> IdsAdd_NVHD = request.Where(x => x.Action == ACTION.ADD && (x.Type == "DEBIT" || x.Type == "INVOICE")).Select(x => x.Id).ToList();
                    List<Guid> IdsUpdate_NVHD = request.Where(x => x.Action == ACTION.UPDATE && (x.Type == "DEBIT" || x.Type == "INVOICE")).Select(x => x.Id).ToList();
                    List<Guid> IdsAdd_NVCP = request.Where(x => x.Action == ACTION.ADD && x.Type == "CREDIT").Select(x => x.Id).ToList();
                    List<Guid> IdsUpdate_NVCP = request.Where(x => x.Action == ACTION.UPDATE && x.Type == "CREDIT").Select(x => x.Id).ToList();

                    List<SyncModel> listAdd_NVHD = (IdsAdd_NVHD.Count > 0) ? accountingService.GetListCdNoteToSync(IdsAdd_NVHD) : new List<SyncModel>();
                    List<SyncModel> listUpdate_NVHD = (IdsUpdate_NVHD.Count > 0) ? accountingService.GetListCdNoteToSync(IdsUpdate_NVHD) : new List<SyncModel>();
                    List<SyncModel> listAdd_NVCP = (IdsAdd_NVCP.Count > 0) ? accountingService.GetListCdNoteToSync(IdsAdd_NVCP) : new List<SyncModel>();
                    List<SyncModel> listUpdate_NVCP = (IdsUpdate_NVCP.Count > 0) ? accountingService.GetListCdNoteToSync(IdsUpdate_NVCP) : new List<SyncModel>();

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

                    if (listAdd_NVCP.Count > 0)
                    {
                        resAdd_NVCP = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncAdd", listAdd_NVCP, loginResponse.TokenKey);
                        responseAddModel_NVCP = await resAdd_NVCP.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListCdNoteToAccountant",
                            FuncPartner = "EFMSVoucherDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd_NVCP),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel_NVCP),
                            Major = "Nghiệp Vụ Chi Phí"
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    if (listUpdate_NVCP.Count > 0)
                    {
                        resUpdate_NVCP = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncUpdate", listUpdate_NVCP, loginResponse.TokenKey);
                        responseUpdateModel_NVCP = await resUpdate_NVCP.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListCdNoteToAccountant",
                            FuncPartner = "EFMSVoucherDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate_NVCP),
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
        public async Task<IActionResult> SyncListSoaToAccountant(List<RequestIntTypeListModel> request)
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
                    List<int> ids = request.Select(x => x.Id).ToList();

                    List<int> IdsAdd_NVHD = request.Where(x => x.Action == ACTION.ADD && x.Type?.ToUpper() == "DEBIT").Select(x => x.Id).ToList();
                    List<int> IdsUpdate_NVHD = request.Where(x => x.Action == ACTION.UPDATE && x.Type?.ToUpper() == "DEBIT").Select(x => x.Id).ToList();
                    List<int> IdsAdd_NVCP = request.Where(x => x.Action == ACTION.ADD && x.Type?.ToUpper() == "CREDIT").Select(x => x.Id).ToList();
                    List<int> IdsUpdate_NVCP = request.Where(x => x.Action == ACTION.UPDATE && x.Type?.ToUpper() == "CREDIT").Select(x => x.Id).ToList();

                    List<SyncModel> listAdd_NVHD = accountingService.GetListSoaToSync(IdsAdd_NVHD);
                    List<SyncModel> listUpdate_NVHD = accountingService.GetListSoaToSync(IdsUpdate_NVHD);
                    List<SyncModel> listAdd_NVCP = accountingService.GetListSoaToSync(IdsAdd_NVCP);
                    List<SyncModel> listUpdate_NVCP = accountingService.GetListSoaToSync(IdsUpdate_NVCP);

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

                    if (listAdd_NVCP.Count > 0)
                    {
                        resAdd_NVCP = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncAdd", listAdd_NVCP, loginResponse.TokenKey);
                        responseAddModel_NVCP = await resAdd_NVCP.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListSoaToAccountant",
                            FuncPartner = "EFMSVoucherDataSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd_NVCP),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel_NVCP),
                            Major = "Nghiệp Vụ Chi Phí"
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    if (listUpdate_NVCP.Count > 0)
                    {
                        resUpdate_NVCP = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncUpdate", listUpdate_NVCP, loginResponse.TokenKey);
                        responseUpdateModel_NVCP = await resUpdate_NVCP.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncListSoaToAccountant",
                            FuncPartner = "EFMSVoucherDataSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate_NVCP),
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
    }
}