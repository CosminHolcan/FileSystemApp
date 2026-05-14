using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.DTO;
using Shared.Enums;

namespace BLL
{
    public class AppFilesBLL
    {
        private UsersDAL _usersDAL;
        private AppFilesDAL _appFilesDAL;
        private FileVersionsDAL _fileVersionsDAL;
        private StorageAccountsDAL _storageAccountsDAL;
        private readonly ILogger<AppFilesBLL> _logger;

        public AppFilesBLL(UsersDAL usersDAL, AppFilesDAL appFilesDAL, FileVersionsDAL fileVersionsDAL, StorageAccountsDAL storageAccountsDAL, ILogger<AppFilesBLL> logger)
        {
            this._usersDAL = usersDAL;
            this._appFilesDAL = appFilesDAL;
            this._fileVersionsDAL = fileVersionsDAL;
            this._storageAccountsDAL = storageAccountsDAL;
            this._logger = logger;
        }

        public async Task<AppFileDTO> AddFile(CreateFileDTO dto, Guid userId, IFormFile file)
        {
            _logger.LogInformation("AddFile called by user {UserId} for file name {FileName}", userId, dto?.Name);
            DateTime startingTime = DateTime.Now;

            Guid mainFileId = Guid.NewGuid();
            Guid replicaFileId = Guid.NewGuid();

            AppFile mainFile = null, replicaFile = null;
            bool firstFileFailed = false, secondFileFailed = false;

            try
            {
                StorageAccount mainStorage = await _storageAccountsDAL.GetStorageAccountByFeatures((Location)dto.Location, (Redundancy)dto.Redundancy, (bool)dto.Versioning);

                mainFile = await this.CreateAndStoreFile(
                    mainFileId, dto.Name, userId, mainStorage, startingTime, file,
                    dto.VersionName, dto.SecondaryLocation != null ? replicaFileId : null, false, (bool)dto.Versioning);
                _logger.LogInformation("Main file {MainFileId} created successfully for user {UserId}", mainFileId, userId);
            }
            catch (Exception ex)
            {
                firstFileFailed = true;
                _logger.LogError(ex, "Error while creating and storing the main file with id {FileId} for user {UserId}", mainFileId, userId);
            }

            try
            {
                if (dto.SecondaryLocation != null)
                {
                    StorageAccount replicaStorage = await _storageAccountsDAL.GetStorageAccountByFeatures((Location)dto.SecondaryLocation, (Redundancy)dto.Redundancy, (bool)dto.Versioning);

                    replicaFile = await this.CreateAndStoreFile(
                        replicaFileId, dto.Name, userId, replicaStorage, startingTime, file,
                        dto.VersionName, mainFileId, true, (bool)dto.Versioning);
                    _logger.LogInformation("Replica file {ReplicaFileId} created successfully for user {UserId}", replicaFileId, userId);
                }
            }
            catch
            {
                secondFileFailed = true;
                _logger.LogError("Error while creating and storing the replica file with id {ReplicaFileId} for user {UserId}", replicaFileId, userId);
            }

            if (firstFileFailed && dto.SecondaryLocation == null)
            {
                throw new Exception("The file could not be saved.");
            }

            if (!firstFileFailed || !secondFileFailed)
            {
                AppFile successfulFile = !firstFileFailed ? mainFile : replicaFile;

                _logger.LogInformation("AddFile returning DTO for file {FileId} (user {UserId})", successfulFile.Id, userId);
                return new AppFileDTO
                {
                    Id = successfulFile.Id,
                    Name = dto.Name,
                    StorageAccountId = successfulFile.StorageAccountId,
                    Location = dto.Location,
                    Redundancy = dto.SecondaryLocation != null ? Redundancy.Custom : dto.Redundancy,
                    SecondaryLocation = dto.SecondaryLocation,
                    Versioning = dto.Versioning,
                    CreationDate = Helper.FormatDateOnly(startingTime)
                };
            }

            throw new Exception("The file could not be saved.");
        }

