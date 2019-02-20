using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Common
{
    public static class GenerateID
    {
        public static string GenerateJobID(string servicePrefix, int number)
        {
            number = number + 1;
            var currentDate = DateTime.Now;
            return servicePrefix + currentDate.Year + String.Format("{0:00}", currentDate.Month) + String.Format("{0:00000}", number);
        }
    }
}
