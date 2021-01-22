using System;

namespace eFMS.API.Common.Helpers
{
    public static class NumberHelper
    {
        public static decimal RoundNumber(decimal number, int decimals)
        {
            return Math.Round(number, decimals, MidpointRounding.AwayFromZero);
        }
    }
}
