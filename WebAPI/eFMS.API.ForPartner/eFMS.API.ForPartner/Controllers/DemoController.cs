﻿using eFMS.API.ForPartner.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace eFMS.API.ForPartner.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class DemoController : ControllerBase
    {

        /// <summary>
        /// Contructor controller Accounting Report
        /// </summary>
        public DemoController()
        {
        }
        /// <summary>
        /// Get all value
        /// </summary>
        /// 
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Ok");
        }
    }
}