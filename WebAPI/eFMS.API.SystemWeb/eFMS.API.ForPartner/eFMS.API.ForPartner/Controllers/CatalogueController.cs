using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;
using System;
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

        public CatalogueController(ICatBankService catBankService)
        {
            _catBankService = catBankService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("BankInfoSyncUpdateStatus")]
        public async Task<IActionResult> UpdateBankInfoSyncStatus(BankStatusUpdateModel model/*, [Required] string apiKey, [Required] string hash*/)
        {
            //var _startDateProgress = DateTime.Now;
            //if (!_catBankService.ValidateApiKey(apiKey))
            //{
            //    return new CustomUnauthorizedResult(ForPartnerConstants.API_KEY_INVALID);
            //}
            //if (!_catBankService.ValidateHashString(model, apiKey, hash))
            //{
            //    return new CustomUnauthorizedResult(ForPartnerConstants.HASH_INVALID);
            //}

            if (!ModelState.IsValid) return BadRequest();
            var hs = await _catBankService.UpdateBankInfoSyncStatus(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = string.Format(@""), Data = model.BankInfo };
            var _endDateProgress = DateTime.Now;
            return Ok(result);
        }
    }
}
