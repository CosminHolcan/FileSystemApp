using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class AppFilesDAL : BaseDAL
    {
        public AppFilesDAL(FileSystemAppDbContext dbContext) : base(dbContext) { }

        public async Task<AppFile> AddFile(AppFile appFile)
        {
            AppFile existingFile = await this._dbContext.AppFiles.FirstOrDefaultAsync(f => f.StorageAccountId == appFile.StorageAccountId && f.Name == appFile.Name);
            if (existingFile != null)
                throw new Exception("There is already a file with this title.");

            StorageAccount storageAccount = await this._dbContext.StorageAccounts.FirstOrDefaultAsync(sa => sa.Id == appFile.StorageAccountId);

            if (storageAccount == null)
                throw new Exception("StorageAccount not found.");

            appFile.StorageAccount = storageAccount;

            this._dbContext.AppFiles.Add(appFile);
            await this._dbContext.SaveChangesAsync();

            return appFile;
        }

        public async Task UpdateTimeInteractionsForFile(Guid fileId, DateTime time, bool changeLastUpdate)
        {
            AppFile appFile = await this.GetFileById(fileId);
            appFile.LastInteraction = time;
            if (changeLastUpdate)
            {
                appFile.LastUpdate = time;
            }

            await this.UpdateFile(appFile);
        }

        public async Task UpdateFile(AppFile updatedFile)
        {
            _dbContext.AppFiles.Update(updatedFile);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<AppFile> GetFileById(Guid fileId)
        {
            return await this._dbContext.AppFiles.FirstOrDefaultAsync(f => f.Id == fileId);
        }

        public async Task<List<AppFile>> GetFilesByUser(Guid userId)
        {
            return await this._dbContext.AppFiles
                .Include(f => f.StorageAccount)
                .Where(f => f.UserId == userId).ToListAsync();
        }

        public async Task<AppFile> GetFileByIdWithStorageAccount(Guid fileId)
        {
            return await this._dbContext.AppFiles
                .Include(f => f.StorageAccount)
                .FirstOrDefaultAsync(f => f.Id == fileId);
        }

        public async Task<AppFile> GetFullFileById(Guid fileId)
        {
            return await this._dbContext.AppFiles
                .Include(f => f.Versions)
                .Include(f => f.StorageAccount)
                .FirstOrDefaultAsync(f => f.Id == fileId);
        }

        public async Task<List<AppFile>> GetFilesWithReplica()
        {
            return await this._dbContext.AppFiles
                .Include(f => f.StorageAccount)
                .Include(f => f.Versions)
                .Where(f => f.ReplicaId != null)
                .ToListAsync();
        }

        public async Task DeleteFile(AppFile appFile)
        {
            _dbContext.AppFiles.Remove(appFile);
            await _dbContext.SaveChangesAsync();
        }
    }
}
