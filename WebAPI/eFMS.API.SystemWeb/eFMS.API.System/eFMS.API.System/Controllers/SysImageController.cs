using System;
using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.Infrastructure.Common;
using eFMS.API.System.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.System.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysImageUploadController : ControllerBase
    {
        private readonly ISysImageService imageService;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IMapper mapper;

        public SysImageUploadController(IStringLocalizer<LanguageSub> localizer, ISysImageService imageService, IMapper mapper)
        {
            this.imageService = imageService;
            this.stringLocalizer = localizer;
            this.mapper = mapper;
            //currentUser = currUser; //TODO
        }

        [HttpPost]
        [Route("image")]
        public IActionResult UploadImage(IFormFile file)
        {
            string folderName = Request.Headers["Module"];
            string objectId = Request.Headers["ObjectId"];
            var hs = imageService.UploadImage(file, folderName, objectId);

            ResultHandle result = new ResultHandle { Status = hs.Result.Status, Message = hs.Result.Message, Data = hs.Result.Data };
            if (!hs.Result.Status)
            {
                return BadRequest(result);
            }
            return Ok(result.Data);
        }

        [HttpGet]
        [Route("company")]
        public IActionResult GetImageCompany()
        {
            var response = imageService.GetImageCompany();
            return Ok(response);

        }

        [HttpGet]
        [Route("User")]
        public IActionResult GetImageUser(string userId)
        {
            var response = imageService.GetImageUser(userId);
            return Ok(response);

        }
    }
}