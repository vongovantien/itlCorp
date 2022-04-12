using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Common
{
    public static class GenerateID
    {
        public static string GenerateJobID(string servicePrefix, int number)
        {
            number = number + 1;
            var currentDate = DateTime.Now;
            return servicePrefix + currentDate.Year + String.Format("{0:00}", currentDate.Month) + String.Format("{0:00000}", number);
        }
        public static string GeneratePrefixHousbillNo()
        {
            Random rnd = new Random();
            int card = rnd.Next(52);
            var currentDate = DateTime.Now;
            return currentDate.Year.ToString().Substring(2) + String.Format("{0:00}", currentDate.Month) + String.Format("{0:00}", currentDate.Day) 
                        + String.Format("{0:00}", card);
        }
        public static string GenerateHousebillNo(string prefix, int number)
        {
            number = number + 1;
            var currentDate = DateTime.Now;
            return prefix + String.Format("{0:0000}", number);
        }
        public static string GenerateOPSJobID(string servicePrefix, int number)
        {
            number = number + 1;
            var currentDate = DateTime.Now;
            return servicePrefix + currentDate.Year.ToString().Substring(2) + String.Format("{0:00}", currentDate.Month)  + "/" + String.Format("{0:00000}", number);
        }
    }
}
