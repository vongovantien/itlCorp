using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.Service.ViewModels;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace eFMS.API.Documentation.DL.Services
{
    public class DatabaseUpdateService: IDatabaseUpdateService
    {
        private readonly ICurrentUser currentUser;
        private string typeApproval = "Settlement";
        private decimal _decimalNumber = Constants.DecimalNumber;
        IContextBase<sp_InsertRowToDataBase> context;
        public DatabaseUpdateService(IContextBase<sp_InsertRowToDataBase> repository): base()
        {
            context = repository;
        }

        /// <summary>
        /// Insert A Row Data To DB
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public sp_InsertRowToDataBase InsertDataToDB(object obj)
        {
            var sql = "<tag/>";
            var values = new List<object>();
            var typeInfo = obj.GetType().GetTypeInfo();
            foreach (var propertyInfo in typeInfo.GetProperties())
            {
                if (propertyInfo.GetValue(obj) != null)
                {
                    if (propertyInfo.PropertyType == typeof(DateTime) || propertyInfo.PropertyType == typeof(DateTime?))
                    {
                        values.Add("\'" + DateTime.Parse(propertyInfo.GetValue(obj).ToString()).ToString("yyyy-MM-dd HH:mm:ss.fff") + "\'");
                    }
                    else if (propertyInfo.PropertyType == typeof(decimal?) || propertyInfo.PropertyType == typeof(int?))
                    {
                        values.Add(propertyInfo.GetValue(obj));
                    }
                    else
                    {
                        values.Add("N'" + propertyInfo.GetValue(obj) + "\'");
                    }
                }
                else
                {
                    values.Add("NULL");
                }
            }
            sql += string.Join("<tag>", values);
            sql += "<tag/>";
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@table", Value = obj.GetType().Name.Replace("Model", "") },
                new SqlParameter(){ ParameterName = "@values", Value = sql }
            };
            var result = context.DC.ExecuteProcedure<sp_InsertRowToDataBase>(parameters);
            return result.FirstOrDefault();
        }
    }
}

