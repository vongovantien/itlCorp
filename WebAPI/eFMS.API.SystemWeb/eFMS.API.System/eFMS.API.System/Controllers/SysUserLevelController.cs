using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.System.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysUserLevelController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysUserLevelService userLevelService;
        private readonly ICurrentUser currentUser;
        private readonly IMapper mapper;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="sysUserLevel"></param>
        /// <param name="localizer"></param>
        public SysUserLevelController(ISysUserLevelService sysUserLevel, IStringLocalizer<SystemLanguageSub> localizer, ICurrentUser currUser, IMapper iMapper)
        {
            userLevelService = sysUserLevel;
            stringLocalizer = localizer;
            currentUser = currUser;
            mapper = iMapper;
        }

        /// <summary>
        /// get by group id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetByLevel/{id}")]
        public IActionResult GetByLevel(short id)
        {
            var results = userLevelService.GetByLevel(id);
            return Ok(results);
        }

        /// <summary>
        /// get by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var result = userLevelService.GetDetail(id);
            return Ok(result);
        }

        /// <summary>
        /// get by userid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetByUserId")]
        public IActionResult GetByUserId(string  id)
        {
            var result = userLevelService.GetByUserId(id);
            return Ok(result);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public IActionResult Add(SysUserLevelModel model)
        {
            model.UserCreated = currentUser.UserID;
            model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
            model.Active = true;
            var hs = userLevelService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        /// <summary>
        /// add user to office
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        /// 
        [HttpPost]
        [Route("AddUserToOffice")]
        [Authorize]
        public IActionResult AddUser(List<SysUserLevelModel> users)
        {
            if (!ModelState.IsValid) return BadRequest();
            SysUserLevelModel modelDupdatelidate = null;
            var checkDupUser = users.GroupBy(x => x.UserId)
                                .Where(t => t.Count() > 1)
                                .Select(y => y.Key)
                                .ToList();
            if (checkDupUser.Count > 0)
            {
                return Ok(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_ITEM_DUPLICATE_USER_ON_USER_LEVEL, "office"].Value, Data = checkDupUser });
            }
            foreach (var item in users)
            {
                if (CheckExistUserLevelOnOffice(item))
                {
                    modelDupdatelidate = item;
                    break;
                };
            }
            if (modelDupdatelidate != null)
            {
                return Ok(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_ITEM_EXISTED_USER_ON_USER_LEVEL, "office"].Value, Data = modelDupdatelidate });
            }
            var hs = userLevelService.AddUser(users);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// add user to company
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddUserToCompany")]
        [Authorize]
        public IActionResult AddUserToCompany(List<SysUserLevelModel> users)
        {
            if (!ModelState.IsValid) return BadRequest();

            SysUserLevelModel modelDupdatelidate = null;

            var checkDupUser = users.GroupBy(x => x.UserId)
                                .Where(t => t.Count() > 1)
                                .Select(y => y.Key)
                                .ToList();
            if (checkDupUser.Count > 0)
            {
                return Ok(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_ITEM_DUPLICATE_USER_ON_USER_LEVEL, "company"].Value, Data = checkDupUser });
            }
            foreach (var item in users)
            {
                if (CheckExistUserLevelOnCompany(item))
                {
                    modelDupdatelidate = item;
                    break;
                };
            }
            if (modelDupdatelidate != null)
            {
                return Ok(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_ITEM_EXISTED_USER_ON_USER_LEVEL, "company"].Value, Data = modelDupdatelidate });
            }

            var hs = userLevelService.AddUser(users);

            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };

            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private bool CheckExistUserLevelOnOffice(SysUserLevelModel model)
        {
            bool isDuplicate = false;
            if (model.Id == 0)
            {
                if (userLevelService.Any(x => x.CompanyId == model.CompanyId
                                            && x.OfficeId == model.OfficeId
                                            && x.UserId == model.UserId
                                            && x.DepartmentId == null
                                            && x.GroupId == SystemConstants.SpecialGroup))
                {
                    isDuplicate = true;
                }
            }
            else
            {
                if (userLevelService.Any(x => x.CompanyId == model.CompanyId
                                            && x.OfficeId == model.OfficeId
                                            && x.UserId == model.UserId
                                            && x.GroupId == SystemConstants.SpecialGroup
                                            && x.DepartmentId == null
                                            && x.Id != model.Id))
                {
                    isDuplicate = true;
                }
            }
            return isDuplicate;
        }

        private bool CheckExistUserLevelOnCompany(SysUserLevelModel model)
        {
            bool isDuplicate = false;
            if (model.Id == 0)
            {
                if (userLevelService.Any(x => x.CompanyId == model.CompanyId
                                            && x.UserId == model.UserId
                                            && x.OfficeId == null
                                            && x.DepartmentId == null
                                            && x.GroupId == SystemConstants.SpecialGroup))
                {
                    isDuplicate = true;
                }
            }
            else
            {
                if (userLevelService.Any(x => x.CompanyId == model.CompanyId
                                            && x.UserId == model.UserId
                                            && x.GroupId == SystemConstants.SpecialGroup
                                            && x.OfficeId == null
                                            && x.DepartmentId == null
                                            && x.Id != model.Id))
                {
                    isDuplicate = true;
                }
            }
            return isDuplicate;
        }

        /// <summary>
        /// update an existed item
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        public IActionResult Update(SysUserLevelModel model)
        {
            model.UserModified = currentUser.UserID;
            model.DatetimeModified = DateTime.Now;
            var hs = userLevelService.Update(model, x => x.Id == model.Id);
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
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var item = userLevelService.GetDetail(id);
            //item.Active = false;
            //item.UserModified = currentUser.UserID;
            //item.InactiveOn = DateTime.Now;
            //var hs = userLevelService.Update(item, x => x.Id == id);
            var hs = userLevelService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Query user level
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>

        [HttpPost]
        [Route("Query")]
        public IActionResult Get(SysUserLevelCriteria criteria)
        {
            var results = userLevelService.Query(criteria);
            return Ok(results);
        }

        /// <summary>
        /// add user to department
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddUserToDepartment")]
        [Authorize]
        public IActionResult AddUserToDepartment(List<SysUserLevelModel> users)
        {
            if (!ModelState.IsValid) return BadRequest();
            SysUserLevelModel modelDupdatelidate = null;
            var checkDupUser = users.GroupBy(x => x.UserId)
                                .Where(t => t.Count() > 1)
                                .Select(y => y.Key)
                                .ToList();
            if (checkDupUser.Count > 0)
            {
                return Ok(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_ITEM_DUPLICATE_USER_ON_USER_LEVEL, "department"].Value, Data = checkDupUser });
            }
            foreach (var item in users)
            {
                if (CheckExistUserLevelOnDepartment(item))
                {
                    modelDupdatelidate = item;
                    break;
                };
            }
            if (modelDupdatelidate != null)
            {
                return Ok(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_ITEM_EXISTED_USER_ON_USER_LEVEL, "department"].Value, Data = modelDupdatelidate });
            }
            var hs = userLevelService.AddUser(users);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private bool CheckExistUserLevelOnDepartment(SysUserLevelModel model)
        {
            bool isDuplicate = false;
            if (model.Id == 0)
            {
                if (userLevelService.Any(x => x.CompanyId == model.CompanyId
                                            && x.OfficeId == model.OfficeId
                                            && x.DepartmentId == model.DepartmentId
                                            && x.GroupId == SystemConstants.SpecialGroup
                                            && x.UserId == model.UserId))
                {
                    isDuplicate = true;
                }
            }
            else
            {
                if (userLevelService.Any(x => x.CompanyId == model.CompanyId
                                            && x.OfficeId == model.OfficeId
                                            && x.DepartmentId == model.DepartmentId
                                            && x.UserId == model.UserId
                                            && x.GroupId == SystemConstants.SpecialGroup
                                            && x.Id != model.Id))
                {
                    isDuplicate = true;
                }
            }
            return isDuplicate;
        }

        /// <summary>
        /// add user to group
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddUserToGroup")]
        [Authorize]
        public IActionResult AddUserToGroup(List<SysUserLevelModel> users)
        {
            if (!ModelState.IsValid) return BadRequest();
            SysUserLevelModel modelDupdatelidate = null;
            var checkDupUser = users.GroupBy(x => x.UserId)
                                .Where(t => t.Count() > 1)
                                .Select(y => y.Key)
                                .ToList();
            if (checkDupUser.Count > 0)
            {
                return Ok(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_ITEM_DUPLICATE_USER_ON_USER_LEVEL, "group"].Value, Data = checkDupUser });
            }
            foreach (var item in users)
            {
                if (CheckExistUserLevelOnGroup(item))
                {
                    modelDupdatelidate = item;
                    break;
                };
            }
            if (modelDupdatelidate != null)
            {
                return Ok(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_ITEM_EXISTED_USER_ON_USER_LEVEL, "group"].Value, Data = modelDupdatelidate });
            }
            var hs = userLevelService.AddUser(users);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private bool CheckExistUserLevelOnGroup(SysUserLevelModel model)
        {
            bool isDuplicate = false;
            if (model.Id == 0)
            {
                if (userLevelService.Any(x => x.CompanyId == model.CompanyId
                                            && x.OfficeId == model.OfficeId
                                            && x.DepartmentId == model.DepartmentId
                                            && x.UserId == model.UserId
                                            && x.GroupId == model.GroupId
                                            && x.GroupId != SystemConstants.SpecialGroup))
                {
                    isDuplicate = true;
                }
            }
            else
            {
                if (userLevelService.Any(x => x.CompanyId == model.CompanyId
                                            && x.OfficeId == model.OfficeId
                                            && x.DepartmentId == model.DepartmentId
                                            && x.UserId == model.UserId
                                            && x.GroupId != SystemConstants.SpecialGroup
                                            && x.GroupId == model.GroupId
                                            && x.Id != model.Id))
                {
                    isDuplicate = true;
                }
            }
            return isDuplicate;
        }

        /// <summary>
        /// Get user by type
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetUsersByType")]
        public IActionResult GetUsersByType(UserLevelCriteria criteria)
        {
            var results = userLevelService.GetUsersByType(criteria);
            return Ok(results);
        }
    }
}
