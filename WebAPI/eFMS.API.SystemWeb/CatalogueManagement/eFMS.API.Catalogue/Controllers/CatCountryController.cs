using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Common;
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
    public class CatCountryController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatCountryService catCountryService;
        public CatCountryController(IStringLocalizer<LanguageSub> localizer, ICatCountryService service)
        {
            stringLocalizer = localizer;
            catCountryService = service;
        }

        [HttpPost]
        [Route("getAll/{pageNumber}/{pageSize}")]
        public IActionResult Get(CatCountryCriteria criteria,int pageNumber,int pageSize)
        {
            var data = catCountryService.GetCountries(criteria,pageNumber,pageSize, out int rowCount);
            var result = new { data, totalItems = rowCount, pageNumber, pageSize };
            return Ok(result);
        }

        [HttpGet]
        [Route("getById/{id}")]
        public IActionResult Get(int id)
        {
            var result = catCountryService.Get(x => x.Id == id).FirstOrDefault();
            return Ok(result);
        }

        [HttpPost]
        [Route("addNew")]
        public IActionResult Add(CatCountryModel catCountry)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(0, catCountry);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            catCountry.DatetimeCreated = DateTime.Now;
            catCountry.UserCreated = "Thor";
            catCountry.Inactive = true;
            var hs = catCountryService.Add(catCountry);
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
        public IActionResult Upadte(CatCountryModel catCountry)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(catCountry.Id, catCountry);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            catCountry.DatetimeModified = DateTime.Now;
            catCountry.UserModified = "Thor";
            var hs = catCountryService.Update(catCountry,x=>x.Id==catCountry.Id);
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
        public IActionResult Delete(int id)
        {
            var hs = catCountryService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetByLanguage")]
        public IActionResult GetByLanguage()
        {
            var results = catCountryService.GetByLanguage();
            return Ok(results);
        }


        private string CheckExist(int id, CatCountryModel model)
        {
            string message = string.Empty;
            if (id == 0)
            {
                if (catCountryService.Any(x => x.Code.ToLower() == model.Code.ToLower()))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            else
            {
                if (catCountryService.Any(x => x.Code.ToLower() == model.Code.ToLower() && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            return message;
        }
    }
}