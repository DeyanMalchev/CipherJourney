using Azure;
using CipherJourney.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace CipherJourney.Services
{
    public class DB_Queries
    {

        // Check if the user exists on signUp
        public static User GetExistingUser(SignUpModel model, CipherJourneyDBContext _context)
        {
            return _context.Users.FirstOrDefault(u => u.Username == model.Username || u.Email == model.Email);
        }

        public static void AddUser(SignUpModel model, CipherJourneyDBContext _context)
        {

            byte[] salt = GenerateSalt();
            var newUser = new User
            {
                Username = model.Username,
                Email = model.Email,
                Salt = salt,
                Password = HashPassword(model.Password, salt),
                IsEmailVerified = false,
            };


            // Add the new user to the Users 
            _context.Users.Add(newUser);
            _context.SaveChanges();

            var newUserUnverified = new UsersUnverified
            {
                UserID = newUser.Id,
                DateOfCreation = DateTime.Now
            };

            _context.UsersUnverified.Add(newUserUnverified);
            _context.SaveChanges();
        }

        public static UserPoints ConfigureTablesAfterVerification(User user, UsersUnverified userUnverified, CipherJourneyDBContext _context) 
        {
            user.IsEmailVerified = true;
            _context.UsersUnverified.Remove(userUnverified);
            _context.SaveChanges();

            var userPoints = new UserPoints
            {
                UserId = user.Id,
                DailyScore = 0,
                WeeklyScore = 0,
                DailiesDone = 0,
                WeekliesDone = 0
            };

            _context.UserPoints.Add(userPoints);
            _context.SaveChanges();

            var leaderboard = new Leaderboard
            {
                UserId = user.Id,
                Username = user.Username,
                TotalPoints = userPoints.DailyScore + userPoints.WeeklyScore
            };

            _context.Leaderboard.Add(leaderboard);
            _context.SaveChanges();

            return userPoints;
        }

        public static void VerifyUserEmail(User user, IEmailService _emailService, CipherJourneyDBContext _context) 
        {

            Regex emailRegex = new Regex(@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|
                             \\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]
                             |2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c
                             \x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])");

            var userUnverified = _context.UsersUnverified.FirstOrDefault(u => u.UserID == user.Id);

            userUnverified.VerificationToken = GenerateVerificationToken();
            _context.SaveChanges();

            _emailService.SendEmailAsync(user.Email, userUnverified.VerificationToken);

        }

        public static string GenerateVerificationToken()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static byte[] GenerateSalt()
        {
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes
            return salt;
        }

        public static string HashPassword(string password, byte[] salt)
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
                    return user;
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
                    return user;
                }
            }
            return null;
        }

        public static UserPoints GetUserPoints(User user,CipherJourneyDBContext _context)
        {
            UserPoints? userPoints = _context.UserPoints.FirstOrDefault(u => u.UserId == user.Id);

            return userPoints;
        }

        public static void DeleteAccount(User user, CipherJourneyDBContext _context)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public static string[] GetDailyModeConfiguration (CipherJourneyDBContext _context)
        {
            SentenceDailyModel sentenceDailyModel = _context.SentencesDaily.FirstOrDefault(u => u.Id == 1);
            string sentence = sentenceDailyModel.Sentence;

            CipherModel cipherModel = _context.Ciphers.FirstOrDefault(u => u.Id == 1);
            string cipher = cipherModel.Cipher;

            return [cipher, sentence];
        }
    }
}
