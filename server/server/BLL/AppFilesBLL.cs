using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using server.DAL;
using server.DAL.Entities;
using server.DTO;
using server.Enums;
using server.Utils;

namespace server.BLL
{
    public class AppFilesBLL
    {
        private UsersDAL _usersDAL;
        private AppFilesDAL _appFilesDAL;
        private FileVersionsDAL _fileVersionsDAL;
        private StorageAccountsDAL _storageAccountsDAL;

        public AppFilesBLL(UsersDAL usersDAL, AppFilesDAL appFilesDAL, FileVersionsDAL fileVersionsDAL, StorageAccountsDAL storageAccountsDAL)
        {
            this._usersDAL = usersDAL;
            this._appFilesDAL = appFilesDAL;
            this._fileVersionsDAL = fileVersionsDAL;
            this._storageAccountsDAL = storageAccountsDAL;
        }

        public async Task<AppFileDTO> AddFile(CreateFileDTO dto, Guid userId, IFormFile file)
        {
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
            }
            catch
            {
                firstFileFailed = true;
            }

            try
            {
                if (dto.SecondaryLocation != null)
                {
                    StorageAccount replicaStorage = await _storageAccountsDAL.GetStorageAccountByFeatures((Location)dto.SecondaryLocation, (Redundancy)dto.Redundancy, (bool)dto.Versioning);

                    replicaFile = await this.CreateAndStoreFile(
                        replicaFileId, dto.Name, userId, replicaStorage, startingTime, file,
                        dto.VersionName, mainFileId, true, (bool)dto.Versioning);
                }
            }
            catch
            {
                secondFileFailed = true;
            }

            if (firstFileFailed && dto.SecondaryLocation == null)
            {
                throw new Exception("The file could not be saved.");
            }

            if (!firstFileFailed || !secondFileFailed)
            {
                AppFile successfulFile = !firstFileFailed ? mainFile : replicaFile;

                return new AppFileDTO
                {
                    Id = successfulFile.Id,
                    Name = dto.Name,
                    StorageAccountId = successfulFile.StorageAccountId,
                    Location = dto.Location,
                    Redundancy = dto.SecondaryLocation != null ? Redundancy.Custom : dto.Redundancy,
                    SecondaryLocation = dto.SecondaryLocation,
                    Versioning = dto.Versioning,
                    CreationDate = GeneralUtils.FormatDateOnly(startingTime)
                };
            }

            throw new Exception("The file could not be saved.");
        }

        public async Task<AppFileDTO> UploadNewContent(Guid userId, Guid fileId, IFormFile file)
        {
            DateTime startingTime = DateTime.Now;
            AppFile appFile = await _appFilesDAL.GetFileByIdWithStorageAccount(fileId), replicaFile = null;
            if (appFile.UserId != userId)
            {
                throw new Exception("Unauthorized operation.");
            }

            bool firstFileFailed = false, secondFileFailed = false;

            try
            {
                await this.UploadFileToBlob(appFile.StorageAccount, fileId, file, appFile.Name, appFile.StorageAccount.Versioning);
                await this._appFilesDAL.UpdateTimeInteractionsForFile(fileId, startingTime, true);
            }
            catch
            {
                firstFileFailed = true;
            }

            try
            {
                if (appFile.ReplicaId != null)
                {
                    replicaFile = await _appFilesDAL.GetFileByIdWithStorageAccount((Guid)appFile.ReplicaId);
                    await this.UploadFileToBlob(replicaFile.StorageAccount, replicaFile.Id, file, replicaFile.Name, replicaFile.StorageAccount.Versioning);
                    await this._appFilesDAL.UpdateTimeInteractionsForFile(replicaFile.Id, startingTime, true);
                }
            }
            catch
            {
                secondFileFailed = true;
            }

            if (firstFileFailed && appFile.ReplicaId == null)
            {
                throw new Exception("The file could not be updated.");
            }

            if (!firstFileFailed || !secondFileFailed)
            {
                AppFile successfulFile = !firstFileFailed ? appFile : replicaFile;
                string azureFileName = successfulFile.Id.ToString() + Path.GetExtension(successfulFile.Name);

                return new AppFileDTO
                {
                    Id = successfulFile.Id,
                    Name = successfulFile.Name,
                    TokenSAS = SASTokensGenerator.GenerateSasToken(
                            successfulFile.StorageAccount.ConnectionString,
                            "container",
                            azureFileName),
                    Versioning = successfulFile.StorageAccount.Versioning,
                    ReplicaId = successfulFile.Id,
                    IsReplica = successfulFile.IsReplica
                };
            }

            throw new Exception("The file could not be updated.");
        }

