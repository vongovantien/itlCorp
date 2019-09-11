using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.NoSql;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.SettlementPayment;
using eFMS.API.Shipment.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
    public class AcctSettlementPaymentController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAcctSettlementPaymentService acctSettlementPaymentService;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="user"></param>
        public AcctSettlementPaymentController(IStringLocalizer<LanguageSub> localizer, IAcctSettlementPaymentService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            acctSettlementPaymentService = service;
            currentUser = user;
        }
        
        /// <summary>
        /// get and paging the list of Settlement Payment by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="pageNumber">page to retrieve data</param>
        /// <param name="pageSize">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(AcctSettlementPaymentCriteria criteria, int pageNumber, int pageSize)
        {
            var data = acctSettlementPaymentService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }

        /// <summary>
        /// Get list shipment of settlement payment list by settlementNo
        /// </summary>
        /// <param name="settlementNo">settlementNo that want to retrieve Settlement Payment</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetShipmentOfSettlements")]
        public IActionResult GetShipmentOfSettlements(string settlementNo)
        {
            var data = acctSettlementPaymentService.GetShipmentOfSettlements(settlementNo);
            return Ok(data);
        }

        /// <summary>
        /// delete an settlement payment existed item
        /// </summary>
        /// <param name="settlementNo">settlementNo of existed item that want to delete</param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        [Route("Delete")]
        public IActionResult Delete(string settlementNo)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;

            HandleState hs = acctSettlementPaymentService.DeleteSettlementPayment(settlementNo);

            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return Ok(result);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetDetailSettlementPaymentById")]
        public IActionResult GetDetailSettlementPaymentById(Guid settlementId)
        {
            var settlement = acctSettlementPaymentService.GetSettlementPaymentById(settlementId);
            List<ShipmentSettlement> chargeGrpSettlement = new List<ShipmentSettlement>();
            if (settlement != null)
            {
                chargeGrpSettlement = acctSettlementPaymentService.GetListShipmentSettlementBySettlementNo(settlement.SettlementNo);
            }
            var data = new { settlement = settlement, chargeGrpSettlement = chargeGrpSettlement };
            return Ok(data);
        }

        /// <summary>
        /// Get settlement payment by settlementId
        /// </summary>
        /// <param name="settlementId">settlementId that want to retrieve Settlement Payment</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSettlementPaymentById")]
        public IActionResult GetSettlementPaymentById(Guid settlementId)
        {
            var data = acctSettlementPaymentService.GetSettlementPaymentById(settlementId);
            return Ok(data);
        }

        /// <summary>
        /// Get list shipment settlement by settlementNo
        /// </summary>
        /// <param name="settlementNo">settlementNo that want to retrieve Settlement Payment</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetShipmentsSettlementBySettlementNo")]
        public IActionResult GetShipmentsSettlementBySettlementNo(string settlementNo)
        {
            var data = acctSettlementPaymentService.GetListShipmentSettlementBySettlementNo(settlementNo);
            return Ok(data);
        }

        /// <summary>
        /// Get Payment Management By Shipment
        /// </summary>
        /// <param name="JobId"></param>
        /// <param name="MBL"></param>
        /// <param name="HBL"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPaymentManagementByShipment")]
        public IActionResult GetPaymentManagementByShipment(string JobId, string MBL, string HBL)
        {
            var advancePayment = acctSettlementPaymentService.GetAdvancePaymentMngts(JobId, MBL, HBL);
            var settlementPayment = acctSettlementPaymentService.GetSettlementPaymentMngts(JobId, MBL, HBL);

            //Lấy ra list các currency của cả 2 (không trùng currency)
            List<string> currencies = new List<string>();
            if(advancePayment.Count > 0)
            {
                var advance = advancePayment.Where(x => !currencies.Contains(x.AdvanceCurrency));
                foreach (var item in advance)
                {
                    currencies.Add(item.AdvanceCurrency);
                }
            }

            if(settlementPayment.Count > 0)
            {
                var settle = settlementPayment.Where(x => !currencies.Contains(x.SettlementCurrency));
                foreach(var item in settle)
                {
                    currencies.Add(item.SettlementCurrency);
                }
            }

            var totalAdvance = "";
            var totalSettlement = "";
            var balance = "";
            if (currencies.Count > 0)
            {
                foreach (var currency in currencies)
                {
                    decimal totalAdv = Math.Round(advancePayment.Where(x => x.AdvanceCurrency == currency).Sum(su => su.TotalAmount), 2);
                    decimal totalSet = Math.Round(settlementPayment.Where(x => x.SettlementCurrency == currency).Sum(su => su.TotalAmount), 2);
                    decimal bal = (totalAdv - totalSet);
                    totalAdvance += string.Format("{0:n}", totalAdv) + " " + currency + " | ";
                    totalSettlement += string.Format("{0:n}", totalSet) + " " + currency + " | ";
                    balance += (bal < 0 ? "(" + string.Format("{0:n}", Math.Abs(bal)) + ")" : string.Format("{0:n}", bal) + "") + " " + currency + " | ";
                }
                totalAdvance = (totalAdvance += ")").Replace(" | )", "");
                totalSettlement = (totalSettlement += ")").Replace(" | )", "");
                balance = (balance += ")").Replace(" | )", "");
            }

            var result = new
            {
                jobId = JobId,
                mbl = MBL,
                hbl = HBL,
                totalAdvance = totalAdvance,
                totalSettlement = totalSettlement,
                balance = balance,
                AdvancePayment = advancePayment,
                SettlementPayment = settlementPayment
            };
            return Ok(result);
        }

        
    }
}