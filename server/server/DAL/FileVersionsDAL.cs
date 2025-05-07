using DataAbstractionLayer;
using Microsoft.EntityFrameworkCore;
using server.DAL.Entities;

namespace server.DAL
{
    public class FileVersionsDAL: BaseDAL
    {
        public FileVersionsDAL(FileSystemAppDbContext dbContext) : base(dbContext) { }

        public async Task<FileVersion> AddVersion(FileVersion fileVersion)
        {
            this._dbContext.FileVersions.Add(fileVersion);
            await this._dbContext.SaveChangesAsync();

            return fileVersion;
        }

        public async Task<List<FileVersion>> GetFileVersionsByOriginalFileId(Guid originalFileId)
        {
            return await this._dbContext.FileVersions.Where(f => f.OriginalFileId == originalFileId).ToListAsync();
        }
    }
}
