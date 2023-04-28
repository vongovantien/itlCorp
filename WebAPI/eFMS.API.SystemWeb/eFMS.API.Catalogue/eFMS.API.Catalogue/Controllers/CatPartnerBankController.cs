using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Linq;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common.Infrastructure.Common;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using eFMS.API.Common.Helpers;
using System.Net.Http;
using Microsoft.Extensions.Options;
using eFMS.API.Accounting.DL.IService;
using System.Collections.Generic;
using Newtonsoft.Json;
using eFMS.API.Catalogue.DL.Models.Catalogue;

namespace eFMS.API.Catalogue.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatPartnerBankController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatPartnerBankService catPartnerBankService;
        private readonly ICurrentUser currentUser;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOptions<ESBUrl> _webUrl;
        private readonly IActionFuncLogService actionFuncLogService;
        private readonly BravoLoginModel loginInfo;
        /// <summary>
        ///
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface ICatPartnerBankService</param>
        /// <param name="currUser">inject interface ICurrentUser</param>
        /// <param name="hostingEnvironment">inject interface IHostingEnvironment</param>
        public CatPartnerBankController(IStringLocalizer<LanguageSub> localizer, ICatPartnerBankService service,
            ICurrentUser currUser, IHostingEnvironment hostingEnvironment, IActionFuncLogService actionFuncLog, IOptions<ESBUrl> webUrl)
        {
            stringLocalizer = localizer;
            catPartnerBankService = service;
            currentUser = currUser;
            _hostingEnvironment = hostingEnvironment;
            actionFuncLogService = actionFuncLog;
            _webUrl = webUrl;
            loginInfo = new BravoLoginModel
            {
                UserName = "bravo",
                Password = "br@vopro"
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var data = await catPartnerBankService.Get().OrderBy(x => x.DatetimeCreated).ToListAsync();
            return Ok(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetByPartner")]
        public IActionResult GetByPartner(Guid partnerId)
        {
            var data = catPartnerBankService.GetByPartner(partnerId);
            return Ok(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDetail")]
        public async Task<IActionResult> GetDetail(Guid Id)
        {
            var data = await catPartnerBankService.GetDetail(Id);
            return Ok(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddNew")]
        [Authorize]
        public async Task<IActionResult> AddNew(CatPartnerBankModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var messageError = await catPartnerBankService.CheckExistedPartnerBank(model);
            if (!string.IsNullOrEmpty(messageError))
            {
                return BadRequest(new ResultHandle { Status = false, Message = messageError });
            }
            var hs = await catPartnerBankService.AddNew(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        [Authorize]
        public async Task<IActionResult> Update(CatPartnerBankModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var messageError = await catPartnerBankService.CheckExistedPartnerBank(model);
            if (!string.IsNullOrEmpty(messageError))
            {
                return BadRequest(new ResultHandle { Status = false, Message = messageError });
            }
            var hs = await catPartnerBankService.Update(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete("{Id}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var hs = await catPartnerBankService.Delete(Id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("ReviseBankInformation")]
        [Authorize]
        public async Task<IActionResult> ReviseBankInformation(Guid bankId)
        {
            var hs = await catPartnerBankService.ReviseBankInformation(bankId);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("SyncBankInfoToAccountantSystem")]
        [Authorize]
        public async Task<IActionResult> SyncBankAccountToAccountantSystem(List<RequestGuidListModel> request)
        {
            var _startDateProgress = DateTime.Now;
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                // 1. Login
                HttpResponseMessage responseFromApi = await HttpClientService.PostAPI(_webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
                BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;

                if (loginResponse.Success == "1")
                {
                    // 2. Get Data To Sync.
                    List<Guid> ids = request.Select(x => x.Id).ToList();

                    List<Guid> idsAdd = request.Where(x => x.Action == ACTION.ADD).Select(x => x.Id).ToList();
                    List<Guid> idsUpdate = request.Where(x => x.Action == ACTION.UPDATE).Select(x => x.Id).ToList();

                    List<BankSyncModel> listAdd = (idsAdd.Count > 0) ? await catPartnerBankService.GetListPartnerBankInfoToSync(idsAdd) : new List<BankSyncModel>();
                    List<BankSyncModel> listUpdate = (idsUpdate.Count > 0) ? await catPartnerBankService.GetListPartnerBankInfoToSync(idsUpdate) : new List<BankSyncModel>();

                    HttpResponseMessage resAdd = new HttpResponseMessage();
                    HttpResponseMessage resUpdate = new HttpResponseMessage();
                    BravoResponseModel responseAddModel = new BravoResponseModel();
                    BravoResponseModel responseUpdateModel = new BravoResponseModel();

                    // 3. Call Bravo to SYNC.
                    if (listAdd.Count > 0)
                    {
                        resAdd = await HttpClientService.PostAPI(_webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSBankInfoSyncAdd", listAdd, loginResponse.TokenKey);
                        responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncBankAccountToAccountantSystem",
                            FuncPartner = "EFMSBankInfoSyncAdd",
                            ObjectRequest = JsonConvert.SerializeObject(listAdd),
                            ObjectResponse = JsonConvert.SerializeObject(responseAddModel),
                            Major = "Tạo mới thông tin ngân hàng",
                            StartDateProgress = _startDateProgress,
                            EndDateProgress = DateTime.Now
                        };
                        var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                        #endregion
                    }

                    if (listUpdate.Count > 0)
                    {
                        resUpdate = await HttpClientService.PostAPI(_webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSBankInfoSyncUpdate", listUpdate, loginResponse.TokenKey);
                        responseUpdateModel = await resUpdate.Content.ReadAsAsync<BravoResponseModel>();

                        #region -- Ghi Log --
                        var modelLog = new SysActionFuncLogModel
                        {
                            FuncLocal = "SyncBankAccountToAccountantSystem",
                            FuncPartner = "EFMSBankInfoSyncUpdate",
                            ObjectRequest = JsonConvert.SerializeObject(listUpdate),
                            ObjectResponse = JsonConvert.SerializeObject(responseUpdateModel),
                            Major = "Cập nhật thông tin ngân hàng",
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
                        var hs = await catPartnerBankService.UpdateByStatus(ids, "Processing");
                        ResultHandle result = new ResultHandle { Status = true, Message = stringLocalizer["MSG_SYNC_SUCCESS"].Value, Data = null };
                        return Ok(result);
                    }
                    else
                    {
                        if (responseAddModel.Success == null || responseUpdateModel.Success == null)
                        {
                            return BadRequest(new ResultHandle { Status = true, Message = stringLocalizer["MSG_SYNC_FAIL"].Value, Data = null });
                        }
                        ResultHandle result = new ResultHandle { Status = false, Message = responseAddModel.Msg + "\n" + responseUpdateModel.Msg, Data = ids };
                        return BadRequest(result);
                    }
                }
                return BadRequest(new ResultHandle { Status = true, Message = stringLocalizer["MSG_SYNC_FAIL"].Value, Data = null });
            }
            catch (Exception)
            {
                return BadRequest(new ResultHandle { Status = true, Message = stringLocalizer["MSG_SYNC_FAIL"].Value, Data = null });
            }
        }
    }
}