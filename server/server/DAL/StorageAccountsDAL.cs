using DataAbstractionLayer;
using Microsoft.EntityFrameworkCore;
using server.DAL.Entities;

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
    }
}
