using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.NoSql;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
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
        private readonly ICsShipmentSurchargeService surchargeService;
        private readonly IMapper mapper;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="user"></param>
        public AcctSettlementPaymentController(IStringLocalizer<LanguageSub> localizer, IAcctSettlementPaymentService service, ICurrentUser user, ICsShipmentSurchargeService surcharge, IMapper _mapper)
        {
            stringLocalizer = localizer;
            acctSettlementPaymentService = service;
            currentUser = user;
            surchargeService = surcharge;
            mapper = _mapper;
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

        /// <summary>
        /// Get details settlement payment by settlementId
        /// </summary>
        /// <param name="settlementId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDetailSettlementPaymentById")]
        public IActionResult GetDetailSettlementPaymentById(Guid settlementId)
        {
            var settlement = acctSettlementPaymentService.GetSettlementPaymentById(settlementId);
            List<ShipmentSettlement> chargeGrpSettlement = new List<ShipmentSettlement>();
            List<ShipmentChargeSettlement> chargeNoGrpSettlement = new List<ShipmentChargeSettlement>();
            if (settlement != null)
            {
                chargeGrpSettlement = acctSettlementPaymentService.GetListShipmentSettlementBySettlementNo(settlement.SettlementNo);
                chargeNoGrpSettlement = acctSettlementPaymentService.GetListShipmentChargeSettlementNoGroup(settlement.SettlementNo);
            }
            var data = new { settlement = settlement, chargeGrpSettlement = chargeGrpSettlement, chargeNoGrpSettlement = chargeNoGrpSettlement };
            return Ok(data);
        }

        /// <summary>
        /// Get settlement payment by settlementId
        /// </summary>
        /// <param name="settlementId">settlementId that want to retrieve Settlement Payment</param>
        /// <returns></returns>
        //[HttpGet]
        //[Route("GetSettlementPaymentById")]
        //public IActionResult GetSettlementPaymentById(Guid settlementId)
        //{
        //    var data = acctSettlementPaymentService.GetSettlementPaymentById(settlementId);
        //    return Ok(data);
        //}

        /// <summary>
        /// Get list shipment settlement by settlementNo
        /// </summary>
        /// <param name="settlementNo">settlementNo that want to retrieve Settlement Payment</param>
        /// <returns></returns>
        //[HttpGet]
        //[Route("GetShipmentsSettlementBySettlementNo")]
        //public IActionResult GetShipmentsSettlementBySettlementNo(string settlementNo)
        //{
        //    var data = acctSettlementPaymentService.GetListShipmentSettlementBySettlementNo(settlementNo);
        //    return Ok(data);
        //}

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
        
        /// <summary>
        /// Get exists charge by shipment
        /// </summary>
        /// <param name="JobId"></param>
        /// <param name="HBL"></param>
        /// <param name="MBL"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetExistsCharge")]
        public IActionResult GetExistsCharge(string JobId, string HBL, string MBL)
        {
            var data = acctSettlementPaymentService.GetExistsCharge(JobId, HBL, MBL);
            return Ok(data);
        }

        /// <summary>
        /// Check duplicate shipment settlement
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("CheckDuplicateShipmentSettlement")]
        public IActionResult CheckDuplicateShipmentSettlement(CheckDuplicateShipmentSettlementCriteria criteria)
        {
            var data = acctSettlementPaymentService.CheckDuplicateShipmentSettlement(criteria);
            ResultHandle result = new ResultHandle { Status = data, Message = data ? "Charge has already existed!" : "" };
            return Ok(result);
        }

        /// <summary>
        /// add new settlement payment
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Add(CreateUpdateSettlementModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            //Check duplicate
            if (model.ShipmentCharge.Count > 0)
            {
                foreach (var item in model.ShipmentCharge)
                {
                    var shipment = new CheckDuplicateShipmentSettlementCriteria
                    {
                        SettlementNo = item.SettlementCode,
                        ChargeID = item.ChargeId,
                        TypeCharge = item.Type,
                        HBLID = item.Hblid,
                        Partner = item.Type.Equals("BUY") ? item.PaymentObjectId : item.PayerId,
                        CustomNo = item.ClearanceNo,
                        InvoiceNo = item.InvoiceNo,
                        ContNo = item.ContNo
                    };
                    if (acctSettlementPaymentService.CheckDuplicateShipmentSettlement(shipment))
                    {
                        ResultHandle _result = new ResultHandle { Status = false, Message = "Charge has already existed!" };
                        return Ok(_result);
                    }
                }
            }
            else
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Settlement Payment don't have any charge in this period, Please check it again!" };
                return Ok(_result);
            }

            var hs = acctSettlementPaymentService.AddSettlementPayment(model);

            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return Ok(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Update Settlement Payment
        /// </summary>
        /// <param name="model">object to update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(CreateUpdateSettlementModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (!model.Settlement.StatusApproval.Equals("New") && !model.Settlement.StatusApproval.Equals("Denied"))
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Only allowed to edit the settlement payment status is New or Deny" };
                return Ok(_result);
            }

            //Check duplicate
            if (model.ShipmentCharge.Count > 0)
            {
                foreach (var item in model.ShipmentCharge)
                {
                    var shipment = new CheckDuplicateShipmentSettlementCriteria
                    {
                        SettlementNo = item.SettlementCode,
                        ChargeID = item.ChargeId,
                        TypeCharge = item.Type,
                        HBLID = item.Hblid,
                        Partner = item.Type.Equals("BUY") ? item.PaymentObjectId : item.PayerId,
                        CustomNo = item.ClearanceNo,
                        InvoiceNo = item.InvoiceNo,
                        ContNo = item.ContNo
                    };
                    if (acctSettlementPaymentService.CheckDuplicateShipmentSettlement(shipment))
                    {
                        ResultHandle _result = new ResultHandle { Status = false, Message = "Charge has already existed!" };
                        return Ok(_result);
                    }
                }
            }
            else
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Settlement Payment don't have any charge in this period, Please check it again!" };
                return Ok(_result);
            }

            var hs = acctSettlementPaymentService.UpdateSettlementPayment(model);

            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return Ok(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Save and Send Request
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SaveAndSendRequest")]
        [Authorize]
        public IActionResult SaveAndSendRequest(CreateUpdateSettlementModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            HandleState hs;
            if (string.IsNullOrEmpty(model.Settlement.SettlementNo))//Insert Settlement Payment
            {
                hs = acctSettlementPaymentService.AddSettlementPayment(model);
            }
            else //Update Settlement Payment
            {
                if (!model.Settlement.StatusApproval.Equals("New") && !model.Settlement.StatusApproval.Equals("Denied"))
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = "Only allowed to edit the settlement payment status is New or Deny" };
                    return Ok(_result);
                }
                hs = acctSettlementPaymentService.UpdateSettlementPayment(model);
            }

            //Check duplicate
            if (model.ShipmentCharge.Count > 0)
            {
                foreach (var item in model.ShipmentCharge)
                {
                    var shipment = new CheckDuplicateShipmentSettlementCriteria
                    {
                        SettlementNo = item.SettlementCode,
                        ChargeID = item.ChargeId,
                        TypeCharge = item.Type,
                        HBLID = item.Hblid,
                        Partner = item.Type.Equals("BUY") ? item.PaymentObjectId : item.PayerId,
                        CustomNo = item.ClearanceNo,
                        InvoiceNo = item.InvoiceNo,
                        ContNo = item.ContNo
                    };
                    if (acctSettlementPaymentService.CheckDuplicateShipmentSettlement(shipment))
                    {
                        ResultHandle _result = new ResultHandle { Status = false, Message = "Charge has already existed!" };
                        return Ok(_result);
                    }
                }
            }
            else
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Settlement Payment don't have any charge in this period, Please check it again!" };
                return Ok(_result);
            }

            AcctApproveSettlementModel approve = new AcctApproveSettlementModel
            {
                SettlementNo = model.Settlement.SettlementNo,
                Requester = model.Settlement.Requester
            };
            var resultInsertUpdateApprove = acctSettlementPaymentService.InsertOrUpdateApprovalSettlement(approve);
            if (!resultInsertUpdateApprove.Success)
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = resultInsertUpdateApprove.Exception.Message };
                return BadRequest(_result);
            }

            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return Ok(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Update Approve Settlement
        /// </summary>
        /// <param name="settlementId">settlementId that want to retrieve Update Approve</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateApprove")]
        [Authorize]
        public IActionResult UpdateApprove(Guid settlementId)
        {
            var updateApproval = acctSettlementPaymentService.UpdateApproval(settlementId);
            ResultHandle _result;
            if (!updateApproval.Success)
            {
                _result = new ResultHandle { Status = updateApproval.Success, Message = updateApproval.Exception.Message };
            }
            else
            {
                _result = new ResultHandle { Status = updateApproval.Success };
            }
            return Ok(_result);
        }

        /// <summary>
        /// Denie Approve Settlement
        /// </summary>
        /// <param name="settlementId">settlementId that want to retrieve Denie Approve</param>
        /// <param name="comment">comment reason that want to retrieve Denie Approve</param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeniedApprove")]
        [Authorize]
        public IActionResult DeniedApprove(Guid settlementId, string comment)
        {
            var denieApproval = acctSettlementPaymentService.DeniedApprove(settlementId, comment);
            ResultHandle _result;
            if (!denieApproval.Success)
            {
                _result = new ResultHandle { Status = denieApproval.Success, Message = denieApproval.Exception.Message };
            }
            else
            {
                _result = new ResultHandle { Status = denieApproval.Success };
            }
            return Ok(_result);
        }

        /// <summary>
        /// Get information approve settlement by settlementNo
        /// </summary>
        /// <param name="settlementNo">settlementNo that want to retrieve approve settlement</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetInfoApproveSettlementBySettlementNo")]
        public IActionResult GetInfoApproveSettlementBySettlementNo(string settlementNo)
        {
            var data = acctSettlementPaymentService.GetInfoApproveSettlementBySettlementNo(settlementNo);
            return Ok(data);
        }

        /// <summary>
        /// Preview Settlement 
        /// </summary>
        /// <param name="settlementNo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Preview")]
        public IActionResult Preview(string settlementNo)
        {
            var data = acctSettlementPaymentService.Preview(settlementNo);         
            return Ok(data);
        }
    }
}