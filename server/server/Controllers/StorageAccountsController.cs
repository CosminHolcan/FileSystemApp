using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using server.BLL;
using server.DTO;
using server.Utils;

namespace server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StorageAccountsController : Controller
    {
        private JWTService _jwtService;
        private StorageAccountsBLL _storageAccountsBLL;

        public StorageAccountsController(JWTService jWTService, StorageAccountsBLL storageAccountsBLL)
        {
            this._jwtService = jWTService;
            this._storageAccountsBLL = storageAccountsBLL;
        }

        [HttpPost("all")]
        public async Task<ActionResult<AppFileDTO>> GetAllStorageAccounts(BaseDTO dto)
        {
            try
            {
                JwtSecurityToken token = _jwtService.Verify(dto.Jwt);
                Guid userId = new Guid(token.Issuer);

                List<StorageAccountDTO> storageAccountsDTO = await this._storageAccountsBLL.GetAllStorageAccounts();
                return Ok(storageAccountsDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
