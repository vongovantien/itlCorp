using System;
using System.Reflection;
using System.Globalization;

namespace ITL.NetCore.Common
{
    public class Cast
    {
        public static string ToSafeString(object o)
        {
            return (o ?? "").ToString();
        }

        public static byte ToByte(object o)
        {
            byte i = 0;
            if (!Utility.IsNull(o))
                byte.TryParse(o.ToString(), out i);
            return i;
        }

        public static int ToInt(object o)
        {
            int i = (int)ToDouble(o);
            return i;
        }
        public static int? ToIntNullable(object o)
        {
            int? i = (int)ToDoubleNullable(o);
            return i;
        }

        public static float ToFloat(object o)
        {
            float i = 0;
            if (!Utility.IsNull(o))
                float.TryParse(o.ToString(), out i);
            return i;
        }

        public static double ToDouble(object o)
        {
            double i = 0;
            if (!Utility.IsNull(o))
                double.TryParse(o.ToString(), out i);
            return i;
        }
        public static double? ToDoubleNullable(object o)
        {
            double i = 0;
            if (!Utility.IsNull(o))
            {
                try
                {
                    i = double.Parse(o.ToString(), CultureInfo.InvariantCulture);
                    return i;
                }
                catch { }
            }
            return null;
        }

        public static decimal ToDecimal(object o)
        {
            decimal i = 0;
            if (!Utility.IsNull(o))
                decimal.TryParse(o.ToString(), out i);
            return i;
        }
        public static decimal? ToDecimalNullable(object o)
        {
            decimal i = 0;
            if (!Utility.IsNull(o))
            {
                try
                {
                    i = decimal.Parse(o.ToString(), CultureInfo.InvariantCulture);
                    return i;
                }
                catch { }
            }
            return null;
        }

        public static bool ToBoolean(object o)
        {
            bool i = false;
            if (!Utility.IsNull(o))
                bool.TryParse(o.ToString(), out i);
            return i;
        }

        public static DateTime ToDateTime(object o)
        {
            DateTime i = new DateTime(1, 1, 1);
            if (!Utility.IsNull(o))
            {
                if (o is DateTime)
                    return (DateTime)o;
                bool passed = DateTime.TryParse(o.ToString(), CultureInfo.InstalledUICulture, DateTimeStyles.None, out i);
                if (!passed)
                    DateTime.TryParse(o.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out i);
            }
            return i;
        }
        public static DateTime? ToDateTimeNullable(object o)
        {
            DateTime i = new DateTime(1, 1, 1);
            if (!Utility.IsNull(o))
            {
                try
                {
                    if (o is DateTime)
                        return (DateTime)o;

                    bool passed = DateTime.TryParse(o.ToString(), CultureInfo.InstalledUICulture, DateTimeStyles.None, out i);
                    if (!passed)
                        DateTime.TryParse(o.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out i);
                    return i;
                }
                catch { }
            }
            return null;
        }

        public static DateTime? ToSafeDateTime(object o)
        {
            if (Utility.IsNull(o) || o.ToString() == "")
                return null;
            else
            {
                DateTime i = new DateTime(1, 1, 1);
                if (o is DateTime)
                    i = (DateTime)o;
                else
                    i = ToDateTime(o);
                if (((DateTime)o).Year == 1)
                    return null;
                return i;
            }
        }

        public static TimeSpan ToTimeSpan(object o)
        {
            TimeSpan i = new TimeSpan(0);
            if (!Utility.IsNull(o))
                TimeSpan.TryParse(o.ToString(), out i);
            return i;
        }

        public static object Nz(object o, object oReplace)
        {
            if (!Utility.IsNull(o))
                return o;
            return oReplace;
        }

        public static object TryConvert(PropertyInfo pro, object o)
        {
            string strType = pro.PropertyType.ToString();

            if (strType.Contains("Decimal")) return ToDecimal(o);
            if (strType.Contains("Double")) return ToDouble(o);
            if (strType.Contains("Int32")) return ToInt(o);
            if (strType.Contains("Boolean")) return ToBoolean(o);
            if (strType.Contains("DateTime")) return ToDateTime(o);
            if (strType.Contains("String")) return Nz(o, "").ToString();

            return o;
        }

        public static short ToShort(object o)
        {
            short i = (short)ToDouble(o);
            return i;
        }
    }
}