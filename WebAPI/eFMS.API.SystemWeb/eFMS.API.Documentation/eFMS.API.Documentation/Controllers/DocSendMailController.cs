using System;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
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
    public class DocSendMailController : ControllerBase
    {
        readonly IDocSendMailService sendMailService;
        private readonly IStringLocalizer stringLocalizer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="localizer"></param>
        public DocSendMailController(IDocSendMailService service, IStringLocalizer<LanguageSub> localizer)
        {
            sendMailService = service;
            stringLocalizer = localizer;
        }

        /// <summary>
        /// Send mail
        /// </summary>
        /// <param name="emailContent"></param>
        /// <returns></returns>
        [HttpPost("SendMailDocument")]
        public IActionResult SendMailDocument(EmailContentModel emailContent)
        {
            var status = sendMailService.SendMailDocument(emailContent);
            var result = new ResultHandle { Status = status, Message = status ? "Send mail successful" : "Send mail failed" };
            return Ok(result);
        }

        /// <summary>
        /// Get info mail housebill of Air Import
        /// </summary>
        /// <param name="hblId">Housebill ID</param>
        /// <returns></returns>
        [HttpGet("GetInfoMailHBLAirImport")]
        [Authorize]
        public IActionResult GetInfoMailHBLAirImport(Guid hblId)
        {
            var data = sendMailService.GetInfoMailHBLAirImport(hblId);
            return Ok(data);
        }

        /// <summary>
        /// Get info mail housebill of Air Export
        /// </summary>
        /// <param name="hblId">Housebill ID</param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet("GetInfoMailHBLAirExport")]
        [Authorize]
        public IActionResult GetInfoMailHBLAirExport(Guid? hblId, Guid? jobId)
        {
            if (hblId != null && hblId != Guid.Empty)
            {
                var data = sendMailService.GetInfoMailHBLAirExport(hblId);
                return Ok(data);
            }
            else
            {
                var data = sendMailService.GetInfoMailAEPreAlert(jobId);
                return Ok(data);
            }
        }

        /// <summary>
        /// Get info mail housebill of Air Export
        /// </summary>
        /// <param name="hblId">Housebill ID</param>
        /// <param name="jobId"></param>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        [HttpGet("GetInfoMailHBLPreAlerSeaExport")]
        [Authorize]
        public IActionResult GetInfoMailHBLPreAlerSeaExport(Guid? hblId, Guid? jobId, string serviceId)
        {
            if (hblId != null && hblId != Guid.Empty)
            {
                var data = sendMailService.GetInfoMailHBLPreAlerSeaExport(hblId, serviceId);
                return Ok(data);
            }
            else
            {
                var data = sendMailService.GetInfoMailPreAlerSeaExport(jobId, serviceId);
                return Ok(data);
            }
        }

        /// <summary>
        /// Get info mail shipping instruction of Sea Export
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns></returns>
        [HttpGet("GetInfoMailSISeaExport")]
        [Authorize]
        public IActionResult GetInfoMailSISeaExport(Guid jobId)
        {
            var data = sendMailService.GetInfoMailSISeaExport(jobId);
            return Ok(data);
        }

        /// <summary>
        /// Get info mail housebill of Sea Import
        /// </summary>
        /// <param name="hblId">Housebill ID</param>
        /// <param name="serviceId">Type of sea service</param>
        /// <returns></returns>
        [HttpGet("GetInfoMailHBLSeaImport")]
        [Authorize]
        public IActionResult GetInfoMailHBLSeaImport(Guid hblId, string serviceId)
        {
            var data = sendMailService.GetInfoMailHBLSeaImport(hblId, serviceId);
            return Ok(data);
        }

        /// <summary>
        /// Send mail alert to customer cash with outstanding debit
        /// </summary>
        /// <returns></returns>
        [HttpGet("SendMailContractCashWithOutstandingDebit")]
        public IActionResult SendMailContractCashWithOutstandingDebit()
        {
            var hs = sendMailService.SendMailContractCashWithOutstandingDebit();
            if (hs)
            {
                ResultHandle result = new ResultHandle { Status = hs, Message = "Send Successful!" };
                return Ok(result);
            }
            return BadRequest("Send Fail!");
        }

        /// <summary>
        /// Get detail shipment oustanding debit for saleman
        /// </summary>
        /// <param name="salemanId"></param>
        /// <returns></returns>
        [HttpGet("GetDataOustandingDebit")]
        public IActionResult GetDataOustandingDebit(string salemanId)
        {
            var hs = sendMailService.GetDataOustandingDebit(salemanId);
            return Ok(hs);
        }
    }
}