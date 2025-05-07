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

        public async Task<FileVersionDTO> AddVersion(AddFileVersionDTO dto)
        {
            FileVersion fileVersion = new FileVersion()
            {
                Name = dto.Name,
                AzureId = dto.AzureId,
                OriginalFileId = dto.OriginalFileId,
                CreationTime = DateTime.Now
            };

            FileVersion createdVersion = await this._fileVersionsDAL.AddVersion(fileVersion);
            AppFile appFile = await this._appFilesDAL.GetFileByIdWithStorageAccount(createdVersion.OriginalFileId);
            string azureFileName = appFile.Id.ToString() + Path.GetExtension(appFile.Name);

            return new FileVersionDTO()
            {
                Id = createdVersion.Id,
                Name = createdVersion.Name,
                OriginalFileId = createdVersion.OriginalFileId,
                CreationTime = createdVersion.CreationTime.ToShortDateString() + " " + createdVersion.CreationTime.ToShortTimeString(),
                TokenSAS = SASTokensGenerator.GenerateSasToken(appFile.StorageAccount.ConnectionString, "container", azureFileName, createdVersion.AzureId)
            };
        }

        public async Task<List<FileVersionDTO>> GetFileVersionsByOriginalFileId(Guid originalFileId)
        {
            List<FileVersion> fileVersions = await this._fileVersionsDAL.GetFileVersionsByOriginalFileId(originalFileId);
            AppFile appFile = await this._appFilesDAL.GetFileByIdWithStorageAccount(originalFileId);
            string azureFileName = appFile.Id.ToString() + Path.GetExtension(appFile.Name);

            return fileVersions.Select(f => new FileVersionDTO()
            {
                Id = f.Id,
                Name = f.Name,
                OriginalFileId = f.OriginalFileId,
                CreationTime = f.CreationTime.ToShortDateString() + " " + f.CreationTime.ToShortTimeString(),
                AzureId = f.AzureId,
                TokenSAS = SASTokensGenerator.GenerateSasToken(appFile.StorageAccount.ConnectionString, "container", azureFileName, f.AzureId)
            }).ToList();
        }
    }
}
