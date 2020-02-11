using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Infrastructure.Common;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.API.System.Models;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;

namespace eFMS.API.System.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysOfficeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysOfficeService sysOfficeService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;
        public SysOfficeController(IStringLocalizer<LanguageSub> localizer, ISysOfficeService service, IMapper iMapper ,ICurrentUser icurrentUser)
        {
            stringLocalizer = localizer;
            sysOfficeService = service;
            mapper = iMapper;
            currentUser = icurrentUser;
        }

        /// <summary>
        /// get all data
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var results = sysOfficeService.GetOffices();
            return Ok(results);
        }
        /// <summary>
        /// get and paging the list of office
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(SysOfficeCriteria criteria, int page, int size)
        {
            var data = sysOfficeService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// get the list of branch
        /// </summary>
        [HttpPost]
        [Route("Query")]
        public IActionResult Get(SysOfficeCriteria criteria)
        {
            var results = sysOfficeService.Query(criteria);
            return Ok(results);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of data that need to delete</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var hs = sysOfficeService.DeleteOffice(id);
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
        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Put( SysOfficeEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
        
            var checkExistMessage = CheckExist(model.Id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }

            var branch = mapper.Map<SysOfficeModel>(model);
            branch.Id = model.Id;
            branch.UserModified = currentUser.UserID;
            branch.DatetimeModified = DateTime.Now;
            var hs = sysOfficeService.UpdateOffice(branch);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// add new office
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Post(SysOfficeEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(Guid.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var branch = mapper.Map<SysOfficeModel>(model);
            branch.UserCreated = branch.UserModified = currentUser.UserID;
            branch.Id = Guid.NewGuid();
            branch.DatetimeCreated = branch.DatetimeModified = DateTime.Now;
            var hs = sysOfficeService.AddOffice(branch);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value , Data = branch};
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        /// <summary>
        /// get office by companyId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet]
        [Route("GetByCompany/{id}")]
        public IActionResult GetOfficeBy (Guid id)
        {
            var offices = sysOfficeService.GetOfficeByCompany(id);

            ResultHandle hs = new ResultHandle { Data = offices , Status = true };
            return Ok(offices);
        }

        [HttpGet]
        [Route("GetOfficePermission/{username}/{companyId}")]
        public IActionResult GetOfficePermission(string username,Guid companyId)
        {
            var officesPermission = sysOfficeService.GetOfficePermission(username,companyId);
            return Ok(officesPermission);
        }

        /// get office by id
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public IActionResult GetBy(Guid id)
        {
            var result = sysOfficeService.First(x => x.Id == id);
            if (result == null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Error", Data = result });
            }
            else
            {
                return Ok(new ResultHandle { Status = true, Message = "Success", Data = result });
            }
        }

        private string CheckExist(Guid id, SysOfficeEditModel model)
        {
            string message = string.Empty;
            if (id == Guid.Empty)
            {
                if (sysOfficeService.Any(x => x.BranchNameEn == model.BranchNameEn  || x.ShortName == model.ShortName))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            else
            {
                if (sysOfficeService.Any(x => (x.BranchNameEn == model.BranchNameEn || x.BranchNameVn == model.BranchNameVn) && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            return message;
        }




    }
}