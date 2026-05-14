using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class FileVersionsDAL : BaseDAL
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

        public async Task<FileVersion> GetFileVersionById(Guid fileVersionId)
        {
            return await this._dbContext.FileVersions.FirstOrDefaultAsync(fv => fv.Id == fileVersionId);
        }

        public async Task DeleteFileVersion(FileVersion fileVersion)
        {
            _dbContext.FileVersions.Remove(fileVersion);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateFileVersion(FileVersion updatedFileVersion)
        {
            _dbContext.FileVersions.Update(updatedFileVersion);
            await _dbContext.SaveChangesAsync();
        }
    }
}
