using System;
using System.Collections.Generic;
using System.Linq;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.Operation.DL.IService;
using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.DL.Models.Criteria;
using eFMS.API.Operation.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Operation.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class EcusConnectionController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IEcusConnectionService ecusConnectionService;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface IEcusConnectionService</param>
        public EcusConnectionController(IStringLocalizer<LanguageSub> localizer, IEcusConnectionService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            ecusConnectionService = service;
            currentUser = user;
        }

        /// <summary>
        /// add new ecus connection
        /// </summary>
        /// <param name="model">model to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult AddNew(SetEcusConnectionModel model)
        {

            PermissionRange permissionRange;
            ICurrentUser _user =  PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingEcusConnection);

            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var existedMessage = CheckExist(model);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }

            model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
            model.UserCreated = model.UserModified = currentUser.UserID;
            model.GroupId = currentUser.GroupId;
            model.DepartmentId = currentUser.DepartmentId;
            model.OfficeId = currentUser.OfficeID;
            model.CompanyId = currentUser.CompanyID;

            var hs = ecusConnectionService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };

            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// update an existed item
        /// </summary>
        /// <param name="model">object to update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        [Authorize]
        [AuthorizeEx(Menu.settingEcusConnection, UserPermission.Update)]
        public IActionResult Update(SetEcusConnectionModel model)
        {

            PermissionRange permissionRange;
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingEcusConnection);
            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);

            if (permissionRange == PermissionRange.None)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            bool code = ecusConnectionService.CheckAllowPermissionAction(model.Id, permissionRange);

            if (code == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var existedMessage = CheckExist(model);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }

            model.DatetimeModified = DateTime.Now;
            model.UserModified = currentUser.UserID;
            var hs = ecusConnectionService.Update(model, x => x.Id == model.Id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };

            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of item want to delete</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            PermissionRange permissionRange;
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingEcusConnection);

            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            bool isAllowDelete = ecusConnectionService.CheckAllowPermissionAction(id, permissionRange);
            if (isAllowDelete == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }
            var hs = ecusConnectionService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Check permission delete ecus
        /// </summary>
        /// <param name="id">id of item want to delete</param>
        /// <returns></returns>
        [HttpGet("CheckAllowDelete/{id}")]
        [Authorize]
        public IActionResult CheckAllowDelete(int id)
        {
            PermissionRange permissionRange;
            ICurrentUser _user = null;

            SetEcusConnectionModel ecus = ecusConnectionService.Get(x => x.Id == id).FirstOrDefault();
            if (ecus == null)
            {
                return Ok(false);
            }
            _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingEcusConnection);

            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            return Ok(ecusConnectionService.CheckAllowPermissionAction(id, permissionRange));
        }

        /// <summary>
        /// Check permission view detail ecus
        /// </summary>
        /// <param name="id">id of item want to delete</param>
        /// <returns></returns>
        [HttpGet("CheckAllowDetail/{id}")]
        [Authorize]
        public IActionResult CheckAllowDetail(int id)
        {
            PermissionRange permissionRange;
            ICurrentUser _user = null;

            SetEcusConnectionModel ecus = ecusConnectionService.Get(x => x.Id == id).FirstOrDefault();
            if (ecus == null)
            {
                return Ok(false);
            }
            _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingEcusConnection);

            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);
            return Ok(ecusConnectionService.CheckAllowPermissionAction(id, permissionRange));
        }

        /// <summary>
        /// get and paging the list of custom declarations by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="pageNumber">page to retrieve data</param>
        /// <param name="pageSize">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(SetEcusConnectionCriteria criteria, int pageNumber, int pageSize)
        {
            var data = ecusConnectionService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }

        /// <summary>
        /// get all ecus connections
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAll")]
        public List<SetEcusConnectionModel> GetAll()
        {
            return ecusConnectionService.GetConnections();
        }

        /// <summary>
        /// get detail of ecus connection by id
        /// </summary>
        /// <param name="id">id of data that want to retrieve</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDetails")]
        public SetEcusConnectionModel GetDetails(int id)
        {
            return ecusConnectionService.GetConnectionDetails(id);
        }
        private string CheckExist(SetEcusConnectionModel model)
        {
            string message = string.Empty;
            var existed = ecusConnectionService.Any(x => x.Id != model.Id && x.UserId == model.UserId && x.ServerName == model.ServerName && x.Dbname == model.Dbname);
            message = existed ? "This connection of " + model.Username + " has already existed, Please check again!" : null;
            return message;
        }

        /// <summary>
        /// get data from ecus system by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="serverName">server name</param>
        /// <param name="dbusername">user name sql</param>
        /// <param name="dbpassword">password sql</param>
        /// <param name="database">database name</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDataEcusByUser")]
        public IActionResult GetDataEcusByUser(string userId, string serverName, string dbusername, string dbpassword, string database)
        {
            var results = ecusConnectionService.GetDataEcusByUser(userId, serverName, dbusername, dbpassword, database);
            return Ok(results);
        }
    }
}
