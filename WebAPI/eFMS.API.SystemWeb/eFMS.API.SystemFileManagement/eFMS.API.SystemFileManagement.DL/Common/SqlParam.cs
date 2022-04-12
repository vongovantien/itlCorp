using System;
using System.Data;
using System.Data.SqlClient;

namespace eFMS.API.SystemFileManagement.DL.Common
{
    public static class SqlParam
    {
        public static SqlParameter GetParameter(string parameterName, object value)
        {
            try
            {
                var dt = (DateTime)value;
                if (dt.Year == 1)
                {
                    value = null;
                }
            }
            catch
            {

            }
            object val = value ?? DBNull.Value;
            return new SqlParameter(parameterName, val);
        }

        public static SqlParameter GetParameter(string parameterName, object value, SqlDbType dbType)
        {
            var parameter = GetParameter(parameterName, value);
            parameter.SqlDbType = dbType;

            return parameter;
        }
    }
}
