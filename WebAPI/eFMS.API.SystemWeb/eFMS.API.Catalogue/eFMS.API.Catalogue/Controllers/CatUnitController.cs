using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;

namespace eFMS.API.Catalogue.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatUnitController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatUnitService catUnitService;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject IStringLocalizer service</param>
        /// <param name="service">inject ICatUnitService service</param>
        public CatUnitController(IStringLocalizer<LanguageSub> localizer,ICatUnitService service, ICurrentUser currUser)
        {
            stringLocalizer = localizer;
            catUnitService = service;
            currentUser = currUser;
        }

        /// <summary>
        /// get all units
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var result = catUnitService.Get();
            return Ok(result);
        }

        /// <summary>
        /// get all unit types
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetUnitTypes")]
        public IActionResult GetUnitTypes()
        {
            return Ok(catUnitService.GetUnitTypes());
        }

        /// <summary>
        /// get the list of units
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Query")]
        public IActionResult Get(CatUnitCriteria criteria)
        {
            var result = catUnitService.Query(criteria);
            return Ok(result);
        }

        /// <summary>
        /// get and paging the list of units by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        public IActionResult Get(CatUnitCriteria criteria,int page,int size)
        {
            var data = catUnitService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// get unit by id
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult Get(short id)
        {
            var data = catUnitService.Get(x => x.Id == id);
            return Ok(data);
        }

        /// <summary>
        /// add new unit
        /// </summary>
        /// <param name="model">object to delete</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Post(CatUnitModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (model.Code.Contains("´")) model.Code = model.Code.Replace("´", "'");
            if (model.UnitNameEn.Contains("´")) model.UnitNameEn = model.UnitNameEn.Replace("´", "'");
            var checkExistMessage = CheckExist(0, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            model.UserCreated = model.UserModified = currentUser.UserID;
            model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
            model.Active = true;
            var hs = catUnitService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            if (!hs.Success)
            {
                if(message == LanguageSub.MSG_DATA_NOT_FOUND)
                {
                    return BadRequest(new ResultHandle { Status = hs.Success, Message = hs.Exception.Message });
                }
                return BadRequest(new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value });
            }
            return Ok(new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value });  
        }

        /// <summary>
        /// update an existed item
        /// </summary>
        /// <param name="model">object to update</param>
        /// <returns></returns>
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
            var hs = catUnitService.Update(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            if (!hs.Success)
            {
                if (message == LanguageSub.MSG_DATA_NOT_FOUND)
                {
                    return BadRequest(new ResultHandle { Status = hs.Success, Message = hs.Exception.Message });
                }
                return BadRequest(new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value });
            }
            return Ok(new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value });
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of data that want to delete</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete/{id}")]
        public IActionResult Delete(short id)
        {
            var hs = catUnitService.Delete(id);

            var message = HandleError.GetMessage(hs, Crud.Delete);
            if (!hs.Success)
            {
                if (message == LanguageSub.MSG_DATA_NOT_FOUND)
                {
                    return BadRequest(new ResultHandle { Status = hs.Success, Message = hs.Exception.Message });
                }
                return BadRequest(new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value });
            }
            return Ok(new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value });
        }


        private string CheckExist(int id, CatUnitModel model)
        {
            string message = string.Empty;
            if (id == 0)
            {
                if (catUnitService.Any(x => (x.Code.ToLower() == model.Code.ToLower()) || string.IsNullOrEmpty(model.Code)))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
                else if(catUnitService.Any(x => x.UnitNameEn.ToLower() == model.UnitNameEn.ToLower() || string.IsNullOrEmpty(model.UnitNameEn))){
                    message = stringLocalizer[CatalogueLanguageSub.MSG_NAME_EN_EXISTED].Value;
                }
                else if(catUnitService.Any(x => x.UnitNameVn.ToLower() == model.UnitNameVn.ToLower() || string.IsNullOrEmpty(model.UnitNameVn))){
                    message = stringLocalizer[CatalogueLanguageSub.MSG_NAME_LOCAL_EXISTED].Value;
                }
            }
            else
            {
                if (catUnitService.Any(x => (x.Code.ToLower() == model.Code.ToLower() && x.Id != id) || string.IsNullOrEmpty(model.Code)))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
                else if(catUnitService.Any(x => (x.UnitNameEn.ToLower() == model.UnitNameEn.ToLower() && x.Id != id) || string.IsNullOrEmpty(model.UnitNameEn)))
                {
                    message = stringLocalizer[CatalogueLanguageSub.MSG_NAME_EN_EXISTED].Value;
                }
                else if(catUnitService.Any(x => (x.UnitNameVn.ToLower() == model.UnitNameVn.ToLower() && x.Id != id) || string.IsNullOrEmpty(model.UnitNameVn)))
                {
                    message = stringLocalizer[CatalogueLanguageSub.MSG_NAME_LOCAL_EXISTED].Value;
                }
            }
            return message;
        }
    }
}