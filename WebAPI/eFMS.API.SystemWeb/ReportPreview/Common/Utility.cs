using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Serialization;
using System.Data;
using System.Web.Script.Serialization;
using System.Xml;

namespace ReportPerview.Common
{
    public static class Utility
    {
        public static bool IsNull(object o)
        {
            return (o == null || Convert.IsDBNull(o));
        }

        public static bool IsEmail(string inputEmail)
        {
            if (!String.IsNullOrEmpty(inputEmail))
            {

                String strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                                        @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                                        @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                Regex re = new Regex(strRegex);
                return re.IsMatch(inputEmail);
            }
            return false;
        }
        //Validate email allow empty
        public static bool IsEmail(string inputEmail, bool allowEmpty)
        {
            if (string.IsNullOrEmpty(inputEmail) && allowEmpty)
                return true;
            else if (!string.IsNullOrEmpty(inputEmail))
                return IsEmail(inputEmail);

            return false;
        }

        //Validate phone
        public static bool IsPhone(string inputPhone, bool allowEmpty)
        {
            if (allowEmpty && string.IsNullOrEmpty(inputPhone))
            {
                return true;
            }
            else if (!String.IsNullOrEmpty(inputPhone))
            {

                String strRegex = @"^(([+][1-9]{2,})|[0-9])\d{5,13}$";
                Regex re = new Regex(strRegex);
                return re.IsMatch(inputPhone.Trim());
            }
            return false;
        }

        //Validate fax
        public static bool IsFax(string inputFax, bool allowEmpty)
        {
            if (allowEmpty && string.IsNullOrEmpty(inputFax))
            {
                return true;
            }
            else if (!String.IsNullOrEmpty(inputFax))
            {

                String strRegex = @"^\d{3,15}$";
                Regex re = new Regex(strRegex);
                return re.IsMatch(inputFax.Trim());
            }
            return false;
        }

        public static string GetSHA1(string str)
        {
            return GetSHA(str, SHA1.Create());
        }

        public static string GetSHA256(string str)
        {
            return GetSHA(str, SHA256.Create());
        }

        public static string GetSHA512(string str)
        {
            return GetSHA(str, SHA512.Create());
        }

        public static string GetMD5(string str)
        {
            return GetSHA(str, MD5.Create());
        }

        private static string GetSHA(string str, HashAlgorithm hash)
        {
            try
            {
                string[] combined = hash.ComputeHash(Encoding.UTF8.GetBytes(str)).Select(x => x.ToString("X2")).ToArray();
                string rethash = string.Join("", combined).ToUpper();

                //byte[] combined = new ASCIIEncoding().GetBytes(str);                
                //hash.ComputeHash(combined);
                //string rethash = Convert.ToBase64String(hash.Hash);                

                return rethash;
            }
            catch { }
            return "";
            //return FormsAutication.HashPasswordForStoringInConfigFile(str, "sha1")
        }

        public static decimal RoundTo10000(decimal value)
        {
            return Math.Round((value) / 10000, 0) * 10000;
        }

        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(fileName))
                    {
                        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                    }
                }
            }
            catch { }

            return "";
        }

        public static string Serialize<T>(T mi, string xmlPathTemp)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (TextWriter writer = new StreamWriter(xmlPathTemp))
                {
                    serializer.Serialize(writer, mi);
                }
                string xmlDetail = System.IO.File.ReadAllText(xmlPathTemp);
                System.IO.File.Delete(xmlPathTemp);

                return xmlDetail;
            }
            catch { }
            return "";
        }

        public static T Deserialize<T>(string xmlDetail)
            where T : class, new()
        {
            try
            {
                T mi;
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (TextReader reader = new StringReader(xmlDetail))
                {
                    mi = serializer.Deserialize(reader) as T;
                }
                return mi;
            }
            catch { }
            return null;
        }

        public static string GetEnumString(Enum en)
        {
            return Enum.GetName(en.GetType(), en);
        }
        public static string ConvertDataTableToJson(DataTable table)
        {
            string result = string.Empty;
            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                Dictionary<string, object> row;
                foreach (DataRow dr in table.Rows)
                {
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in table.Columns)
                    {
                        row.Add(col.ColumnName, dr[col]);
                    }
                    rows.Add(row);
                }
                result = serializer.Serialize(rows);
            }
            catch { }
            return result;
        }
        public static string ConvertDataTableToXML(DataTable table)
        {
            string result = string.Empty;
            try
            {
                using (StringWriter sw = new StringWriter())
                {
                    table.WriteXml(sw, XmlWriteMode.WriteSchema, true);
                    result = sw.ToString();
                }
            }
            catch { }
            return result;
        }
        public static dynamic GetValueBy(this Object obj, string pro)
        {
            return obj.GetType().GetProperty(pro).GetValue(obj, null);
        }

        public static void WriteToFile(string logName, string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" + logName + "_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}