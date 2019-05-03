using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ITL.NetCore.Connection
{
    public static class DataReaderEx
    {
        public static List<T> MapToList<T>(this DbDataReader dr) where T: new()
        {
            if(dr != null && dr.HasRows)
            {
                var entity = typeof(T);
                var entities = new List<T>();
                var propDict = new Dictionary<string, PropertyInfo>();
                var props = entity.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                propDict = props.ToDictionary(p => p.Name.ToUpper(), p => p);

                while(dr.Read())
                {
                    T newObject = new T();
                    if(dr.FieldCount == 1)
                    {
                        newObject = (T)dr.GetValue(0);
                    }else
                    {
                        for (int i = 0; i < dr.FieldCount; i++)
                        {
                            if (propDict.ContainsKey(dr.GetName(i).ToUpper()))
                            {
                                var info = propDict[dr.GetName(i).ToUpper()];
                                if (info != null && info.CanWrite)
                                {
                                    var val = dr.GetValue(i);
                                    info.SetValue(newObject, (val == DBNull.Value) ? null : val, null);
                                }
                            }
                        }
                    }                    
                    entities.Add(newObject);
                }
                return entities;
            }
            return null;
        }
    }
}
