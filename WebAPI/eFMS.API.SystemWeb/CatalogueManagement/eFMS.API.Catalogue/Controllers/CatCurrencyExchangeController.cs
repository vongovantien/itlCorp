using System;
using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Catalogue.Resources;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Catalogue.Controllers
{
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
        public CatCurrencyExchangeController(IStringLocalizer<LanguageSub> localizer, ICatCurrencyExchangeService service, IMapper imapper, ICurrentUser user)
        {
            stringLocalizer = localizer;
            catCurrencyExchangeService = service;
            mapper = imapper;
            currentUser = user;
        }

        [HttpPost]
        [Route("GetExchangeRateHistory/Paging")]
        public IActionResult GetExchangeRateHistory(CatCurrencyExchangeCriteria criteria, int page, int size)
        {
            var data = catCurrencyExchangeService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }
        [HttpGet("GetCurrencies")]
        public IActionResult GetCurrencies()
        {
            var result = catCurrencyExchangeService.GetCurrency();
            return Ok(result);
        }
        [HttpGet("GetNewest")]
        public IActionResult GetNewest()
        {
            var result = catCurrencyExchangeService.GetCurrencyExchangeNewest();
            return Ok(result);
        }
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

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var result = catCurrencyExchangeService.Get(x => x.Id == id);
            return Ok(result);
        }

        [HttpPost]
        [Route("Add")]
        public IActionResult Post(CatCurrencyExchangeModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.Inactive = false;
            var hs = catCurrencyExchangeService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

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