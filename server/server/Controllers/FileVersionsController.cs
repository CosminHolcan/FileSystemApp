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

                var result = await _fileVersionsBLL.AddVersion(userId, dtoData, file);
                return Ok(result);
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

                AppFile availableFile = await this._appFilesBLL.GetAvailableFileReplica(userId, originalFileId, true);
                List<FileVersionDTO> fileVersionsDTO = await this._fileVersionsBLL.GetFileVersionsByOriginalFileId(availableFile);
                
                return Ok(fileVersionsDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("delete/{fileVersionId}")]
        public async Task<IActionResult> DeleteFileVersion(Guid fileVersionId, [FromBody] BaseDTO dto)
        {
            try
            {
                JwtSecurityToken token = JWTService.Verify(dto.Jwt);
                Guid userId = new Guid(token.Issuer);

                await this._fileVersionsBLL.DeleteFileVersion(userId, fileVersionId);
                return NoContent(); ;
            }
            catch (Exception ex)
            {
                return BadRequest("File version could not be deleted.");
            }
        }

        [HttpPost("updateFileVersionName/{fileVersionId}")]
        public async Task<ActionResult<AppFileDTO>> UpdateFileName(Guid fileVersionId, [FromBody] UpdateFileNameDTO dto)
        {
            try
            {
                JwtSecurityToken token = JWTService.Verify(dto.Jwt);
                Guid userId = new Guid(token.Issuer);

                await this._fileVersionsBLL.UpdateFileVersionName(userId, fileVersionId, dto.NewFileName);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("File version name could not be updated.");
            }
        }
    }
}
