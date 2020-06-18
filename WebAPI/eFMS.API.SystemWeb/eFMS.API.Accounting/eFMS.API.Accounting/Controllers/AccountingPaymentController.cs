using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common.Globals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Accounting.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AccountingPaymentController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAccAccountingPaymentService accountingPaymentService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="paymentService"></param>
        public AccountingPaymentController(IStringLocalizer<LanguageSub> localizer,
            IAccAccountingPaymentService paymentService)
        {
            stringLocalizer = localizer;
            accountingPaymentService = paymentService;
        }
        
        /// <summary>
        /// query and paging VAT invoice / SOA
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost("Paging")]
        public IActionResult PagingPayment(PaymentCriteria criteria, int pageNumber, int pageSize)
        {
            var data = accountingPaymentService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }
        /// <summary>
        /// get list payment by refNo
        /// </summary>
        /// <param name="refNo"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetBy(string refNo)
        {
            var results = accountingPaymentService.GetBy(refNo);
            return Ok(results);
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
