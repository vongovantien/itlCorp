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
using eFMS.API.Catalogue.Service.Helpers;
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
    public class CatCommodityGroupController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatCommodityGroupService catComonityGroupService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;
        public CatCommodityGroupController(IStringLocalizer<LanguageSub> localizer, ICatCommodityGroupService service, IMapper iMapper,
            ICurrentUser user)
        {
            stringLocalizer = localizer;
            catComonityGroupService = service;
            mapper = iMapper;
            currentUser = user;
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Get(CatCommodityGroupCriteria criteria)
        {
            var results = catComonityGroupService.Query(criteria);
            return Ok(results);
        }

        [HttpPost]
        [Route("Paging")]
        public IActionResult Get(CatCommodityGroupCriteria criteria, int page, int size)
        {
            var data = catComonityGroupService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpGet]
        [Route("GetByLanguage")]
        public IActionResult GetByLanguage()
        {
            var results = catComonityGroupService.GetByLanguage();
            return Ok(results);
        }

        [HttpGet("{id}")]
        public IActionResult Get(short id)
        {
            var data = catComonityGroupService.First(x => x.Id == id);
            return Ok(data);
        }

        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Post(CatCommodityGroupEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var checkExistMessage = CheckExist(0, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var catCommodityGroup = mapper.Map<CatCommodityGroupModel>(model);
            catCommodityGroup.UserCreated = currentUser.UserID;
            catCommodityGroup.DatetimeCreated = DateTime.Now;
            catCommodityGroup.Inactive = false;
            var hs = catComonityGroupService.Add(catCommodityGroup);
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
        public IActionResult Put(short id, CatCommodityGroupEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var commonityGroup = mapper.Map<CatCommodityGroupModel>(model);
            commonityGroup.UserModified = currentUser.UserID;
            commonityGroup.DatetimeModified = DateTime.Now;
            commonityGroup.Id = id;
            if (commonityGroup.Inactive == true)
            {
                commonityGroup.InactiveOn = DateTime.Now;
            }
            var hs = catComonityGroupService.Update(commonityGroup, x => x.Id == id);
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
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = catComonityGroupService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        private string CheckExist(short id, CatCommodityGroupEditModel model)
        {
            string message = string.Empty;
            if (id == 0)
            {
                if (catComonityGroupService.Any(x => x.GroupNameEn == model.GroupNameEn && x.GroupNameVn == model.GroupNameVn))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            else
            {
                if (catComonityGroupService.Any(x => x.GroupNameEn == model.GroupNameEn && x.GroupNameVn == model.GroupNameVn && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            return message;
        }
    }
}