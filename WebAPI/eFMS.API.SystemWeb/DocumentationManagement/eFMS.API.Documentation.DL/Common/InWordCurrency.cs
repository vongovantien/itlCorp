using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Common
{
    public static class InWordCurrency
    {
        //Code chuyển số tiền thành chữ
        public static string ConvertNumberCurrencyToString(decimal _number, string currency)
        {
            if (_number == 0)
                return String.Format("Không {0}.", currency);

            string _source = String.Format("{0:0,0}", _number);

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

            return String.Format("{0}{1} {2}.", _letter.Substring(0, 1).ToUpper(), _letter.Substring(1, _letter.Length - 1).Trim(), currency);
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
    }
    //Code chuyển số tiền thành chữ
}

