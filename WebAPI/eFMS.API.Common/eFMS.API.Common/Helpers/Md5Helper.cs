

using System;
using System.Security.Cryptography;
using System.Text;

namespace eFMS.API.Common.Helpers
{
    public class Md5Helper
    {
        public static string CreateMD5(string input)
        {
            using (var md5 = MD5.Create()) //or MD5 SHA256 etc.
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashedBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
