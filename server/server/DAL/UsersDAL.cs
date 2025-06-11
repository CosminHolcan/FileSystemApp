using Microsoft.EntityFrameworkCore;
using server.DAL.Entities;

namespace server.DAL
{
    public class UsersDAL : BaseDAL
    {
        public UsersDAL(FileSystemAppDbContext dbContext) : base(dbContext) { }

        public async Task<User> AddUser(User user)
        {
            User existingUser = await this._dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
                throw new Exception("There is already a user with this email.");

            this._dbContext.Users.Add(user);
            await this._dbContext.SaveChangesAsync();

            return user;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await this._dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByUserId(Guid userId)
        {
            return await this._dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}