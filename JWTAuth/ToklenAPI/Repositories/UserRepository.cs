using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using ToklenAPI.Data;
using ToklenAPI.Interfaces;
using ToklenAPI.Models;
using ToklenAPI.Models.Dtos;

namespace ToklenAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private string secretKey;

        public UserRepository(ApplicationDbContext bd, IConfiguration configuration)
        {
            _context = bd;
            secretKey = configuration.GetValue<string>("JwtSettings:key");
        }

        public async Task<string> Register (UserRegisterDto register){
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == register.Email);

            if (user != null)
            {
                throw new BadHttpRequestException("User already exists");
            }

            var newUser = new User
            {
                Email = register.Email,
                Fullname = register.Fullname,
                Password = HashPassword(register.Password, GenerateSalt()),
            };
            await _context.Users.AddAsync(newUser);

            var result = await _context.SaveChangesAsync();
            if(result==0)
            {
                throw new BadHttpRequestException("Error registering user");
            }
            return $"user {register.Email} has been created";
        }

        private  byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var salt = new byte[16];
                rng.GetBytes(salt);
                return salt;
            }
        }

        private string HashPassword(string password, byte[] salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedPassword = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
                var hash = sha256.ComputeHash(hashedPassword);
                return Convert.ToBase64String(hash);
            }
        }

        public string Login()
        {
            return "";
        }
    }
}
