using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Mobile.Common;
using API.Mobile.Infrastructure.Middlewares;
using API.Mobile.Models;
using API.Mobile.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API.Mobile.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class UserController : ControllerBase
    {
        private User user = FakeData.user;
        private readonly IConfiguration configuration;
        public UserController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (model.StaffId == user.StaffId && model.Password == user.Password)
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.StaffId),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("UserId", user.UserId),
                    new Claim("Role", user.Role)
                };

                var token = new JwtSecurityToken
                (
                    issuer: configuration["Issuer"],
                    audience: configuration["Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddDays(30),
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["SigningKey"])),
                            SecurityAlgorithms.HmacSha256)
                );
                var u = new { user.StaffId, user.Role, user.UserId, user.Email };
                return Ok(new { u, token = new JwtSecurityTokenHandler().WriteToken(token) });
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}