using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Catalogue.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Catalogue.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatIncotermController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;

        private readonly ICatIncotermService catIncotermService;
        private readonly ICurrentUser currentUser;
        private readonly IMapper mapper;


        public CatIncotermController(
            IStringLocalizer<LanguageSub> localizer, 
            ICatIncotermService service,
            ICurrentUser curUser,
            IMapper iMapper
            )
        {
            stringLocalizer = localizer;
            catIncotermService = service;
            currentUser = curUser;
            mapper = iMapper;

        }

        [HttpPost]
        [Route("Query")]
        public IActionResult QueryIncoterm(CatIncotermCriteria criteria)
        {
            var result = catIncotermService.Query(criteria);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete/{id}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialIncoterm);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);

            if (!catIncotermService.CheckAllowPermissionAction(id, permissionRange))
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            HandleState hs = catIncotermService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Post(CatIncotermEditModel model)
        {
            PermissionRange permissionRange;
            ICurrentUser _user = null;

            _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialIncoterm);

            if(_user.UserMenuPermission == null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            if (!ModelState.IsValid) return BadRequest();

            bool checkExistMessage = CheckExisIncoterm(Guid.Empty, model.Incoterm.Code);
            if (checkExistMessage)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value });
            }

            HandleState hs = catIncotermService.AddNew(model);

            string message = HandleError.GetMessage(hs, Crud.Insert);
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
        public IActionResult Update(CatIncotermEditModel model)
        {
            PermissionRange permissionRange;
            ICurrentUser _user = null;

            _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialIncoterm);

            if (_user.UserMenuPermission == null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            if (!ModelState.IsValid) return BadRequest();

            bool checkExistMessage = CheckExisIncoterm(model.Incoterm.Id, model.Incoterm.Code);
            if (checkExistMessage)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value });
            }

            HandleState hs = catIncotermService.Update(model);

            string message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetById/{id}")]
        [Authorize]
        public IActionResult Get(Guid id)
        {
            CatIncotermEditModel result = catIncotermService.GetDetail(id);
            return Ok(result);
        }

        [HttpGet("CheckAllowDetail/{id}")]
        [Authorize]
        public IActionResult CheckAllowDetail(Guid id)
        {
            var charge = catIncotermService.First(x => x.Id == id);
            if (charge == null)
            {
                return Ok(false);
            }

            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialIncoterm);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);

            return Ok(catIncotermService.CheckAllowPermissionAction(id, permissionRange));
        }

        [HttpGet("CheckAllowDelete/{id}")]
        [Authorize]
        public IActionResult CheckAllowDelete(Guid id)
        {
            var charge = catIncotermService.First(x => x.Id == id);
            if (charge == null)
            {
                return Ok(false);
            }
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialIncoterm);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);

            return Ok(catIncotermService.CheckAllowPermissionAction(id, permissionRange));
        }
        private bool CheckExisIncoterm(Guid Id, string code)
        {
            bool isDuplicate = false;
            if (Id == Guid.Empty)
            {
                isDuplicate = catIncotermService.Any(x => !string.IsNullOrEmpty(x.Code) && x.Code == code);
            }
            else
            {
                isDuplicate = catIncotermService.Any(x => !string.IsNullOrEmpty(x.Code) && x.Code == code && x.Id != Id);

            }

            return isDuplicate;
        }
    }
}