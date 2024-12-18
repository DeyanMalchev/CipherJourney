using CipherJourney.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace CipherJourney.Services
{
    public class DB_Queries
    {

        // Check if the user exists on signUp
        public static void CheckIfUserExists(SignUpModel model, CipherJourneyDBContext _context)
        {
            if (_context.Users.Any(u => u.Username == model.Username || u.Email == model.Email))
            {
                throw new InvalidOperationException("Username or Email already exists.");
            }
        }


        public static void AddUser(SignUpModel model, CipherJourneyDBContext _context, string verificationCode)
        {

            byte[] salt = GenerateSalt();
            var newUser = new User
            {
                Username = model.Username,
                Email = model.Email,
                Salt = salt,
                Password = HashPassword(model.Password, salt),
                IsEmailVerified = false,
                VerificationToken = verificationCode
            };


            // Add the new user to the Users 
            _context.Users.Add(newUser);
            _context.SaveChanges();

            var newUserPoints = new UserPoints
            {
                UserId = newUser.Id,
                Score = 0,
                DailyAmountDone = 0,
                WeeklyAmountDone = 0
            };

            _context.UserPoints.Add(newUserPoints);
            _context.SaveChanges();
        }

        public static string GenerateVerificationToken()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static byte[] GenerateSalt()
        {
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes
            return salt;
        }

        private static string HashPassword(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
        }

        public static User? Login(LoginModel loginModel, CipherJourneyDBContext _context)
        {

            Regex emailRegex = new Regex(@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|
                             \\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]
                             |2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c
                             \x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])");

            // Login user with EMAIL
            if (emailRegex.IsMatch(loginModel.Info))
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == loginModel.Info);
                if (user == null)
                {
                    return null;
                }

                // Hash the input password with the stored salt and compare
                var hashedPassword = HashPassword(loginModel.Password, user.Salt);
                if (hashedPassword == user.Password)
                {
                    if (user.IsEmailVerified == true)
                    {
                        return user;
                    }
                }
                return null;
            }
            else 
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == loginModel.Info);
                if (user == null)
                {
                    return null;
                }

                // Hash the input password with the stored salt and compare
                var hashedPassword = HashPassword(loginModel.Password, user.Salt);
                if (hashedPassword == user.Password)
                {
                    if (user.IsEmailVerified == true)
                    {
                        return user;
                    }
                    return null;
                }
            }
            return null;
        }
    }
}
