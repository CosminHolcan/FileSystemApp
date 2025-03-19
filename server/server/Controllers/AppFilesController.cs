using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using server.BLL;
using server.DTO;
using server.Utils;

namespace server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AppFilesController : Controller
    {
        private AppFilesBLL _appFilesBLL;
        private JWTService _jwtService;

        public AppFilesController(AppFilesBLL appFiles, JWTService jwtService)
        {
            this._appFilesBLL = appFiles;
            this._jwtService = jwtService;
        }

        [HttpPost("add")]
        public async Task<ActionResult<AppFileDTO>> AddFile([FromForm] IFormFile file, [FromForm] CreateFileDTO dtoData)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided.");
                }

                //var dtoData = JsonSerializer.Deserialize<CreateFileDTO>(dtoData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                JwtSecurityToken token = _jwtService.Verify(dtoData.Jwt);
                Guid userId = new Guid(token.Issuer);

                AppFileDTO appFileDTO = await _appFilesBLL.AddFile(dtoData, userId);

                BlobServiceClient blobServiceClient = new BlobServiceClient(dtoData.StorageAccount);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("container");
                BlobClient blobClient = containerClient.GetBlobClient(dtoData.Name);

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                return Ok(appFileDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("filesByUser")]
        public async Task<ActionResult<AppFileDTO>> GetFilesByUser(BaseDTO dto)
        {
            try
            {
                JwtSecurityToken token = _jwtService.Verify(dto.Jwt);
                Guid userId = new Guid(token.Issuer);

                List<AppFileDTO> appFileDTOs = await this._appFilesBLL.GetFilesByUser(userId);
                return Ok(appFileDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
