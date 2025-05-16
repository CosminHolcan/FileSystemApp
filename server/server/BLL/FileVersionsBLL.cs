using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
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

        public async Task<FileVersionDTO> AddVersion(AddFileVersionDTO dto, IFormFile file )
        {
            DateTime startingTime = DateTime.Now;
            AppFile appFile = await _appFilesDAL.GetFileByIdWithStorageAccount(dto.OriginalFileId);

            BlobServiceClient blobServiceClient = new BlobServiceClient(appFile.StorageAccount.ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("container");
            BlobClient blobClient = containerClient.GetBlobClient(appFile.Id.ToString() + Path.GetExtension(appFile.Name));

            BlobHttpHeaders blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = GeneralUtils.GetContentType(appFile.Name),
                ContentDisposition = "inline"
            };

            string versionId;
            using (var stream = file.OpenReadStream())
            {
                var result = await blobClient.UploadAsync(
                    stream,
                    new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
                versionId = result.Value.VersionId;
            }

            var fileVersion = new FileVersion
            {
                Name = dto.Name,
                AzureId = versionId,
                OriginalFileId = dto.OriginalFileId,
                CreationTime = startingTime
            };

            FileVersion createdVersion = await _fileVersionsDAL.AddVersion(fileVersion);

            string azureFileName = appFile.Id.ToString() + Path.GetExtension(appFile.Name);

            FileVersionDTO toReturn = new FileVersionDTO
            {
                Id = createdVersion.Id,
                Name = createdVersion.Name,
                OriginalFileId = createdVersion.OriginalFileId,
                CreationTime = createdVersion.CreationTime.ToString("yyyy-MM-dd HH:mm"),
                TokenSAS = SASTokensGenerator.GenerateSasToken(
                    appFile.StorageAccount.ConnectionString,
                    "container",
                    azureFileName,
                    createdVersion.AzureId)
            };

            await this._appFilesDAL.UpdateTimeInteractionsForFile(dto.OriginalFileId, startingTime, true);

            if (appFile.ReplicaId != null)
            {
                AppFile replicaAppFile = await _appFilesDAL.GetFileByIdWithStorageAccount((Guid)appFile.ReplicaId);

                BlobServiceClient replicaBlobServiceClient = new BlobServiceClient(appFile.StorageAccount.ConnectionString);
                BlobContainerClient replicaContainerClient = replicaBlobServiceClient.GetBlobContainerClient("container");
                BlobClient replicaBlobClient = replicaContainerClient.GetBlobClient(appFile.Id.ToString() + Path.GetExtension(appFile.Name));

                string replicaVerionId;
                using (var stream = file.OpenReadStream())
                {
                    var result = await replicaBlobClient.UploadAsync(
                        stream,
                        new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
                    replicaVerionId = result.Value.VersionId;
                }

                var replicaFileVersion = new FileVersion
                {
                    Name = dto.Name,
                    AzureId = versionId,
                    OriginalFileId = (Guid)appFile.ReplicaId,
                    CreationTime = startingTime
                };

                await _fileVersionsDAL.AddVersion(replicaFileVersion);

                await this._appFilesDAL.UpdateTimeInteractionsForFile((Guid)appFile.ReplicaId, startingTime, true);
            }

            return toReturn;
        }

        public async Task<List<FileVersionDTO>> GetFileVersionsByOriginalFileId(AppFile appFile)
        {
            List<FileVersion> fileVersions = await this._fileVersionsDAL.GetFileVersionsByOriginalFileId(appFile.Id);
            string azureFileName = appFile.Id.ToString() + Path.GetExtension(appFile.Name);

            return fileVersions.Select(f => new FileVersionDTO()
            {
                Id = f.Id,
                Name = f.Name,
                OriginalFileId = f.OriginalFileId,
                CreationTime = f.CreationTime.ToString("yyyy-MM-dd HH:mm"),
                AzureId = f.AzureId,
                TokenSAS = SASTokensGenerator.GenerateSasToken(appFile.StorageAccount.ConnectionString, "container", azureFileName, f.AzureId)
            }).ToList();
        }
    }
}
