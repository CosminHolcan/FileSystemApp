using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using server.BLL;
using server.DAL.Entities;
using server.DTO;
using server.Utils;

namespace server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AppFilesController : Controller
    {
        private JWTService _jwtService;
        private AppFilesBLL _appFilesBLL;
        private StorageAccountsBLL _storageAccountsBLL;

        public AppFilesController(JWTService jwtService, AppFilesBLL appFiles, StorageAccountsBLL storageAccountsBLL)
        {
            this._appFilesBLL = appFiles;
            this._jwtService = jwtService;
            this._storageAccountsBLL = storageAccountsBLL;
        }

        [HttpPost("add")]
        public async Task<ActionResult<AppFileDTO>> AddFile([FromForm] IFormFile file,[FromForm] string dto)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided.");
                }

                var dtoData = JsonSerializer.Deserialize<CreateFileDTO>(dto, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                dtoData.Versionning = dtoData.Versionning == null ? false : true;

                JwtSecurityToken token = _jwtService.Verify(dtoData.Jwt);
                Guid userId = new Guid(token.Issuer);

                AppFileDTO appFileDTO = await _appFilesBLL.AddFile(dtoData, userId);

                StorageAccount storageAccount = await this._storageAccountsBLL.GetStorageAccountById(appFileDTO.StorageAccountId);

                BlobServiceClient blobServiceClient = new BlobServiceClient(storageAccount.ConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("container");
                BlobClient blobClient = containerClient.GetBlobClient(appFileDTO.Id.ToString() + Path.GetExtension(appFileDTO.Name));

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