        public async Task<AppFileDTO> UploadNewContent(Guid userId, Guid fileId, IFormFile file)
        {
            _logger.LogInformation("UploadNewContent called for file {FileId} by user {UserId}", fileId, userId);
            DateTime startingTime = DateTime.Now;
            AppFile appFile = await _appFilesDAL.GetFileByIdWithStorageAccount(fileId), replicaFile = null;
            if (appFile.UserId != userId)
            {
                _logger.LogError("Unauthorized UploadNewContent attempt for file {FileId} by user {UserId}", fileId, userId);
                throw new Exception("Unauthorized operation.");
            }

            bool firstFileFailed = false, secondFileFailed = false;

            try
            {
                await this.UploadFileToBlob(appFile.StorageAccount, fileId, file, appFile.Name, appFile.StorageAccount.Versioning);
                await this._appFilesDAL.UpdateTimeInteractionsForFile(fileId, startingTime, true);
                _logger.LogInformation("Uploaded new content to primary file {FileId}", fileId);
            }
            catch (Exception ex)
            {
                firstFileFailed = true;
                _logger.LogError(ex, "Error uploading new content to primary file {FileId}", fileId);
            }

            try
            {
                if (appFile.ReplicaId != null)
                {
                    replicaFile = await _appFilesDAL.GetFileByIdWithStorageAccount((Guid)appFile.ReplicaId);
                    await this.UploadFileToBlob(replicaFile.StorageAccount, replicaFile.Id, file, replicaFile.Name, replicaFile.StorageAccount.Versioning);
                    await this._appFilesDAL.UpdateTimeInteractionsForFile(replicaFile.Id, startingTime, true);
                    _logger.LogInformation("Uploaded new content to replica file {ReplicaId}", replicaFile.Id);
                }
            }
            catch (Exception ex)
            {
                secondFileFailed = true;
                _logger.LogError(ex, "Error uploading new content to replica file for original file {FileId}", fileId);
            }

            if (firstFileFailed && appFile.ReplicaId == null)
            {
                _logger.LogError("UploadNewContent failed: primary failed and no replica exists for file {FileId}", fileId);
                throw new Exception("The file could not be updated.");
            }

            if (!firstFileFailed || !secondFileFailed)
            {
                AppFile successfulFile = !firstFileFailed ? appFile : replicaFile;
                string azureFileName = successfulFile.Id.ToString() + Path.GetExtension(successfulFile.Name);

                _logger.LogInformation("UploadNewContent succeeded for file {FileId}", successfulFile.Id);

                return new AppFileDTO
                {
                    Id = successfulFile.Id,
                    Name = successfulFile.Name,
                    TokenSAS = await SASTokensGenerator.GenerateSasTokenAsync(
                            successfulFile.StorageAccount.BlobServicePath,
                            "container",
                            azureFileName),
                    Versioning = successfulFile.StorageAccount.Versioning,
                    ReplicaId = successfulFile.Id,
                    IsReplica = successfulFile.IsReplica
                };
            }

            _logger.LogError("UploadNewContent failed for file {FileId} after both primary and replica attempts", fileId);
            throw new Exception("The file could not be updated.");
        }

        public async Task<List<AppFileDTO>> GetFilesByUser(Guid userId)
        {
            _logger.LogDebug("GetFilesByUser called for user {UserId}", userId);
            List<AppFile> files = await this._appFilesDAL.GetFilesByUser(userId);

            return files.Where(f => f.IsReplica == null || f.IsReplica == false).Select(f => new AppFileDTO()
            {
                Id = f.Id,
                Name = f.Name,
                StorageAccountId = f.StorageAccountId,
                CreationDate = Helper.FormatDateOnly(f.CreationDate),
                Location = f.StorageAccount.Location,
                SecondaryLocation = f.ReplicaId != null ? files.Find(r => r.Id == f.ReplicaId).StorageAccount.Location : null,
                Redundancy = f.ReplicaId != null ? Redundancy.Custom : f.StorageAccount.Redundancy,
                Versioning = f.StorageAccount.Versioning
            }).ToList();
        }

        public async Task<AppFile> GetFileByIdWithStorageAccount(Guid fileId)
        {
            _logger.LogDebug("GetFileByIdWithStorageAccount called for file {FileId}", fileId);
            return await this._appFilesDAL.GetFileByIdWithStorageAccount(fileId);
        }

        public async Task<FileWithVersionsDTO> GetFileByIdWithVersions(Guid userId, Guid fileId)
        {
            _logger.LogInformation("GetFileByIdWithVersions called for file {FileId} by user {UserId}", fileId, userId);
            AppFile appFile = await this._appFilesDAL.GetFullFileById(fileId);
            if (appFile.UserId != userId)
            {
                _logger.LogError("Unauthorized GetFileByIdWithVersions attempt for file {FileId} by user {UserId}", fileId, userId);
                throw new Exception("Unauthorized operation.");
            }

            string azureFileName = appFile.Id.ToString() + Path.GetExtension(appFile.Name);

            await this._appFilesDAL.UpdateTimeInteractionsForFile(fileId, DateTime.Now, false);

            var versions = new List<FileVersionDTO>();
            foreach (var v in appFile.Versions)
            {
                string token = await SASTokensGenerator.GenerateSasTokenAsync(appFile.StorageAccount.BlobServicePath, "container", azureFileName, v.AzureId);
                versions.Add(new FileVersionDTO()
                {
                    Id = v.Id,
                    Name = v.Name,
                    AzureId = v.AzureId,
                    CreationTime = Helper.FormatDateTime(v.CreationTime),
                    OriginalFileId = v.OriginalFileId,
                    TokenSAS = token
                });
            }

            _logger.LogInformation("GetFileByIdWithVersions returning {Count} versions for file {FileId}", versions.Count, fileId);

            return new FileWithVersionsDTO()
            {
                Id = appFile.Id,
                Name = appFile.Name,
                Versions = versions,
            };
        }

