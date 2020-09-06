using System;
using System.Security.Cryptography;
using System.Text;

namespace Cashback.Infra.CrossCutting.Auth
{
    public class Hasher
    {
        public static string CreatePasswordHash(string email, string password)
        {
            var passwd = Encoding.ASCII.GetBytes(password);
            var salt = Encoding.ASCII.GetBytes(email);
            HashAlgorithm algorithm = new SHA256Managed();

            byte[] passWithSalt = new byte[passwd.Length + salt.Length];

            for (int i = 0; i < passwd.Length; i++)
                passWithSalt[i] = passwd[i];
            for (int i = 0; i < salt.Length; i++)
                passWithSalt[passwd.Length + i] = salt[i];

            return Convert.ToBase64String(algorithm.ComputeHash(passWithSalt));
        }
    }
}
