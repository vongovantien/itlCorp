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
    public class CatChargeDefaultAccountController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatChargeDefaultAccountService catChargeDefaultAccountService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;

        public CatChargeDefaultAccountController(IStringLocalizer<LanguageSub> localizer, ICatChargeDefaultAccountService service, IMapper imapper, ICurrentUser user)
        {
            stringLocalizer = localizer;
            catChargeDefaultAccountService = service;
            mapper = imapper;
            currentUser = user;
        }

        [HttpGet]
        [Route("getAll")]
        public IActionResult Get()
        {
            var results = catChargeDefaultAccountService.Get();
            return Ok(results);
        }

        [HttpPost]
        [Route("addNew")]
        [Authorize]
        public IActionResult Add(CatChargeDefaultAccountModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(0, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }

            var catChargeDefaultAccount = mapper.Map<CatChargeDefaultAccountModel>(model);
            catChargeDefaultAccount.UserCreated = currentUser.UserID;
            catChargeDefaultAccount.DatetimeCreated = DateTime.Now;
            catChargeDefaultAccount.Inactive = false;
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var hs = catChargeDefaultAccountService.Add(catChargeDefaultAccount);
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
        public IActionResult Update(CatChargeDefaultAccountModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model.Id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }

            var catChargeDefaultAccount = mapper.Map<CatChargeDefaultAccountModel>(model);
            catChargeDefaultAccount.UserModified = currentUser.UserID;
            catChargeDefaultAccount.DatetimeModified = DateTime.Now;
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var hs = catChargeDefaultAccountService.Update(catChargeDefaultAccount,x=>x.Id==model.Id);
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
        public IActionResult Delete(int id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = catChargeDefaultAccountService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private string CheckExist(int id, CatChargeDefaultAccountModel model)
        {
            string message = string.Empty;
            if (id == 0)
            {
                if (catChargeDefaultAccountService.Any(x => (x.Type.ToLower() == model.Type.ToLower())))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            else
            {
                if (catChargeDefaultAccountService.Any(x => ((x.Type.ToLower() == model.Type.ToLower())) && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            return message;
        }
    }
}