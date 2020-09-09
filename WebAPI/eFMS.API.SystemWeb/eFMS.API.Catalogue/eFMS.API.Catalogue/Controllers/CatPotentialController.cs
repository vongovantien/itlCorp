using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Catalogue.Controllers
{
    
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatPotentialController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;

        private readonly ICatPotentialService catPotentialService;
        private readonly ICurrentUser currentUser;
        private readonly IMapper mapper;
        public CatPotentialController(
            IStringLocalizer<LanguageSub> localizer,
            ICatPotentialService service,
            ICurrentUser curUser,
            IMapper iMapper)
        {
            stringLocalizer = localizer;
            catPotentialService = service;
            currentUser = curUser;
            mapper = iMapper;
        }
        //
        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(CatPotentialCriteria criteria, int page, int size)
        {
            var data = catPotentialService.Paging(criteria, page, size, out int rowsCount);
            var result = new { data, totalItems = rowsCount, page, size };
            return Ok(result);
        }
        //
        [HttpPost]
        [Route("Create")]
        [Authorize]
        public IActionResult Create(CatPotentialEditModel model)
        {
            PermissionRange permissionRange;
            ICurrentUser _user = null;

            _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialPotential);

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

            bool checkExistMessage = CheckExistPotential(Guid.Empty, model.Potential.Taxcode);
            if (checkExistMessage)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value });
            }

            HandleState hs = catPotentialService.AddNew(model);

            string message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        //
        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(CatPotentialEditModel model)
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

            bool checkExistMessage = CheckExistPotential(model.Potential.Id, model.Potential.Taxcode);
            if (checkExistMessage)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value });
            }

            HandleState hs = catPotentialService.Update(model);

            string message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        //
        [HttpDelete]
        [Route("delete/{id}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialIncoterm);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);

            if (!catPotentialService.CheckAllowPermissionAction(id, permissionRange))
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            HandleState hs = catPotentialService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        //
        [HttpGet]
        [Route("GetById/{id}")]
        [Authorize]
        public IActionResult GetById(Guid id)
        {
            CatPotentialEditModel result = catPotentialService.GetDetail(id);
            return Ok(result);
        }
        //
        [HttpGet("CheckAllowDetail/{id}")]
        [Authorize]
        public IActionResult CheckAllowDetail(Guid id)
        {
            var charge = catPotentialService.First(x => x.Id == id);
            if (charge == null)
            {
                return Ok(false);
            }

            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialPotential);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);

            return Ok(catPotentialService.CheckAllowPermissionAction(id, permissionRange));
        }
        //
        [HttpGet("CheckAllowDelete/{id}")]
        [Authorize]
        public IActionResult CheckAllowDelete(Guid id)
        {
            var charge = catPotentialService.First(x => x.Id == id);
            if (charge == null)
            {
                return Ok(false);
            }
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialPotential);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);

            return Ok(catPotentialService.CheckAllowPermissionAction(id, permissionRange));
        }
        //
        private bool CheckExistPotential(Guid Id, string Taxcode)
        {
            bool isDuplicate = false;
            if (Id == Guid.Empty)
            {
                isDuplicate = catPotentialService.Any(x => !string.IsNullOrEmpty(x.Taxcode) && x.Taxcode == Taxcode);
            }
            else
            {
                isDuplicate = catPotentialService.Any(x => !string.IsNullOrEmpty(x.Taxcode) && x.Taxcode == Taxcode && x.Id != Id);

            }

            return isDuplicate;
        }
    }
}