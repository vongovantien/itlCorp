using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.DL.Service;
using eFMS.API.ForPartner.Infrastructure.Extensions;
using eFMS.API.ForPartner.Infrastructure.Middlewares;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
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
        public CatalogueController(ICatPartnerBankService catPartnerBankService, IStringLocalizer<LanguageSub> stringLocalizer, IActionFuncLogService actionFuncLog)
        {
            _catPartnerBankService = catPartnerBankService;
            _stringLocalizer = stringLocalizer;
            actionFuncLogService = actionFuncLog;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("BankInfoSyncUpdateStatus")]
        public async Task<IActionResult> UpdateBankInfoSyncStatus(BankStatusUpdateModel model, [Required] string apiKey, [Required] string hash)
        {
            var _startDateProgress = DateTime.Now;
            if (!_catPartnerBankService.ValidateApiKey(apiKey))
            {
                return new CustomUnauthorizedResult(ForPartnerConstants.API_KEY_INVALID);
            }
            if (!_catPartnerBankService.ValidateHashString(model, apiKey, hash))
            {
                return new CustomUnauthorizedResult(ForPartnerConstants.HASH_INVALID);
            }
            var fieldRequire = GetFieldRequireForUpdatePartnerBank(model);
            if (!string.IsNullOrEmpty(fieldRequire))
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = string.Format(@"Trường {0} không có dữ liệu. Vui lòng kiểm tra lại!", fieldRequire), Data = model };
                return BadRequest(_result);
            }
            var hs = await _catPartnerBankService.UpdatePartnerBankInfoSyncStatus(model);
            string _message = hs.Success ? "Cập nhật thông tin ngân hàng thành công" : string.Format("{0}. Cập nhật thông tin ngân hàng thất bại", hs.Message.ToString());
            var result = new ResultHandle { Status = hs.Success, Message = _message, Data = model };

            string _objectRequest = JsonConvert.SerializeObject(model);
            var _endDateProgress = DateTime.Now;
            #region -- Ghi Log --
            string _funcLocal = "UpdateBankInfoSyncStatus";
            string _major = "Cập nhật thông tin ngân hàng";
            var hsAddLog = actionFuncLogService.AddActionFuncLog(_funcLocal, _objectRequest, JsonConvert.SerializeObject(result), _major, _startDateProgress, _endDateProgress);
            #endregion

            if (!hs.Success)
                return BadRequest(result);
            return Ok(result);
        }

        private string GetFieldRequireForUpdatePartnerBank(BankStatusUpdateModel model)
        {
            string message = string.Empty;
            if (string.IsNullOrEmpty(model.PartnerCode))
            {
                message += "PartnerCode";
            }

            return message;
        }

    }
}
