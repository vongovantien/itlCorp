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
using eFMS.API.Catalogue.DL.Models.CatalogueBank;
using eFMS.API.Common.Helpers;
using ITL.NetCore.Common;
using System.Net.Http;
using Microsoft.Extensions.Options;
using eFMS.API.Accounting.DL.IService;

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
        IOptions<ESBUrl> webUrl;    
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
            ICurrentUser currUser, IHostingEnvironment hostingEnvironment, IActionFuncLogService actionFuncLog)
        {
            stringLocalizer = localizer;
            catPartnerBankService = service;
            currentUser = currUser;
            _hostingEnvironment = hostingEnvironment;
            actionFuncLogService = actionFuncLogService;
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
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("SyncBankInfoToAccountantSystem")]
        [Authorize]
        public async Task<IActionResult> SyncBankAccountToAccountantSystem(Guid bankId, ACTION action)
        {
            if (!ModelState.IsValid) return BadRequest();
            try
            {
                HandleState hs = new HandleState();
                var loginInfo = new BravoLoginModel
                {
                    UserName = "bravo",
                    Password = "br@vopro"
                };

                HttpResponseMessage responseFromApi = await HttpClientService.PostAPI(_webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
                BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;

                HttpResponseMessage response = new HttpResponseMessage();
                BravoResponseModel responseModel = new BravoResponseModel();

                if (loginResponse.Success == "1")
                {
                    var requestModel = await catPartnerBankService.GetModelBankInfoToSync(bankId);

                    switch (action)
                    {
                        case ACTION.ADD:
                            response = await HttpClientService.PostAPI(_webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSBankInfoSyncAdd", requestModel, loginResponse.TokenKey);
                            responseModel = await response.Content.ReadAsAsync<BravoResponseModel>();
                            break;
                        case ACTION.UPDATE:
                            response = await HttpClientService.PostAPI(_webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSBankInfoSyncUpdate", requestModel, loginResponse.TokenKey);
                            responseModel = await response.Content.ReadAsAsync<BravoResponseModel>();
                            break;
                        default:
                            break;
                    }
                }

                if (responseModel.Success == "1")
                {
                    var catBankModel = await catBankService.Get(x => x.Id == bankId).FirstOrDefaultAsync();
                    catBankModel.ApproveStatus = "Processing";
                    hs = catBankService.Update(catBankModel, x => x.Id == bankId);

                    ResultHandle result = new ResultHandle { Status = true, Message = "Sync Data to Accountant System Successful", Data = responseModel };
                    return Ok(result);
                }
                else
                {
                    ResultHandle result = new ResultHandle { Status = false, Message = "Sync Data Fail", Data = responseModel };
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {

                new LogHelper("eFMS_SYNC_LOG", ex.ToString());
                return BadRequest(new ResultHandle { Message = "Sync fail" });
            }
        }
    }
}