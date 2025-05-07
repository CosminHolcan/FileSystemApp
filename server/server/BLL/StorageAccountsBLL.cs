using server.DAL;
using server.DAL.Entities;
using server.DTO;

namespace server.BLL
{
    public class StorageAccountsBLL
    {
        private StorageAccountsDAL _storageAccountsDAL;

        public StorageAccountsBLL(StorageAccountsDAL storageAccountsDAL)
        {
            this._storageAccountsDAL = storageAccountsDAL;
        }

        public async Task<List<StorageAccountDTO>> GetAllStorageAccounts()
        {
            List<StorageAccount> storageAccounts = await this._storageAccountsDAL.GetAllStorageAccounts();

            return storageAccounts.Select(s => new StorageAccountDTO()
            {
                Id = s.Id,
                Location = s.Location,
                Redundancy = s.Redundancy,
                Versioning = s.Versioning,
            }).ToList();
        }

        public async Task<StorageAccount> GetStorageAccountById(Guid id)
        {
            return await this._storageAccountsDAL.GetStorageAcccountById(id);
        }

        public async Task<StorageAccount> GetStorageAccountByFileId(Guid fileId)
        {
            return await this._storageAccountsDAL.GetStorageAcccountByFileId(fileId);
        }
    }
}