        public async Task<AppFileDTO> GetFileById(AppFile appFile)
        {
            _logger.LogDebug("GetFileById called for file {FileId}", appFile.Id);
            string azureFileName = appFile.Id.ToString() + Path.GetExtension(appFile.Name);

            await this._appFilesDAL.UpdateTimeInteractionsForFile(appFile.Id, DateTime.Now, false);

            return new AppFileDTO()
            {
                Id = appFile.Id,
                Name = appFile.Name,
                Versioning = appFile.StorageAccount.Versioning,
                TokenSAS = await SASTokensGenerator.GenerateSasTokenAsync(appFile.StorageAccount.BlobServicePath, "container", azureFileName),
                ReplicaId = appFile.Id,
                IsReplica = appFile.IsReplica
            };
        }

        public async Task<AppFile> GetAvailableFileReplica(Guid userId, Guid fileId, bool includeVersions)
        {
            _logger.LogInformation("GetAvailableFileReplica called for file {FileId} (includeVersions={IncludeVersions}) by user {UserId}", fileId, includeVersions, userId);

            AppFile originalRequestedFile = includeVersions
                ? await this._appFilesDAL.GetFullFileById(fileId)
                : await this._appFilesDAL.GetFileByIdWithStorageAccount(fileId);

            if (originalRequestedFile.ReplicaId == null)
            {
                _logger.LogDebug("No replica exists for file {FileId}", fileId);
                return originalRequestedFile;
            }

            AppFile replicaFile = includeVersions
                ? await this._appFilesDAL.GetFullFileById((Guid)originalRequestedFile.ReplicaId)
                : await this._appFilesDAL.GetFileByIdWithStorageAccount((Guid)originalRequestedFile.ReplicaId);

            AppFile firstCheckedFile = originalRequestedFile.LastUpdate >= replicaFile.LastUpdate ? originalRequestedFile : replicaFile;
            AppFile secondCheckedFile = firstCheckedFile.Id == originalRequestedFile.Id ? replicaFile : originalRequestedFile;

            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(firstCheckedFile.StorageAccount.BlobServicePath), new DefaultAzureCredential());
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("container");
                BlobClient blobClient = containerClient.GetBlobClient(Helper.GetAzureFileName(firstCheckedFile.Id, firstCheckedFile.Name));

                await blobClient.GetPropertiesAsync();
                _logger.LogInformation("Found available replica: {FileId} at {BlobServicePath}", firstCheckedFile.Id, firstCheckedFile.StorageAccount.BlobServicePath);
                return firstCheckedFile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Primary check failed for file {FileId}, trying the other replica", firstCheckedFile.Id);
                try
                {
                    BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(secondCheckedFile.StorageAccount.BlobServicePath), new DefaultAzureCredential());
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("container");
                    BlobClient blobClient = containerClient.GetBlobClient(Helper.GetAzureFileName(secondCheckedFile.Id, secondCheckedFile.Name));

