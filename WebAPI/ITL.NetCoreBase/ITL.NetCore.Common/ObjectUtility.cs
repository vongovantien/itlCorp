using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ITL.NetCore.Common
{
    public class ObjectUtility
    {
        public static object GetValue(object Ob, string PropertyName)
        {
            if (Ob != null)
            {
                if (Ob is DataRow) return ((DataRow)Ob)[PropertyName];
                if (Ob is DataRowView) return ((DataRowView)Ob)[PropertyName];

                PropertyInfo property = Ob.GetType().GetProperty(PropertyName);
                if (property != null)
                    return property.GetValue(Ob, null);
            }
            return null;
        }

        public static void SetValue(object Ob, string PropertyName, object Value)
        {
            if (Ob is DataRow)
                ((DataRow)Ob)[PropertyName] = Value ?? DBNull.Value;
            else if (Ob is DataRowView)
                ((DataRowView)Ob)[PropertyName] = Value ?? DBNull.Value;
            else
            {
                var p = Ob.GetType().GetProperty(PropertyName);
                if (p != null)
                    p.SetValue(Ob, Value, null);
            }
        }

        public static object ExecMethod(object o, string MethodName, params object[] args)
        {
            if (o != null)
            {
                return args != null
                           ? o.GetType().InvokeMember(MethodName, BindingFlags.InvokeMethod, null, o, args)
                           : o.GetType().InvokeMember(MethodName, BindingFlags.InvokeMethod, null, o, null);
            }
            return null;
        }

        public static object ExecFunction(object Fu, params object[] args)
        {
            if (Fu != null)
            {
                return args != null ? ((Delegate)Fu).DynamicInvoke(args) : ((Delegate)Fu).DynamicInvoke();
            }
            return null;
        }

        public static void AssigneValues(object ObSource, object ObDestination)
        {
            if (ObSource != null && ObDestination != null)
            {
                var prs = ObSource.GetType().GetProperties();
                foreach (var pr in prs)
                    SetValue(ObDestination, pr.Name, pr.GetValue(ObSource, null));
            }
        }

        public static string ToSQLParam(object obj)
        {
            var objType = obj.GetType();
            if (objType == typeof(DateTime))
            {
                var dt = (DateTime)obj;
                int iCentury = 101;
                if (dt.TimeOfDay.TotalSeconds > 0)
                {
                    iCentury = 120;
                }
                return String.Format("CONVERT(VARCHAR, {0}, {1})", obj, iCentury);
            }

            //Check unicode
            string str = obj.ToString();
            if (objType == typeof(string) && str.Any(c => c > 255))
            {
                return String.Format("N'{0}'", str);
            }
            return String.Format("'{0}'", str);
        }


        public static T TrimAllString<T>(T entity)
        {
            if (entity == null)
                return entity;

            T rs = entity;
            foreach (var prop in rs.GetType().GetProperties())
            {
                var val = prop.GetValue(rs);
                if (val == null)
                    continue;
                if (val.GetType() == typeof(string))
                {
                    SetValue(rs, prop.Name, val.ToString().Trim());
                }
            }

            return rs;
        }


        /// <summary>
        /// Trim all property in object when it's a string type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static T TrimByProperties<T>(T entity, string[] properties)
        {
            if (entity == null || properties == null)
                return entity;

            T rs = entity;
            foreach (var prop in properties)
            {
                var val = GetValue(entity, prop);
                if (val == null)
                {
                    continue;
                }

                if (val.GetType() == typeof(string))
                {
                    SetValue(rs, prop, val.ToString().Trim());
                }
            }

            return rs;
        }

    }
}