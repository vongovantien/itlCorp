using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
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

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(catIncotermService.Get());
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
        [Route("update")]
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
        [Route("getById/{id}")]
        [Authorize]
        public IActionResult Get(Guid id)
        {
            CatIncotermEditModel result = catIncotermService.GetDetail(id);
            return Ok(result);
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