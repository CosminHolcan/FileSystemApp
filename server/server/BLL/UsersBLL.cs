using server.DAL;
using server.DAL.Entities;
using server.DTO;
using server.Utils;

namespace server.BLL
{
    public class UsersBLL
    {
        private UsersDAL _usersDAL;
        private readonly ILogger<UsersBLL> _logger;

        public UsersBLL(UsersDAL usersDAL, ILogger<UsersBLL> logger)
        {
            this._usersDAL = usersDAL;
            this._logger = logger;
        }

        public async Task<UserDTO> RegisterUser(RegisterUserDTO dto)
        {
            _logger.LogInformation("RegisterUser called for email {Email}", dto?.Email);

            if (string.IsNullOrEmpty(dto.Email))
            {
                _logger.LogError("RegisterUser validation failed: empty email");
                throw new Exception("Invalid email.");
            }

            if (string.IsNullOrEmpty(dto.Password))
            {
                _logger.LogError("RegisterUser validation failed: empty password for email {Email}", dto.Email);
                throw new Exception("Invalid password.");
            }

            if (dto.Password.Length < 6)
            {
                _logger.LogError("RegisterUser validation failed: password too short for email {Email}", dto.Email);
                throw new Exception("Password too short! It should be at least 6 characters.");
            }

            User user = new User()
            {
                Email = dto.Email,
                Password = EncryptionDecryption.Encrypt(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            User createdUser = await this._usersDAL.AddUser(user);
            _logger.LogInformation("User created with id {UserId} and email {Email}", createdUser.Id, createdUser.Email);

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
            _logger.LogInformation("LoginUser called for email {Email}", dto?.Email);
            User existingUser = await this._usersDAL.GetUserByEmail(dto.Email);
            if (existingUser == null)
            {
                _logger.LogError("LoginUser failed: no user with email {Email}", dto.Email);
                throw new Exception("There is no user with this email");
            }

            if (existingUser.Password != EncryptionDecryption.Encrypt(dto.Password))
            {
                _logger.LogError("LoginUser failed: incorrect password for email {Email}", dto.Email);
                throw new Exception("Incorrect password");
            }

            _logger.LogInformation("LoginUser succeeded for user {UserId}", existingUser.Id);

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
