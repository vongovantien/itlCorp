using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.System.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysCompanyController : ControllerBase
    {
        private readonly ISysImageService imageService;
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysCompanyService sysCompanyService;
        private readonly ICurrentUser currentUser;
        private readonly IMapper mapper;

        public SysCompanyController(IStringLocalizer<LanguageSub> localizer, ISysCompanyService sysCompanyService,
            IMapper mapper, ICurrentUser currUser, ISysImageService imageService
            ) 
        {
            stringLocalizer = localizer;
            this.sysCompanyService = sysCompanyService;
            this.mapper = mapper;
            this.imageService = imageService;
            currentUser = currUser;
        }


        [HttpGet]
        public IActionResult Get()
        {
            var response = sysCompanyService.Get();
            return Ok(response);
        }

        /// <summary>
        /// get by userId
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetByUserId")]
        public IActionResult GetByUserId(string id)
        {
            var results = sysCompanyService.GetByUserId(id);
            return Ok(results);
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Get(SysCompanyCriteria search)
        {
            var results = sysCompanyService.Query(search);
            return Ok(results);
        }

        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(SysCompanyCriteria search, int page, int size)
        {
            var data = sysCompanyService.Paging(search, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Add(SysCompanyAddModel company)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(company);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var hs = sysCompanyService.Add(company);

            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = company };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetBy(Guid id)
        {
            var result = sysCompanyService.Get(id);
            if (result == null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Không tìm thấy Company", Data = result });
            }
            else
            {
                return Ok(new ResultHandle { Status = true, Message = "Success", Data = result });
            }
        }

        [HttpPut]
        [Route("{id}/Update")]
        [Authorize]
        public IActionResult Update(Guid id, SysCompanyAddModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var hs = sysCompanyService.Update(id, model);

            var message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var hs = sysCompanyService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private string CheckExist(SysCompanyAddModel company)
        {
            string message = string.Empty;
            if (company.CompanyCode != "" || company.CompanyCode != null)
            {
                if (sysCompanyService.Any(x => (x.Code == company.CompanyCode)))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            return message;
        }

        [HttpGet]
        [Route("GetCompanyPermissionLevel")]
        public List<SysCompanyModel> GetImages()
        {
            var companyLevel = sysCompanyService.GetCompanyPermissionLevel();
            return companyLevel;
        }
        //[HttpDelete]
        //[Route("Delete")]
        //[Authorize]
        //public async Task<bool> Delete([FromForm]SysImage image)
        //{
        //    HandleState img = imageService.Delete(image.Id);
        //    string message = HandleError.GetMessage(img, Crud.Delete);
        //    ResultHandle result = new ResultHandle { Status = img.Success, Message = stringLocalizer[message].Value };

        //    if (!img.Success)
        //    {
        //        return false;
        //    }
        //    var result1 = await ImageHelper.DeleteFile(image.Name, "Company", "images");
        //    return result1;
        //}
    }
}