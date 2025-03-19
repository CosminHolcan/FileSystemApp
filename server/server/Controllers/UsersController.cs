using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using server.BLL;
using server.DTO;
using server.Utils;

namespace server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private UsersBLL _usersBLL;
        private JWTService _jwtService;

        public UsersController(UsersBLL usersBLL, JWTService jwtService)
        {
            this._usersBLL = usersBLL;
            this._jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> RegisterUser(RegisterUserDTO dto)
        {
            try
            {
                UserDTO userDto = await _usersBLL.RegisterUser(dto);
                userDto.Jwt = this._jwtService.Generate(userDto.Id);

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
                userDto.Jwt = this._jwtService.Generate(userDto.Id);

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
                JwtSecurityToken token = _jwtService.Verify(dto.Jwt);
                Guid userId = new Guid(token.Issuer);
                string newToken = _jwtService.Generate(userId);

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
