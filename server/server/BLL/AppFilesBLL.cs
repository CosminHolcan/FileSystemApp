using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
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

            StorageAccount mainStorage = await _storageAccountsDAL.GetStorageAccountByFeatures(
                (Location)dto.Location, (Redundancy)dto.Redundancy, (bool)dto.Versioning);

            Guid mainFileId = Guid.NewGuid();
            Guid replicaFileId = Guid.NewGuid();

            AppFile mainFile = new AppFile
            {
                Id = mainFileId,
                UserId = userId,
                Name = dto.Name,
                StorageAccountId = mainStorage.Id,
                CreationDate = DateOnly.FromDateTime(startingTime),
                LastInteraction = startingTime,
                LastUpdate = startingTime,
                ReplicaId = dto.SecondaryLocation != null ? replicaFileId : null,
                IsReplica = false
            };

            mainFile = await _appFilesDAL.AddFile(mainFile);

            string mainVersionId = await UploadFileToBlob(mainStorage, mainFile.Id, file, dto.Name, dto.Versioning);

            if (!string.IsNullOrEmpty(mainVersionId))
            {
                await this._fileVersionsDAL.AddVersion(new FileVersion
                {
                    Name = dto.VersionName,
                    AzureId = mainVersionId,
                    OriginalFileId = mainFileId,
                    CreationTime = startingTime
                });
            }

            if (dto.SecondaryLocation != null)
            {
                StorageAccount replicaStorage = await _storageAccountsDAL.GetStorageAccountByFeatures(
                    (Location)dto.SecondaryLocation, (Redundancy)dto.Redundancy, (bool)dto.Versioning);

                AppFile replicaFile = new AppFile
                {
                    Id = replicaFileId,
                    UserId = userId,
                    Name = dto.Name,
                    StorageAccountId = replicaStorage.Id,
                    CreationDate = DateOnly.FromDateTime(startingTime),
                    LastInteraction = startingTime,
                    LastUpdate = startingTime,
                    IsReplica = true,
                    ReplicaId = mainFileId
                };

                replicaFile = await _appFilesDAL.AddFile(replicaFile);

                string replicaVersionId = await UploadFileToBlob(replicaStorage, replicaFile.Id, file, dto.Name, dto.Versioning);

                if (!string.IsNullOrEmpty(replicaVersionId))
                {
                    await this._fileVersionsDAL.AddVersion(new FileVersion
                    {
                        Name = dto.VersionName,
                        AzureId = replicaVersionId,
                        OriginalFileId = replicaFileId,
                        CreationTime = startingTime
                    });
                }

            }

            return new AppFileDTO
            {
                Id = mainFile.Id,
                Name = mainFile.Name,
                StorageAccountId = mainFile.StorageAccountId,
                Location = mainStorage.Location,
                Redundancy = mainStorage.Redundancy,
                Versioning = mainStorage.Versioning,
                CreationDate = mainFile.CreationDate.ToShortDateString()
            };
        }

        public async Task<AppFileDTO> UploadNewContent(Guid fileId, IFormFile file)
        {
            DateTime startingTime = DateTime.Now;
            AppFile appFile = await _appFilesDAL.GetFileByIdWithStorageAccount(fileId);

            BlobServiceClient blobServiceClient = new BlobServiceClient(appFile.StorageAccount.ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("container");
            BlobClient blobClient = containerClient.GetBlobClient(appFile.Id.ToString() + Path.GetExtension(appFile.Name));

            BlobHttpHeaders blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = GeneralUtils.GetContentType(appFile.Name),
                ContentDisposition = "inline"
            };

            using (var stream = file.OpenReadStream())
            {
                var result = await blobClient.UploadAsync(
                    stream,
                    new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
            }

            string azureFileName = appFile.Id.ToString() + Path.GetExtension(appFile.Name);

            AppFileDTO toReturn = new AppFileDTO
            {
                Id = appFile.Id,
                Name = appFile.Name,
                TokenSAS = SASTokensGenerator.GenerateSasToken(
                    appFile.StorageAccount.ConnectionString,
                    "container",
                    azureFileName),
                Versioning = appFile.StorageAccount.Versioning,
                ReplicaId = appFile.Id,
                IsReplica = appFile.IsReplica
            };

            await this._appFilesDAL.UpdateTimeInteractionsForFile(fileId, startingTime, true);

            if (appFile.ReplicaId != null)
            {
                AppFile replicaAppFile = await _appFilesDAL.GetFileByIdWithStorageAccount((Guid)appFile.ReplicaId);

                BlobServiceClient replicaBlobServiceClient = new BlobServiceClient(appFile.StorageAccount.ConnectionString);
                BlobContainerClient replicaContainerClient = replicaBlobServiceClient.GetBlobContainerClient("container");
                BlobClient replicaBlobClient = replicaContainerClient.GetBlobClient(appFile.Id.ToString() + Path.GetExtension(appFile.Name));

                using (var stream = file.OpenReadStream())
                {
                    var result = await replicaBlobClient.UploadAsync(
                        stream,
                        new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
                }

                await this._appFilesDAL.UpdateTimeInteractionsForFile((Guid)appFile.ReplicaId, startingTime, true);
            }

            return toReturn;
        }

        public async Task<List<AppFileDTO>> GetFilesByUser(Guid userId)
        {
            List<AppFile> files = await this._appFilesDAL.GetFilesByUser(userId);

            return files.Where(f => f.IsReplica == null || f.IsReplica == false).Select(f => new AppFileDTO()
            {
                Id = f.Id,
                Name = f.Name,
                StorageAccountId = f.StorageAccountId,
                CreationDate = f.CreationDate.ToShortDateString(),
                Location = f.StorageAccount.Location,
                SecondaryLocation = f.ReplicaId != null ? files.Find(r => r.Id == f.ReplicaId).StorageAccount.Location : null,
                Redundancy = f.StorageAccount.Redundancy,
                Versioning = f.StorageAccount.Versioning
            }).ToList();
        }

        public async Task<AppFile> GetFileByIdWithStorageAccount(Guid fileId)
        {
            return await this._appFilesDAL.GetFileByIdWithStorageAccount(fileId);
        }

        public async Task<FileWithVersionsDTO> GetFileByIdWithVersions(Guid fileId)
        {
            AppFile appFile = await this._appFilesDAL.GetFullFileById(fileId);
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
                    CreationTime = v.CreationTime.ToShortDateString() + " " + v.CreationTime.ToShortTimeString(),
                    OriginalFileId = v.OriginalFileId,
                    TokenSAS = SASTokensGenerator.GenerateSasToken(appFile.StorageAccount.ConnectionString, "container", azureFileName, v.AzureId)
                }).ToList(),
            };
        }

        public async Task<AppFileDTO> GetFileById(Guid fileId)
        {
            AppFile appFile = await this._appFilesDAL.GetFileByIdWithStorageAccount(fileId);
            string azureFileName = appFile.Id.ToString() + Path.GetExtension(appFile.Name);

            await this._appFilesDAL.UpdateTimeInteractionsForFile(fileId, DateTime.Now, false);

            return new AppFileDTO()
            {
                Id = appFile.Id,
                Name = appFile.Name,
                Versioning = appFile.StorageAccount.Versioning,
                TokenSAS = SASTokensGenerator.GenerateSasToken(appFile.StorageAccount.ConnectionString, "container", azureFileName),
                ReplicaId = appFile.Id,
                IsReplica  = appFile.IsReplica
            };
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
    }
}