        public async Task<List<AppFileDTO>> GetFilesByUser(Guid userId)
        {
            List<AppFile> files = await this._appFilesDAL.GetFilesByUser(userId);

            return files.Where(f => f.IsReplica == null || f.IsReplica == false).Select(f => new AppFileDTO()
            {
                Id = f.Id,
                Name = f.Name,
                StorageAccountId = f.StorageAccountId,
                CreationDate = GeneralUtils.FormatDateOnly(f.CreationDate),
                Location = f.StorageAccount.Location,
                SecondaryLocation = f.ReplicaId != null ? files.Find(r => r.Id == f.ReplicaId).StorageAccount.Location : null,
                Redundancy = f.ReplicaId != null ? Redundancy.Custom : f.StorageAccount.Redundancy,
                Versioning = f.StorageAccount.Versioning
            }).ToList();
        }

        public async Task<AppFile> GetFileByIdWithStorageAccount(Guid fileId)
        {
            return await this._appFilesDAL.GetFileByIdWithStorageAccount(fileId);
        }

        public async Task<FileWithVersionsDTO> GetFileByIdWithVersions(Guid userId, Guid fileId)
        {
            AppFile appFile = await this._appFilesDAL.GetFullFileById(fileId);
            if (appFile.UserId != userId)
            {
                throw new Exception("Unauthorized operation.");
            }

            string azureFileName = appFile.Id.ToString() + Path.GetExtension(appFile.Name);

            await this._appFilesDAL.UpdateTimeInteractionsForFile(fileId, DateTime.Now, false);

            return new FileWithVersionsDTO()
            {
                Id = appFile.Id,
                Name = appFile.Name,
                Versions = appFile.Versions.Select(v => new FileVersionDTO()
                {
                    Id = v.Id,
                    Name = v.Name,
                    AzureId = v.AzureId,
                    CreationTime = GeneralUtils.FormatDateTime(v.CreationTime),
                    OriginalFileId = v.OriginalFileId,
                    TokenSAS = SASTokensGenerator.GenerateSasToken(appFile.StorageAccount.ConnectionString, "container", azureFileName, v.AzureId)
                }).ToList(),
            };
        }

        public async Task<AppFileDTO> GetFileById(AppFile appFile)
        {
            string azureFileName = appFile.Id.ToString() + Path.GetExtension(appFile.Name);

            await this._appFilesDAL.UpdateTimeInteractionsForFile(appFile.Id, DateTime.Now, false);

            return new AppFileDTO()
            {
                Id = appFile.Id,
                Name = appFile.Name,
                Versioning = appFile.StorageAccount.Versioning,
                TokenSAS = SASTokensGenerator.GenerateSasToken(appFile.StorageAccount.ConnectionString, "container", azureFileName),
                ReplicaId = appFile.Id,
                IsReplica = appFile.IsReplica
            };
        }

