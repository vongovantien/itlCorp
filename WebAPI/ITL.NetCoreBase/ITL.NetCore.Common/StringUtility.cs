using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ITL.NetCore.Common
{
    public class StringUtility
    {
        public static Stream GetStream(string Str)
        {
            byte[] byteArray = Encoding.Unicode.GetBytes(Str);
            MemoryStream stream = new MemoryStream(byteArray);

            return stream;
        }
    }
}
