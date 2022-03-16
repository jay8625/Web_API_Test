using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Web_API_Test.DAL.Authentication;
using Web_API_Test.DAL.Exceptions;
using Web_API_Test.Service.Repositories;

namespace Web_API_Test.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly IAuthentication _authentication;

        public AuthenticateController(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var token = await _authentication.Login(model);
                if (token != null)
                {
                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
                }
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });
            }
            catch (StatusException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                var result = await _authentication.Register(model);
                return Ok("User Registered Sucessfully.");
            }
            catch (StatusException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            try
            {
                var result = await _authentication.AdminRegister(model);
                return Ok(new Response { Status = "Success", Message = "User created successfully!" });
            }
            catch (StatusException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}