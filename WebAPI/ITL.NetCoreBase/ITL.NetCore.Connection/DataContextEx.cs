using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using System.Collections;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ITL.NetCore.Connection
{
    public static class DataContextEx
    {
        public static IQueryable<T> Paging<T, TResult>(this IQueryable<T> query, int page, int size, Expression<Func<T, TResult>> orderByProperty, bool isAscendingOrder, out int rowsCount)
        {
            rowsCount = query.Count();
            if (rowsCount <= size || page < 1) page = 1;
            if (orderByProperty != null)
                query = isAscendingOrder
                            ? query.OrderBy(orderByProperty)
                            : query.OrderByDescending(orderByProperty);
            var excludedRows = (page - 1) * size;
            return query.Skip(excludedRows).Take(size);
        }

        public static async Task<(List<T> result, int rowsCount)> PagingAsync<T, TResult>(this IQueryable<T> query, int page, int size, Expression<Func<T, TResult>> orderByProperty, bool isAscendingOrder)
        {
            int count = query.Count();
            if (count <= size || page < 1) page = 1;
            if (orderByProperty != null)
                query = isAscendingOrder
                            ? query.OrderBy(orderByProperty)
                            : query.OrderByDescending(orderByProperty);
            var excludedRows = (page - 1) * size;
            return (result: await query.Skip(excludedRows).Take(size).ToListAsync(), rowsCount: count);
        }

        public static IList ToList(IQueryable query)
        {
            return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(query.ElementType), query);
        }

        public static XElement ToXML<T>(this IList<T> lstToConvert, Func<T, bool> filter, string rootName)
        {
            var lstConvert = (filter == null) ? lstToConvert : lstToConvert.Where(filter);
            return new XElement(rootName,
               (from node in lstConvert
                select new XElement(typeof(T).ToString(),
                from subnode in node.GetType().GetProperties()
                select new XElement(subnode.Name, subnode.GetValue(node, null)))));
        }
        public static List<T> GetViewData<T>(this DbContext context) where T : new()
        {
            return ExecuteAll<T>(context, typeof(T).Name, CommandTypeEx.View, null);
        }

        public static object ExecuteFuncScalar(this DbContext context, string strName, params DbParameter[] parms)
        {
            if (parms != null)
            {
                foreach(var param in parms)
                {
                    if (param.Value == null)
                        param.Value = DBNull.Value;
                }
                if (parms.Any(chk => chk.Value != DBNull.Value && !chk.Value.ToString().IsSafeSQL()))
                    return null;
            }

            string strParams = String.Join(',', parms.Select(p => p.ParameterName));
            string strQuery = String.Format("SELECT {0}({1})", strName, strParams);

            try
            {
                context.Database.OpenConnection();
                DbCommand cmd = context.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = strQuery;
                cmd.CommandType = CommandType.Text;
                if (parms != null)
                    cmd.Parameters.AddRange(parms);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader != null && reader.HasRows && reader.FieldCount == 1)
                    {
                        while (reader.Read())
                        {
                            return reader.GetValue(0);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                context.Database.CloseConnection();
            }

            return null;
        }

        public static List<T> ExecuteFuncTable<T>(this DbContext context, params DbParameter[] parms) where T : new()
        {
            return ExecuteAll<T>(context, typeof(T).Name, CommandTypeEx.TableValue, parms);
        }
        public static List<T> ExecuteFuncTable<T>(this DbContext context,string FunctionName, params DbParameter[] parms) where T : new()
        {
            return ExecuteAll<T>(context, FunctionName, CommandTypeEx.TableValue, parms);
        }
        public static List<T> ExecuteProcedure<T>(this DbContext context, params DbParameter[] parms) where T : new()
        {
            return ExecuteAll<T>(context, typeof(T).Name, CommandTypeEx.StoredProcedure, parms);
        }
        
        public static int ExecuteProcedure(this DbContext context, string ProcedureName, params object[] parmas)
        {
            try
            {
                context.Database.OpenConnection();
                DbCommand cmd = context.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = ProcedureName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(parmas);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                context.Database.CloseConnection();
            }
        }

        public static List<T> GetQuery<T>(this DbContext context, string strSQL) where T:new()
        {
            return ExecuteAll<T>(context, strSQL, CommandTypeEx.Text, null);
        }

        private static List<T> ExecuteAll<T>(this DbContext context, string procOrFunc, CommandTypeEx cmdType, params DbParameter[] parmas) where T : new()
        {
            List<T> lstResult = new List<T>();

            //Check SQL Injection
            if(parmas != null)
            {
                foreach (var param in parmas)
                {
                    if (param.Value == null)
                        param.Value = DBNull.Value;
                }

                if (parmas.Any(chk => chk.Value != DBNull.Value && !chk.Value.ToString().IsSafeSQL()))
                    return lstResult;
            }                

            CommandType commandType = CommandType.Text;
            string strQuery = procOrFunc;

            if ((short)cmdType == (short)CommandType.StoredProcedure)
            {
                commandType = CommandType.StoredProcedure;
            }
            else if (cmdType == CommandTypeEx.ScalarValue || cmdType == CommandTypeEx.TableValue)
            {
                string strParams = String.Join(',', parmas.Select(p => p.ParameterName));
                if (cmdType == CommandTypeEx.ScalarValue)
                    strQuery = String.Format("SELECT {0}({1})", procOrFunc, strParams);
                if (cmdType == CommandTypeEx.TableValue)
                    strQuery = String.Format("SELECT * FROM {0}({1})", procOrFunc, strParams);
            }
            else if (cmdType == CommandTypeEx.View)
            {
                string strCondition = "";
                if (parmas != null)
                    strCondition = String.Join(' ', parmas.Select(sl => String.Format("AND {0}={1}", sl.ParameterName.Replace("@", ""), sl.ParameterName)));
                strQuery = String.Format("SELECT * FROM {0} WHERE 1=1 {1}", procOrFunc, strCondition);
            }

            try
            {
                context.Database.OpenConnection();
                DbCommand cmd = context.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = strQuery;
                cmd.CommandType = commandType;
                if(parmas != null)
                    cmd.Parameters.AddRange(parmas);
                using (var reader = cmd.ExecuteReader())
                {
                    lstResult = reader.ToList<T>();
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                context.Database.CloseConnection();
            }

            return lstResult;
        }


        #region comment fucntions
        ///// <summary>
        ///// Get data by execution a sql query. It maybe a store procedure or function. This return T object list
        ///// </summary>
        ///// <typeparam name="T"></typeparam>       
        //public static List<T> GetData<T>(this DbContext context, string sqlString, CommandType cmdType, params DbParameter[] param)
        //{
        //    try
        //    {
        //        using (var cmd = context.Database.GetDbConnection().CreateCommand())
        //        {
        //            cmd.CommandText = sqlString;
        //            cmd.CommandType = cmdType;
        //            cmd.Parameters.AddRange(param);
        //            return cmd.GetData<T>();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}
        ///// <summary>
        ///// Get data by execution a sql query. It maybe a store procedure or function. This return one value
        ///// </summary>
        ///// <typeparam name="T"></typeparam>       
        //public static object GetScalarData<T>(this DbContext context, string sqlString, CommandType cmdType, params DbParameter[] param)
        //{
        //    try
        //    {
        //        using (var cmd = context.Database.GetDbConnection().CreateCommand())
        //        {
        //            cmd.CommandText = sqlString;
        //            cmd.CommandType = cmdType;
        //            cmd.Parameters.AddRange(param);
        //            return cmd.GetScalarData();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}
        ///// <summary>
        ///// Get data by execution a sql query. It maybe a store procedure or function. This return one value
        ///// </summary>
        ///// <typeparam name="T"></typeparam>       
        //public static async Task<object> GetScalarDataAsync(this DbContext context, string sqlString, CommandType cmdType, params DbParameter[] param)
        //{
        //    try
        //    {
        //        using (var cmd = context.Database.GetDbConnection().CreateCommand())
        //        {
        //            cmd.CommandText = sqlString;
        //            cmd.CommandType = cmdType;
        //            cmd.Parameters.AddRange(param);
        //            return await cmd.GetScalarDataAsync();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}
        ///// <summary>
        ///// A asynchronous function of getting data by execution a sql query. It maybe a store procedure or function. This return T object list
        ///// </summary>
        ///// <typeparam name="T"></typeparam>       
        //public static async Task<List<T>> GetDataAsync<T>(this DbContext context, string sqlString, CommandType cmdType, params DbParameter[] param)
        //{
        //    try
        //    {
        //        using (var cmd = context.Database.GetDbConnection().CreateCommand())
        //        {
        //            cmd.CommandText = sqlString;
        //            cmd.CommandType = cmdType;
        //            cmd.Parameters.AddRange(param);
        //            return await cmd.GetDataAsync<T>();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}       
        ///// <summary>
        ///// Get data by execution a sql query. It maybe a store procedure or function. This return T object list
        ///// </summary> 
        //private static List<T> GetData<T>(this DbCommand command)
        //{
        //    using (command)
        //    {
        //        if (command.Connection.State == ConnectionState.Closed)
        //            command.Connection.Open();
        //        try
        //        {
        //            using (var reader = command.ExecuteReader())
        //            {                       
        //                return reader.ToList<T>();
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            throw (e);
        //        }
        //        finally
        //        {
        //            command.Connection.Close();
        //        }
        //    }
        //}
        ///// <summary>
        ///// Get data by execution a sql query. It maybe a store procedure or function. This return one value
        ///// </summary> 
        //private static object GetScalarData(this DbCommand command)
        //{
        //    using (command)
        //    {
        //        if (command.Connection.State == ConnectionState.Closed)
        //            command.Connection.Open();
        //        try
        //        {
        //            return command.ExecuteScalar();
        //        }
        //        catch (Exception e)
        //        {
        //            throw (e);
        //        }
        //        finally
        //        {
        //            command.Connection.Close();
        //        }
        //    }
        //}
        ///// <summary>
        ///// A asynchronous of getting data by execution a sql query. It maybe a store procedure or function. This return one value
        ///// </summary> 
        //private static async Task<object> GetScalarDataAsync(this DbCommand command)
        //{
        //    using (command)
        //    {
        //        if (command.Connection.State == ConnectionState.Closed)
        //            command.Connection.Open();
        //        try
        //        {
        //            return await command.ExecuteScalarAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            throw (e);
        //        }
        //        finally
        //        {
        //            command.Connection.Close();
        //        }
        //    }
        //}
        ///// <summary>
        ///// A asynchronous of getting data by execution a sql query. It maybe a store procedure or function. This return T object list
        ///// </summary> 
        //private static async Task<List<T>> GetDataAsync<T>(this DbCommand command)
        //{
        //    using (command)
        //    {
        //        if (command.Connection.State == ConnectionState.Closed)
        //            command.Connection.Open();
        //        try
        //        {
        //            using (var reader = await command.ExecuteReaderAsync())
        //            {
        //                return reader.ToList<T>();
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            throw (e);
        //        }
        //        finally
        //        {
        //            command.Connection.Close();
        //        }
        //    }
        //}

        #endregion

        /// <summary>
        /// Execute a sql query, return 1 value if executing success else return -1
        /// </summary>  
        private static async Task<int> ExcuteQuery(this DbCommand command)
        {
            using (command)
            {
                
                try
                {
                    int result = await command.ExecuteNonQueryAsync();
                    return result;
                }
                catch (Exception e)
                {
                    throw (e);
                }
                finally
                {
                    command.Connection.Close();
                }
            }
        }
        /// <summary>
        /// Execute a sql query, return 1 value if executing success else return -1
        /// </summary>  
        public static async Task<int> ExcuteQuery<T>(this DbContext context, string sqlString, CommandType cmdType, params DbParameter[] param)
        {
            try
            {
                using (var cmd = context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sqlString;
                    cmd.CommandType = cmdType;
                    cmd.Parameters.AddRange(param);
                    return await cmd.ExcuteQuery();
                }
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
        public static async Task<object> ExecuteScalarAsync(this DbContext context, string sqlString, CommandType cmdType, params DbParameter[] param)
        {
            using (var cmd = context.Database.GetDbConnection().CreateCommand())
            {
                try
                {
                    cmd.CommandText = sqlString;
                    cmd.CommandType = cmdType;
                    cmd.Parameters.AddRange(param);

                    if (cmd.Connection.State == ConnectionState.Closed)
                        cmd.Connection.Open();

                    return await cmd.ExecuteScalarAsync();
                }
                catch (Exception ex)
                {
                    return null;
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
        }

        private static List<T> ToList<T>(this DbDataReader dr)
        {
            var objList = new List<T>();
            var props = typeof(T).GetRuntimeProperties();

            var colMapping = dr.GetColumnSchema()
              .Where(x => props.Any(y => y.Name.ToLower() == x.ColumnName.ToLower()))
              .ToDictionary(key => key.ColumnName.ToLower());

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    T obj = Activator.CreateInstance<T>();
                    foreach (var prop in props)
                    {
                        var val =
                          dr.GetValue(colMapping[prop.Name.ToLower()].ColumnOrdinal.Value);
                        prop.SetValue(obj, val == DBNull.Value ? null : val);
                    }
                    objList.Add(obj);
                }
            }
            return objList;
        }        
    }

    public enum CommandTypeEx {
        /// <summary>
        /// An SQL text command.
        /// </summary>
        Text = 1,
        /// <summary>
        /// The name of a stored procedure.
        /// </summary>
        StoredProcedure = 4,
        /// <summary>
        /// The name of table-valued function.
        /// </summary>
        TableValue = 2,
        /// <summary>
        /// The name of scalar-valued function.
        /// </summary>
        ScalarValue = 3,
        /// <summary>
        /// The name of view
        /// </summary>
        View = 5
    }
}