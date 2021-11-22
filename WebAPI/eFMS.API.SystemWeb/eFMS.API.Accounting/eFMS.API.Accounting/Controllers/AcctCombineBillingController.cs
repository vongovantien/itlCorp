using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;

namespace eFMS.API.Accounting.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AcctCombineBillingController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAcctCombineBillingService combineBillingService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ICurrentUser currentUser;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="accService"></param>
        /// <param name="currUser"></param>
        public AcctCombineBillingController(
            IStringLocalizer<LanguageSub> localizer,
            IHostingEnvironment hostingEnvironment,
            IAcctCombineBillingService accService,
            ICurrentUser currUser)
        {
            stringLocalizer = localizer;
            combineBillingService = accService;
            _hostingEnvironment = hostingEnvironment;
            currentUser = currUser;
        }

        /// <summary>
        /// get and paging the list of Accounting Management by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="pageNumber">page to retrieve data</param>
        /// <param name="pageSize">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(AcctCombineBillingCriteria criteria, int pageNumber, int pageSize)
        {
            var data = combineBillingService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }

        /// <summary>
        /// Add new Combine Billing Data
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Add(AcctCombineBillingModel model)
        {
            currentUser.Action = "AddCombineBilling";

            if (!ModelState.IsValid) return BadRequest();

            model.Id = Guid.NewGuid();
            var hs = combineBillingService.AddCombineBilling(model);

            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                result.Data = null;
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Add new Combine Billing Data
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(AcctCombineBillingModel model)
        {
            currentUser.Action = "UpdateCombineBilling";
            if (!ModelState.IsValid) return BadRequest();
            //Check Exist Data
            var isExistedCombine = combineBillingService.CheckExistedCombineData(model.Id);
            if (!isExistedCombine)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Combine Billing not existed" });
            }
            var hs = combineBillingService.UpdateCombineBilling(model);

            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                result.Data = null;
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Check combine billing is existed
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("CheckExisting/{id}")]
        [Authorize]
        public IActionResult CheckExisting(Guid id)
        {
            var result = combineBillingService.CheckExistedCombineData(id);
            return Ok(result);
        }

        /// <summary>
        /// Delete combine billing
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        [Route("Delete")]
        public IActionResult Delete(Guid? id)
        {
            currentUser.Action = "DeleteCombineBilling";

            //Check Exist Data
            var isExistedCombine = combineBillingService.CheckExistedCombineData((Guid)id);
            if (!isExistedCombine)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Combine Billing not existed" });
            }


            HandleState hs = combineBillingService.DeleteCombineBilling(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Generate billing no
        /// </summary>
        /// <returns></returns>
        [HttpGet("GenerateCombineBillingNo")]
        public IActionResult GenerateHBLSeaExport()
        {
            string billingNo = combineBillingService.GenerateCombineBillingNo();
            return Ok(new { billingNo });
        }

        /// <summary>
        /// Check Esiting Combine No With Document No
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckDocumentNoExisted")]
        public IActionResult CheckDocumentNoExisted(ShipmentCombineCriteria criteria)
        {
            var result = combineBillingService.CheckDocumentNoExisted(criteria);
            ResultHandle _result = new ResultHandle { Status = true };
            if (!string.IsNullOrEmpty(result))
            {
                _result = new ResultHandle { Status = false, Message = result };
            }
            return Ok(_result);
        }

        /// <summary>
        /// Check allow detail
        /// </summary>
        /// <param name="id">Id of combine</param>
        /// <returns></returns>
        [HttpGet("CheckAllowViewDetailCombine/{id}")]
        public IActionResult CheckAllowDetail(Guid id)
        {
            var result = combineBillingService.CheckAllowViewDetailCombine(id);
            return Ok(result);
        }

        /// <summary>
        /// Get list charge shipment by conditions
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetListShipmentInfo")]
        public IActionResult GetListShipmentInfo(ShipmentCombineCriteria criteria)
        {
            var data = combineBillingService.GetCombineBillingDetailList(criteria);
            return Ok(data);
        }

        /// <summary>
        /// get detail combine
        /// </summary>
        /// <param name="id">id of combine</param>
        /// <returns></returns>
        [HttpGet("GetDetailByCombineId/{id}")]
        [Authorize]
        public IActionResult GetDetailByCombineId(string id)
        {
            var combineData = combineBillingService.GetCombineBillingDetailWithId(id);
            if (combineData == null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "This combine billing does not exist" });
            }
            return Ok(combineData);
        }

        /// <summary>
        /// Preview As Debit Template
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PreviewCombineDebitTemplate")]
        public IActionResult PreviewCombineDebitTemplate(AcctCombineBillingModel model)
        {
            var cdNoteModel = combineBillingService.GetDataPreviewDebitNoteTemplate(model);
            var result = combineBillingService.PreviewCombineDebitTemplate(cdNoteModel);
            return Ok(result);
        }

    }
}
