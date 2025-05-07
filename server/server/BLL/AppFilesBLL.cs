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
        private StorageAccountsDAL _storageAccountsDAL;

        public AppFilesBLL(UsersDAL usersDAL, AppFilesDAL appFilesDAL, StorageAccountsDAL storageAccountsDAL)
        {
            this._usersDAL = usersDAL;
            this._appFilesDAL = appFilesDAL;
            _storageAccountsDAL = storageAccountsDAL;
        }

        public async Task<AppFileDTO> AddFile(CreateFileDTO dto, Guid userId)
        {
            StorageAccount storageAccount = await this._storageAccountsDAL.GetStorageAccountByFeatures((Location)dto.Location, (Redundancy)dto.Redundancy, (bool)dto.Versioning);
            AppFile appFile = new AppFile()
            {
                UserId = userId,
                Name = dto.Name,
                StorageAccountId = storageAccount.Id,
                CreationDate = DateOnly.FromDateTime(DateTime.Now),
                LastInteraction = DateOnly.FromDateTime(DateTime.Now)
            };

            AppFile createdAppFile = await this._appFilesDAL.AddFile(appFile);

            return new AppFileDTO()
            {
                Id = createdAppFile.Id,
                Name = createdAppFile.Name,
                StorageAccountId = createdAppFile.StorageAccountId,
                Location = createdAppFile.StorageAccount.Location,
                Redundancy = createdAppFile.StorageAccount.Redundancy,
                Versioning = createdAppFile.StorageAccount.Versioning,
                CreationDate = createdAppFile.CreationDate.ToShortDateString()
            };
        }

        public async Task<List<AppFileDTO>> GetFilesByUser(Guid userId)
        {
            List<AppFile> files = await this._appFilesDAL.GetFilesByUser(userId);

            return files.Select(f => new AppFileDTO()
            {
                Id = f.Id,
                Name = f.Name,
                StorageAccountId = f.StorageAccountId,
                CreationDate = f.CreationDate.ToShortDateString(),
                Location = f.StorageAccount.Location,
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
    }
}
