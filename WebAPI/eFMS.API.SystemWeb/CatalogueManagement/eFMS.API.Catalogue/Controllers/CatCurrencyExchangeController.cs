﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

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
        public CatCurrencyExchangeController(IStringLocalizer<LanguageSub> localizer, ICatCurrencyExchangeService service, IMapper imapper)
        {
            stringLocalizer = localizer;
            catCurrencyExchangeService = service;
            mapper = imapper;
        }

        [HttpPost]
        [Route("GetExchangeRateHistory/Paging")]
        public IActionResult GetExchangeRateHistory(CatCurrencyExchangeCriteria criteria, int pageNumber, int pageSize)
        {
            var data = catCurrencyExchangeService.Paging(criteria, pageNumber, pageSize, out int rowCount);
            var result = new { data, totalItems = rowCount, pageNumber, pageSize };
            return Ok(result);
        }

        [HttpGet("GetNewest")]
        public IActionResult GetNewest()
        {
            var result = catCurrencyExchangeService.GetCurrencyExchangeNewest();
            return Ok(result);
        }
        [HttpGet("GetExchangeRates")]
        public IActionResult GetExchangeRates(DateTime date, string localCurrency, string createdBy)
        {
            var result = catCurrencyExchangeService.GetExchangeRates(date, localCurrency, createdBy);
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
        public IActionResult Put(CatCurrencyExchangeEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
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
    }
}