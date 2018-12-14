using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
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
        private readonly ICurrentUser currentUser;

        public CatStageController(IStringLocalizer<LanguageSub> localizer, ICatStageService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            catStageService = service;
            currentUser = user;
        }


        [HttpPost]
        [Route("getAll/{pageNumber}/{pageSize}")]
        public IActionResult Get(CatStageCriteria criteria,int pageNumber,int pageSize)
        {
            var data = catStageService.GetStages(criteria, pageNumber, pageSize, out int rowCount);
            var result = new { data, totalItems = rowCount, pageNumber, pageSize };
            return Ok(result);
        }

        [HttpPost]
        [Route("query")]
        public IActionResult Get(CatStageCriteria criteria)
        {
            var data = catStageService.Query(criteria);      
            return Ok(data);
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
        [Authorize]
        public IActionResult AddStage(CatStageModel catStageModel)
        {
            catStageModel.DatetimeCreated = DateTime.Now;
            catStageModel.UserCreated = currentUser.UserID;
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
        [Authorize]
        public IActionResult UpdateStage(CatStageModel catStageModel)
        {
            catStageModel.DatetimeModified = DateTime.Now;
            catStageModel.UserModified = currentUser.UserID;
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
        [Authorize]
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