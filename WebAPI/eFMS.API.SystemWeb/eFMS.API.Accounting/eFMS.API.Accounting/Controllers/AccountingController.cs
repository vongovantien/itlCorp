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

        private readonly BravoLoginModel loginInfo;

        public AccountingController(
            IStringLocalizer<LanguageSub> localizer,
            IAccountingService service,
            IOptions<ESBUrl> appSettings
            )
        {
            stringLocalizer = localizer;
            accountingService = service;
            webUrl = appSettings;

            loginInfo = new BravoLoginModel
            {
                UserName = "bravo",
                Password = "br@vopro"
            };
        }

        [HttpPost("GetListCdNoteToSync")]
        public IActionResult GetListCdNoteToSync(List<Guid> ids, string type)
        {
            var result = accountingService.GetListCdNoteToSync(ids, type);
            return Ok(result);
        }

        [HttpPost("GetListSoaToSync")]
        public IActionResult GetListSoaToSync(List<int> ids, string type)
        {
            var result = accountingService.GetListSoaToSync(ids, type);
            return Ok(result);
        }

        [HttpPost("GetListInvoicePaymentToSync")]
        public IActionResult GetListInvoicePaymentToSync(List<Guid> ids)
        {
            var result = accountingService.GetListInvoicePaymentToSync(ids);
            return Ok(result);
        }

        [HttpPost("GetListObhPaymentToSync")]
        public IActionResult GetListObhPaymentToSync(List<int> ids)
        {
            var result = accountingService.GetListObhPaymentToSync(ids);
            return Ok(result);
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
                    List<BravoAdvanceModel> list = accountingService.GetListAdvanceToSyncBravo(Ids);

                    // 3. Call Bravo to SYNC.
                    List<Guid> IdsAdd = request.Where(action => action.Action == ACTION.ADD).Select(x => x.Id).ToList();
                    List<Guid> IdsUpdate = request.Where(action => action.Action == ACTION.UPDATE).Select(x => x.Id).ToList();

                    HttpResponseMessage resAdd = new HttpResponseMessage();
                    HttpResponseMessage resUpdate = new HttpResponseMessage();
                    BravoResponseModel responseAddModel = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel = new BravoResponseModel();

                    if (IdsAdd.Count > 0)
                    {
                        resAdd = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSAdvanceSyncAdd", list, loginResponse.TokenKey);
                        responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();
                    }

                    if (IdsUpdate.Count > 0)
                    {
                        resUpdate = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSAdvanceSyncUpdate", list, loginResponse.TokenKey);
                        responseUpdateModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();
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
                    return BadRequest(responseAddModel.Msg + "\n" + responseUpdateModel.Msg );

                }
                return BadRequest("Sync fail");
            }
            catch (Exception)
            {
                return BadRequest("Sync fail");
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
                    List<BravoSettlementModel> list = accountingService.GetListSettlementToSyncBravo(Ids);

                    List<Guid> IdsAdd = request.Where(action => action.Action == ACTION.ADD).Select(x => x.Id).ToList();
                    List<Guid> IdsUpdate = request.Where(action => action.Action == ACTION.UPDATE).Select(x => x.Id).ToList();

                    HttpResponseMessage resAdd = new HttpResponseMessage();
                    HttpResponseMessage resUpdate = new HttpResponseMessage();
                    BravoResponseModel responseAddModel = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel = new BravoResponseModel();

                    // 3. Call Bravo to SYNC.
                    if (IdsAdd.Count > 0)
                    {
                        resAdd = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSAdvanceSyncAdd", list, loginResponse.TokenKey);
                        responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();
                    }

                    if (IdsUpdate.Count > 0)
                    {
                        resUpdate = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSAdvanceSyncUpdate", list, loginResponse.TokenKey);
                        responseUpdateModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();
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
                    return BadRequest(responseAddModel.Msg + "\n" + responseUpdateModel.Msg);
                }
                return BadRequest("Sync fail");
            }
            catch (Exception)
            {
                return BadRequest("Sync fail");
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

                    HttpResponseMessage resAdd = new HttpResponseMessage();
                    HttpResponseMessage resUpdate = new HttpResponseMessage();
                    BravoResponseModel responseAddModel = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel = new BravoResponseModel();

                    // 3. Call Bravo to SYNC.
                    if (IdsAdd.Count > 0)
                    {
                        resAdd = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncAdd", list, loginResponse.TokenKey);
                        responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();
                    }

                    if (IdsUpdate.Count > 0)
                    {
                        resUpdate = await HttpService.PostAPI(webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSVoucherDataSyncUpdate", list, loginResponse.TokenKey);
                        responseUpdateModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();
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
                    return BadRequest(responseAddModel.Msg + "\n" + responseUpdateModel.Msg);
                }
                return BadRequest("Sync fail");
            }
            catch (Exception)
            {
                return BadRequest("Sync fail");
            }

        }

        [HttpPut("SyncListCdNoteToAccountant")]
        [Authorize]
        public IActionResult SyncListCdNoteToAccountant(List<Guid> ids)
        {
            if (!ModelState.IsValid) return BadRequest();

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

        [HttpPut("SyncListSoaToAccountant")]
        [Authorize]
        public IActionResult SyncListSoaToAccountant(List<int> ids)
        {
            if (!ModelState.IsValid) return BadRequest();

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
    }
}