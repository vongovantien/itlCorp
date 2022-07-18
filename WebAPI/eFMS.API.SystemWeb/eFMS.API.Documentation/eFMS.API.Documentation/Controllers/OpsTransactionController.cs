using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.ForPartner.DL.Models.Receivable;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
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
    //[Authorize]
    public class OpsTransactionController : CustomAuthcontroller
    {
        private readonly IStringLocalizer stringLocalizer;
        private ICurrentUser currentUser;
        private readonly IOpsTransactionService transactionService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IAccAccountReceivableService AccAccountReceivableService;
        private readonly IOptions<ApiServiceUrl> apiServiceUrl;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="curUser"></param>
        /// <param name="AccAccountReceivable"></param>
        /// <param name="menu"></param>
        public OpsTransactionController(IStringLocalizer<LanguageSub> localizer,
            IOpsTransactionService service,
            IHostingEnvironment hostingEnvironment,
            ICurrentUser curUser,
            IAccAccountReceivableService AccAccountReceivable,
            IOptions<ApiServiceUrl> serviceUrl,
            Menu menu = Menu.opsJobManagement) : base(curUser, menu)
        {
            stringLocalizer = localizer;
            currentUser = curUser;
            transactionService = service;
            _hostingEnvironment = hostingEnvironment;
            AccAccountReceivableService = AccAccountReceivable;
            apiServiceUrl = serviceUrl;
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
        [Authorize]
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
            currentUser.Action = "AddOpsTransaction";
            if (!ModelState.IsValid) return BadRequest();

            var existedMessage = transactionService.CheckExist(model, string.Empty, string.Empty);
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
            currentUser.Action = "UpdateOpsTransaction";

            if (!ModelState.IsValid) return BadRequest();
            var existedMessage = transactionService.CheckExist(model, string.Empty, string.Empty);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }

            string msgCheckUpdateServiceDate = CheckUpdateServiceDate(model);
            if (msgCheckUpdateServiceDate.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = msgCheckUpdateServiceDate, Data = new { errorCode = "serviceDate" } });
            }


            string msgCheckUpdateMawb = CheckHasMBLUpdatePermitted(model);
            if (msgCheckUpdateMawb.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = msgCheckUpdateMawb });
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
            currentUser.Action = "InsertDuplicateJobOps";

            if (!ModelState.IsValid) return BadRequest();
            model.Id = Guid.NewGuid();
            var existedMessage = transactionService.CheckExist(model, string.Empty, string.Empty);
            if (existedMessage != null)
            {
                return Ok(new ResultHandle { Status = false, Message = existedMessage });
            }
            ResultHandle hs = transactionService.ImportDuplicateJob(model, out List<Guid> surchargeIds);
            if (!hs.Status)
            {
                return BadRequest(new ResultHandle { Status = false, Message = hs.Message });
            }
            else if (surchargeIds.Count > 0)
            {

                Response.OnCompleted(async () =>
                {
                    List<ObjectReceivableModel> modelReceivableList = AccAccountReceivableService.GetListObjectReceivableBySurchargeIds(surchargeIds);
                    if(modelReceivableList.Count > 0)
                    {
                        await CalculatorReceivable(modelReceivableList);
                    }
                });
            }
            return Ok(hs);
        }

        private async Task<HandleState> CalculatorReceivable(List<ObjectReceivableModel> model)
        {
            Uri urlAccounting = new Uri(apiServiceUrl.Value.ApiUrlAccounting);
            string accessToken = Request.Headers["Authorization"].ToString();

            HttpResponseMessage resquest = await HttpClientService.PutAPI(urlAccounting + "/api/v1/e/AccountReceivable/CalculateDebitAmount", model, accessToken);
            var response = await resquest.Content.ReadAsAsync<HandleState>();
            return response;
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
            currentUser.Action = "DeleteJobOps";

            if (transactionService.CheckAllowDeleteJobUsed(id) == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[DocumentationLanguageSub.MSG_NOT_ALLOW_DELETED].Value });
            }
            var hs = transactionService.SoftDeleteJob(id, out List<ObjectReceivableModel> modelReceivableList);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            else
            {
                Response.OnCompleted(async () =>
                {
                    if(modelReceivableList.Count > 0)
                    {
                        await CalculatorReceivable(modelReceivableList);
                    }
                });
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
            currentUser.Action = "ConvertClearanceToJob";
            var permissionRange = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return Forbid();

            HandleState hs = transactionService.ConvertClearanceToJob(model);

            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
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
            currentUser.Action = "ConvertExistedClearancesToJobs";

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
            currentUser.Action = "LockOpsTransaction";

            HandleState hs = transactionService.LockOpsTransaction(id);

            string message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = null };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("ChargeFromReplicate")]
        [Authorize]
        public IActionResult ChargeFromReplicate([FromBody] ChargeFromReplicateCriteria model)
        {
            currentUser.Action = "ChargeFromReplicate";
            ResultHandle hs = transactionService.ChargeFromReplicate(model.ArrJobRep, out List<Guid> Ids);
            if (!hs.Status)
                return BadRequest(hs);
            else
            {
                Response.OnCompleted(async () =>
                {
                    List<ObjectReceivableModel> modelReceivableList = AccAccountReceivableService.GetListObjectReceivableBySurchargeIds(Ids);
                    if(modelReceivableList.Count > 0)
                    {
                        await CalculatorReceivable(modelReceivableList);
                    }
                });
            }
            return Ok(hs);
        }

        /// <summary>
        /// Calling AutoRateReplicate
        /// </summary>
        /// <param name="settleNo">Settlement Code</param>
        /// <param name="jobNo">Job No</param>
        /// <returns></returns>
        [HttpGet("AutoRateReplicate")]
        public IActionResult AutoRateReplicate(string settleNo, string jobNo)
        {
            currentUser.Action = "AutoRateReplicate";
            ResultHandle hs = transactionService.AutoRateReplicate(settleNo, jobNo);
            if (!hs.Status)
                return BadRequest(hs);
            return Ok(hs);
        }

        private string CheckHasMBLUpdatePermitted(OpsTransactionModel model)
        {
            string errorMsg = string.Empty;
            string mblNo = string.Empty;
            List<string> advs = new List<string>();

            int statusCode = transactionService.CheckUpdateMBL(model, out mblNo, out advs);
            if (statusCode == 1)
            {
                errorMsg = String.Format("MBL {0} has Charges List that Synced to accounting system, Please you check Again!", mblNo);
            }

            if (statusCode == 2)
            {
                errorMsg = String.Format("MBL {0} has  Advances {1} that Synced to accounting system, Please you check Again!", mblNo, string.Join(",", advs.ToArray()));
            }

            return errorMsg;
        }

        private string CheckUpdateServiceDate(OpsTransactionModel model)
        {
            string errorMsg = string.Empty;
            OpsTransactionModel currentJob = transactionService.Get(x => x.Id == model.Id).FirstOrDefault();

            errorMsg = model.ServiceDate.Value.Month != currentJob.ServiceDate.Value.Month ? stringLocalizer[DocumentationLanguageSub.MSG_SERVICE_DATE_CANNOT_CHANGE_MONTH].Value : errorMsg;

            return errorMsg;
        }


        [HttpPost("ReplicateJob")]
        [Authorize]
        public async Task<IActionResult> ReplicateJob(ReplicateIds model)
        {
            currentUser.Action = "ReplicateJob";

            HandleState hs = await transactionService.ReplicateJobs(model);

            string message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = null };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
