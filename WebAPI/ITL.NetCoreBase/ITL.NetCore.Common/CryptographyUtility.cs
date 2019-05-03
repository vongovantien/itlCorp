using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace ITL.NetCore.Common
{
    public class CryptographyUtility
    {
        public static string Encrypt(string text)
        {
            return Encrypt(text, "hsaeawjghewwwfrtertefgggunivmfuo");
        }

        public static string Encrypt(string text, string strKey)
        {
            try
            {
                if (strKey.Length < 8) return "";

                byte[] clearTextBytes = Encoding.UTF8.GetBytes(text);
                SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();

                MemoryStream ms = new MemoryStream();
                byte[] rgbIV = Encoding.ASCII.GetBytes("rwaewayufygfherj");
                byte[] key = Encoding.ASCII.GetBytes(strKey);
                CryptoStream cs = new CryptoStream(ms, rijn.CreateEncryptor(key, rgbIV), CryptoStreamMode.Write);

                cs.Write(clearTextBytes, 0, clearTextBytes.Length);

                cs.Close();

                return Convert.ToBase64String(ms.ToArray());
            }
            catch { }
            return "";
        }

        public static string Decrypt(string text)
        {
            return Decrypt(text, "hsaeawjghewwwfrtertefgggunivmfuo");
        }

        public static string Decrypt(string text, string strKey)
        {
            try
            {
                byte[] encryptedTextBytes = Convert.FromBase64String(text);
                MemoryStream ms = new MemoryStream();
                SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();

                byte[] rgbIV = Encoding.ASCII.GetBytes("rwaewayufygfherj");
                byte[] key = Encoding.ASCII.GetBytes(strKey);

                CryptoStream cs = new CryptoStream(ms, rijn.CreateDecryptor(key, rgbIV), CryptoStreamMode.Write);

                cs.Write(encryptedTextBytes, 0, encryptedTextBytes.Length);

                cs.Close();

                return Encoding.UTF8.GetString(ms.ToArray());
            }
            catch { }
            return "";
        }
    }
}
