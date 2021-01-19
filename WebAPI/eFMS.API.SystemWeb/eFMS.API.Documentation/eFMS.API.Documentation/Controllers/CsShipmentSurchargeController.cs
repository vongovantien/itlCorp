﻿using System;
using System.Collections.Generic;
using System.Linq;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
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
    public class CsShipmentSurchargeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsShipmentSurchargeService csShipmentSurchargeService;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="user"></param>
        public CsShipmentSurchargeController(IStringLocalizer<LanguageSub> localizer, ICsShipmentSurchargeService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            csShipmentSurchargeService = service;
            currentUser = user;
        }

        /// <summary>
        /// get list of surcharge by house bill and type
        /// </summary>
        /// <param name="hbId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetByHB")]
        public IActionResult GetByHouseBill(Guid hbId, string type)
        {
            return Ok(csShipmentSurchargeService.GetByHB(hbId, type));
        }

        /// <summary>
        /// get list of surcharge by house bill anf partner id
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="partnerID"></param>
        /// <param name="IsHouseBillID"></param>
        /// <param name="cdNoteCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GroupByListHB")]
        public List<GroupChargeModel> GetByListHouseBill(Guid Id, string partnerID, bool IsHouseBillID, string cdNoteCode)
        {
            return csShipmentSurchargeService.GroupChargeByHB(Id, partnerID, IsHouseBillID, cdNoteCode);
        }

        /// <summary>
        /// get surcharge by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetBy")]
        public IActionResult GetBy(Guid id)
        {
            var result = csShipmentSurchargeService.Get(x => x.Id == id);
            return Ok(result);
        }

        /// <summary>
        /// get partners have surcharge
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="IsHouseBillID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPartners")]
        [Authorize]
        public List<CatPartner> GetPartners(Guid Id, bool IsHouseBillID)
        {
            return csShipmentSurchargeService.GetAllParner(Id, IsHouseBillID);
        }


        /// <summary>
        /// get profit of a house bill
        /// </summary>
        /// <param name="hblid"></param>
        /// <returns></returns>
        [HttpGet("GetHouseBillProfit")]
        public IActionResult GetHouseBillProfit(Guid hblid)
        {
            var result = csShipmentSurchargeService.GetHouseBillTotalProfit(hblid);
            return Ok(result);
        }


        /// <summary>
        /// get profit of a shipment
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet("GetshipmentProfit")]
        [Authorize]
        public IActionResult GetshipmentProfit(Guid jobId)
        {
            var result = csShipmentSurchargeService.GetShipmentTotalProfit(jobId);
            return Ok(result);
        }

        /// <summary>
        /// add new surcharge
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult AddNew(CsShipmentSurchargeModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserCreated = currentUser.UserID;
            model.Id = Guid.NewGuid();
            model.ExchangeDate = DateTime.Now;
            model.DatetimeCreated = DateTime.Now;
            model.Total = Math.Round(model.Total, model.CurrencyId != DocumentConstants.CURRENCY_LOCAL ? 3 : 0);
            var hs = csShipmentSurchargeService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// add list surcharge
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [HttpPost("AddAndUpdate")]
        [Authorize]
        public IActionResult Add([FromBody]List<CsShipmentSurchargeModel> list)
        {
            if (!ModelState.IsValid) return BadRequest();

            string type = list.Select(t => t.Type).FirstOrDefault();
            if(type == "BUY" || type == "OBH")
            {
                var query = list.Where(x => !isSurchargeSpecialCase(x) && !string.IsNullOrEmpty(x.InvoiceNo)).GroupBy(x => new { x.InvoiceNo, x.ChargeId })
                             .Where(g => g.Count() > 1)
                             .Select(y => y.Key);

                if (query.Any())
                {
                    return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[DocumentationLanguageSub.MSG_SURCHARGE_ARE_DUPLICATE_INVOICE].Value });
                }
            }
            list.ForEach(fe => {
                fe.Total = Math.Round(fe.Total, fe.CurrencyId != DocumentConstants.CURRENCY_LOCAL ? 3 : 0); //Làm tròn charge VND
            });
            var hs = csShipmentSurchargeService.AddAndUpdate(list);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }


        /// <summary>
        /// check account receivable credit term
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [HttpPost("NotificationCreditTerm")]
        [Authorize]
        public IActionResult NotificationCreditTerm([FromBody]List<CsShipmentSurchargeModel> list)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = csShipmentSurchargeService.NotificationCreditTerm(list);
            return Ok(hs.Success);
        }


        /// <summary>
        /// check account receivable expired agreement
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [HttpPost("NotificationExpiredAgreement")]
        [Authorize]
        public IActionResult NotificationExpiredAgreement([FromBody]List<CsShipmentSurchargeModel> list)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = csShipmentSurchargeService.NotificationExpiredAgreement(list);
            return Ok(hs.Success);
        }


        /// <summary>
        /// check account receivable payment terrm
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [HttpPost("NotificationPaymentTerm")]
        [Authorize]
        public IActionResult CheckAccountReceivablePaymentTerm([FromBody]List<CsShipmentSurchargeModel> list)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = csShipmentSurchargeService.NotificationPaymenTerm(list);
            return Ok(hs.Success);
        }

        /// <summary>
        /// check valid account receivable
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [HttpPost("CheckAccountReceivable")]
        public IActionResult CheckValidAccountReceivable([FromBody]List<CsShipmentSurchargeModel> list)
        {
            if (!ModelState.IsValid) return BadRequest();
            object rs = csShipmentSurchargeService.CheckAccountReceivable(list);
            return Ok(rs);
        }
        /// <summary>
        /// update an existed surcharge
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(CsShipmentSurchargeModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserModified = currentUser.UserID;
            model.DatetimeModified = DateTime.Now;
            model.Total = Math.Round(model.Total, model.CurrencyId != DocumentConstants.CURRENCY_LOCAL ? 3 : 0);
            var hs = csShipmentSurchargeService.Update(model, x => x.Id == model.Id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }

        /// <summary>
        /// delete an existed surcharge
        /// </summary>
        /// <param name="chargId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(Guid chargId)
        {
            var hs = csShipmentSurchargeService.DeleteCharge(chargId);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Get list charge shipment by conditions
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("ListChargeShipment")]
        public ChargeShipmentResult ListChargeShipment(ChargeShipmentCriteria criteria)
        {
            var data = csShipmentSurchargeService.GetListChargeShipment(criteria);
            return data;
        }

        /// <summary>
        /// get total profit of a house bill
        /// </summary>
        /// <param name="hblid"></param>
        /// <returns></returns>
        [HttpGet("GetHouseBillTotalProfit")]
        public IActionResult GetHouseBillTotalProfit(Guid hblid)
        {
            var result = csShipmentSurchargeService.GetHouseBillTotalProfit(hblid);
            return Ok(result);
        }

        /// <summary>
        /// get total profit of a shipment
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet("GetShipmentTotalProfit")]
        [Authorize]
        public IActionResult GetShipmentTotalProfit(Guid jobId)
        {
            var result = csShipmentSurchargeService.GetShipmentTotalProfit(jobId);
            return Ok(result);
        }

        /// <summary>
        /// get list of surcharge not exists in cd note by house bill and partner id
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GroupByListHBNotExistsCDNote")]
        public List<GroupChargeModel> GroupByListHBNotExists(GroupByListHBCriteria criteria)
        {
            var dataSearch = csShipmentSurchargeService.GroupChargeByHB(criteria.Id, criteria.partnerID, criteria.IsHouseBillID, string.Empty);
            var dataTmp = dataSearch;
            //Ds idcharge Param            
            List<Guid> chargeIdParam = new List<Guid>();
            foreach (var charges in criteria.listData)
            {
                foreach (var charge in charges.listCharges)
                {
                    chargeIdParam.Add(charge.Id);
                }
            }
            
            for (int i = 0; i < dataTmp.Count; i++)
            {
                //Lấy ra ds charge có chứa idCharge
                var charge = dataTmp[i].listCharges.Where(x => chargeIdParam.Contains(x.Id)).ToList();
                for (int j = 0; j < charge.Count(); j++)
                {
                    var hs = dataSearch[i].listCharges.Remove(charge[j]);
                }
                //if (dataTmp[i].listCharges.Count() == 0)
                //{
                //    var hs = dataSearch.Remove(dataTmp[i]);
                //}
            }

            return dataSearch;
        }

        /// <summary>
        /// get recently charges
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetRecentlyCharges")]
        [Authorize]
        public IActionResult GetRecentlyCharges(RecentlyChargeCriteria criteria)
        {
            var results = csShipmentSurchargeService.GetRecentlyCharges(criteria);
            return Ok(results);
        }

        private bool isSurchargeSpecialCase(CsShipmentSurcharge charge)
        {
            return !string.IsNullOrEmpty(charge.Soano)
            || !string.IsNullOrEmpty(charge.CreditNo)
            || !string.IsNullOrEmpty(charge.DebitNo)
            || !string.IsNullOrEmpty(charge.SettlementCode)
            || !string.IsNullOrEmpty(charge.VoucherId)
            ;
        }
    }
}


