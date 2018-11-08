using System;
using System.Globalization;
using System.Threading;
using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Catalogue.Models;
using eFMS.API.Common;
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
    public class CatCurrencyController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatCurrencyService catCurrencyService;
        private readonly IMapper mapper;

        public CatCurrencyController(IStringLocalizer<LanguageSub> localizer, ICatCurrencyService service, IMapper imapper)
        {
            stringLocalizer = localizer;
            catCurrencyService = service;
            mapper = imapper;
        }

        [HttpGet]
        [Route("getAll")]
        public IActionResult Get()
        {
            var data = catCurrencyService.Get();
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
        public IActionResult Post(CatCurrencyModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(string.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var catCurrencyModel = mapper.Map<CatCurrencyModel>(model);
            catCurrencyModel.UserCreated = "01";
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
        public IActionResult Put(CatCurrencyModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model.Id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var catCurrencyModel = mapper.Map<CatCurrencyModel>(model);
            catCurrencyModel.UserModified = "01";
            catCurrencyModel.DatetimeModified = DateTime.Now;           
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var hs = catCurrencyService.Update(catCurrencyModel,x=>x.Id==model.Id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete/{id}")]
        public IActionResult Delete(string id)
        {
            var hs = catCurrencyService.Delete(x => x.Id == id);
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
                if (catCurrencyService.Any(x => (x.Id.ToLower() == model.Id.ToLower())))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            else
            {
                if (catCurrencyService.Any(x => ((x.Id.ToLower() == model.Id.ToLower())) && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            return message;
        }

    }
}