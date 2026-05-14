using BLL;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.DTO;
using System.IdentityModel.Tokens.Jwt;

namespace server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private UsersBLL _usersBLL;

        public UsersController(UsersBLL usersBLL)
        {
            this._usersBLL = usersBLL;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> RegisterUser(RegisterUserDTO dto)
        {
            try
            {
                UserDTO userDto = await _usersBLL.RegisterUser(dto);
                userDto.Jwt = JWTService.Generate(userDto.Id);

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> LoginUser(LoginUserDTO dto)
        {
            try
            {
                UserDTO userDto = await _usersBLL.LoginUser(dto);
                userDto.Jwt = JWTService.Generate(userDto.Id);

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("refreshToken")]
        public IActionResult RefreshToken(BaseDTO dto)
        {
            try
            {
                JwtSecurityToken token = JWTService.Verify(dto.Jwt);
                Guid userId = new Guid(token.Issuer);
                string newToken = JWTService.Generate(userId);

                return Ok(new BaseDTO
                {
                    Jwt = newToken,
                });
            }
            catch (Exception exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
        }
    }
}
