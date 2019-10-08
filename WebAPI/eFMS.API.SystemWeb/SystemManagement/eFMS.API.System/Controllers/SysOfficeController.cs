using AutoMapper;
using eFMS.API.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Infrastructure.Common;
using eFMS.API.System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

namespace eFMS.API.System.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysOfficeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysBranchService sysBranchService;
        private readonly IMapper mapper;
        public SysOfficeController(IStringLocalizer<LanguageSub> localizer, ISysBranchService service, IMapper iMapper)
        {
            stringLocalizer = localizer;
            sysBranchService = service;
            mapper = iMapper;
        }

        /// <summary>
        /// get all data
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var results = sysBranchService.GetBranchs();
            return Ok(results);
        }
        /// <summary>
        /// get and paging the list of branch
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(SysBranchCriteria criteria, int page, int size)
        {
            var data = sysBranchService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// get the list of branch
        /// </summary>
        [HttpPost]
        [Route("Query")]
        public IActionResult Get(SysBranchCriteria criteria)
        {
            var results = sysBranchService.Query(criteria);
            return Ok(results);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of data that need to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var hs = sysBranchService.DeleteBranch(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
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
        /// <param name="id">id of data that need to update</param>
        /// <param name="model">object to update</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        //[Authorize]
        public IActionResult Put(Guid id, SysBranchEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
        
            var checkExistMessage = CheckExist(id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var branch = mapper.Map<SysBranchModel>(model);
            branch.Id = id;
            var hs = sysBranchService.UpdateBranch(branch);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// add new partner
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        //[Authorize]
        public IActionResult Post(SysBranchEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(Guid.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var branch = mapper.Map<SysBranchModel>(model);
            var hs = sysBranchService.Add(branch);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private string CheckExist(Guid id, SysBranchEditModel model)
        {
            string message = string.Empty;
            if (id == Guid.Empty)
            {
                if (sysBranchService.Any(x => x.BranchNameEn == model.BranchNameEn || x.BranchNameVn == model.BranchNameVn || x.ShortName == model.ShortName))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            else
            {
                if (sysBranchService.Any(x => (x.BranchNameEn == model.BranchNameEn || x.BranchNameVn == model.BranchNameVn) && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            return message;
        }




    }
}