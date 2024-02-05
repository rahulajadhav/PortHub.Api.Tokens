using Microsoft.AspNetCore.Mvc;
using PortHub.Api.Tokens.Data.Models.ViewModel;
using PortHub.Api.Tokens.Data.Models.viewModels;
using PortHub.Api.Tokens.Interface;

namespace PortHub.Api.Tokens.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private ITokenGenerator _service;
        public TokenController(ITokenGenerator tokenGenerator)
        {
            _service = tokenGenerator;
            
        }
        [HttpPost("refresh-login-user/")]

        public async Task<IActionResult> RefreshToken([FromBody] ApplicationUserVM user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please provide all the required Field");
            }
            var result = await _service.RefreshTokenGeneratorAsync(user);
            if (result.IsSuccess)
            {
                return Ok(result.refreshToken);
            }
            else {
                return BadRequest(result.ErrorMessage);
            };


        }

        [HttpPost("verify-Token/")]

        public async Task<IActionResult> verifyToken([FromBody] RefreshTokenVM token)
        { 
            if (!ModelState.IsValid)
            {
                return BadRequest("Please provide Token");
            }
            var result = await _service.RefreshTokenVM(token);
            if (result.isSuccess)
            {
                return Ok(result.auth);
            }
            else {
                return BadRequest(result.ErrorMessage);
            }
        }

    }
}
