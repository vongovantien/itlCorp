﻿using System.Collections.Generic;
using System.Linq;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using ITL.NetCore.Common;
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
    public class ShipmentController : ControllerBase
    {
        readonly IShipmentService shipmentService;
        private readonly IStringLocalizer stringLocalizer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        public ShipmentController(IShipmentService service, IStringLocalizer<LanguageSub> localizer)
        {
            shipmentService = service;
            stringLocalizer = localizer;
        }

        /// <summary>
        /// get list of shipment available
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetShipmentNotLocked")]
        [Authorize]
        public IActionResult GetShipmentNotLocked()
        {
            var list = shipmentService.GetShipmentNotLocked();
            return Ok(list);
        }

        /// <summary>
        /// get list shipment credit payer
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="productServices"></param>
        /// <returns></returns>
        [HttpGet("GetShipmentsCreditPayer")]
        public IActionResult GetShipmentsCreditPayer(string partner, List<string> productServices)
        {
            var data = shipmentService.GetShipmentsCreditPayer(partner, productServices);
            return Ok(data);
        }

        /// <summary>
        /// Get list shipment copy by search option and keywords
        /// </summary>
        /// <param name="searchOption"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        [HttpPost("GetShipmentsCopyListBySearchOption")]
        [Authorize]
        public IActionResult GetShipmentsCopyListBySearchOption(string searchOption, [FromBody]List<string> keywords)
        {
            var data = shipmentService.GetListShipmentBySearchOptions(searchOption, keywords);
            List<string> shipmentNotExits = new List<string>();
            if (searchOption == "JobNo")
            {
                shipmentNotExits = keywords.Where(x => !data.Select(s => s.JobId).Contains(x)).Select(s => s).ToList();
            }
            else if (searchOption == "Hwbno")
            {
                shipmentNotExits = keywords.Where(x => !data.Select(s => s.HBL).Contains(x)).Select(s => s).ToList();
            }
            else if (searchOption == "Mawb")
            {
                shipmentNotExits = keywords.Where(x => !data.Select(s => s.MBL).Contains(x)).Select(s => s).ToList();
            }
            else if (searchOption == "ClearanceNo")
            {
                shipmentNotExits = keywords.Where(x => !data.Select(s => s.CustomNo).Contains(x)).Select(s => s).ToList();
            }

            var _status = true;
            var _message = string.Empty;
            if (shipmentNotExits.Count > 0)
            {
                _status = false;
                _message = stringLocalizer[DocumentationLanguageSub.MSG_NOT_EXIST_SHIPMENT_COPY, string.Join(", ", shipmentNotExits.Distinct())].Value;
            }
            ResultHandle result = new ResultHandle { Status = _status, Message = _message, Data = data };
            return Ok(result);
        }

        [HttpPost("GetShipmentNotExist")]
        [Authorize]
        public IActionResult GetShipmentNotExist(string typeSearch, [FromBody]List<string> shipments)
        {
            var listShipment = shipmentService.GetShipmentNotDelete();
            List<string> shipmentNotExits = new List<string>();
            if (shipments != null && shipments.Count > 0)
            {
                shipments = shipments.Select(s => s.Trim()).ToList();
                if (typeSearch == "JOBID")
                {
                    shipmentNotExits = shipments.Where(x => !listShipment.Select(s => s.JobId).Contains(x)).Select(s => s).ToList();
                }
                else if (typeSearch == "MBL")
                {
                    shipmentNotExits = shipments.Where(x => !listShipment.Select(s => s.MBL).Contains(x)).Select(s => s).ToList();
                }
                else if (typeSearch == "HBL")
                {
                    shipmentNotExits = shipments.Where(x => !listShipment.Select(s => s.HBL).Contains(x)).Select(s => s).ToList();
                }
                else if (typeSearch == "CustomNo")
                {
                    shipmentNotExits = shipments.Where(x => !listShipment.Select(s => s.CustomNo).Contains(x)).Select(s => s).ToList();
                }
            }
            var _status = false;
            var _message = string.Empty;
            if(shipmentNotExits.Count > 0)
            {
                _status = true;
                _message = stringLocalizer[DocumentationLanguageSub.MSG_NOT_EXIST_SHIPMENT, string.Join(", ", shipmentNotExits)].Value;
            }
            ResultHandle result = new ResultHandle { Status = _status, Message = _message };
            return Ok(result);
        }
        
        [HttpPost("GetShipmentToUnLock")]
        public IActionResult GetShipmentToUnLock(ShipmentCriteria criteria)
        {
            var results = shipmentService.GetShipmentToUnLock(criteria);
            return Ok(results);
        }

        [HttpPost("UnLockShipment")]
        [Authorize]
        public IActionResult UnLockShipment([FromBody]List<LockedLogModel> shipments)
        {
            HandleState result = shipmentService.UnLockShipment(shipments);
            return Ok(result);
        }

        [HttpPost("LockShipmentList")]
        [Authorize]
        public IActionResult LockShipmentList([FromBody]List<string> JobIds)
        {
            HandleState hs = shipmentService.LockShipmentList(JobIds);

            string message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = null };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        /// <summary>
        /// Get data for general report
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetDataGeneralReport")]
        public IActionResult GetDataGeneralReport(GeneralReportCriteria criteria, int page, int size)
        {
            var data = shipmentService.GetDataGeneralReport(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// Query data for general report
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("QueryDataGeneralReport")]
        public IActionResult QueryDataGeneralReport(GeneralReportCriteria criteria)
        {
            var data = shipmentService.QueryDataGeneralReport(criteria);
            return Ok(data);
        }

        /// <summary>
        /// Get data for export shipment overview
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetDataExportShipmentOverview")]
        public IActionResult GetDataExportShipmentOverview(GeneralReportCriteria criteria)
        {
            var data = shipmentService.GetDataGeneralExportShipmentOverview(criteria);
            var result = data;
            return Ok(result);
        }

        /// <summary>
        /// Get data for export accounting P/L
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetDataExportAccountingPlSheet")]
        [Authorize]
        public IActionResult GetDataExportAccountingPlSheet(GeneralReportCriteria criteria)
        {
            var data = shipmentService.GetDataAccountingPLSheet(criteria);
            return Ok(data);
        }

        /// <summary>
        /// Get data for export accounting P/L
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetDataSummaryOfCostsIncurred")]
        [Authorize]
        public IActionResult GetDataSummaryOfCostsIncurred(GeneralReportCriteria criteria)
        {
            var data = shipmentService.GetDataSummaryOfCostsIncurred(criteria);
            return Ok(data);
        }

        /// Get data for export accounting P/L
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetDataSummaryOfRevenueIncurred")]
        [Authorize]
        public IActionResult GetDataSummaryOfRevenueIncurred(GeneralReportCriteria criteria)
        {
            var data = shipmentService.GetDataSummaryOfRevenueIncurred(criteria);
            return Ok(data);
        }

        /// Get data for export costs by partner
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetDataCostsByPartner")]
        [Authorize]
        public IActionResult GetDataCostsByPartner(GeneralReportCriteria criteria)
        {
            var data = shipmentService.GetDataCostsByPartner(criteria);
            return Ok(data);
        }
        /// <summary>
        /// get list of shipment assign or PIC is current user
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetShipmentAssignPIC")]
        [Authorize]
        public IActionResult GetShipmentAssignPIC()
        {
            var list = shipmentService.GetShipmentAssignPIC();
            return Ok(list);
        }

        /// <summary>
        /// Get data for export accounting P/L
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetDataJobProfitAnalysis")]
        [Authorize]
        public IActionResult GetDataJobProfitAnalysis(GeneralReportCriteria criteria)
        {
            var data = shipmentService.GetDataJobProfitAnalysis(criteria);
            return Ok(data);
        }

        /// <summary>
        /// Query data for commission report
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("GetCommissionReport")]
        public IActionResult GetCommissionReport(CommissionReportCriteria criteria, string userId, string rptType)
        {
            var data = shipmentService.GetCommissionReport(criteria, userId, rptType);
            return Ok(data);
        }

        /// <summary>
        /// Query data for incentive report
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("GetIncentiveReport")]
        public IActionResult GetIncentiveReport(CommissionReportCriteria criteria, string userId)
        {
            var data = shipmentService.GetIncentiveReport(criteria, userId);
            return Ok(data);
        }
    }
}