                    await blobClient.GetPropertiesAsync();
                    _logger.LogInformation("Found available replica: {FileId} at {BlobServicePath}", secondCheckedFile.Id, secondCheckedFile.StorageAccount.BlobServicePath);
                    return secondCheckedFile;
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "Both replica checks failed for file {FileId}", fileId);
                    throw new Exception("File not available");
                }
            }
        }

        public async Task DeleteFile(Guid userId, Guid fileId)
        {
            _logger.LogInformation("DeleteFile called for file {FileId} by user {UserId}", fileId, userId);
            AppFile file, replica = null;
            file = await this._appFilesDAL.GetFileByIdWithStorageAccount(fileId);
            if (file == null)
            {
                _logger.LogDebug("DeleteFile: file {FileId} not found", fileId);
                return;
            }

            if (file.UserId != userId)
            {
                _logger.LogError("Unauthorized DeleteFile attempt for file {FileId} by user {UserId}", fileId, userId);
                throw new Exception("Unauthorized operation.");
            }

            if (file.ReplicaId != null)
            {
                replica = await this._appFilesDAL.GetFileByIdWithStorageAccount((Guid)file.ReplicaId);
            }

            try
            {
                await this._appFilesDAL.DeleteFile(file);
                await this.DeleteFileFromAzure(file);
                _logger.LogInformation("Deleted file {FileId} and attempted Azure deletion", fileId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting primary file {FileId}", fileId);
            }

            try
            {
                if (replica != null)
                {
                    await this._appFilesDAL.DeleteFile(replica);
                    await this.DeleteFileFromAzure(replica);
                    _logger.LogInformation("Deleted replica file {ReplicaId} and attempted Azure deletion", replica.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting replica file for original file {FileId}", fileId);
            }
        }

        public async Task UpdateFileName(Guid userId, Guid fileId, string newFileName)
        {
            _logger.LogInformation("UpdateFileName called for file {FileId} by user {UserId} newName={NewName}", fileId, userId, newFileName);
            AppFile appFile = await this._appFilesDAL.GetFileById(fileId);
            if (appFile.UserId != userId)
            {
                _logger.LogError("Unauthorized UpdateFileName attempt for file {FileId} by user {UserId}", fileId, userId);
                throw new Exception("Unauthorized operation.");
            }

            appFile.Name = newFileName;
            await this._appFilesDAL.UpdateFile(appFile);
            _logger.LogInformation("Updated name for primary file {FileId} to {NewName}", fileId, newFileName);

            if (appFile.ReplicaId != null)
            {
                AppFile replica = await this._appFilesDAL.GetFileById((Guid)appFile.ReplicaId);
                replica.Name = newFileName;
                await this._appFilesDAL.UpdateFile(replica);
                _logger.LogInformation("Updated name for replica file {ReplicaId} to {NewName}", replica.Id, newFileName);
            }
        }

        private async Task<string> UploadFileToBlob(StorageAccount storageAccount, Guid fileId, IFormFile file, string originalFileName, bool? versioning)
        {
            _logger.LogDebug("UploadFileToBlob called for file {FileId} using storage {StorageId}", fileId, storageAccount?.Id);
            BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(storageAccount.BlobServicePath), new DefaultAzureCredential());
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("container");
            BlobClient blobClient = containerClient.GetBlobClient(fileId.ToString() + Path.GetExtension(originalFileName));

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = Helper.GetContentType(originalFileName),
                ContentDisposition = "inline"
            };

            using (var stream = file.OpenReadStream())
            {
                var result = await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders,
                });

                string versionId = versioning == true ? result.Value.VersionId : null;
                _logger.LogInformation("UploadFileToBlob finished for file {FileId}. versionId={VersionId}", fileId, versionId);
                return versionId;
            }
        }

        private async Task<AppFile> CreateAndStoreFile(Guid fileId, string name, Guid userId, StorageAccount storageAccount, DateTime timestamp,
            IFormFile file, string versionName, Guid? replicaId, bool isReplica, bool versioningEnabled)
        {
            _logger.LogDebug("CreateAndStoreFile called for id {FileId} (isReplica={IsReplica})", fileId, isReplica);

            AppFile appFile = new AppFile
            {
                Id = fileId,
                UserId = userId,
                Name = name,
                StorageAccountId = storageAccount.Id,
                CreationDate = DateOnly.FromDateTime(timestamp),
                LastInteraction = timestamp,
                LastUpdate = timestamp,
                ReplicaId = replicaId,
                IsReplica = isReplica
            };
            string versionId = await UploadFileToBlob(storageAccount, fileId, file, name, versioningEnabled);
            appFile = await _appFilesDAL.AddFile(appFile);

            if (!string.IsNullOrEmpty(versionId))
            {
                await _fileVersionsDAL.AddVersion(new FileVersion
                {
                    Name = versionName,
                    AzureId = versionId,
                    OriginalFileId = fileId,
                    CreationTime = timestamp
                });
                _logger.LogInformation("Created file {FileId} with version {VersionId}", fileId, versionId);
            }
            else
            {
                _logger.LogInformation("Created file {FileId} without version", fileId);
            }

            return appFile;
        }

        private async Task DeleteFileFromAzure(AppFile appFile)
        {
            _logger.LogDebug("DeleteFileFromAzure called for file {FileId}", appFile.Id);
            BlobServiceClient serviceClient = new BlobServiceClient(new Uri(appFile.StorageAccount.BlobServicePath), new DefaultAzureCredential());
            BlobContainerClient containerClient = serviceClient.GetBlobContainerClient("container");
            BlobClient blobClient = containerClient.GetBlobClient(Helper.GetAzureFileName(appFile.Id, appFile.Name));

            await blobClient.DeleteAsync();
            _logger.LogInformation("DeleteFileFromAzure requested delete for blob representing file {FileId}", appFile.Id);
        }
    }
}
