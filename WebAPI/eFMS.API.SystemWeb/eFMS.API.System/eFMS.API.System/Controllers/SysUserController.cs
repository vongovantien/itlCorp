using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.System.DL.Common;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models.Criteria;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using eFMS.API.System.DL.Models;
using Microsoft.AspNetCore.Authorization;
using eFMS.API.Common;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using System.IO;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.Common.Infrastructure.Common;
using ITL.NetCore.Common;
using eFMS.API.System.Service.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.System.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysUserController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysUserService sysUserService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;
        private readonly ISysEmployeeService sysEmployeeService;
        private readonly ISysUserLevelService sysUserLevelService;

        public SysUserController(IStringLocalizer<LanguageSub> localizer, ISysUserService service, IMapper iMapper, ICurrentUser currUser, ISysEmployeeService isysEmployeeService, ISysUserLevelService isysUserLevelService)
        {
            stringLocalizer = localizer;
            sysUserService = service;
            mapper = iMapper;
            currentUser = currUser;
            sysEmployeeService = isysEmployeeService;
            sysUserLevelService = isysUserLevelService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var results = sysUserService.GetAll();
            return Ok(results);
        }

        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(SysUserCriteria criteria, int page, int size)
        {
            var data = sysUserService.Paging(criteria, page, size, out int? rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

       

        [HttpPost]
        [Route("Query")]
        public IActionResult Get(SysUserCriteria criteria)
        {
            var results = sysUserService.Query(criteria);
            return Ok(results);
        }

        [HttpPost]
        [Route("GetUserByLevel")]
        public IActionResult GetUserByLevel(SysUserCriteria criteria)
        {
            var results = sysUserService.QueryPermission(criteria);
            return Ok(results);
        }

        /// <summary>
        /// add new group
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public IActionResult Add(SysUserModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.Password = SystemConstants.Password;
            model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var existedMessage = CheckExistCode(model.SysEmployeeModel.StaffCode, "0");
            var existedName = CheckExistUserName(model.Username, "0");

            if (existedMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            if (existedName.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedName });
            }
            model.SysEmployeeModel.Active = true;
            model.SysEmployeeModel.Id = Guid.NewGuid().ToString();
            var hsEmloyee = sysEmployeeService.Insert(model.SysEmployeeModel);
            var messageEmployee = HandleError.GetMessage(hsEmloyee, Crud.Insert);
            ResultHandle resultEmployee = new ResultHandle { Status = hsEmloyee.Success, Message = stringLocalizer[messageEmployee].Value };
            if (hsEmloyee.Success)
            {
                model.EmployeeId = model.SysEmployeeModel.Id;
                model.UserCreated = model.UserModified = currentUser.UserID;
                model.Id = Guid.NewGuid().ToString();
                model.DatetimeCreated = model.DatetimeModified  = DateTime.Now;
                var hs = sysUserService.Insert(model);
                var message = HandleError.GetMessage(hs, Crud.Insert);

                ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value,Data = model.Id };
                if (!hs.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            else
            {
                return BadRequest(resultEmployee);
            }
        }

        /// <summary>
        /// update an existed group
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        public IActionResult Update(SysUserModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var userCurrent = sysUserService.GetUserById(model.Id);
            var existedMessage = CheckExistCode(model.SysEmployeeModel.StaffCode, userCurrent.EmployeeId);
            var existedName = CheckExistUserName(model.Username, model.Id);
            if (existedMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            if (existedName.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedName });
            }
            var employeeCurrent = sysEmployeeService.Get(x => x.Id == userCurrent.EmployeeId).FirstOrDefault();
            var hsEmployee = new HandleState();
            if (employeeCurrent != null)
            {
                model.SysEmployeeModel.Id = employeeCurrent.Id;
                model.SysEmployeeModel.UserModified = currentUser.UserID;
                model.SysEmployeeModel.DatetimeModified = DateTime.Now;
                model.SysEmployeeModel.Active = true;
                hsEmployee = sysEmployeeService.Update(model.SysEmployeeModel);
            }
            else
            {
                model.SysEmployeeModel.Id = Guid.NewGuid().ToString();

                model.SysEmployeeModel.UserCreated = model.SysEmployeeModel.UserModified = currentUser.UserID;
                model.SysEmployeeModel.DatetimeModified = DateTime.Now;
                model.SysEmployeeModel.Active = true;
                hsEmployee = sysEmployeeService.Insert(model.SysEmployeeModel);
            }
           

 
            var messageEmployee = HandleError.GetMessage(hsEmployee, Crud.Update);
            ResultHandle resultEmployee = new ResultHandle { Status = hsEmployee.Success, Message = stringLocalizer[messageEmployee].Value };
            if (hsEmployee.Success)
            {
                model.UserModified = currentUser.UserID;
                model.DatetimeModified = DateTime.Now;
                model.Password = userCurrent.Password;
                model.EmployeeId = model.SysEmployeeModel.Id;
                model.UserCreated = userCurrent.UserCreated;
                model.DatetimeCreated = userCurrent.DatetimeCreated;
                var hs = sysUserService.Update(model);
                var message = HandleError.GetMessage(hs, Crud.Update);

                ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
                if (!hs.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            else
            {
                return BadRequest(resultEmployee);
            }
     
        }

        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(string id)
        {
            var item = sysUserService.Get(x => x.Id == id).FirstOrDefault();
            if (item.Active == true)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_ITEM_IS_ACTIVE_NOT_ALLOW_DELETED].Value });
            }
            item.Active = false;
            //if (sysUserLevelService.Any(x=>x.UserId == id))
            //{
            //    return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_ITEM_IS_ACTIVE_NOT_ALLOW_DELETED].Value });
            //}
            var user = sysUserService.Get(x => x.Id == id).FirstOrDefault();
            var employee = sysEmployeeService.Get(x => x.Id == user.EmployeeId).FirstOrDefault();
            if(employee != null)
            {
                var hsEmployee = sysEmployeeService.Delete(x => x.Id == employee.Id, true);
            }
            var hs = sysUserService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        /// get user by id
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public IActionResult GetBy(string id)
        {
            var result = sysUserService.GetUserModelById(id);
            
            if (result == null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Error", Data = result });
            }
            else
            {
                return Ok(new ResultHandle { Status = true, Message = "Success", Data = result });
            }
        }
        /// <summary>
        /// reset password of user to default
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet]
        [Route("ResetPassword")]
        [Authorize]
        public IActionResult ResetPassword(string id)
        {
            var item = sysUserService.Get(x => x.Id == id).FirstOrDefault();
            item.Password = SystemConstants.Password;
            item.Password = BCrypt.Net.BCrypt.HashPassword(item.Password);
            var hs = sysUserService.Update(item, x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut]
        [Route("UpdateProfile")]
        [Authorize]
        public IActionResult UpdateProfile(UserProfileCriteria criteria)
        {
            HandleState hs = sysUserService.UpdateProfile(criteria, out object data);
            if (!hs.Success)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Error"});
            }
            return Ok(new ResultHandle { Status = true, Message = "Success", Data = data });
        }

        /// <summary>
        /// download file excel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("DownloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            var result = await new FileHelper().ExportExcel(Directory.GetCurrentDirectory(), Templates.SysUser.ExelImportFileName);
            if (result != null)
            {
                return result;
            }
            else
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
            }
        }

        /// <summary>
        /// read data from file excel
        /// </summary>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("uploadFile")]
        [Authorize]
        public IActionResult UploadFile(IFormFile uploadedFile)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.NOT_FOUND_DATA_EXCEL].Value });
                List<SysUserImportModel> list = new List<SysUserImportModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    var userobj = new SysUserImportModel
                    {
                        IsValid = true,
                        StaffCode = worksheet.Cells[row, 2].Value?.ToString(),
                        Username =  worksheet.Cells[row, 3].Value?.ToString(),
                        EmployeeNameVn = worksheet.Cells[row, 4].Value?.ToString(),
                        EmployeeNameEn = worksheet.Cells[row, 5].Value?.ToString(),
                        Title =  worksheet.Cells[row, 6].Value?.ToString(),
                        UserType =  worksheet.Cells[row, 7].Value?.ToString(),
                        Status = worksheet.Cells[row, 8].Value?.ToString(),
                        WorkingStatus = worksheet.Cells[row, 9].Value?.ToString(),
                        Email = worksheet.Cells[row, 10].Value?.ToString(),
                        Tel = worksheet.Cells[row, 11].Value?.ToString(),
                        Description = worksheet.Cells[row, 12].Value?.ToString()
                    };
                    list.Add(userobj);
                }
                var data = sysUserService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);

            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });

        }

        [HttpPost]
        [Route("Import")]
        [Authorize]
        public IActionResult Import([FromBody]List<SysUserViewModel> data)
        {
            var hs = sysUserService.Import(data);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Import successfully !!!" };
            if (!hs.Success)
            {
                return BadRequest(new ResultHandle { Status = false, Message = hs.Message.ToString() });
            }
            return Ok(result);
        }


        private string CheckExistUserName(string username, string id)
        {
            string message = string.Empty;
            if (!string.IsNullOrEmpty(username))
            {
                if (id == "0")
                {
                    if (sysUserService.Any(x => x.Username.ToLower().Trim() == username.ToLower().Trim()))
                    {
                        message = stringLocalizer[SystemLanguageSub.MSG_USERNAME_EXISTED].Value;
                    }
                }
                else
                {
                    if (sysUserService.Any(x=>x.Username!= null && x.Username.ToLower().Trim() == username.ToLower().Trim() && x.Id != id))
                    {
                        message = stringLocalizer[SystemLanguageSub.MSG_USERNAME_EXISTED].Value;
                    }
                }
            }
            return message;
        }


        private string CheckExistCode(string code, string id)
        {
            string message = string.Empty;
            if (!string.IsNullOrEmpty(code))
            {
                if (id == "0")
                {
                    if (sysEmployeeService.Any(x => x.StaffCode == code))
                    {
                        message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                    }
                }
                else
                {
                    if (sysEmployeeService.Any(x => x.StaffCode == code && x.Id != id))
                    {
                        message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                    }
                }
            }

            return message;
        }


    }
}
