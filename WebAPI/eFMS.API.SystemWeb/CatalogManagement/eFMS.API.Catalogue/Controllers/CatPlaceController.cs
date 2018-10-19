using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Catalogue.Models;
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
            return Ok();
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

        [HttpPost]
        [Route("Add")]
        public IActionResult Post(CatPlaceEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var catPlace = mapper.Map<CatPlaceModel>(model);
            catPlace.Id = new Guid();
            catPlace.UserCreated = "01";
            catPlace.DatetimeCreated = DateTime.Now;
            var result = catPlaceService.Add(catPlace);
            var message = HandleError.GetMessage(result, Crud.Insert);
            if (!result.Success)
            {
                return BadRequest(stringLocalizer[message]);
            }
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult Put(Guid id, CatPlaceEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var catPlace = mapper.Map<CatPlaceModel>(model);
            catPlace.UserModified = "01";
            catPlace.DatetimeModified = DateTime.Now;
            catPlace.Id = id;
            var result = catPlaceService.Update(catPlace, x => x.Id == id);
            var message = HandleError.GetMessage(result, Crud.Update);
            if (!result.Success)
            {
                return BadRequest(stringLocalizer[message]);
            }
            return Ok(stringLocalizer[message]);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var result = catPlaceService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(result, Crud.Delete);
            if (!result.Success)
            {
                return BadRequest(stringLocalizer[message]);
            }
            return Ok(stringLocalizer[message]);
        }
    }
}