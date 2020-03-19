using System;
using System.Linq;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Infrastructure.Extensions;

namespace eFMS.API.Setting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class TariffController : ControllerBase
    {
        private readonly ITariffService tariffService;
        private readonly IStringLocalizer stringLocalizer;
        readonly ICurrentUser currentUser;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="currUser"></param>
        public TariffController(IStringLocalizer<LanguageSub> localizer, ITariffService service, ICurrentUser currUser)
        {
            stringLocalizer = localizer;
            tariffService = service;
            currentUser = currUser;
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Get(TariffCriteria criteria)
        {
            var results = tariffService.Query(criteria);
            return Ok(results);
        }

        /// <summary>
        /// get and paging the list of tariff
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        //[Authorize]
        public IActionResult Paging(TariffCriteria criteria, int page, int size)
        {
            var data = tariffService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpGet]
        public IActionResult Get()
        {
            var data = tariffService.Get();
            return Ok(data);
        }

        /// <summary>
        /// Add tariff and list tariff details
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult AddTariff(TariffModel model)
        {

            PermissionRange permissionRange;
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingTariff);
            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            if (!ModelState.IsValid) return BadRequest();

            var checkData = tariffService.CheckExistsDataTariff(model);
            if (!checkData.Success) return Ok(new ResultHandle { Status = checkData.Success, Message = checkData.Exception.Message.ToString(), Data = checkData.Code });



            var hs = tariffService.AddTariff(model);

            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            return Ok(result);
        }

        /// <summary>
        /// Update tariff and list tariff details
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        [Authorize]
        [AuthorizeEx(Menu.settingTariff, UserPermission.Update)]
        public IActionResult UpdateTariff(TariffModel model)
        {

            PermissionRange permissionRange;
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingTariff);
            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);

            if (permissionRange == PermissionRange.None)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            bool code = tariffService.CheckAllowPermissionAction(model.setTariff.Id, permissionRange);

            if (code == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            if (!ModelState.IsValid) return BadRequest();

            var checkData = tariffService.CheckExistsDataTariff(model);
            if (!checkData.Success) return Ok(new ResultHandle { Status = checkData.Success, Message = checkData.Exception.Message.ToString(), Data = checkData.Code });

            model.setTariff.UserModified = currentUser.UserID;
            var hs = tariffService.UpdateTariff(model);

            var message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            return Ok(result);
        }

        /// <summary>
        /// Check permission view detail ecus
        /// </summary>
        /// <param name="id">id of item want to delete</param>
        /// <returns></returns>
        [HttpGet("CheckAllowDelete/{id}")]
        [Authorize]
        public IActionResult CheckAllowDelete(Guid id)
        {
            PermissionRange permissionRange;
            ICurrentUser _user = null;
            var result = new TariffModel();
            result.setTariff = tariffService.GetTariffById(id);
            if (result.setTariff == null)
            {
                return Ok(false);
            }
            _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingTariff);

            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            return Ok(tariffService.CheckAllowPermissionAction(id, permissionRange));
        }

        /// <summary>
        /// Delete tariff and list tariff details
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            PermissionRange permissionRange;
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingTariff);

            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            bool isAllowDelete = tariffService.CheckAllowPermissionAction(id, permissionRange);
            if(isAllowDelete == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }
            //Check exists tariff & status tariff
            var checkStatus = tariffService.Get(x => x.Id == id).FirstOrDefault();
            if (checkStatus == null)
            {
                return Ok(new ResultHandle { Status = false, Message = "Not found tariff" });
            }
            else
            {
                if (checkStatus.Status == true)
                {
                    return Ok(new ResultHandle { Status = false, Message = "Not allowed delete tariff" });
                }
            }

            var hs = tariffService.DeleteTariff(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            return Ok(result);
        }
        /// <summary>
        /// Check permission view detail ecus
        /// </summary>
        /// <param name="id">id of item want to delete</param>
        /// <returns></returns>
        [HttpGet("CheckAllowDetail/{id}")]
        [Authorize]
        public IActionResult CheckAllowDetail(Guid id)
        {
            PermissionRange permissionRange;
            ICurrentUser _user = null;

            var result = new TariffModel();
            result.setTariff = tariffService.GetTariffById(id);

            if (result.setTariff == null)
            {
                return Ok(false);
            }
            _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingTariff);

            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);
            return Ok(tariffService.CheckAllowPermissionAction(id, permissionRange));
        }

        /// <summary>
        /// Get tariff and list tariff detail by tariff id
        /// </summary>
        /// <param name="tariffId"></param>
        /// <returns></returns>
        [HttpGet("GetTariff")]
        [Authorize]

        public IActionResult GetTariff(Guid tariffId)
        {
            PermissionRange permissionRange;
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingTariff);

            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);
            bool isAllowDelete = tariffService.CheckAllowPermissionAction(tariffId, permissionRange);
            if (isAllowDelete == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var result = new TariffModel();
            result.setTariff = tariffService.GetTariffById(tariffId);
            result.setTariffDetails = tariffService.GetListTariffDetailByTariffId(tariffId).ToList();
            if (result == null)
            {
                return Ok(new ResultHandle { Status = false, Message = "Không tìm thấy Tariff", Data = result });
            }
            else
            {
                return Ok(new ResultHandle { Status = true, Message = "Success", Data = result });
            }
        }

        /// <summary>
        /// Check duplicate tariff
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("CheckDuplicateTariff")]
        public IActionResult CheckDuplicateTariff(SetTariffModel model)
        {
            var checkData = tariffService.CheckDuplicateTariff(model);
            return Ok(new ResultHandle { Status = checkData.Success, Message = checkData.Exception.Message.ToString(), Data = checkData.Code });
        }

    }
}