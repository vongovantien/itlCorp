using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Log.DL.IService;
using eFMS.API.Log.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemManagementAPI.Infrastructure.Middlewares;

namespace eFMS.API.Log.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CatCurrencyController : ControllerBase
    {
        private readonly ICatCurrencyService catCurrencyService;
        public CatCurrencyController(ICatCurrencyService service)
        {
            catCurrencyService = service;
        }

        //[HttpGet]
        //public IActionResult Get()
        //{
        //    var results = catCurrencyService.GetAll();
        //    return Ok(results);
        //}
        [HttpGet]
        public IEnumerable<CatCurrency> Get()
        {
            try
            {
                return catCurrencyService.Get();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
        [HttpGet]
        [Route("Paging")]
        public IActionResult Paging(string query, int page, int size)
        {
            try
            {
                var data = catCurrencyService.Paging(query, page, size, out long rowCount);
                var result = new { data, totalItems = rowCount, page, size };
                return Ok(result);
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
    }
}