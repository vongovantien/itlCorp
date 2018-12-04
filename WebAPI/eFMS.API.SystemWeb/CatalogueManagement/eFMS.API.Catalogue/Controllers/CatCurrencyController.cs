using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading;
using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Catalogue.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;
using System.Linq;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Catalogue.Service.Helpers;

namespace eFMS.API.Catalogue.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatCurrencyController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatCurrencyService catCurrencyService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;

        public CatCurrencyController(IStringLocalizer<LanguageSub> localizer, ICatCurrencyService service, IMapper imapper,
            ICurrentUser user)
        {
            stringLocalizer = localizer;
            catCurrencyService = service;
            mapper = imapper;
            currentUser = user;
        }

        [HttpGet]
        [Route("getAll")]
        public IActionResult Get()
        {
            var data = catCurrencyService.Get().OrderBy(x => x.CurrencyName);
            return Ok(data);
        }

        [HttpGet]
        [Route("getById/{id}")]
        public IActionResult Get(string id)
        {
            var data = catCurrencyService.First(x => x.Id == id);
            return Ok(data);
        }

        [HttpPost]
        [Route("paging")]
        public IActionResult Get(CatCurrrencyCriteria criteria, int page, int size)
        {
            var data = catCurrencyService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        [Authorize]
        public IActionResult Post(CatCurrencyModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(string.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var catCurrencyModel = mapper.Map<CatCurrencyModel>(model);
            catCurrencyModel.UserCreated = currentUser.UserID;
            catCurrencyModel.DatetimeCreated = DateTime.Now;
            catCurrencyModel.Inactive = false;
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var hs = catCurrencyService.Add(catCurrencyModel);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut]
        [Route("update")]
        [Authorize]
        public IActionResult Put(CatCurrencyModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model.Id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var catCurrencyModel = mapper.Map<CatCurrencyModel>(model);
            catCurrencyModel.UserModified = currentUser.UserID;
            catCurrencyModel.DatetimeModified = DateTime.Now;         
            if(catCurrencyModel.Inactive == true)
            {
                catCurrencyModel.InactiveOn = DateTime.Now;
            }
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var hs = catCurrencyService.Update(catCurrencyModel);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(string id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = catCurrencyService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }


        private string CheckExist(string id, CatCurrencyModel model)
        {
            string message = string.Empty;
            if (id == string.Empty)
            {
                if (catCurrencyService.Any(x => x.Id.ToLower() == model.Id.ToLower()))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
                if (catCurrencyService.Any(x => x.Id.ToLower() == model.Id.ToLower() && x.CurrencyName.ToLower() == model.CurrencyName.ToLower()))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            else
            {
                if (catCurrencyService.Any(x => x.CurrencyName.ToLower() == model.CurrencyName.ToLower() && x.Id.ToLower() != id.ToLower()))
                {
                    message = stringLocalizer[LanguageSub.MSG_NAME_EXISTED].Value;
                }
            }
            return message;
        }

    }
}