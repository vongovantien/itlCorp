using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Catalogue.Models;
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
    public class CatPlaceController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatPlaceService catPlaceService;
        private readonly IMapper mapper;
        public CatPlaceController(IStringLocalizer<LanguageSub> localizer, ICatPlaceService service, IMapper iMapper)
        {
            stringLocalizer = localizer;
            catPlaceService = service;
            mapper = iMapper;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var results = catPlaceService.Get();
            return Ok(results);
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Get(CatPlaceCriteria criteria)
        {
            var results = catPlaceService.Query(criteria);
            return Ok(results);
        }

        [HttpPost]
        [Route("Paging")]
        public IActionResult Get(CatPlaceCriteria criteria, int page, int size)
        {
            var data = catPlaceService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var data = catPlaceService.First(x => x.Id == id);
            return Ok(data);
        }

        [HttpGet]
        [Route("GetProvinces")]
        public IActionResult GetProvinces(short? countryId)
        {
            var results = catPlaceService.GetProvinces(countryId);
            return Ok(results);
        }

        [HttpGet]
        [Route("GetDistricts")]
        public IActionResult GetDistricts(Guid? provinceId)
        {
            var results = catPlaceService.GetDistricts(provinceId);
            return Ok(results);
        }

        [HttpPost]
        [Route("Add")]
        public IActionResult Post(CatPlaceEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(Guid.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            model.PlaceTypeId = PlaceTypeEx.GetPlaceType(model.PlaceType);
            var catPlace = mapper.Map<CatPlaceModel>(model);
            catPlace.Id = Guid.NewGuid();
            catPlace.UserCreated = "01";
            catPlace.DatetimeCreated = DateTime.Now;
            var hs = catPlaceService.Add(catPlace);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult Put(Guid id, CatPlaceEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var catPlace = mapper.Map<CatPlaceModel>(model);
            catPlace.UserModified = "01";
            catPlace.DatetimeModified = DateTime.Now;
            catPlace.Id = id;
            var hs = catPlaceService.Update(catPlace, x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var hs = catPlaceService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private string CheckExist(Guid id, CatPlaceEditModel model)
        {
            string message = string.Empty;
            if (id == Guid.Empty)
            {
                if (catPlaceService.Any(x => x.Code == model.Code))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            else
            {
                if (catPlaceService.Any(x => x.Code == model.Code && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            return message;
        }
    }
}