using server.DAL.Entities;
using server.DAL;
using server.DTO;
using server.Utils;

namespace server.BLL
{
    public class UsersBLL
    {
        private UsersDAL _usersDAL;

        public UsersBLL(UsersDAL usersDAL)
        {
            this._usersDAL = usersDAL;
        }

        public async Task<UserDTO> RegisterUser(RegisterUserDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Email))
                throw new Exception("Invalid email.");

            if (string.IsNullOrEmpty(dto.Password))
                throw new Exception("Invalid password.");

            if (dto.Password.Length < 6)
                throw new Exception("Password too short! It should be at least 6 characters.");

            User user = new User()
            {
                Email = dto.Email,
                Password = EncryptionDecryption.Encrypt(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            User createdUser = await this._usersDAL.AddUser(user);

            return new UserDTO()
            {
                Id = createdUser.Id,
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName
            };
        }

        public async Task<UserDTO> LoginUser(LoginUserDTO dto)
        {
            User existingUser = await this._usersDAL.GetUserByEmail(dto.Email);
            if (existingUser == null)
            {
                throw new Exception("There is no user with this email");
            }

            if (existingUser.Password != EncryptionDecryption.Encrypt(dto.Password))
            {
                throw new Exception("Incorrect password");
            }

            return new UserDTO()
            {
                Id = existingUser.Id,
                Email = existingUser.Email,
                FirstName = existingUser.FirstName,
                LastName = existingUser.LastName
            };
        }
    }
}
