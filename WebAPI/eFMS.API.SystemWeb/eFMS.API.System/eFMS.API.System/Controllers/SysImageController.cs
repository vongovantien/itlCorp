using System;
using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.Infrastructure.Common;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.API.System.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using eFMS.API.Common.Helpers;
using ITL.NetCore.Common;
using System.Threading.Tasks;

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
        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public async Task<bool> Delete([FromForm]SysImage image)
        {
            HandleState img = imageService.Delete(image.Id);
            string message = HandleError.GetMessage(img, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = img.Success, Message = stringLocalizer[message].Value };

            if (!img.Success)
            {
                return false;
            }
            var result1 = await ImageHelper.DeleteFile(image.Name, image.Folder, "images");
            return result1;
        }
     
    }
}