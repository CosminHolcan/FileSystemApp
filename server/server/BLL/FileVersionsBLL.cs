using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using server.DAL;
using server.DAL.Entities;
using server.DTO;
using server.Utils;

namespace server.BLL
{
    public class FileVersionsBLL
    {
        private FileVersionsDAL _fileVersionsDAL;
        private AppFilesDAL _appFilesDAL;
        private readonly ILogger<FileVersionsBLL> _logger;

        public FileVersionsBLL(FileVersionsDAL fileVersionsDAL, AppFilesDAL appFilesDAL, ILogger<FileVersionsBLL> logger)
        {
            this._fileVersionsDAL = fileVersionsDAL;
            this._appFilesDAL = appFilesDAL;
            this._logger = logger;
        }

        public async Task<FileVersionDTO> AddVersion(Guid userId, AddFileVersionDTO dto, IFormFile file)
        {
            _logger.LogInformation("AddVersion called by user {UserId} for original file {OriginalFileId}", userId, dto?.OriginalFileId);

            AppFile appFile = await _appFilesDAL.GetFileByIdWithStorageAccount(dto.OriginalFileId);
            if (appFile.UserId != userId)
            {
                _logger.LogError("Unauthorized AddVersion attempt by user {UserId} for original file {OriginalFileId}", userId, dto.OriginalFileId);
                throw new Exception("Unauthorized operation.");
            }

            DateTime startingTime = DateTime.Now;
            FileVersionDTO toReturn = null;
            bool firstFileFailed = false;

            try
            {
                string versionId = await this.UploadFileToBlob(appFile, file);

                var fileVersion = new FileVersion
                {
                    Name = dto.Name,
                    AzureId = versionId,
                    OriginalFileId = dto.OriginalFileId,
                    CreationTime = startingTime
                };

                FileVersion createdVersion = await _fileVersionsDAL.AddVersion(fileVersion);
                await this._appFilesDAL.UpdateTimeInteractionsForFile(dto.OriginalFileId, startingTime, true);

                toReturn = new FileVersionDTO
                {
                    Id = createdVersion.Id,
                    Name = createdVersion.Name,
                    OriginalFileId = createdVersion.OriginalFileId,
                    CreationTime = GeneralUtils.FormatDateTime(createdVersion.CreationTime),
                    TokenSAS = await SASTokensGenerator.GenerateSasTokenAsync(appFile.StorageAccount.BlobServicePath, "container", GeneralUtils.GetAzureFileName(appFile), createdVersion.AzureId)
                };

                _logger.LogInformation("Added version {VersionId} for original file {OriginalFileId} in primary storage", createdVersion.Id, dto.OriginalFileId);
            }
            catch (Exception ex)
            {
                firstFileFailed = true;
                _logger.LogError(ex, "Failed to add version to primary storage for original file {OriginalFileId}", dto.OriginalFileId);
            }

            try
            {
                if (appFile.ReplicaId != null)
                {
                    AppFile replicaFile = await _appFilesDAL.GetFileByIdWithStorageAccount((Guid)appFile.ReplicaId);
                    string replicaVerionId = await this.UploadFileToBlob(replicaFile, file);

                    FileVersion replicaFileVersion = new FileVersion
                    {
                        Name = dto.Name,
                        AzureId = replicaVerionId,
                        OriginalFileId = replicaFile.Id,
                        CreationTime = startingTime
                    };

                    FileVersion createdVersion = await _fileVersionsDAL.AddVersion(replicaFileVersion);
                    await this._appFilesDAL.UpdateTimeInteractionsForFile(replicaFile.Id, startingTime, true);

                    if (firstFileFailed)
                    {
                        toReturn = new FileVersionDTO
                        {
                            Id = createdVersion.Id,
                            Name = createdVersion.Name,
                            OriginalFileId = createdVersion.OriginalFileId,
                            CreationTime = GeneralUtils.FormatDateTime(createdVersion.CreationTime),
                            TokenSAS = await SASTokensGenerator.GenerateSasTokenAsync(replicaFile.StorageAccount.BlobServicePath, "container", GeneralUtils.GetAzureFileName(replicaFile), createdVersion.AzureId)
                        };

                        _logger.LogInformation("Added version {VersionId} for original file {OriginalFileId} in replica storage", createdVersion.Id, dto.OriginalFileId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add version to replica storage for original file {OriginalFileId}", dto.OriginalFileId);
            }

            if (toReturn == null)
            {
                _logger.LogError("AddVersion failed for original file {OriginalFileId}", dto.OriginalFileId);
                throw new Exception("Could not add new version for file.");
            }

            return toReturn;
        }

        public async Task<List<FileVersionDTO>> GetFileVersionsByOriginalFileId(AppFile appFile)
        {
            _logger.LogDebug("GetFileVersionsByOriginalFileId called for file {FileId}", appFile.Id);
            List<FileVersion> fileVersions = await this._fileVersionsDAL.GetFileVersionsByOriginalFileId(appFile.Id);
            fileVersions.Sort((v1, v2) => v1.CreationTime.CompareTo(v2.CreationTime));
            string azureFileName = appFile.Id.ToString() + Path.GetExtension(appFile.Name);

            var result = new List<FileVersionDTO>();
            foreach (var f in fileVersions)
            {
                var token = await SASTokensGenerator.GenerateSasTokenAsync(appFile.StorageAccount.BlobServicePath, "container", azureFileName, f.AzureId);
                result.Add(new FileVersionDTO()
                {
                    Id = f.Id,
                    Name = f.Name,
                    OriginalFileId = f.OriginalFileId,
                    CreationTime = GeneralUtils.FormatDateTime(f.CreationTime),
                    AzureId = f.AzureId,
                    TokenSAS = token
                });
            }

            _logger.LogInformation("GetFileVersionsByOriginalFileId returning {Count} versions for file {FileId}", result.Count, appFile.Id);
            return result;
        }

        public async Task DeleteFileVersion(Guid userId, Guid fileVersionId)
        {
            _logger.LogInformation("DeleteFileVersion called for fileVersion {FileVersionId} by user {UserId}", fileVersionId, userId);
            FileVersion fileVersion = await this._fileVersionsDAL.GetFileVersionById(fileVersionId);
            if (fileVersion == null)
            {
                _logger.LogDebug("DeleteFileVersion: fileVersion {FileVersionId} not found", fileVersionId);
                return;
            }

            AppFile file, replica = null;
            file = await this._appFilesDAL.GetFileByIdWithStorageAccount(fileVersion.OriginalFileId);
            if (file == null)
            {
                _logger.LogDebug("DeleteFileVersion: original file {OriginalFileId} not found", fileVersion.OriginalFileId);
                return;
            }

            if (file.UserId != userId)
            {
                _logger.LogError("Unauthorized DeleteFileVersion attempt for fileVersion {FileVersionId} by user {UserId}", fileVersionId, userId);
                throw new Exception("Unauthorized operation.");
            }

            if (file.ReplicaId != null)
            {
                replica = await this._appFilesDAL.GetFileByIdWithStorageAccount((Guid)file.ReplicaId);
            }

            try
            {
                await this._fileVersionsDAL.DeleteFileVersion(fileVersion);
                await this.DeleteFileVersionFromAzure(file, fileVersion);
                _logger.LogInformation("Deleted file version {FileVersionId} from primary storage", fileVersionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file version {FileVersionId} from primary storage", fileVersionId);
            }

            try
            {
                if (replica != null)
                {
                    FileVersion replicaFileVersion = replica.Versions.FirstOrDefault(fv => fv.CreationTime == fileVersion.CreationTime);
                    if (replicaFileVersion == null)
                    {
                        _logger.LogDebug("DeleteFileVersion: matching replica file version not found for original creation time {CreationTime}", fileVersion.CreationTime);
                        return;
                    }

                    await this._fileVersionsDAL.DeleteFileVersion(replicaFileVersion);
                    await this.DeleteFileVersionFromAzure(replica, replicaFileVersion);
                    _logger.LogInformation("Deleted file version {FileVersionId} from replica storage", replicaFileVersion.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file version from replica storage for original file version {FileVersionId}", fileVersionId);
            }
        }

        public async Task UpdateFileVersionName(Guid userId, Guid fileVersionId, string newName)
        {
            _logger.LogInformation("UpdateFileVersionName called for fileVersion {FileVersionId} by user {UserId} newName={NewName}", fileVersionId, userId, newName);
            FileVersion fileVersion = await this._fileVersionsDAL.GetFileVersionById(fileVersionId);
            if (fileVersion == null)
            {
                _logger.LogDebug("UpdateFileVersionName: fileVersion {FileVersionId} not found", fileVersionId);
                return;
            }

            AppFile file, replica = null;
            file = await this._appFilesDAL.GetFileByIdWithStorageAccount(fileVersion.OriginalFileId);
            if (file == null)
            {
                _logger.LogDebug("UpdateFileVersionName: original file {OriginalFileId} not found", fileVersion.OriginalFileId);
                return;
            }

            if (file.UserId != userId)
            {
                _logger.LogError("Unauthorized UpdateFileVersionName attempt for fileVersion {FileVersionId} by user {UserId}", fileVersionId, userId);
                throw new Exception("Unauthorized operation.");
            }

            if (file.ReplicaId != null)
            {
                replica = await this._appFilesDAL.GetFileByIdWithStorageAccount((Guid)file.ReplicaId);
            }

            fileVersion.Name = newName;
            await this._fileVersionsDAL.UpdateFileVersion(fileVersion);
            _logger.LogInformation("Updated name for primary file version {FileVersionId} to {NewName}", fileVersionId, newName);

            if (file.ReplicaId != null)
            {
                FileVersion replicaFileVersion = replica.Versions.FirstOrDefault(fv => fv.CreationTime == fileVersion.CreationTime);
                if (replicaFileVersion == null)
                {
                    _logger.LogDebug("UpdateFileVersionName: matching replica file version not found for creation time {CreationTime}", fileVersion.CreationTime);
                    return;
                }

                replicaFileVersion.Name = newName;
                await this._fileVersionsDAL.UpdateFileVersion(replicaFileVersion);
                _logger.LogInformation("Updated name for replica file version {ReplicaFileVersionId} to {NewName}", replicaFileVersion.Id, newName);
            }
        }


        private async Task<string> UploadFileToBlob(AppFile appFile, IFormFile file)
        {
            _logger.LogDebug("UploadFileToBlob called for file {FileId} using storage {StorageId}", appFile.Id, appFile.StorageAccount?.Id);
            BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(appFile.StorageAccount.BlobServicePath), new DefaultAzureCredential());
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("container");
            BlobClient blobClient = containerClient.GetBlobClient(GeneralUtils.GetAzureFileName(appFile));

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = GeneralUtils.GetContentType(appFile.Name),
                ContentDisposition = "inline"
            };

            using (var stream = file.OpenReadStream())
            {
                var result = await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders,
                });

                string versionId = result.Value.VersionId;
                _logger.LogInformation("Uploaded version {VersionId} for file {FileId}", versionId, appFile.Id);
                return versionId;
            }
        }
        private async Task DeleteFileVersionFromAzure(AppFile appFile, FileVersion fileVersion)
        {
            _logger.LogDebug("DeleteFileVersionFromAzure called for file {FileId} version {VersionId}", appFile.Id, fileVersion.AzureId);
            BlobServiceClient serviceClient = new BlobServiceClient(new Uri(appFile.StorageAccount.BlobServicePath), new DefaultAzureCredential());
            BlobContainerClient containerClient = serviceClient.GetBlobContainerClient("container");
            BlobClient blobClient = containerClient.GetBlobClient(GeneralUtils.GetAzureFileName(appFile)).WithVersion(fileVersion.AzureId);

            await blobClient.DeleteIfExistsAsync();
            _logger.LogInformation("Requested delete for blob version {VersionId} of file {FileId}", fileVersion.AzureId, appFile.Id);
        }
    }
}
