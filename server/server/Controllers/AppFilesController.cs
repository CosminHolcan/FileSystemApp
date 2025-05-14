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

                AppFileDTO appFileDTO = await _appFilesBLL.AddFile(dtoData, userId, file);

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

        [HttpPost("getWithVersions/{fileId}")]
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

        [HttpPost("get/{fileId}")]
        public async Task<ActionResult<AppFileDTO>> ReadFileById(Guid fileId, [FromBody] BaseDTO dto)
        {
            try
            {
                JwtSecurityToken token = JWTService.Verify(dto.Jwt);
                Guid userId = new Guid(token.Issuer);

                AppFileDTO appFileDTO = await this._appFilesBLL.GetFileById(fileId);
                return Ok(appFileDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("uploadNewContent/{fileId}")]
        public async Task<ActionResult<AppFileDTO>> UploadNewContent(Guid fileId, [FromForm] string dto, [FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided.");
                }

                var dtoData = JsonSerializer.Deserialize<BaseDTO>(dto, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                JwtSecurityToken token = JWTService.Verify(dtoData.Jwt);
                Guid userId = new Guid(token.Issuer);

                AppFileDTO appFileDTO = await this._appFilesBLL.UploadNewContent(fileId, file);
                return Ok(appFileDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
