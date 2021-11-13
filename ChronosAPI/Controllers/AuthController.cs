using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChronosAPI.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using ChronosAPI.Services;
using ChronosAPI.Helpers;

namespace ChronosAPI.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [Route("api/auth/login")]
        [HttpPost]
        public IActionResult Login(AuthenticateRequest req)
        {
            var response = _userService.Authenticate(req);

            if(response == null)
            {
                return BadRequest(new { message = "Username or password is incorrect!" });
            }

            return Ok(response);
        }


        [Route("api/auth/register")]
        [HttpPost]
        public IActionResult Register(User user)
        {
            var response = _userService.Register(user);
            if (response == null)
            {
                return BadRequest(new { message = "Register failed" });
            }

            return Ok(response);
        }
    }
}