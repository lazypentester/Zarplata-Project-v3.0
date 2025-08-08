using System;
using System.Collections.Generic;
using System.Text;

namespace Hashing
{
    public static class secretCodeHash
    {
        /*public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                //return Convert.ToHexString(hashBytes); // .NET 5 +

                //convert the byte array to hexadecimal string prior to.net 5
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }*/

        /*public static string generateHash(string pass)
        {
            string pass_hash = "";
            try
            {
                pass_hash = BCrypt.Net.BCrypt.HashPassword(pass);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error to hash password.\n{e.Message}\n{e.Data}");
            };
            return pass_hash;
        }*/

        /*public static bool verifyPass(string pass, string hash)
        {
            bool verify = false;
            try
            {
                verify = BCrypt.Net.BCrypt.Verify(pass, hash);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error to verify password hash.\n{e.Message}\n{e.Data}");
            };
            return verify;
        }*/
    }
}
