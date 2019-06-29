﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.NoSql;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Shipment.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

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
    public class OpsTransactionController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IOpsTransactionService transactionService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject IStringLocalizer</param>
        /// <param name="user">inject ICurrentUser</param>
        /// <param name="service">inject IOpsTransactionService</param>
        public OpsTransactionController(IStringLocalizer<LanguageSub> localizer, ICurrentUser user, IOpsTransactionService service)
        {
            stringLocalizer = localizer;
            currentUser = user;
            transactionService = service;
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
        /// get data detail by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get(Guid id)
        {
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
        [Authorize]
        [HttpPost]
        [Route("Add")]
        public IActionResult Add(OpsTransactionModel model)
        {
            var existedMessage = CheckExist(model);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
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
        [Authorize]
        [HttpPut]
        [Route("Update")]       
        public IActionResult Update(OpsTransactionModel model)
        {
            var existedMessage = CheckExist(model);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }

            model.ModifiedDate = DateTime.Now;
            model.UserModified = currentUser.UserID;
            var hs = transactionService.Update(model,x=>x.Id==model.Id);
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
        /// <param name="id">id of item that want to delete</param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        [Route("Delete/{id}")]
        public IActionResult Delete(Guid id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            if (transactionService.CheckAllowDelete(id) == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.MSG_NOT_ALLOW_DELETED].Value });
            }
            var hs = transactionService.Delete(x => x.Id == id);
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

        /// <summary>
        /// convert a custom clearance to a job
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("ConvertClearanceToJob")]
        public IActionResult ConvertClearanceToJob(OpsTransactionClearanceModel model)
        {
            var hs = transactionService.ConvertClearanceToJob(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private string CheckExist(OpsTransactionModel model)
        {
            var existedHBL = transactionService.Any(x => x.Id != model.Id && x.Hwbno == model.Hwbno);
            var existedMBL = transactionService.Any(x => x.Id != model.Id && x.Mblno == model.Mblno);
            if (existedHBL)
            {
                return "HBL no is existed !";
            }
            if (existedMBL)
            {
                return "MBL no is existed !";
            }
            return null;
        }
    }
}
