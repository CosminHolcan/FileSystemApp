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
        private StorageAccountsBLL _storageAccountsBLL;

        public StorageAccountsController(StorageAccountsBLL storageAccountsBLL)
        {
            this._storageAccountsBLL = storageAccountsBLL;
        }

        [HttpPost("all")]
        public async Task<ActionResult<AppFileDTO>> GetAllStorageAccounts(BaseDTO dto)
        {
            try
            {
                JwtSecurityToken token = JWTService.Verify(dto.Jwt);
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
