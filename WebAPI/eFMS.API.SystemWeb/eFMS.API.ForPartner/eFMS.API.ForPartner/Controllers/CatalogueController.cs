using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Infrastructure.Extensions;
using eFMS.API.ForPartner.Infrastructure.Filters;
using eFMS.API.ForPartner.Infrastructure.Middlewares;
using eFMS.API.ForPartner.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;

namespace eFMS.API.ForPartner.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatalogueController : ControllerBase
    {
        private readonly ICatPartnerBankService _catPartnerBankService;
        private readonly IStringLocalizer _stringLocalizer;
        private readonly IActionFuncLogService actionFuncLogService;
        private readonly IContextBase<CatPartner> catPartnerRepository;
        private readonly ICurrentUser _currentUser;
        public CatalogueController(ICatPartnerBankService catPartnerBankService, IStringLocalizer<LanguageSub> stringLocalizer, IActionFuncLogService actionFuncLog, IContextBase<CatPartner> catPartnerRepo, ICurrentUser currentUser)
        {
            _catPartnerBankService = catPartnerBankService;
            _stringLocalizer = stringLocalizer;
            actionFuncLogService = actionFuncLog;
            catPartnerRepository = catPartnerRepo;
            _currentUser = currentUser;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [APIKeyAuth]
        [ValidateModel]
        [Route("BankInfoSyncUpdateStatus")]
        public async Task<IActionResult> UpdateBankInfoSyncStatus(BankStatusUpdateModel model, [Required] string apiKey, [Required] string hash)
        {
            if (!ModelState.IsValid) return BadRequest();
            var stopwatch = new Stopwatch();
            stopwatch.Restart();
            if (!_catPartnerBankService.ValidateApiKey(apiKey))
            {
                return new CustomUnauthorizedResult(ForPartnerConstants.API_KEY_INVALID);
            }
            if (!_catPartnerBankService.ValidateHashString(model, apiKey, hash))
            {
                return new CustomUnauthorizedResult(ForPartnerConstants.HASH_INVALID);
            }

            ICurrentUser currentUser = await _catPartnerBankService.SetCurrentUserPartner(_currentUser, apiKey);
            currentUser.UserID = _currentUser.UserID;
            currentUser.GroupId = _currentUser.GroupId;
            currentUser.DepartmentId = _currentUser.DepartmentId;
            currentUser.OfficeID = _currentUser.OfficeID;
            currentUser.CompanyID = _currentUser.CompanyID;
            currentUser.Action = "UpdateBankInfoSyncStatus";

            var hs = await _catPartnerBankService.UpdatePartnerBankInfoSyncStatus(model);
            string _message = hs.Success ? "Cập nhật thông tin ngân hàng thành công." : string.Format("{0}. Cập nhật thông tin ngân hàng thất bại.", hs.Message.ToString());
            var result = new ResultHandle { Status = hs.Success, Message = _message, Data = model };

            Response.OnCompleted(async () =>
            {
                string _objectRequest = JsonConvert.SerializeObject(model);
                var _endDateProgress = DateTime.Now;
                #region -- Ghi Log --
                string _funcLocal = "UpdateBankInfoSyncStatus";
                string _major = "Cập nhật thông tin ngân hàng";
                var hsAddLog = actionFuncLogService.AddActionFuncLog(_funcLocal, _objectRequest, JsonConvert.SerializeObject(result), _major, DateTime.Now, DateTime.Now + stopwatch.Elapsed);
                #endregion
            });

            if (!hs.Success)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
