using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Shipment.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsArrivalController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsArrivalFrieghtChargeService arrivalFreightChargeServices;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="freightChargeService"></param>
        /// <param name="user"></param>
        public CsArrivalController(IStringLocalizer<LanguageSub> localizer, ICsArrivalFrieghtChargeService freightChargeService, ICurrentUser user)
        {
            stringLocalizer = localizer;
            arrivalFreightChargeServices = freightChargeService;
            currentUser = user;
        }

        /// <summary>
        /// update arrival info
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Update(CsArrivalFrieghtChargeEditModel model)
        {
            var hs = arrivalFreightChargeServices.UpdateArrival(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// delete an existed charge
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("DeleteCharge")]
        public IActionResult DeleteCharge(Guid id)
        {
            var hs = arrivalFreightChargeServices.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
