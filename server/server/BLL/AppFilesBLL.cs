using server.DAL;
using server.DAL.Entities;
using server.DTO;
using server.Enums;

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
            StorageAccount storageAccount = await this._storageAccountsDAL.GetStorageAccountByFeatures((Location)dto.Location, (Redundancy)dto.Redundancy, (bool)dto.Versionning);
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
                Versionning = createdAppFile.StorageAccount.Versioning,
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
                Versionning = f.StorageAccount.Versioning
            }).ToList();
        }
    }
}
