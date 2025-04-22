using Azure;
using CipherJourney.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

            var newUserUnverified = new UserVerificationTokens
            {
                UserID = newUser.Id,
                VerificationToken = GenerateVerificationToken(),
                ExpirationDate = DateTime.Now.AddMinutes(15),
                Purpose = "email"
            };

            _context.UserVerificationTokens.Add(newUserUnverified);
            _context.SaveChanges();
        }

        public static UserPoints ConfigureTablesAfterVerification(User user, UserVerificationTokens userVerificationTokens, CipherJourneyDBContext _context)
        {
            user.IsEmailVerified = true;
            _context.UserVerificationTokens.Remove(userVerificationTokens);
            _context.SaveChanges();

            var userPoints = new UserPoints
            {
                UserId = user.Id,
                RiddlesSolved = 0,
                Score = 0,
                GuessCount = 0
            };

            _context.UserPoints.Add(userPoints);
            _context.SaveChanges();

            var leaderboard = new Leaderboard
            {
                UserId = user.Id,
                Username = user.Username,
                TotalPoints = userPoints.Score
            };

            _context.Leaderboard.Add(leaderboard);
            _context.SaveChanges();

            return userPoints;
        }

        public static void SendEmailVerification(User user, IEmailService _emailService, CipherJourneyDBContext _context)
        {

            Regex emailRegex = new Regex(@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|
                                 \\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]
                                 |2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c
                                 \x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])");

            var userVerificationTokens = _context.UserVerificationTokens.FirstOrDefault(u => u.UserID == user.Id);

            userVerificationTokens.VerificationToken = GenerateVerificationToken();
            _context.SaveChanges();

            _emailService.SendEmailAsync(user.Email, userVerificationTokens.VerificationToken);

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

        public static UserPoints GetUserPoints(User user, CipherJourneyDBContext _context)
        {
            UserPoints? userPoints = _context.UserPoints.FirstOrDefault(u => u.UserId == user.Id);

            return userPoints;
        }

        public static void DeleteAccount(User user, CipherJourneyDBContext _context)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public static void UpdateUserPointsAfterDaily(int guessCount, int userId, CipherJourneyDBContext _context)
        {

            bool hasCompleted = _context.UsersCompletedDaily
                        .Any(uc => uc.UserId == userId && uc.CompletionDate == DateTime.UtcNow.Date);

            if (hasCompleted)
            {
                return;
            }
            // Find the user points entry for the given user
            var userPoints = _context.UserPoints.FirstOrDefault(up => up.UserId == userId);

            if (userPoints == null)
            {
                throw new InvalidOperationException("User points entry not found for userId: " + userId);
            }

            int awardedPoints;
            if (guessCount <= 1) awardedPoints = 10;
            else if (guessCount == 2) awardedPoints = 9;
            else if (guessCount == 3) awardedPoints = 8;
            else if (guessCount == 4) awardedPoints = 6;
            else if (guessCount == 5) awardedPoints = 5;
            else if (guessCount == 6) awardedPoints = 3;
            else if (guessCount == 7) awardedPoints = 2;
            else awardedPoints = 1;

            // Update values
            userPoints.Score += awardedPoints;
            userPoints.RiddlesSolved += 1;
            userPoints.GuessCount += guessCount;


            var leaderboard = _context.Leaderboard.FirstOrDefault(lb => lb.UserId == userId);
            leaderboard.TotalPoints = userPoints.Score;

            _context.SaveChanges();

            UserCompletedDaily userCompleted = new UserCompletedDaily
            {
                UserId = userPoints.UserId,
                CompletionDate = DateTime.UtcNow.Date
            };

            _context.UsersCompletedDaily.Add(userCompleted);
            _context.SaveChanges();

        }

        public static void GenerateDailyConfiguration(CipherJourneyDBContext _context, HttpResponse response, HttpRequest request)
        {
            DailyGameConfiguration dailyGameConfiguration = _context.DailyConfiguration.OrderByDescending(d => d.Date).FirstOrDefault();
            var currectDate = DateTime.UtcNow.Date;

            if (dailyGameConfiguration != null) 
            {
                if (dailyGameConfiguration.Date == currectDate) return;
            }

            // 1. Get random sentence from DB
            var sentenceCount = _context.SentencesDaily.Count();
            if (sentenceCount == 0) return;

            var random = new Random();
            int sentenceIndex = random.Next(0, sentenceCount);
            var sentence = _context.SentencesDaily.Skip(sentenceIndex).FirstOrDefault()?.Sentence;

            // 2. Get random cipher from DB
            var cipherCount = _context.Ciphers.Count();
            if (cipherCount == 0 || string.IsNullOrEmpty(sentence)) return;

            int cipherIndex = random.Next(0, cipherCount);
            var cipher = _context.Ciphers.Skip(cipherIndex).FirstOrDefault()?.Cipher;

            // 3. Generate cipher key 
            string key = "3"; // default
            if (cipher == "Caesar")
            {
                key = random.Next(1, 25).ToString(); // Avoid 0/26
            }


            dailyGameConfiguration = new DailyGameConfiguration()
            {
                Sentence = sentence,
                Cipher = cipher,
                Key = key,
                Date = currectDate
            };

            _context.DailyConfiguration.Add(dailyGameConfiguration);
            _context.SaveChanges();

            // REMOVE ALL CONTENT/ROWS
            var allCompletedDailyUsers = _context.UsersCompletedDaily.ToList();
            _context.UsersCompletedDaily.RemoveRange(_context.UsersCompletedDaily);
            _context.SaveChanges();

            response.Cookies.Delete("DailyMode");
        }

        public static UserCompletedDaily GetUserCompletedDaily(CipherJourneyDBContext _context, HttpRequest request) 
        {
            var userCookie = request.Cookies["CipherJourney"];
            if (userCookie == null) return null;

            var userData = JsonConvert.DeserializeObject<Dictionary<string, string>>(userCookie);
            int userID = int.Parse(userData["UserId"]);

            UserCompletedDaily userCompletedDaily = _context.UsersCompletedDaily.FirstOrDefault(ucd => ucd.UserId == userID);

            return userCompletedDaily;
        }

        public static DailyGameConfiguration GetDailyGameConfiguration(CipherJourneyDBContext _context) {
            return _context.DailyConfiguration.OrderByDescending(d => d.Date).FirstOrDefault();
        }

        public static void AddUserVerificationTokensPassword(User user, CipherJourneyDBContext _context)
        {
            UserVerificationTokens userVerificationTokens = new UserVerificationTokens()
            {
                UserID = user.Id,
                VerificationToken = GenerateVerificationToken(),
                Purpose = "password",
                ExpirationDate = DateTime.Now.AddMinutes(15)
            };

            _context.UserVerificationTokens.Add(userVerificationTokens);
            _context.SaveChanges();
        }

        public static UserVerificationTokens GetUserVerificationTokens(User user, CipherJourneyDBContext _context) {
            return _context.UserVerificationTokens.FirstOrDefault(u => u.UserID == user.Id);
        }
    }
}
