using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Infrastructure.Extensions;
using eFMS.API.ForPartner.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
        private readonly ICatBankService _catBankService;
        private readonly IStringLocalizer _stringLocalizer;

        public CatalogueController(ICatBankService catBankService, IStringLocalizer<LanguageSub> stringLocalizer)
        {
            _catBankService = catBankService;
            _stringLocalizer = stringLocalizer;
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
            if (!_catBankService.ValidateApiKey(apiKey))
            {
                return new CustomUnauthorizedResult(ForPartnerConstants.API_KEY_INVALID);
            }
            if (!_catBankService.ValidateHashString(model, apiKey, hash))
            {
                return new CustomUnauthorizedResult(ForPartnerConstants.HASH_INVALID);
            }

            if (!ModelState.IsValid) return BadRequest();

            var hs = await _catBankService.UpdateBankInfoSyncStatus(model);

            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = _stringLocalizer[message].Value, Data = null };

            return Ok(result);
        }
    }
}
