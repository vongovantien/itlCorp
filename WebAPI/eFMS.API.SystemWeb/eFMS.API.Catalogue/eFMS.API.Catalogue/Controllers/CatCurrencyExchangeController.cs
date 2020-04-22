using System;
using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Catalogue.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatCurrencyExchangeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatCurrencyExchangeService catCurrencyExchangeService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface ICatCurrencyExchangeService</param>
        /// <param name="imapper">inject interface IMapper</param>
        /// <param name="user">inject interface ICurrentUser</param>
        public CatCurrencyExchangeController(IStringLocalizer<LanguageSub> localizer, ICatCurrencyExchangeService service, IMapper imapper, ICurrentUser user)
        {
            stringLocalizer = localizer;
            catCurrencyExchangeService = service;
            mapper = imapper;
            currentUser = user;
        }

        /// <summary>
        /// get and paging currency exchage rate
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve dat</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetExchangeRateHistory/Paging")]
        public IActionResult GetExchangeRateHistory(CatCurrencyExchangeCriteria criteria, int page, int size)
        {
            var data = catCurrencyExchangeService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// get currnecy
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetCurrencies")]
        public IActionResult GetCurrencies()
        {
            var result = catCurrencyExchangeService.GetCurrency();
            return Ok(result);
        }

        /// <summary>
        /// get newest exchage rate
        /// </summary>
        /// <param name="currencyToId"></param>
        /// <returns></returns>
        [HttpGet("GetNewest")]
        public IActionResult GetNewest(string currencyToId)
        {
            var result = catCurrencyExchangeService.GetCurrencyExchangeNewest(currencyToId);
            return Ok(result);
        }

        /// <summary>
        /// get exchange rate by condition
        /// </summary>
        /// <param name="date"></param>
        /// <param name="localCurrency"></param>
        /// <param name="fromCurrency"></param>
        /// <returns></returns>
        [HttpGet("GetExchangeRatesBy")]
        public IActionResult GetExchangeRates(DateTime date, string localCurrency, string fromCurrency)
        {
            var result = catCurrencyExchangeService.GetExchangeRates(date, localCurrency, fromCurrency);
            return Ok(result);
        }

        [HttpGet("ConvertRate")]
        public IActionResult ConvertRate(DateTime date, string localCurrency, string fromCurrency)
        {
            var result = catCurrencyExchangeService.ConvertRate(date, localCurrency, fromCurrency);
            return Ok(result);
        }

        /// <summary>
        /// get data by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var result = catCurrencyExchangeService.Get(x => x.Id == id);
            return Ok(result);
        }

        /// <summary>
        /// update exchange rate
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateRate")]
        [Authorize]
        public IActionResult Put(CatCurrencyExchangeEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserModified = currentUser.UserID;
            var hs = catCurrencyExchangeService.UpdateRate(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public IActionResult Put(int id, CatCurrencyExchangeModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserModified = currentUser.UserID;
            model.DatetimeModified = DateTime.Now;
            var hs = catCurrencyExchangeService.Update(model, x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of data that need to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var hs = catCurrencyExchangeService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// remove an exchange rate
        /// </summary>
        /// <param name="currencyFrom"></param>
        /// <returns></returns>
        [HttpDelete("RemoveExchangeCurrency")]
        [Authorize]
        public IActionResult RemoveExchangeCurrency(string currencyFrom)
        {
            var hs = catCurrencyExchangeService.RemoveExchangeCurrency(currencyFrom, currentUser.UserID);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}