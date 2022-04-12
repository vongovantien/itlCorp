using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.IdentityServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAllOrigins")]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        readonly ISysUserLogService userLogService;
        public AccountController(ISysUserLogService userLog)
        {
            userLogService = userLog;
        }

        [HttpGet("Signout")]
        public async Task<IActionResult> SignoutAsync()
        {
            var userId = User.Claims.FirstOrDefault(wh => wh.Type == "id")?.Value;
            var uLogs = userLogService.Get(x => x.UserId == userId && x.LoggedOffOn == null).OrderByDescending(x => x.LoggedInOn);
            if (uLogs.Count() > 0)
            {
                foreach(var item in uLogs)
                {
                    item.LoggedOffOn = DateTime.Now;
                    var s = userLogService.Update(item, x => x.Id == item.Id, false);
                }
                userLogService.SubmitChanges();
            }
            await HttpContext.SignOutAsync();
            return Ok();
        }

        [HttpPost("GetLDAPInfo")]
        public IActionResult GetLDAPInfo([FromBody] List<string> username)
        {
            var data = userLogService.GetLDAPInfo(username, out List<string> userInactive);

            return Ok(new { userInactive });
        }
    }
}
