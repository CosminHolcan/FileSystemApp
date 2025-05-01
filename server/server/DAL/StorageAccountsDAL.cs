using DataAbstractionLayer;
using Microsoft.EntityFrameworkCore;
using server.DAL.Entities;
using server.Enums;

namespace server.DAL
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

        public async Task<StorageAccount> GetStorageAccountByFeatures(Location location, Redundancy redundancy, bool versioning)
        {
            return await _dbContext.StorageAccounts.FirstOrDefaultAsync(s => s.Location == location && s.Redundancy == redundancy && s.Versioning == versioning);
        }
    }
}
