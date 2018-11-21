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
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
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
    public class CatCommonityController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatCommodityService catComonityService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;
        public CatCommonityController(IStringLocalizer<LanguageSub> localizer, ICatCommodityService service, IMapper iMapper, ICurrentUser user)
        {
            stringLocalizer = localizer;
            catComonityService = service;
            mapper = iMapper;
            currentUser = user;
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Get(CatCommodityCriteria criteria)
        {
            var results = catComonityService.Query(criteria);
            return Ok(results);
        }

        [HttpPost]
        [Route("Paging")]
        public IActionResult Get(CatCommodityCriteria criteria, int page, int size)
        {
            var data = catComonityService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult Get(short id)
        {
            var data = catComonityService.First(x => x.Id == id);
            return Ok(data);
        }

        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Post(CatCommodityEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var checkExistMessage = CheckExist(0, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var commonity = mapper.Map<CatCommodityModel>(model);
            commonity.UserCreated = currentUser.UserID;
            commonity.DatetimeCreated = DateTime.Now;
            commonity.Inactive = false;
            var hs = catComonityService.Add(commonity);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Put(short id, CatCommodityEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var commonity = mapper.Map<CatCommodityModel>(model);
            commonity.UserModified = currentUser.UserID;
            commonity.DatetimeModified = DateTime.Now;
            commonity.Id = id;
            if (commonity.Inactive == true)
            {
                commonity.InactiveOn = DateTime.Now;
            }
            var hs = catComonityService.Update(commonity, x => x.Id == id);
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
        public IActionResult Delete(short id)
        {
            var hs = catComonityService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        private string CheckExist(short id, CatCommodityEditModel model)
        {
            string message = string.Empty;
            if (id == 0)
            {
                if (catComonityService.Any(x => x.CommodityNameEn == model.CommodityNameEn || x.CommodityNameVn == model.CommodityNameVn))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            else
            {
                if (catComonityService.Any(x => (x.CommodityNameEn == model.CommodityNameEn || x.CommodityNameVn == model.CommodityNameVn) && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            return message;
        }
    }
}