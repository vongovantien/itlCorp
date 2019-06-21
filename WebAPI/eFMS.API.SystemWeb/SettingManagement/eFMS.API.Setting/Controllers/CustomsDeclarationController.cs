using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using eFMS.API.Setting.Infrastructure.Middlewares;
using eFMS.API.Setting.Resources;
using System.Diagnostics.Contracts;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.DL.Common;
using Microsoft.Extensions.Caching.Distributed;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Setting.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CustomsDeclarationController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICustomsDeclarationService customsDeclarationService;
        private readonly IDistributedCache cache;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface ICustomsDeclarationService</param>
        public CustomsDeclarationController(IStringLocalizer<LanguageSub> localizer, ICustomsDeclarationService service, IDistributedCache distributedCache)
        {
            stringLocalizer = localizer;
            customsDeclarationService = service;
            cache = distributedCache;
        }

        /// <summary>
        /// get the list of custom declarations
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var results = customsDeclarationService.Get();
            return Ok(results);
        }

        /// <summary>
        /// get the list of custom declarations by job id
        /// </summary>
        /// <param name="jobId">jobId that want to retrieve custom declarations</param>
        /// <returns></returns>
        [HttpGet("GetBy/{jobId}")]
        public IActionResult GetBy(Guid jobId)
        {
            var results = customsDeclarationService.GetBy(jobId);
            return Ok(results);
        }

        /// <summary>
        /// get the list of custom declarations by conditions
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("Query")]
        public IActionResult Query(CustomsDeclarationCriteria criteria)
        {
            var data = customsDeclarationService.Query(criteria);
            return Ok(data);
        }

        /// <summary>
        /// get and paging the list of custom declarations by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="pageNumber">page to retrieve data</param>
        /// <param name="pageSize">number items per page</param>
        /// <returns></returns>
        [HttpPost("Paging")]
        public IActionResult Paging(CustomsDeclarationCriteria criteria, int pageNumber, int pageSize)
       {
            var data = customsDeclarationService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }
        
        /// <summary>
        /// add new custom clearance
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        public IActionResult AddNew(CustomsDeclarationModel model)
        {
            var existedMessage = CheckExist(model, model.Id);
            if (existedMessage.Length >0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            model.DatetimeCreated = DateTime.Now;
            model.DatetimeModified = DateTime.Now;
            model.UserCreated = model.UserModified = "admin";
            model.Source = Constants.FromEFMS;
            var hs = customsDeclarationService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            else
            {
                cache.Remove(Templates.CustomDeclaration.NameCaching.ListName);
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
        public IActionResult Update(CustomsDeclarationModel model)
        {
            var existedMessage = CheckExist(model, model.Id);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            model.DatetimeModified = DateTime.Now;
            model.UserModified = "admin";
            var hs = customsDeclarationService.Update(model, x => x.Id == model.Id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                cache.Remove(Templates.CustomDeclaration.NameCaching.ListName);
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of existed item that want to delete</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete")]
        public IActionResult Delete(int id)
        {
            var hs = customsDeclarationService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// import custom declaration from ecus system
        /// </summary>
        /// <returns></returns>
        [HttpGet("ImportClearancesFromEcus")]
        public IActionResult ImportClearancesFromEcus()
        {
            var result = customsDeclarationService.ImportClearancesFromEcus();
            if (result.Success)
            {
                cache.Remove(Templates.CustomDeclaration.NameCaching.ListName);
            }
            return Ok(result);
        }

        /// <summary>
        /// get clearance types(types, cargoTypes, routes, serviceTypes)
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetClearanceTypes")]
        public IActionResult GetClearanceTypes()
        {
            var results = customsDeclarationService.GetClearanceTypeData();
            return Ok(results);
        }

        /// <summary>
        /// add( update) job to clearances
        /// </summary>
        /// <param name="clearances">list of clearances</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateJobToClearances(List<CustomsDeclarationModel> clearances)
        {
            var result = customsDeclarationService.UpdateJobToClearances(clearances);
            if (result.Success)
            {
                cache.Remove(Templates.CustomDeclaration.NameCaching.ListName);
            }
            return Ok(result);
        }

        private string CheckExist(CustomsDeclarationModel model, decimal id)
        {
            string message = string.Empty;
            if (id == 0)
            {
                if (customsDeclarationService.Any(x => x.ClearanceNo == model.ClearanceNo))
                {
                    message = stringLocalizer[LanguageSub.MSG_CLEARANCENO_EXISTED].Value;
                }
            }
            else
            {
                if (customsDeclarationService.Any(x => (x.ClearanceNo == model.ClearanceNo && x.Id != id)))
                {
                    message = stringLocalizer[LanguageSub.MSG_CLEARANCENO_EXISTED].Value;
                }
            }
            return message;
        }

        /// <summary>
        /// get custom declarations by id
        /// </summary>
        /// <param name="id">id that want to retrieve custom declarations</param>
        /// <returns></returns>
        [HttpGet("GetById/{id}")]
        public IActionResult GetById(string id)
        {
            var results = customsDeclarationService.GetById(id);
            return Ok(results);
        }
    }
}
