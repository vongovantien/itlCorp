using System;

namespace eFMS.API.Common.Helpers
{
    public static class NumberHelper
    {
        public static decimal RoundNumber(decimal number, int? decimals = null)
        {
            int _decimals = decimals ?? 0;
            return Math.Round(number, _decimals, MidpointRounding.AwayFromZero);
        }
    }
}
