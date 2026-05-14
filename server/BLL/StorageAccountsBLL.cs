using DAL;
using DAL.Entities;
using Microsoft.Extensions.Logging;
using Shared.DTO;

namespace BLL
{
    public class StorageAccountsBLL
    {
        private StorageAccountsDAL _storageAccountsDAL;
        private readonly ILogger<StorageAccountsBLL> _logger;

        public StorageAccountsBLL(StorageAccountsDAL storageAccountsDAL, ILogger<StorageAccountsBLL> logger)
        {
            this._storageAccountsDAL = storageAccountsDAL;
            this._logger = logger;
        }

        public async Task<List<StorageAccountDTO>> GetAllStorageAccounts()
        {
            _logger.LogDebug("GetAllStorageAccounts called");
            List<StorageAccount> storageAccounts = await this._storageAccountsDAL.GetAllStorageAccounts();

            var result = storageAccounts.Select(s => new StorageAccountDTO()
            {
                Id = s.Id,
                Location = s.Location,
                Redundancy = s.Redundancy,
                Versioning = s.Versioning,
            }).ToList();

            _logger.LogInformation("GetAllStorageAccounts returning {Count} storage accounts", result.Count);
            return result;
        }

        public async Task<StorageAccount> GetStorageAccountById(Guid id)
        {
            _logger.LogDebug("GetStorageAccountById called for id {Id}", id);
            return await this._storageAccountsDAL.GetStorageAcccountById(id);
        }

        public async Task<StorageAccount> GetStorageAccountByFileId(Guid fileId)
        {
            _logger.LogDebug("GetStorageAccountByFileId called for fileId {FileId}", fileId);
            return await this._storageAccountsDAL.GetStorageAcccountByFileId(fileId);
        }
    }
}
