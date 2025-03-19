using server.DAL;
using server.DAL.Entities;
using server.DTO;
using server.Utils;

namespace server.BLL
{
    public class AppFilesBLL
    {
        private UsersDAL _usersDAL;
        private AppFilesDAL _appFilesDAL;

        public AppFilesBLL(UsersDAL usersDAL, AppFilesDAL appFilesDAL)
        {
            this._usersDAL = usersDAL;
            this._appFilesDAL = appFilesDAL;
        }

        public async Task<AppFileDTO> AddFile(CreateFileDTO dto, Guid userId)
        {
            AppFile appFile = new AppFile()
            {
                UserId = userId,
                Name = dto.Name,
                StorageAccount = dto.StorageAccount
            };

            AppFile createdAppFile = await this._appFilesDAL.AddFile(appFile);

            return new AppFileDTO()
            {
                Id = createdAppFile.Id,
                Name = createdAppFile.Name,
                StorageAccount = createdAppFile.StorageAccount
            };
        }

        public async Task<List<AppFileDTO>> GetFilesByUser(Guid userId)
        {
            List<AppFile> files = await this._appFilesDAL.GetFilesByUser(userId);

            return files.Select(f => new AppFileDTO()
            {
                Id = f.Id,
                Name = f.Name,
                StorageAccount = f.StorageAccount
            }).ToList();
        }
    }
}
