using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eFMS.API.Shipment.Infrastructure.Common
{
    public class StringExtensions
    {
        public static string RejectMarks(string text)
        {
            string[] pattern = new string[7];

            pattern[0] = "a | (á | ả | à | ạ | ã | ă | ắ | ẳ | ằ | ặ | ẵ | â | ấ | ẩ | ầ | ậ | ẫ)";
            pattern[1] = "o | (ó | ỏ | ò | ọ | õ | ô | ố | ổ | ồ | ộ | ỗ | ơ | ớ | ở | ờ | ợ | ỡ)";
            pattern[2] = "e | (é | è | ẻ | ẹ | ẽ | ê | ế | ề | ể | ệ | ễ)";
            pattern[3] = "u | (ú | ù | ủ | ụ | ũ | ư | ứ | ừ | ử | ự | ữ)";
            pattern[4] = "i | (í | ì | ỉ | ị | ĩ)";
            pattern[5] = "y | (ý | ỳ | ỷ | ỵ | ỹ)";
            pattern[6] = "d | đ";

            for (int i = 0; i < pattern.Length; i++)
            {
                char replaceChar = pattern[i][0];
                MatchCollection matchs = Regex.Matches(text, pattern[i]);

                foreach (Match m in matchs)
                {
                    text = text.Replace(m.Value[0], replaceChar);
                }
            }
            return text;
        }
    }
}
