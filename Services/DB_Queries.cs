﻿using CipherJourney.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

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

            // Add the new user to the Users DbSet
            _context.Users.Add(newUser);

            // Save changes to the database
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

        public static User? LoginUser(string username, string password, CipherJourneyDBContext _context)
        {
            // Find the user by username
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return null;
            }

            // Hash the input password with the stored salt and compare
            var hashedPassword = HashPassword(password, user.Salt);
            if (hashedPassword == user.Password)
            {
                return user;
            }

            return null;
        }
    }
}
