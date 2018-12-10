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
using eFMS.API.Catalogue.Service.Helpers;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
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
    public class CatUnitController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatUnitService catUnitService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;

        public CatUnitController(IStringLocalizer<LanguageSub> localizer,ICatUnitService service, IMapper imapper, ICurrentUser user)
        {
            stringLocalizer = localizer;
            catUnitService = service;
            mapper = imapper;
            currentUser = user;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var result = catUnitService.Get();
            return Ok(result);
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Get(CatUnitCriteria criteria)
        {
            var result = catUnitService.Query(criteria);
            return Ok(result);
        }

        [HttpPost]
        [Route("Paging")]
        public IActionResult Get(CatUnitCriteria criteria,int page,int size)
        {
            var data = catUnitService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var data = catUnitService.First(x => x.Id == id);
            return Ok(data);
        }

        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Post(CatUnitModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(0, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var catUnit = mapper.Map<CatUnitModel>(model);
            catUnit.UserCreated = currentUser.UserID;
            catUnit.DatetimeCreated = DateTime.Now;
            catUnit.Inactive = false;
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var hs = catUnitService.Add(catUnit);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);  
        }

        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Put(CatUnitModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model.Id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var catUnit = mapper.Map<CatUnitModel>(model);
            catUnit.UserModified = currentUser.UserID;
            catUnit.DatetimeModified = DateTime.Now;
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var hs = catUnitService.Update(catUnit,x=>x.Id==model.Id);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }


        [HttpDelete]
        [Route("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = catUnitService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }



        private string CheckExist(int id, CatUnitModel model)
        {
            string message = string.Empty;
            if (id == 0)
            {
                if (catUnitService.Any(x => (x.Code.ToLower() == model.Code.ToLower()) || (x.UnitNameEn.ToLower() == model.UnitNameEn.ToLower()) || (x.UnitNameVn.ToLower() == model.UnitNameEn.ToLower())))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            else
            {
                if (catUnitService.Any(x => ((x.Code.ToLower() == model.Code.ToLower()) || (x.UnitNameEn.ToLower() == model.UnitNameEn.ToLower()) || (x.UnitNameVn.ToLower() == model.UnitNameVn.ToLower())) && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            return message;
        }
    }
}