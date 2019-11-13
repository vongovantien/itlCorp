using System;
using System.Collections.Generic;
using System.Linq;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Shipment.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsTransactionController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsTransactionService csTransactionService;
        private readonly ICurrentUser currentUser;
        private readonly ICsShipmentSurchargeService surchargeService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject IStringLocalizer</param>
        /// <param name="service">inject ICsTransactionService</param>
        /// <param name="user">inject ICurrentUser</param>
        /// <param name="serviceSurcharge">inject ICsShipmentSurchargeService</param>
        public CsTransactionController(IStringLocalizer<LanguageSub> localizer, ICsTransactionService service, ICurrentUser user,
            ICsShipmentSurchargeService serviceSurcharge)
        {
            stringLocalizer = localizer;
            csTransactionService = service;
            currentUser = user;
            surchargeService = serviceSurcharge;
        }

        /// <summary>
        /// count job by date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet("CountJobByDate/{{date}}")]
        public IActionResult CountJob(DateTime date)
        {
            var result = csTransactionService.Count(x => x.DatetimeCreated == date);
            return Ok(result);
        }

        /// <summary>
        /// get total profit by job
        /// </summary>
        /// <param name="JobId">job id that want to get total profit</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTotalProfit")]
        public List<object> GetTotalProfit(Guid JobId)
        {
            return csTransactionService.GetListTotalHB(JobId);
        }

        #region -- LIST & PAGING --

        /// <summary>
        /// get list transactions by search condition
        /// </summary>
        /// <param name="criteria">search condition</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Query")]
        public IActionResult Query(CsTransactionCriteria criteria)
        {
            return Ok(csTransactionService.Query(criteria));
        }

        /// <summary>
        /// get and paging list transaction by search condition
        /// </summary>
        /// <param name="criteria">search condition</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(CsTransactionCriteria criteria, int page, int size)
        {
            var data = csTransactionService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }
        #endregion -- LIST & PAGING --

        /// <summary>
        /// get transaction by id
        /// </summary>
        /// <param name="id">id that want to retrieve transaction</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var data = csTransactionService.GetById(id);
            return Ok(data);
        }

        #region -- INSERT & UPDATE
        /// <summary>
        /// add new transaction
        /// </summary>
        /// <param name="model">model to add</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public IActionResult Post(CsTransactionEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            string checkExistMessage = CheckExist(model.Id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            model.UserCreated = currentUser.UserID;
            var result = csTransactionService.AddCSTransaction(model);
            return Ok(result);
        }

        /// <summary>
        /// update an existed item
        /// </summary>
        /// <param name="model">model to update</param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        public IActionResult Put(CsTransactionEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (!csTransactionService.Any(x => x.Id == model.Id))
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Not found transaction" });
            }

            string checkExistMessage = CheckExist(model.Id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            model.UserModified = currentUser.UserID;
            var hs = csTransactionService.UpdateCSTransaction(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        #endregion -- INSERT & UPDATE

        #region -- DELETE --
        /// <summary>
        /// check allow delete an existed item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("CheckAllowDelete/{id}")]
        public IActionResult CheckAllowDelete(Guid id)
        {
            return Ok(csTransactionService.CheckAllowDelete(id));
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of existed data that want to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (!csTransactionService.Any(x => x.Id == id))
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Not found transaction" });
            }

            if (csTransactionService.CheckAllowDelete(id) == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.MSG_NOT_ALLOW_DELETED].Value });
            }
            var hs = csTransactionService.DeleteCSTransaction(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        #endregion -- DELETE --

        /// <summary>
        /// import transaction
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("Import")]
        public IActionResult Import(CsTransactionEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            string checkExistMessage = CheckExist(Guid.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return Ok(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            model.UserCreated = currentUser.UserID;
            var result = csTransactionService.ImportCSTransaction(model);
            return Ok(result);
        }

        #region -- METHOD PRIVATE --
        private string CheckExist(Guid id, CsTransactionEditModel model)
        {
            model.TransactionType = DataTypeEx.GetType(model.TransactionTypeEnum);
            if (model.TransactionType == string.Empty)
                return "Not found type transaction";
            string message = string.Empty;

            if (string.IsNullOrEmpty(model.Mawb))
            {
                return "MBL is required!";
            }
                model.TransactionType = DataTypeEx.GetType(model.TransactionTypeEnum);
                if (model.TransactionType == string.Empty)
                    message = "Not found type transaction";
                if (id == Guid.Empty)
                {
                    if (csTransactionService.Any(x => x.Mawb.ToLower() == model.Mawb.ToLower() && x.TransactionType == model.TransactionType))
                    {
                        message = stringLocalizer[LanguageSub.MSG_MAWB_EXISTED].Value;
                    }
                }
                else
                {
                    if (csTransactionService.Any(x => (x.Mawb.ToLower() == model.Mawb.ToLower() && x.Id != id)))
                    {
                        message = stringLocalizer[LanguageSub.MSG_MAWB_EXISTED].Value;
                    }
                }

            if (model.CsMawbcontainers == null || model.CsMawbcontainers.Count == 0)
            {
                message = "Shipment container list must have at least 1 row of data!";
            }

            var resultMessage = string.Empty;
            if (model.TransactionTypeEnum == TransactionTypeEnum.SeaFCLImport)
            {
                resultMessage = CheckExistsSIF(id, model);
            }

            message = resultMessage.Length == 0 ? message : resultMessage;

            return message;
        }

        private string CheckExistsSIF(Guid id, CsTransactionEditModel model)
        {
            string message = string.Empty;
            if (model.Etd.HasValue)
            {
                message = model.Etd.Value.Date >= model.Eta.Value.Date ? "ETD date must be before ETA date" : message;
            }

            if (model.Eta.HasValue)
            {
                if (model.Etd.HasValue)
                {
                    message = model.Eta.Value.Date <= model.Etd.Value.Date ? "ETA date must be after ETD date" : message;
                }
            }
            else
            {
                message = "ETA is required!";
            }

            message = string.IsNullOrEmpty(model.Mbltype) ? "Master Bill of Lading Type is required!" : message;

            message = string.IsNullOrEmpty(model.ShipmentType) ? "Shipment Type is required!" : message;

            if (model.Pol != null && model.Pol != Guid.Empty)
            {
                message = model.Pol == model.Pod ? "Port of Loading must be different from Port of Destination" : message;
            }

            if (model.Pod == null || model.Pod == Guid.Empty)
            {
                message = "Port of Destination is required!";
            }
            else
            {
                message = model.Pod == model.Pol ? "Port of Destination must be different from Port of Loading" : message;
            }

            if(model.DeliveryPlace != null && model.DeliveryPlace != Guid.Empty)
            {
                message = model.DeliveryPlace == model.Pol ? "Port of Destination must be different from Port of Loading" : message;
            }

            message = string.IsNullOrEmpty(model.TypeOfService) ? "Service Type is required!" : message;

            message = string.IsNullOrEmpty(model.PersonIncharge) ? "Person In Charge is required!" : message;

            return message;
        }
        #endregion -- METHOD PRIVATE --
    }
}
