using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Mobile.Common;
using API.Mobile.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Mobile.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserController : ControllerBase
    {
        private User user = FakeData.user;

        [HttpGet]
        public IActionResult Login(string staffId, string password)
        {
            if(staffId == user.StaffId && password == user.Password)
            {
                return Ok(user);
            }
            else
            {
                return BadRequest("Not Found");
            }
        }
    }
}