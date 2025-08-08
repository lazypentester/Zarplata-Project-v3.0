using System;
using System.Collections.Generic;
using System.Text;

namespace Hashing
{
    public static class passwordHash
    {
        private static char[] letters = "abcdefghijklmnopqrstuvwxyz!@#$%^&*()_.,/;:".ToCharArray();

        public static string generateSalt(int length)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                builder.Append(letters[random.Next(0, 41)]);
            }

            return builder.ToString();
        }

        public static string generateHash(string pass, string salt)
        {
            StringBuilder builder = new StringBuilder(pass);
            builder.Append(salt);
            string pass_hash = "";
            try
            {
                pass_hash = BCrypt.Net.BCrypt.HashPassword(builder.ToString());
            } 
            catch (Exception e)
            {
                Console.WriteLine($"Error to hash password.\n{e.Message}\n{e.Data}");
            };
            return pass_hash;
        }

        public static bool verifyPass(string pass, string salt, string hash)
        {
            StringBuilder builder = new StringBuilder(pass);
            builder.Append(salt);
            bool verify = false;
            try
            {
                verify = BCrypt.Net.BCrypt.Verify(builder.ToString(), hash);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error to verify password hash.\n{e.Message}\n{e.Data}");
            };
            return verify;
        }
    }
}
