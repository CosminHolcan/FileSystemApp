using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using server.BLL;
using server.DAL.Entities;
using server.DTO;
using server.Utils;

namespace server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileVersionsController : Controller
    {
        private AppFilesBLL _appFilesBLL;
        private FileVersionsBLL _fileVersionsBLL;

        public FileVersionsController(AppFilesBLL appFilesBLL, FileVersionsBLL fileVersionsBLL)
        {
            this._appFilesBLL = appFilesBLL;
            this._fileVersionsBLL = fileVersionsBLL;
        }

        [HttpPost("add")]
        public async Task<ActionResult<FileVersionDTO>> AddFileVersion([FromForm] IFormFile file, [FromForm] string dto)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided.");
                }

                var dtoData = JsonSerializer.Deserialize<AddFileVersionDTO>(dto, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                JwtSecurityToken token = JWTService.Verify(dtoData.Jwt);
                Guid userId = new Guid(token.Issuer);

                AppFile appFile = await _appFilesBLL.GetFileByIdWithStorageAccount(dtoData.OriginalFileId);

                BlobServiceClient blobServiceClient = new BlobServiceClient(appFile.StorageAccount.ConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("container");
                BlobClient blobClient = containerClient.GetBlobClient(appFile.Id.ToString() + Path.GetExtension(appFile.Name));

                string versionId = "";

                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = GeneralUtils.GetContentType(appFile.Name),
                    ContentDisposition = "inline"
                };

                using (var stream = file.OpenReadStream())
                {
                    var result = await blobClient.UploadAsync(
                        stream,
                        new BlobUploadOptions
                        {
                            HttpHeaders = blobHttpHeaders,
                        });
                    versionId = result.Value.VersionId;
                }

                dtoData.AzureId = versionId;
                FileVersionDTO fileVersionDTO = await _fileVersionsBLL.AddVersion(dtoData);
                return Ok(fileVersionDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("get/{originalFileId}")]
        public async Task<ActionResult<FileVersionDTO>> GetFileVersionByOriginalFileId(Guid originalFileId, [FromBody] BaseDTO dto)
        {
            try
            {
                JwtSecurityToken token = JWTService.Verify(dto.Jwt);
                Guid userId = new Guid(token.Issuer);

                List<FileVersionDTO> fileVersionsDTO = await this._fileVersionsBLL.GetFileVersionsByOriginalFileId(originalFileId);
                return Ok(fileVersionsDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
