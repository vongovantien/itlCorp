using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Common;
using ITL.NetCore.Common;
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
    public class CatStageController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatStageService catStageService;

        public CatStageController(IStringLocalizer<LanguageSub> localizer, ICatStageService service)
        {
            stringLocalizer = localizer;
            catStageService = service;
        }


        [HttpPost]
        [Route("getAll/{pageNumber}/{pageSize}")]
        public IActionResult Get(CatStageCriteria criteria,int pageNumber,int pageSize)
        {
            var data = catStageService.GetStages(criteria, pageNumber, pageSize, out int rowCount);
            var result = new { data, totalItems = rowCount, pageNumber, pageSize };
            return Ok(result);
        }

        [HttpGet]
        [Route("getById/{id}")]
        public IActionResult Get(int id)
        {
            var results = catStageService.Get(x => x.Id == id);
            return Ok(results);
        }

        [HttpPost]
        [Route("addNew")]
        public IActionResult AddStage(CatStageModel catStageModel)
        {
            catStageModel.DatetimeCreated = DateTime.Now;
            catStageModel.UserCreated = "Thor";
            var hs = catStageService.Add(catStageModel);
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
        public IActionResult UpdateStage(CatStageModel catStageModel)
        {
            catStageModel.DatetimeModified = DateTime.Now;
            var hs = catStageService.Update(catStageModel,x=>x.Id==catStageModel.Id);
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
        public IActionResult DeleteStage(int id)
        {
            var hs = catStageService.Delete(x => x.Id == id);
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