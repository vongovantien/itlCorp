﻿using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Common;
using System;
using System.Collections.Generic;
using AutoMapper;
using System.Linq;
using eFMS.API.Accounting.DL.Models.SettlementPayment;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Accounting.DL.Models.ExportResults;
using eFMS.API.Common.Helpers;
using Newtonsoft.Json;

namespace eFMS.API.Accounting.Controllers
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
        private readonly IMapper mapper;
        private string typeApproval = "Settlement";

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="user"></param>
        public AcctSettlementPaymentController(
            IStringLocalizer<LanguageSub> localizer, 
            IAcctSettlementPaymentService service, 
            ICurrentUser user, IMapper _mapper
            )
        {
            stringLocalizer = localizer;
            acctSettlementPaymentService = service;
            currentUser = user;
            mapper = _mapper;
        }

        /// <summary>
        /// Get all settlement payment
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var data = acctSettlementPaymentService.Get();
            return Ok(data);
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
        [Authorize]
        public IActionResult Paging(AcctSettlementPaymentCriteria criteria, int pageNumber, int pageSize)
        {
            var data = acctSettlementPaymentService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }

        /// <summary>
        /// Query data
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("QueryData")]
        [Authorize]
        public IActionResult QueryData(AcctSettlementPaymentCriteria criteria)
        {
            var data = acctSettlementPaymentService.QueryData(criteria);
            return Ok(data);
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
        /// Check allow delete settlement payment
        /// </summary>
        /// <param name="id">Id of settlement payment</param>
        /// <returns></returns>
        [HttpGet("CheckAllowDelete/{id}")]
        [Authorize]
        public IActionResult CheckAllowDelete(Guid id)
        {
            var result = acctSettlementPaymentService.CheckDeletePermissionBySettlementId(id);
            return Ok(result);
        }

        /// <summary>
        /// Check allow detail settlement payment
        /// </summary>
        /// <param name="id">Id of settlement payment</param>
        /// <returns></returns>
        [HttpGet("CheckAllowDetail/{id}")]
        [Authorize]
        public IActionResult CheckAllowDetail(Guid id)
        {
            var result = acctSettlementPaymentService.CheckDetailPermissionBySettlementId(id);
            return Ok(result);
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
            currentUser.Action = "DeleteAcctSettlementPayment";

            var isAllowDelete = acctSettlementPaymentService.CheckDeletePermissionBySettlementNo(settlementNo);
            if (isAllowDelete == false)
            {
               return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            if (!acctSettlementPaymentService.CheckValidateDeleteSettle(settlementNo))
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[AccountingLanguageSub.MSG_SETTLE_NOT_ALLOW_DELETE,settlementNo].Value });
            }

            HandleState hs = acctSettlementPaymentService.DeleteSettlementPayment(settlementNo);
            if (hs.Code == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString() };
                return BadRequest(_result);
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
        [Authorize]
        public IActionResult GetDetailSettlementPaymentById(Guid settlementId)
        {
            var isAllowViewDetail = acctSettlementPaymentService.CheckDetailPermissionBySettlementId(settlementId);
            if (isAllowViewDetail == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var settlement = acctSettlementPaymentService.GetSettlementPaymentById(settlementId);
            List<ShipmentSettlement> chargeGrpSettlement = new List<ShipmentSettlement>();
            List<ShipmentChargeSettlement> chargeNoGrpSettlement = new List<ShipmentChargeSettlement>();
            if (settlement != null)
            {
                chargeGrpSettlement = acctSettlementPaymentService.GetListShipmentSettlementBySettlementNo(settlement.SettlementNo).OrderBy(x => x.JobId).ToList();
                chargeNoGrpSettlement = acctSettlementPaymentService.GetListShipmentChargeSettlementNoGroup(settlement.SettlementNo).OrderBy(x => x.JobId).ToList();
            }
            var data = new { settlement, chargeGrpSettlement, chargeNoGrpSettlement };
            return Ok(data);
        }

        /// <summary>
        /// Get Payment Management By Shipment
        /// </summary>
        /// <param name="JobId"></param>
        /// <param name="MBL"></param>
        /// <param name="HBL"></param>
        /// <param name="requester"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPaymentManagementByShipment")]
        public IActionResult GetPaymentManagementByShipment(string JobId, string MBL, string HBL, string requester)
        {
            var advancePayment = acctSettlementPaymentService.GetAdvancePaymentMngts(JobId, MBL, HBL, requester);
            var settlementPayment = acctSettlementPaymentService.GetSettlementPaymentMngts(JobId, MBL, HBL, requester);

            //Lấy ra list các currency của cả 2 (không trùng currency)
            List<string> currencies = new List<string>();
            if (advancePayment.Count > 0)
            {
                var advance = advancePayment.Where(x => !currencies.Contains(x.AdvanceCurrency));
                foreach (var item in advance)
                {
                    currencies.Add(item.AdvanceCurrency);
                }
            }

            if (settlementPayment.Count > 0)
            {
                var settle = settlementPayment.Where(x => !currencies.Contains(x.SettlementCurrency));
                foreach (var item in settle)
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
                    decimal totalAdv = NumberHelper.RoundNumber(advancePayment.Where(x => x.AdvanceCurrency == currency).Sum(su => su.TotalAmount), 2);
                    decimal totalSet = NumberHelper.RoundNumber(settlementPayment.Where(x => x.SettlementCurrency == currency).Sum(su => su.TotalAmount), 2);
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
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetExistsCharge")]
        public IActionResult GetExistsCharge(ExistsChargeCriteria criteria)
        {
            var data = acctSettlementPaymentService.GetExistsCharge(criteria);
            var dataGroups = data.ToList().GroupBy(x => new { x.JobId, x.HBL, x.MBL, x.Hblid, x.Type, x.ClearanceNo });
            List<ShipmentSettlement> shipmentSettlement = new List<ShipmentSettlement>();
            foreach (var item in dataGroups)
            {
                var shipment = new ShipmentSettlement();
                var advanceLst = acctSettlementPaymentService.GetListAdvanceNoForShipment(item.Key.JobId, item.Key.MBL, item.Key.HBL);
                shipment.JobId = item.Key.JobId;
                shipment.MBL = item.Key.MBL;
                shipment.HBL = item.Key.HBL;
                shipment.ChargeSettlements = item.ToList();
                shipment.HblId = item.Key.Hblid;
                shipment.Type = item.Key.Type;
                shipment.AdvanceNo = advanceLst == null ? null : advanceLst.FirstOrDefault();
                shipment.AdvanceNoList = advanceLst;
                shipment.CustomNo = item.Key.ClearanceNo;
                shipment.TotalNetAmount = item.Where(x => x.CurrencyId != AccountingConstants.CURRENCY_LOCAL).Sum(x => x.NetAmount ?? 0);
                shipment.TotalNetAmountVND = item.Where(x => x.CurrencyId == AccountingConstants.CURRENCY_LOCAL).Sum(x => x.NetAmount ?? 0);
                shipment.TotalAmount = item.Where(x => x.CurrencyId != AccountingConstants.CURRENCY_LOCAL).Sum(x => x.Total);
                shipment.TotalAmountVND = item.Where(x => x.CurrencyId == AccountingConstants.CURRENCY_LOCAL).Sum(x => x.Total);
                shipment.TotalNetVND = item.Sum(x => x.AmountVnd ?? 0);
                shipment.TotalVATVND = item.Sum(x => x.VatAmountVnd ?? 0);
                shipment.TotalNetUSD = item.Sum(x => x.AmountUSD ?? 0);
                shipment.TotalVATUSD = item.Sum(x => x.VatAmountUSD ?? 0);
                shipment.TotalVND = shipment.TotalNetVND + shipment.TotalVATVND;
                shipmentSettlement.Add(shipment);
            }
            var _totalNetVND = shipmentSettlement.Sum(x => x.TotalNetVND);
            var _totalVATVND = shipmentSettlement.Sum(x => x.TotalVATVND);
            var _totalNetUSD = shipmentSettlement.Sum(x => x.TotalNetUSD);
            var _totalVATUSD = shipmentSettlement.Sum(x => x.TotalVATUSD);
            var _totalVND = _totalNetVND + _totalVATVND;
            var _totalUSD = _totalNetUSD + _totalVATUSD;
            var _totalShipment = dataGroups.Count();
            var _totalCharges = data.Select(x => x.ChargeId).Count();
            var total = new
            {
                TotalShipment = _totalShipment,
                TotalCharges = _totalCharges,
                TotalVNDStr = string.Format("{0} = {1} + {2}", _totalVND.ToString("#,##0"), _totalNetVND.ToString("#,##0"), _totalVATVND.ToString("#,##0")),
                TotalUSDStr = string.Format("{0} = {1} + {2}", _totalUSD.ToString("##0.00"), _totalNetUSD.ToString("##0.00"), _totalVATUSD.ToString("##0.00"))
            };
            var result = new { shipmentSettlement, total };
            return Ok(result);
        }

        /// <summary>
        /// Check duplicate shipment settlement
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("CheckDuplicateShipmentSettlement")]
        public IActionResult CheckDuplicateShipmentSettlement(CheckDuplicateShipmentSettlementCriteria criteria)
        {
            var data = acctSettlementPaymentService.CheckDuplicateShipmentSettlement(criteria,out List<DuplicateShipmentSettlementResultModel> listDup);
            ResultHandle result = new ResultHandle { Status = data.Status, Message = data.Message,Data = listDup };
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
            currentUser.Action = "AddAcctSettlementPayment";

            //Check duplicate
            if (model.ShipmentCharge.Count > 0)
            {
                //Check Duplicate phí
                var isDuplicateCharge = CheckDuplicateCharge(model,out object dataDuplicate);
                if (isDuplicateCharge)
                {
                    string mesg = String.Format("Duplicate charge {0} in {1}-{2}-{3}", dataDuplicate.GetValueBy("ChargeCode"), dataDuplicate.GetValueBy("JobId"), dataDuplicate.GetValueBy("MBL"), dataDuplicate.GetValueBy("HBL"));
                    ResultHandle _result = new ResultHandle { Status = false, Message = mesg, Data = dataDuplicate };
                    return BadRequest(_result);
                }

                foreach (var item in model.ShipmentCharge)
                {
                    var shipment = new CheckDuplicateShipmentSettlementCriteria
                    {
                        SurchargeID = item.Id,
                        ChargeID = item.ChargeId,
                        TypeCharge = item.Type,
                        HBLID = item.Hblid,
                        Partner = item.Type.Equals(AccountingConstants.TYPE_CHARGE_BUY) ? item.PaymentObjectId : item.PayerId,
                        CustomNo = item.ClearanceNo,
                        InvoiceNo = item.InvoiceNo,
                        ContNo = item.ContNo,
                        MBLNo = item.MBL,
                        HBLNo = item.HBL,
                        JobNo = item.JobId,
                        Notes = item.Notes
                    };
                    var _checkDuplicate = acctSettlementPaymentService.CheckDuplicateShipmentSettlement(shipment, out List<DuplicateShipmentSettlementResultModel> listDup);
                    if (_checkDuplicate.Status)
                    {
                        ResultHandle _result = new ResultHandle { Status = false, Message = _checkDuplicate.Message, Data = listDup };
                        return BadRequest(_result);
                    }
                }
            }
            else
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Settlement Payment don't have any charge in this period, Please check it again!" };
                return BadRequest(_result);
            }

            var hs = acctSettlementPaymentService.AddSettlementPayment(model);
            if (hs.Code == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
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
            currentUser.Action = "UpdateAcctSettlementPayment";
            if (!ModelState.IsValid) return BadRequest();

            var isAllowUpdate = acctSettlementPaymentService.CheckUpdatePermissionBySettlementId(model.Settlement.Id);
            if (isAllowUpdate == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            //Đã check bên trong function UpdateSettlementPayment
            //if (!model.Settlement.StatusApproval.Equals(Constants.STATUS_APPROVAL_NEW) && !model.Settlement.StatusApproval.Equals(Constants.STATUS_APPROVAL_DENIED))
            //{
            //    ResultHandle _result = new ResultHandle { Status = false, Message = "Only allowed to edit the settlement payment status is New or Deny" };
            //    return Ok(_result);
            //}

            //Check duplicate
            if (model.ShipmentCharge.Count > 0)
            {
                //Check Duplicate phí
                var isDuplicateCharge = CheckDuplicateCharge(model, out object dataDuplicate);
                if (isDuplicateCharge)
                {
                    string mesg = String.Format("Duplicate charge {0} in {1}-{2}-{3}", dataDuplicate.GetValueBy("ChargeCode"), dataDuplicate.GetValueBy("JobId"), dataDuplicate.GetValueBy("MBL"), dataDuplicate.GetValueBy("HBL"));
                    ResultHandle _result = new ResultHandle { Status = false, Message = mesg, Data = dataDuplicate };
                    return BadRequest(_result);
                }

                foreach (var item in model.ShipmentCharge)
                {
                    var shipment = new CheckDuplicateShipmentSettlementCriteria
                    {
                        SurchargeID = item.Id,
                        ChargeID = item.ChargeId,
                        TypeCharge = item.Type,
                        HBLID = item.Hblid,
                        Partner = item.Type.Equals(AccountingConstants.TYPE_CHARGE_BUY) ? item.PaymentObjectId : item.PayerId,
                        CustomNo = item.ClearanceNo,
                        InvoiceNo = item.InvoiceNo,
                        ContNo = item.ContNo,
                        MBLNo = item.MBL,
                        HBLNo = item.HBL,
                        JobNo = item.JobId,
                        Notes = item.Notes,
                        SettlementNo = model.Settlement.SettlementNo
                    };
                    
                    var _checkDuplicate = acctSettlementPaymentService.CheckDuplicateShipmentSettlement(shipment, out List<DuplicateShipmentSettlementResultModel> listDup);
                    if (_checkDuplicate.Status)
                    {
                        ResultHandle _result = new ResultHandle { Status = false, Message = _checkDuplicate.Message,Data = listDup };
                        return BadRequest(_result);
                    }
                }
            }
            else
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Settlement Payment don't have any charge in this period, Please check it again!" };
                return BadRequest(_result);
            }

            var hs = acctSettlementPaymentService.UpdateSettlementPayment(model);
            if (hs.Code == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
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
            currentUser.Action = "SaveAndSendRequestAcctSettlementPayment";

            if (!ModelState.IsValid) return BadRequest();

            //Check duplicate
            if (model.ShipmentCharge.Count > 0)
            {
                //Check Duplicate phí
                var isDuplicateCharge = CheckDuplicateCharge(model, out object dataDuplicate);
                if (isDuplicateCharge)
                {
                    string mesg = String.Format("Duplicate charge {0} in {1}-{2}-{3}", dataDuplicate.GetValueBy("ChargeCode"), dataDuplicate.GetValueBy("JobId"), dataDuplicate.GetValueBy("MBL"), dataDuplicate.GetValueBy("HBL"));
                    ResultHandle _result = new ResultHandle { Status = false, Message = mesg, Data = dataDuplicate };
                    return BadRequest(_result);
                }

                foreach (var item in model.ShipmentCharge)
                {
                    // Check Job Is Locked
                    var isLockedJob = acctSettlementPaymentService.CheckIsLockedShipment(item.JobId);
                    if (isLockedJob)
                    {
                        ResultHandle _result = new ResultHandle { Status = false, Message = item.JobId + " have been locked. You not allow save and send request." };
                        return BadRequest(_result);
                    }

                    var shipment = new CheckDuplicateShipmentSettlementCriteria
                    {
                        SurchargeID = item.Id,
                        ChargeID = item.ChargeId,
                        TypeCharge = item.Type,
                        HBLID = item.Hblid,
                        Partner = item.Type.Equals(AccountingConstants.TYPE_CHARGE_BUY) ? item.PaymentObjectId : item.PayerId,
                        CustomNo = item.ClearanceNo,
                        InvoiceNo = item.InvoiceNo,
                        ContNo = item.ContNo,
                        MBLNo = item.MBL,
                        HBLNo = item.HBL,
                        JobNo = item.JobId,
                        Notes = item.Notes
                    };
                    var _checkDuplicate = acctSettlementPaymentService.CheckDuplicateShipmentSettlement(shipment, out List<DuplicateShipmentSettlementResultModel> listDup);
                    if (_checkDuplicate.Status)
                    {
                        ResultHandle _result = new ResultHandle { Status = false, Message = _checkDuplicate.Message,Data = listDup };
                        return BadRequest(_result);
                    }
                }
            }
            else
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Settlement Payment don't have any charge in this period, Please check it again!" };
                return BadRequest(_result);
            }

            #region -- Check Validate Email Requester --
            var isValidEmail = acctSettlementPaymentService.CheckValidateMailByUserId(model.Settlement.Requester);
            if (!isValidEmail.Success)
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = isValidEmail.Exception.Message.Replace("[name]", "requester") };
                return BadRequest(_result);
            }
            #endregion -- Check Validate Email Requester --

            HandleState hs;
            var message = string.Empty;
            if (string.IsNullOrEmpty(model.Settlement.SettlementNo))//Insert Settlement Payment
            {
                #region -- Check Exist Setting Flow --
                var isExistSettingFlow = acctSettlementPaymentService.CheckExistSettingFlow(typeApproval, currentUser.OfficeID);
                if (!isExistSettingFlow.Success)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = isExistSettingFlow.Exception.Message };
                    return BadRequest(_result);
                }
                #endregion -- Check Exist Setting Flow --

                #region -- Check Exist User Approval --
                var isExistUserApproval = acctSettlementPaymentService.CheckExistUserApproval(typeApproval, currentUser.GroupId, currentUser.DepartmentId, currentUser.OfficeID, currentUser.CompanyID);
                if (!isExistUserApproval.Success)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = isExistUserApproval.Exception.Message };
                    return BadRequest(_result);
                }
                #endregion -- Check Exist User Approval --

                model.Settlement.StatusApproval = AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL;
                hs = acctSettlementPaymentService.AddSettlementPayment(model);
                message = HandleError.GetMessage(hs, Crud.Insert);
                if (hs.Code == 403)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
                }
            }
            else //Update Settlement Payment
            {
                var isAllowUpdate = acctSettlementPaymentService.CheckUpdatePermissionBySettlementId(model.Settlement.Id);
                if (isAllowUpdate == false)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
                }

                var settlementPaymentCurrent = acctSettlementPaymentService.Get(x => x.Id == model.Settlement.Id).FirstOrDefault();
                #region -- Check Exist Settlement Payment --
                if (settlementPaymentCurrent == null)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = "Not found settlement payment" };
                    return BadRequest(_result);
                }
                #endregion -- Check Exist Settlement Payment --

                #region -- Check Exist Setting Flow --
                var isExistSettingFlow = acctSettlementPaymentService.CheckExistSettingFlow(typeApproval, settlementPaymentCurrent.OfficeId);
                if (!isExistSettingFlow.Success)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = isExistSettingFlow.Exception.Message };
                    return BadRequest(_result);
                }
                #endregion -- Check Exist Setting Flow --

                #region -- Check Exist User Approval --
                var isExistUserApproval = acctSettlementPaymentService.CheckExistUserApproval(typeApproval, settlementPaymentCurrent.GroupId, settlementPaymentCurrent.DepartmentId, settlementPaymentCurrent.OfficeId, settlementPaymentCurrent.CompanyId);
                if (!isExistUserApproval.Success)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = isExistUserApproval.Exception.Message };
                    return BadRequest(_result);
                }
                #endregion -- Check Exist User Approval --

                #region -- Check Settlement Payment Approving --
                if (!model.Settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_NEW) && !model.Settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DENIED))
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = "Only allowed to edit the settlement payment status is New or Deny" };
                    return BadRequest(_result);
                }
                #endregion -- Check Settlement Payment Approving --

                model.Settlement.StatusApproval = AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL;
                hs = acctSettlementPaymentService.UpdateSettlementPayment(model);
                message = HandleError.GetMessage(hs, Crud.Update);
                if (hs.Code == 403)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
                }
            }
            
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (hs.Success)
            {
                AcctApproveSettlementModel approve = new AcctApproveSettlementModel
                {
                    SettlementNo = model.Settlement.SettlementNo,
                    Requester = model.Settlement.Requester,
                    RequesterAprDate = DateTime.Now
                };
                var resultInsertUpdateApprove = acctSettlementPaymentService.InsertOrUpdateApprovalSettlement(approve);
                if (!resultInsertUpdateApprove.Success)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = resultInsertUpdateApprove.Exception.Message };
                    return BadRequest(_result);
                }
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
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
            #region -- Check Validate Email Requester --
            var advance = acctSettlementPaymentService.Get(x => x.Id == settlementId).FirstOrDefault();
            if (advance == null) return BadRequest(new ResultHandle { Status = false, Message = "Not found settlement payment" });
            var isValidEmail = acctSettlementPaymentService.CheckValidateMailByUserId(advance.Requester);
            if (!isValidEmail.Success)
            {
                ResultHandle result = new ResultHandle { Status = false, Message = isValidEmail.Exception.Message.Replace("[name]", "requester") };
                return BadRequest(result);
            }
            #endregion -- Check Validate Email Requester --

            #region -- Check Exist User Approval --
            var isExistUserApproval = acctSettlementPaymentService.CheckExistUserApproval(typeApproval, currentUser.GroupId, currentUser.DepartmentId, currentUser.OfficeID, currentUser.CompanyID);
            if (!isExistUserApproval.Success)
            {
                ResultHandle result = new ResultHandle { Status = false, Message = isExistUserApproval.Exception.Message };
                return BadRequest(result);
            }
            #endregion -- Check Exist User Approval --

            var updateApproval = acctSettlementPaymentService.UpdateApproval(settlementId);
            ResultHandle _result;
            if (!updateApproval.Success)
            {
                _result = new ResultHandle { Status = updateApproval.Success, Message = updateApproval.Exception.Message };
                return BadRequest(_result);
            }
            else
            {
                _result = new ResultHandle { Status = updateApproval.Success };
                return Ok(_result);
            }
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
            #region -- Check Validate Email Requester --
            var advance = acctSettlementPaymentService.Get(x => x.Id == settlementId).FirstOrDefault();
            if (advance == null) return BadRequest(new ResultHandle { Status = false, Message = "Not found settlement payment" });
            var isValidEmail = acctSettlementPaymentService.CheckValidateMailByUserId(advance.Requester);
            if (!isValidEmail.Success)
            {
                ResultHandle result = new ResultHandle { Status = false, Message = isValidEmail.Exception.Message.Replace("[name]", "requester") };
                return BadRequest(result);
            }
            #endregion -- Check Validate Email Requester --

            #region -- Check Exist User Approval --
            var isExistUserApproval = acctSettlementPaymentService.CheckExistUserApproval(typeApproval, currentUser.GroupId, currentUser.DepartmentId, currentUser.OfficeID, currentUser.CompanyID);
            if (!isExistUserApproval.Success)
            {
                ResultHandle result = new ResultHandle { Status = false, Message = isExistUserApproval.Exception.Message };
                return BadRequest(result);
            }
            #endregion -- Check Exist User Approval --

            var denieApproval = acctSettlementPaymentService.DeniedApprove(settlementId, comment);
            ResultHandle _result;
            if (!denieApproval.Success)
            {
                _result = new ResultHandle { Status = denieApproval.Success, Message = denieApproval.Exception.Message };
                return BadRequest(_result);
            }
            else
            {
                _result = new ResultHandle { Status = denieApproval.Success };
                return Ok(_result);
            }
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

        /// <summary>
        /// Get list scene charge of settlement by settlementNo
        /// </summary>
        /// <param name="settlementNo">settlementNo that want to retrieve list scene charge</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetListSceneChargeSettlementBySettlementNo")]
        [Authorize]
        public IActionResult GetListSceneChargeSettlementBySettlementNo(string settlementNo)
        {
            //Start change request Modified 14/10/2019 by Andy.Hoa
            //Chỉ search những charge hiện trường theo settlementNo thuộc user current
            var userCurrent = currentUser.UserID;
            var checkSettleOfUser = acctSettlementPaymentService.Get(x => x.UserCreated == userCurrent && x.SettlementNo == settlementNo).Any();
            List<ShipmentChargeSettlement> data = new List<ShipmentChargeSettlement>();
            if (checkSettleOfUser)
            {
                data = acctSettlementPaymentService.GetListShipmentChargeSettlementNoGroup(settlementNo).Where(x => x.IsFromShipment == false).ToList();
            }
            return Ok(data);
            //End change request
        }

        /// <summary>
        /// Copy Charge from Settlement old to settlement new
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CopyCharges")]
        [Authorize]
        public IActionResult CopyCharges(ShipmentsCopyCriteria criteria)
        {
            var data = acctSettlementPaymentService.CopyChargeFromSettlementOldToSettlementNew(criteria);
            return Ok(data);
        }

        /// <summary>
        /// Get settle payment to unlock
        /// </summary>
        /// <param name="keyWords"></param>
        /// <returns></returns>
        [HttpPost("GetSettlePaymentsToUnlock")]
        public IActionResult GetSettlePayments(List<string> keyWords)
        {
            if (keyWords == null) return Ok(new LockedLogResultModel());
            LockedLogResultModel result = acctSettlementPaymentService.GetSettlePaymentsToUnlock(keyWords);
            return Ok(result);
        }

        /// <summary>
        /// Unlock settlement payment
        /// </summary>
        /// <param name="settlePayments"></param>
        /// <returns></returns>
        [HttpPost("UnLock")]
        [Authorize]
        public IActionResult UnLock(List<LockedLogModel> settlePayments)
        {
            var result = acctSettlementPaymentService.UnLock(settlePayments);
            return Ok(result);
        }

        /// <summary>
        /// Export detail settlement payment by settlement id
        /// </summary>
        /// <param name="settlementId">Id of settlement payment</param>
        /// <returns></returns>
        [HttpGet]
        [Route("DetailSettlementPaymentExport")]
        [Authorize]
        public IActionResult DetailSettlementPaymentExport(Guid settlementId)
        {
            var result = acctSettlementPaymentService.SettlementExport(settlementId);
            return Ok(result);
        }

        /// <summary>
        ///Settlement export List within Shipment.
        /// </summary>
        /// <param name="settlementNoList"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryDataSettlementExport")]
        [Authorize]
        public IActionResult Preview(string[] settlementNoList)
        {
            List<SettlementExportGroupDefault> data = acctSettlementPaymentService.QueryDataSettlementExport(settlementNoList);
            return Ok(data);
        }

        [HttpPost]
        [Route("RecallRequest")]
        [Authorize]
        public IActionResult RecalRequest(Guid settlementId)
        {
            HandleState settlementHandleState = acctSettlementPaymentService.RecallRequest(settlementId);
            ResultHandle _result;
            if (!settlementHandleState.Success)
            {
                _result = new ResultHandle { Status = settlementHandleState.Success, Message = settlementHandleState.Exception.Message };
                return BadRequest(_result);
            }
            else
            {
                _result = new ResultHandle { Status = settlementHandleState.Success };
                return Ok(_result);
            }
        }

        private bool CheckDuplicateCharge(CreateUpdateSettlementModel model, out object dataDuplicate)
        {
            dataDuplicate = null;
            var duplicateCharges = model.ShipmentCharge.Where(x =>
                       !string.IsNullOrEmpty(x.ClearanceNo)
                    || !string.IsNullOrEmpty(x.ContNo)
                    || !string.IsNullOrEmpty(x.SeriesNo)
                    || !string.IsNullOrEmpty(x.InvoiceNo)).GroupBy(x => new { x.JobId, x.MBL, x.HBL, x.ChargeCode, x.ChargeId ,x.ClearanceNo, x.ContNo, x.InvoiceNo, x.SeriesNo, x.Notes}).ToList();
            foreach (var charge in duplicateCharges)
            {
                if (charge.Count() > 1)
                {
                    string mes = JsonConvert.SerializeObject(charge);
                    new LogHelper("Settlement_Duplicate_Charge", mes);

                    dataDuplicate = new
                    {
                        charge.Key.JobId,
                        charge.Key.HBL,
                        charge.Key.MBL,
                        charge.Key.ChargeCode,
                        charge.Key.ChargeId
                    };
                    return true;
                }
            }
            return false;
        }

        [HttpGet]
        [Route("GetHistoryDeniedSettlementPayment")]
        public IActionResult GetHistoryDeniedSettlement(string settlementNo)
        {
            var data = acctSettlementPaymentService.GetHistoryDeniedSettlement(settlementNo);
            return Ok(data);
        }
        
        [HttpPost("PreviewMultipleSettlementBySettlementNos")]
        [Authorize]
        public IActionResult PreviewMultipleSettlementBySettlementNos(List<string> settlmentNos)
        {
            var result = acctSettlementPaymentService.PreviewMultipleSettlement(settlmentNos);
            return Ok(result);
        }

        [HttpPut("DenySettlePayments")]
        [Authorize]
        public IActionResult DenySettlePayments(List<Guid> Ids)
        {
            HandleState hs = acctSettlementPaymentService.DenySettlePayments(Ids);

            string message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = Ids };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetPartnerForSettlement")]
        public IActionResult GetPartnerForSettlement(ExistsChargeCriteria criteria)
        {
            var result = acctSettlementPaymentService.GetPartnerForSettlement(criteria);
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckSoaCDNoteIsSynced")]
        public IActionResult CheckSoaCDNoteIsSynced(ExistsChargeCriteria criteria)
        {
            var result = acctSettlementPaymentService.CheckSoaCDNoteIsSynced(criteria);
            return Ok(result);
        }
    }
}