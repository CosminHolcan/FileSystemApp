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

        public FileVersionsBLL(FileVersionsDAL fileVersionsDAL, AppFilesDAL appFilesDAL)
        {
            this._fileVersionsDAL = fileVersionsDAL;
            this._appFilesDAL = appFilesDAL;
        }

        public async Task<FileVersionDTO> AddVersion(Guid userId, AddFileVersionDTO dto, IFormFile file)
        {
            AppFile appFile = await _appFilesDAL.GetFileByIdWithStorageAccount(dto.OriginalFileId);
            if (appFile.UserId != userId)
            {
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
                    CreationTime = createdVersion.CreationTime.ToString("dd-MM-yyy HH:mm"),
                    TokenSAS = SASTokensGenerator.GenerateSasToken(appFile.StorageAccount.ConnectionString, "container", GeneralUtils.GetAzureFileName(appFile), createdVersion.AzureId)
                };
            }
            catch
            {
                firstFileFailed = true;
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
                            CreationTime = createdVersion.CreationTime.ToString("dd-MM-yyy HH:mm"),
                            TokenSAS = SASTokensGenerator.GenerateSasToken(replicaFile.StorageAccount.ConnectionString, "container", GeneralUtils.GetAzureFileName(replicaFile), createdVersion.AzureId)
                        };

                    }
                }
            }
            catch { }

            if (toReturn == null)
            {
                throw new Exception("Could not add new version for file.");
            }

            return toReturn;
        }

        public async Task<List<FileVersionDTO>> GetFileVersionsByOriginalFileId(AppFile appFile)
        {
            List<FileVersion> fileVersions = await this._fileVersionsDAL.GetFileVersionsByOriginalFileId(appFile.Id);
            fileVersions.Sort((v1, v2) => v1.CreationTime.CompareTo(v2.CreationTime));
            string azureFileName = appFile.Id.ToString() + Path.GetExtension(appFile.Name);

            return fileVersions.Select(f => new FileVersionDTO()
            {
                Id = f.Id,
                Name = f.Name,
                OriginalFileId = f.OriginalFileId,
                CreationTime = f.CreationTime.ToString("dd-MM-yyy HH:mm"),
                AzureId = f.AzureId,
                TokenSAS = SASTokensGenerator.GenerateSasToken(appFile.StorageAccount.ConnectionString, "container", azureFileName, f.AzureId)
            }).ToList();
        }

        public async Task DeleteFileVersion(Guid userId, Guid fileVersionId)
        {
            FileVersion fileVersion = await this._fileVersionsDAL.GetFileVersionById(fileVersionId);
            if (fileVersion == null)
                return;

            AppFile file, replica = null;
            file = await this._appFilesDAL.GetFileByIdWithStorageAccount(fileVersion.OriginalFileId);
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

            await this.DeleteFileVersionFromAzure(file, fileVersion);
            await this._fileVersionsDAL.DeleteFileVersion(fileVersion);

            if (replica != null)
            {
                FileVersion replicaFileVersion = replica.Versions.FirstOrDefault(fv => fv.CreationTime == fileVersion.CreationTime);
                if (replicaFileVersion == null)
                    return;

                await this.DeleteFileVersionFromAzure(replica, replicaFileVersion);
                await this._fileVersionsDAL.DeleteFileVersion(replicaFileVersion);
            }
        }

        public async Task UpdateFileVersionName(Guid userId, Guid fileVersionId, string newName)
        {
            FileVersion fileVersion = await this._fileVersionsDAL.GetFileVersionById(fileVersionId);
            if (fileVersion == null)
                return;

            AppFile file, replica = null;
            file = await this._appFilesDAL.GetFileByIdWithStorageAccount(fileVersion.OriginalFileId);
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

            fileVersion.Name = newName;
            await this._fileVersionsDAL.UpdateFileVersion(fileVersion);

            if (file.ReplicaId != null)
            {
                FileVersion replicaFileVersion = replica.Versions.FirstOrDefault(fv => fv.CreationTime == fileVersion.CreationTime);
                if (replicaFileVersion == null)
                    return;

                replicaFileVersion.Name = newName;
                await this._fileVersionsDAL.UpdateFileVersion(replicaFileVersion);
            }
        }


        private async Task<string> UploadFileToBlob(AppFile appFile, IFormFile file)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(appFile.StorageAccount.ConnectionString);
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

                return result.Value.VersionId;
            }
        }
        private async Task DeleteFileVersionFromAzure(AppFile appFile, FileVersion fileVersion)
        {
            BlobServiceClient serviceClient = new BlobServiceClient(appFile.StorageAccount.ConnectionString);
            BlobContainerClient containerClient = serviceClient.GetBlobContainerClient("container");
            BlobClient blobClient = containerClient.GetBlobClient(GeneralUtils.GetAzureFileName(appFile)).WithVersion(fileVersion.AzureId);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}
