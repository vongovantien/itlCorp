using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace eFMS.IdentityServer.Helpers
{
    public class RSAHelper
    {
        private readonly RSA _privateKeyRsaProvider;
        private readonly RSA _publicKeyRsaProvider;
        private readonly HashAlgorithmName _hashAlgorithmName;
        private readonly Encoding _encoding;


        //private string _privateKey =
        //    "MIICXQIBAAKBgQDrCqKUoVKKqah19HGIA1bX116Nuqxlu2Fb9jOSSoDMIM1pJR8l2yf75d9E5uNb9SJXH7ljZ6FEyb19xV18AkFL+O7RVzMA8oH1m1DFjyIAjjNG4HR/IM051ifWLDkMILCEHrq5siZBYPlREY+6f84m4oL6nA/m6ReOsMo057n+ywIDAQABAoGBAMlPvpiW49+oGIWO7H6WfZc4+99gd7BaobTmVp2B+nbb0ZOxG9CMuN5jsKuPJkTo3JTKszqU0+fa8tX6aWuBcvI+um8Qh/AZyFe+NnfflK2x2mh6/kFTyG+lYiY4kRcjcHOhIDr4ZDu13qTc/PEpSdnKQ+LHJoB3uvvta/cZ3XChAkEA/iF+NBYwSdA3ptnRrppx0Z6EembFvGUCsfSg1VwTVTXb3k0ObTHU85DUhrBM+iFCo264Qk6StNUHQUSFPYIxnQJBAOzFMtWDOg65SZwIOmTgvYkw3q/L4oCC1Yja6llwQ850XiR/20gF76M7Thkjs1OtNLboasyRqZaN2tI76QenmYcCQG8VZtwPwuXQ/TKSzeQboJr3RoNWfyKdqLLu8oqw58Z8d3JRjnfOq34YFb4WPF+twDo+QI7DV79xyu0NrYw4Z8UCQQDlCQ1XhQ2QWKRgD4WNN+mg4GlJ3QxKDEXLkRjJU9QDzoWTASt5zGQ3npK5ttMtyosHtGQ7Z1yJisd2PwX4paPZAkB5RAfIb6CGBXDteGO0pTNyg64gfAOGT7V1q2zxhCmYg5PBxyA3nBlq3Q4a3qnxw9YiunqKyIL0WtwcTw9sfsI0";


        //private string _publicKey =
        //    "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDrCqKUoVKKqah19HGIA1bX116Nuqxlu2Fb9jOSSoDMIM1pJR8l2yf75d9E5uNb9SJXH7ljZ6FEyb19xV18AkFL+O7RVzMA8oH1m1DFjyIAjjNG4HR/IM051ifWLDkMILCEHrq5siZBYPlREY+6f84m4oL6nA/m6ReOsMo057n+ywIDAQAB";

        private string _privateKey => this.LoadRSAKey("eFMS_RSA512.priv");
        private string _publicKey => this.LoadRSAKey("eFMS_RSA512.pub");

        public RSAHelper() : this(RSAType.RSA, null, null) { }

        public RSAHelper(RSAType rsaType) : this(rsaType, null, null) { }

        public RSAHelper(RSAType rsaType, string privateKey = null, string publicKey = null)
        {
            _encoding = Encoding.UTF8;
            _privateKeyRsaProvider = CreateRsaProviderFromPrivateKey(privateKey ?? _privateKey);
            _publicKeyRsaProvider = CreateRsaProviderFromPublicKey(publicKey ?? _publicKey);
            _hashAlgorithmName = rsaType == RSAType.RSA ? HashAlgorithmName.SHA1 : HashAlgorithmName.SHA256;
        }

        private string LoadRSAKey(string fileName)
        {
            string txt = string.Empty;

            var assembly = Assembly.GetAssembly(typeof(RSAHelper));
            var resourceStream = assembly.GetManifestResourceStream($"eFMS.IdentityServer.RSAKey.{fileName}");

            if (resourceStream != null)
            {
                //FileStream fs = File.Open(path, FileMode.Open);                
                using (StreamReader sr = new StreamReader(resourceStream))
                {
                    txt = sr.ReadToEnd().Replace(Environment.NewLine, string.Empty);
                }

                if ((txt.StartsWith("-----BEGIN PUBLIC KEY-----") && txt.EndsWith("-----END PUBLIC KEY-----"))
                    || (txt.StartsWith("-----BEGIN RSA PRIVATE KEY-----") && txt.EndsWith("-----END RSA PRIVATE KEY-----")))
                {
                    int iStart = txt.IndexOf("KEY-----") + 8;
                    int iEnd = txt.LastIndexOf("-----END");
                    txt = txt.Substring(iStart, iEnd - iStart);
                }
                else
                {
                    throw new Exception(String.Format("The key {0} invalid format", fileName));
                }
            }
            return txt;
        }

        #region 


        public string Sign(string data)
        {
            byte[] dataBytes = _encoding.GetBytes(data);

            var signatureBytes = _privateKeyRsaProvider.SignData(dataBytes, _hashAlgorithmName, RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signatureBytes);
        }

        #endregion

        #region 


        public bool Verify(string data, string sign)
        {
            byte[] dataBytes = _encoding.GetBytes(data);
            byte[] signBytes = Convert.FromBase64String(sign);

            var verify = _publicKeyRsaProvider.VerifyData(dataBytes, signBytes, _hashAlgorithmName, RSASignaturePadding.Pkcs1);

            return verify;
        }

        #endregion

        #region 

        public string Decrypt(string cipherText)
        {
            if (_privateKeyRsaProvider == null)
            {
                throw new Exception("_privateKeyRsaProvider is null");
            }
            return Encoding.UTF8.GetString(_privateKeyRsaProvider.Decrypt(Convert.FromBase64String(cipherText), RSAEncryptionPadding.Pkcs1));
        }

        #endregion

        #region 

        public string Encrypt(string text)
        {
            if (_publicKeyRsaProvider == null)
            {
                throw new Exception("_publicKeyRsaProvider is null");
            }
            return Convert.ToBase64String(_publicKeyRsaProvider.Encrypt(Encoding.UTF8.GetBytes(text), RSAEncryptionPadding.Pkcs1));
        }

        #endregion

        #region Create an RSA instance with a private key

        public RSA CreateRsaProviderFromPrivateKey(string privateKey)
        {
            var privateKeyBits = Convert.FromBase64String(privateKey);

            var rsa = RSA.Create();
            var rsaParameters = new RSAParameters();

            using (BinaryReader binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                rsaParameters.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.D = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.P = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.Q = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.DP = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.DQ = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            rsa.ImportParameters(rsaParameters);
            return rsa;
        }

        #endregion

        #region Create an RSA instance using the public key

        public RSA CreateRsaProviderFromPublicKey(string publicKeyString)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] seqOid = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];

            var x509Key = Convert.FromBase64String(publicKeyString);

            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            using (MemoryStream mem = new MemoryStream(x509Key))
            {
                using (BinaryReader binr = new BinaryReader(mem))  //wrap Memory Stream with BinaryReader for easy reading
                {
                    byte bt = 0;
                    ushort twobytes = 0;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    seq = binr.ReadBytes(15);       //read the Sequence OID
                    if (!CompareBytearrays(seq, seqOid))    //make sure Sequence for OID is correct
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8203)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    bt = binr.ReadByte();
                    if (bt != 0x00)     //expect null byte next
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                        lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte(); //advance 2 bytes
                        lowbyte = binr.ReadByte();
                    }
                    else
                        return null;
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                    int modsize = BitConverter.ToInt32(modint, 0);

                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {   //if first byte (highest order) of modulus is zero, don't include it
                        binr.ReadByte();    //skip this null byte
                        modsize -= 1;   //reduce modulus buffer size by 1
                    }

                    byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                    if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                        return null;
                    int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                    byte[] exponent = binr.ReadBytes(expbytes);

                    // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                    var rsa = RSA.Create();
                    RSAParameters rsaKeyInfo = new RSAParameters
                    {
                        Modulus = modulus,
                        Exponent = exponent
                    };
                    rsa.ImportParameters(rsaKeyInfo);

                    return rsa;
                }

            }
        }

        #endregion

        #region Import key algorithm

        private int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else
            if (bt == 0x82)
            {
                var highbyte = binr.ReadByte();
                var lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }

        private bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        #endregion

    }

    /// <summary>
    /// RSA algorithm type
    /// </summary>
    public enum RSAType
    {
        /// <summary>
        /// SHA1
        /// </summary>
        RSA = 0,
        /// <summary>
        /// RSA2 The key length is at least 2048
        /// SHA256
        /// </summary>
        RSA2
    }
}
