using System;

namespace eFMS.API.Common.Globals
{
    public static class InWordCurrency
    {
        //Code chuyển số tiền thành chữ
        public static string ConvertNumberCurrencyToString(decimal _number, string currency)
        {
            if (_number == 0)
                return String.Format("Không {0}.", currency);
            decimal _numberUSD = 0;
            string _numOdd = "";
            if (currency == "USD")
            {
                string _sourceUSD = String.Format("{0:0.##}", _number);
                string[] _arrsourceUSD = _sourceUSD.Split('.');
                _numberUSD = Decimal.Parse(_arrsourceUSD[0]);
                _numOdd = _arrsourceUSD[1];
            }
            string _source = currency == "USD" ? String.Format("{0:0,0}", Math.Abs(_numberUSD)) : String.Format("{0:0,0}", Math.Abs(_number));

            string[] _arrsource = _source.Split(',');

            string _letter = "";

            int _numunit = _arrsource.Length;
            foreach (string _str in _arrsource)
            {
                if (ThreeNumber2Letter(_str).Length != 0)
                    _letter += String.Format("{0} {1} ", ThreeNumber2Letter(_str), NumUnit(_numunit));
                _numunit--;
            }

            if (_letter.StartsWith("không trăm"))
                _letter = _letter.Substring(11, _letter.Length - 12);
            if (_letter.StartsWith("lẻ"))
                _letter = _letter.Substring(3, _letter.Length - 3);

            var minusStr = (_number < 0 ? "Âm " : string.Empty); // TH số âm
            if (currency == "USD")
            {
                string cent = Int32.Parse(_numOdd.Substring(1, 1)) >= 2 ? " cents" : " cent";
                return minusStr + String.Format("{0}{1} {2}", _letter.Substring(0, 1).ToUpper(), _letter.Substring(1, _letter.Length - 1).Trim(), currency + " và " + TwoNumber2Letter(_numOdd) + cent);
            }
            return minusStr + String.Format("{0}{1} {2}.", _letter.Substring(0, 1).ToUpper(), _letter.Substring(1, _letter.Length - 1).Trim(), currency);
        }

        private static string ThreeNumber2Letter(string _number)
        {
            int _hunit = 0, _tunit = 0, _nunit = 0;
            if (_number.Length == 3)
            {
                _hunit = int.Parse(_number.Substring(0, 1));
                _tunit = int.Parse(_number.Substring(1, 1));
                _nunit = int.Parse(_number.Substring(2, 1));
            }
            else if (_number.Length == 2)
            {
                _tunit = int.Parse(_number.Substring(0, 1));
                _nunit = int.Parse(_number.Substring(1, 1));
            }
            else if (_number.Length == 1)
                _nunit = int.Parse(_number.Substring(0, 1));

            if (_hunit == 0 && _tunit == 0 && _nunit == 0)
                return "";

            switch (_tunit)
            {
                case 0:
                    if (_nunit == 0)
                        return String.Format("{0} trăm", OneNumber2Letter(_hunit, ""));
                    else
                        return String.Format("{0} trăm lẻ {1}", OneNumber2Letter(_hunit, ""), OneNumber2Letter(_nunit, ""));
                case 1:
                    if (_nunit == 0)
                        return String.Format("{0} trăm mười", OneNumber2Letter(_hunit, ""));
                    else
                        return String.Format("{0} trăm mười {1}", OneNumber2Letter(_hunit, ""), OneNumber2Letter(_nunit, "muoi"));
                default:
                    if (_nunit == 0)
                        return String.Format("{0} trăm {1} mươi", OneNumber2Letter(_hunit, ""), OneNumber2Letter(_tunit, ""));
                    else if (_nunit == 1)
                        return String.Format("{0} trăm {1} mươi mốt", OneNumber2Letter(_hunit, ""), OneNumber2Letter(_tunit, ""));
                    else if (_nunit == 4)
                        return String.Format("{0} trăm {1} mươi tư", OneNumber2Letter(_hunit, ""), OneNumber2Letter(_tunit, ""));
                    else
                        return String.Format("{0} trăm {1} mươi {2}", OneNumber2Letter(_hunit, ""), OneNumber2Letter(_tunit, ""), OneNumber2Letter(_nunit, "muoi"));
            }
        }

        private static string TwoNumber2Letter(string _number)
        {
            int _tunit = 0, _nunit = 0;
            if (_number.Length == 2)
            {
                _tunit = int.Parse(_number.Substring(0, 1));
                _nunit = int.Parse(_number.Substring(1, 1));
            }
            else if (_number.Length == 1)
                _nunit = int.Parse(_number.Substring(0, 1));

            if (_tunit == 0 && _nunit == 0)
                return "";

            switch (_tunit)
            {
                case 0:
                    if (_nunit == 0)
                        return String.Format("0", OneNumber2Letter(_tunit, ""));
                    else
                        return String.Format("{0}", OneNumber2Letter(_nunit, ""));
                default:
                    if (_nunit == 0)
                        return String.Format("{0} mươi", OneNumber2Letter(_tunit, ""));
                    else if (_nunit == 1)
                        return String.Format("{0} mươi mốt", OneNumber2Letter(_tunit, ""));
                    else if (_nunit == 4)
                        return String.Format("{0} mươi tư", OneNumber2Letter(_tunit, ""), OneNumber2Letter(_tunit, ""));
                    else if (_nunit == 5)
                        return String.Format("{0} mươi lăm", OneNumber2Letter(_tunit, ""), OneNumber2Letter(_tunit, ""));
                    else
                        return String.Format("{0} mươi {1}", OneNumber2Letter(_tunit, ""), OneNumber2Letter(_nunit, ""));
            }
        }

        private static string NumUnit(int _unit)
        {
            switch (_unit)
            {
                case 0:
                case 1:
                    return "";
                case 2:
                    return "nghìn";
                case 3:
                    return "triệu";
                case 4:
                    return "tỷ";
                default:
                    return String.Format("{0} {1}", NumUnit(_unit - 3), NumUnit(4));
            }
        }

        private static string OneNumber2Letter(int _number, string muoi)
        {
            switch (_number)
            {
                case 0:
                    return "không";
                case 1:
                    return "một";
                case 2:
                    return "hai";
                case 3:
                    return "ba";
                case 4:
                    return "bốn";
                case 5:
                    return muoi == "muoi" ? "lăm" : "năm";
                case 6:
                    return "sáu";
                case 7:
                    return "bảy";
                case 8:
                    return "tám";
                case 9:
                    return "chín";
                default:
                    return "";
            }

        }
        //Code chuyển số tiền thành chữ


        //Code chuyển số tiền thành chữ tiếng anh
        public static string ConvertNumberCurrencyToStringUSD(decimal number, string currency)
        {
            if (number == 0)
                return currency + " zero";

            if (number < 0)
                return "minus " + ConvertNumberCurrencyToStringUSD(Math.Abs(number), currency);

            string words = string.Empty;

            int intPortion = (int)number;
            decimal fraction = (number - intPortion) * 100;
            int decPortion = (int)fraction;

            words = currency + " " + NumberToWords(intPortion);
            if (decPortion > 0)
            {
                //words +=" and ";
                words += ", cents " + NumberToWords(decPortion);
            }
            return words;
        }

        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }
        //Code chuyển số tiền thành chữ tiếng anh
    }
}
