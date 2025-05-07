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
    public class AppFilesController : Controller
    {
        private AppFilesBLL _appFilesBLL;
        private FileVersionsBLL _fileVersionsBLL;
        private StorageAccountsBLL _storageAccountsBLL;

        public AppFilesController(AppFilesBLL appFilesBLL, FileVersionsBLL fileVersionsBLL, StorageAccountsBLL storageAccountsBLL)
        {
            this._appFilesBLL = appFilesBLL;
            this._fileVersionsBLL = fileVersionsBLL;
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

                JwtSecurityToken token = JWTService.Verify(dtoData.Jwt);
                Guid userId = new Guid(token.Issuer);

                AppFileDTO appFileDTO = await _appFilesBLL.AddFile(dtoData, userId);

                StorageAccount storageAccount = await this._storageAccountsBLL.GetStorageAccountById(appFileDTO.StorageAccountId);

                BlobServiceClient blobServiceClient = new BlobServiceClient(storageAccount.ConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("container");
                BlobClient blobClient = containerClient.GetBlobClient(appFileDTO.Id.ToString() + Path.GetExtension(appFileDTO.Name));

                string versionId = "";

                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = GeneralUtils.GetContentType(appFileDTO.Name),
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
                    if ((bool)appFileDTO.Versioning)
                    {
                        versionId = result.Value.VersionId;
                    }
                }

                if (versionId != "")
                {
                    AddFileVersionDTO versionDTO = new AddFileVersionDTO()
                    {
                        Name = dtoData.VersionName,
                        AzureId = versionId,
                        OriginalFileId = appFileDTO.Id
                    };
                    await this._fileVersionsBLL.AddVersion(versionDTO);
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
                JwtSecurityToken token = JWTService.Verify(dto.Jwt);
                Guid userId = new Guid(token.Issuer);

                List<AppFileDTO> appFileDTOs = await this._appFilesBLL.GetFilesByUser(userId);
                return Ok(appFileDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("get/{fileId}")]
        public async Task<ActionResult<FileWithVersionsDTO>> ReadFileWithVersionsById(Guid fileId, [FromBody] BaseDTO dto)
        {
            try
            {
                JwtSecurityToken token = JWTService.Verify(dto.Jwt);
                Guid userId = new Guid(token.Issuer);

                FileWithVersionsDTO fileWithVersionsDTO = await this._appFilesBLL.GetFileByIdWithVersions(fileId);
                return Ok(fileWithVersionsDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
