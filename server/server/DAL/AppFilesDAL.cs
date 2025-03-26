using DataAbstractionLayer;
using Microsoft.EntityFrameworkCore;
using server.DAL.Entities;

namespace server.DAL
{
    public class AppFilesDAL : BaseDAL
    {
        public AppFilesDAL(FileSystemAppDbContext dbContext) : base(dbContext) { }

        public async Task<AppFile> AddFile(AppFile appFile)
        {
            AppFile existingFile = await this._dbContext.AppFiles.FirstOrDefaultAsync(f => f.StorageAccountId == appFile.StorageAccountId && f.Name == appFile.Name);
            if (existingFile != null)
                throw new Exception("There is already a file with this title.");

            this._dbContext.AppFiles.Add(appFile);
            await this._dbContext.SaveChangesAsync();

            return appFile;
        }

        public async Task<List<AppFile>> GetFilesByUser(Guid userId)
        {
            return await this._dbContext.AppFiles.Where(f => f.UserId == userId).ToListAsync();
        }
    }
}