        public async Task<AppFile> GetAvailableFileReplica(Guid userId, Guid fileId, bool includeVersions)
        {
            AppFile originalRequestedFile = includeVersions
                ? await this._appFilesDAL.GetFullFileById(fileId)
                : await this._appFilesDAL.GetFileByIdWithStorageAccount(fileId);

            if (originalRequestedFile.ReplicaId == null)
            {
                return originalRequestedFile;
            }

            AppFile replicaFile = includeVersions
                ? await this._appFilesDAL.GetFullFileById((Guid)originalRequestedFile.ReplicaId)
                : await this._appFilesDAL.GetFileByIdWithStorageAccount((Guid)originalRequestedFile.ReplicaId);

            AppFile firstCheckedFile = originalRequestedFile.LastUpdate >= replicaFile.LastUpdate ? originalRequestedFile : replicaFile;
            AppFile secondCheckedFile = firstCheckedFile.Id == originalRequestedFile.Id ? replicaFile : originalRequestedFile;

            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(firstCheckedFile.StorageAccount.ConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("container");
                BlobClient blobClient = containerClient.GetBlobClient(GeneralUtils.GetAzureFileName(firstCheckedFile));

                await blobClient.GetPropertiesAsync();
                return firstCheckedFile;
            }
            catch
            {
                try
                {
                    BlobServiceClient blobServiceClient = new BlobServiceClient(secondCheckedFile.StorageAccount.ConnectionString);
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("container");
                    BlobClient blobClient = containerClient.GetBlobClient(GeneralUtils.GetAzureFileName(secondCheckedFile));

                    await blobClient.GetPropertiesAsync();
                    return secondCheckedFile;
                }
                catch
                {
                    throw new Exception("File not available");
                }
            }
        }

        public async Task DeleteFile(Guid userId, Guid fileId)
        {
            AppFile file, replica = null;
            file = await this._appFilesDAL.GetFileByIdWithStorageAccount(fileId);
            if (file == null)
                return;

            if (file.UserId != userId)
            {
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
            }
            catch { }

            try
            {
                if (replica != null)
                {
                    await this._appFilesDAL.DeleteFile(replica);
                    await this.DeleteFileFromAzure(replica);
                }
            }
            catch { }
        }

        public async Task UpdateFileName(Guid userId, Guid fileId, string newFileName)
        {
            AppFile appFile = await this._appFilesDAL.GetFileById(fileId);
            if (appFile.UserId != userId)
            {
                throw new Exception("Unauthorized operation.");
            }

            appFile.Name = newFileName;
            await this._appFilesDAL.UpdateFile(appFile);

            if (appFile.ReplicaId != null)
            {
                AppFile replica = await this._appFilesDAL.GetFileById((Guid)appFile.ReplicaId);
                replica.Name = newFileName;
                await this._appFilesDAL.UpdateFile(replica);
            }
        }

        private async Task<string> UploadFileToBlob(StorageAccount storageAccount, Guid fileId, IFormFile file, string originalFileName, bool? versioning)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageAccount.ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("container");
            BlobClient blobClient = containerClient.GetBlobClient(fileId.ToString() + Path.GetExtension(originalFileName));

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = GeneralUtils.GetContentType(originalFileName),
                ContentDisposition = "inline"
            };

            using (var stream = file.OpenReadStream())
            {
                var result = await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders,
                });

                return versioning == true ? result.Value.VersionId : null;
            }
        }

        private async Task<AppFile> CreateAndStoreFile(Guid fileId, string name, Guid userId, StorageAccount storageAccount, DateTime timestamp,
            IFormFile file, string versionName, Guid? replicaId, bool isReplica, bool versioningEnabled)
        {
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
            }

            return appFile;
        }

        private async Task DeleteFileFromAzure(AppFile appFile)
        {
            BlobServiceClient serviceClient = new BlobServiceClient(appFile.StorageAccount.ConnectionString);
            BlobContainerClient containerClient = serviceClient.GetBlobContainerClient("container");
            BlobClient blobClient = containerClient.GetBlobClient(GeneralUtils.GetAzureFileName(appFile));

            await blobClient.DeleteAsync();
        }
    }
}
