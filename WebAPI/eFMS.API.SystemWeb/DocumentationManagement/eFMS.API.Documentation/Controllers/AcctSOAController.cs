using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.NoSql;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Shipment.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using SystemManagementAPI.Infrastructure.Middlewares;

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AcctSOAController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAcctSOAService acctSOAService;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="user"></param>
        public AcctSOAController(IStringLocalizer<LanguageSub> localizer, IAcctSOAService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            acctSOAService = service;
            currentUser = user;
        }

        /// <summary>
        /// get the list of SOA
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var results = acctSOAService.Get();
            return Ok(results);
        }

        /// <summary>
        /// add new SOA
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult AddNew(AcctSoaModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            model.Status = "New";
            model.DatetimeCreated = DateTime.Now;
            model.DatetimeModified = DateTime.Now;
            model.UserCreated = model.UserModified = currentUser.UserID;

            var hs = acctSOAService.AddSOA(model);

            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// get and paging the list of SOA by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="pageNumber">page to retrieve data</param>
        /// <param name="pageSize">number items per page</param>
        /// <returns></returns>
        [HttpPost("Paging")]
        public IActionResult Paging(AcctSOACriteria criteria, int pageNumber, int pageSize)
        {
            var data = acctSOAService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="soaNo">soaNo of existed item that want to delete</param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        [Route("Delete")]
        public IActionResult Delete(string soaNo)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = acctSOAService.Delete(x => x.Soano == soaNo);
            //Update SOANo = NULL for ShipmentSurcharge
            acctSOAService.UpdateSOASurCharge(soaNo);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// get SOA by soaNo and currencyLocal
        /// </summary>
        /// <param name="soaNo">soaNo that want to retrieve SOA</param>
        /// <param name="currencyLocal">currencyLocal that want to retrieve SOA</param>
        /// <returns></returns>
        [HttpGet("GetBySoaNo/{soaNo}&{currencyLocal}")]
        public IActionResult GetBySoaNoAndCurrencyLocal(string soaNo, string currencyLocal)
        {
            var results = acctSOAService.GetBySoaNoAndCurrencyLocal(soaNo, currencyLocal);
            return Ok(results);
        }

        /// <summary>
        /// get list services
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetListServices")]
        public IActionResult GetListServices()
        {
            var results = acctSOAService.GetListServices();
            return Ok(results);
        }

        /// <summary>
        /// get list status of soa
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetListStatusSoa")]
        public IActionResult GetListStatusSoa()
        {
            var results = acctSOAService.GetListStatusSoa();
            return Ok(results);
        }

    }
}