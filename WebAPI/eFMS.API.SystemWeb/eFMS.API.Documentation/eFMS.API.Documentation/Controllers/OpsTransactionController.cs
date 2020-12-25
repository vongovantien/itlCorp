using System;
using System.Collections.Generic;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    [Authorize]
    public class OpsTransactionController : CustomAuthcontroller
    {
        private readonly IStringLocalizer stringLocalizer;
        private ICurrentUser currentUser;
        private readonly IOpsTransactionService transactionService;
        private readonly IHostingEnvironment _hostingEnvironment;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="curUser"></param>
        /// <param name="menu"></param>
        public OpsTransactionController(IStringLocalizer<LanguageSub> localizer, IOpsTransactionService service, IHostingEnvironment hostingEnvironment,
            ICurrentUser curUser, Menu menu = Menu.opsJobManagement) : base(curUser, menu)
        {
            stringLocalizer = localizer;
            currentUser = curUser;
            transactionService = service;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// get the list of countries by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost("Query")]
        public IActionResult Query(OpsTransactionCriteria criteria)
        {
            var results = transactionService.Query(criteria);
            return Ok(results);
        }


        /// <summary>
        /// check permission of user to view a shipment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("CheckPermission/{id}")]
        public IActionResult CheckDetailPermission(Guid id)
        {
            var result = transactionService.CheckDetailPermission(id);
            if (result == 403) return Ok(false);
            return Ok(true);
        }

        /// <summary>
        /// get data detail by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get(Guid id)
        {

            var statusCode = transactionService.CheckDetailPermission(id);
            if (statusCode == 403) return Forbid();

            var result = transactionService.GetDetails(id); //transactionService.First(x => x.Id == id);
            return Ok(result);
        }
        
        /// <summary>
        /// get and paging the list of countries by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost("Paging")]
        [AuthorizeEx(Menu.opsJobManagement, UserPermission.AllowAccess)]
        public IActionResult Paging(OpsTransactionCriteria criteria, int page, int size)
        {
            var data = transactionService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// add new job
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [AuthorizeEx(Menu.opsJobManagement, UserPermission.Add)]
        public IActionResult Add(OpsTransactionModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var existedMessage = transactionService.CheckExist(model);
            if (existedMessage != null)
            {
                return Ok(new ResultHandle { Status = false, Message = existedMessage });
            }
            model.Hblid = Guid.NewGuid();
            var hs = transactionService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model.Id };
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
        [AuthorizeEx(Menu.opsJobManagement, UserPermission.Update)]
        public IActionResult Update(OpsTransactionModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var existedMessage = transactionService.CheckExist(model);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }

            var hs = transactionService.Update(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// add duplicate job
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("InsertDuplicateJob")]
        [AuthorizeEx(Menu.opsJobManagement, UserPermission.Add)]
        public IActionResult InsertDuplicateJob(OpsTransactionModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var existedMessage = transactionService.CheckExist(model);
            if (existedMessage != null)
            {
                return Ok(new ResultHandle { Status = false, Message = existedMessage });
            }
            var hs = transactionService.ImportDuplicateJob(model);
            return Ok(hs);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of item that want to delete</param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        [Route("Delete/{id}")]
        public IActionResult Delete(Guid id)
        {
            if (transactionService.CheckAllowDelete(id) == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[DocumentationLanguageSub.MSG_NOT_ALLOW_DELETED].Value });
            }
            var hs = transactionService.SoftDeleteJob(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// check an item that is allowed delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("CheckAllowDelete/{id}")]
        public IActionResult CheckAllowDelete(Guid id)
        {
            return Ok(transactionService.CheckAllowDelete(id));
        }

        [HttpPost("CheckAllowConvertJob")]
        [Authorize]
        public IActionResult CheckAllowConvertJob([FromBody]List<CustomsDeclarationModel> list)
        {
            currentUser = PermissionExtention.GetUserMenuPermission(currentUser, Menu.opsCustomClearance);
            var result = transactionService.CheckAllowConvertJob(list);
            return Ok(result);
        }

        /// <summary>
        /// convert a custom clearance to a job
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("ConvertClearanceToJob")]
        [Authorize]
        public IActionResult ConvertClearanceToJob(CustomsDeclarationModel model)
        {
            currentUser = PermissionExtention.GetUserMenuPermission(currentUser, Menu.opsCustomClearance);
            var permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return Forbid();
            var hs = transactionService.ConvertClearanceToJob(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = message };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// convert multi clearances to multi jobs
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [HttpPost("ConvertExistedClearancesToJobs")]
        [Authorize]
        public IActionResult ConvertExistedClearancesToJobs([FromBody]List<CustomsDeclarationModel> list)
        {
            currentUser = PermissionExtention.GetUserMenuPermission(currentUser, Menu.opsCustomClearance);
            var permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return Forbid();
            HandleState hs = transactionService.ConvertExistedClearancesToJobs(list);
            if (!hs.Success)
            {
                ResultHandle result = new ResultHandle { Status = hs.Success, Message = hs.Exception.Message?.ToString() };
                return BadRequest(result);
            }
            else
            {
                ResultHandle result = new ResultHandle { Status = hs.Success, Message = hs.Message?.ToString() };
                return Ok(result);
            }
        }

        /// <summary>
        /// preview Form PL sheet
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        [HttpGet("PreviewFormPLsheet")]
        [Authorize]
        public IActionResult PreviewFormPLsheet(Guid jobId, string currency)
        {
            var result = transactionService.PreviewFormPLsheet(jobId, currency);
            return Ok(result);
        }

        [HttpPut("LockOpsTransaction/{id}")]
        [Authorize]
        public IActionResult LockOpsTransaction(Guid id)
        {
            HandleState hs = transactionService.LockOpsTransaction(id);

            string message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = null };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
