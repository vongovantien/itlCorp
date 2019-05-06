using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ReportPerview.Common
{
    public static class clsPublicFunction
    {
        //Get root url of website
        public static string getFullApplicationPath()
        {
            string appPath = string.Empty;
            Uri uri = HttpContext.Current.Request.Url;
            if (uri != null)
            {
                appPath = string.Format("{0}://{1}{2}{3}", uri.Scheme, uri.Host,
                    uri.Port == 80 ? "" : ":" + uri.Port.ToString(), HttpContext.Current.Request.ApplicationPath);
            }
            if (!appPath.EndsWith("/"))
                appPath += "/";
            return appPath;
        }
        //make a file name
        public static string makeFileName(String str)
        {
            string filename = Regex.Replace(str.Trim(), @"[^a-zA-Z0-9\sÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼỀỀỂưăạảấầẩẫậắằẳẵặẹẻẽềềểếỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪễệỉịọỏốồổỗộớờởỡợụủứừỬỮỰỲỴÝỶỸửữựỳỵỷỹ]*", "");
            filename = Regex.Replace(filename, @"\s+", "-");
            return filename;

        }
        //Convert from a string unicode to unsign string
        public static string ConvertToUnSign(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
    }
}