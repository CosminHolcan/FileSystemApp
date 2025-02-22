using DataAbstractionLayer;
using Microsoft.EntityFrameworkCore;
using server.DAL.Entities;

namespace server.DAL
{
    public class UsersDAL : BaseDAL
    {
        public UsersDAL(MovieReviewDbContext dbContext) : base(dbContext) { }

        public async Task<User> AddUser(User user)
        {
            User existingUser = await this._dbContext.Users.FirstOrDefaultAsync(u => u.UserName == user.UserName);
            if (existingUser != null)
                throw new Exception("There is already a user with this username.");

            this._dbContext.Users.Add(user);
            await this._dbContext.SaveChangesAsync();

            return user;
        }

        public async Task<User> GetUserByUserName(string userName)
        {
            return await this._dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<User> GetUserByUserId(Guid userId)
        {
            return await this._dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }
    }
}