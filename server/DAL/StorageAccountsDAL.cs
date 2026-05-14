using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;

namespace DAL
{
    public class StorageAccountsDAL : BaseDAL
    {
        public StorageAccountsDAL(FileSystemAppDbContext dbContext) : base(dbContext) { }

        public async Task<List<StorageAccount>> GetAllStorageAccounts()
        {
            return await _dbContext.StorageAccounts.ToListAsync();
        }

        public async Task<StorageAccount> GetStorageAcccountById(Guid id)
        {
            return await _dbContext.StorageAccounts.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<StorageAccount> GetStorageAcccountByFileId(Guid fileId)
        {
            AppFile file = await _dbContext.AppFiles
                .Include(f => f.StorageAccount)
                .FirstOrDefaultAsync(f => f.Id == fileId);

            return file.StorageAccount;
        }

        public async Task<StorageAccount> GetStorageAccountByFeatures(Location location, Redundancy redundancy, bool versioning)
        {
            Redundancy actualRedundancy = redundancy == Redundancy.Custom ? Redundancy.Locally : redundancy;
            return await _dbContext.StorageAccounts.FirstOrDefaultAsync(s => s.Location == location && s.Redundancy == actualRedundancy && s.Versioning == versioning);
        }
    }
}
