using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.Service.Contexts;
using eFMS.API.Accounting.Service.ViewModels;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace eFMS.API.Accounting.DL.Services
{
    public class DatabaseUpdateService: IDatabaseUpdateService
    {
        private readonly ICurrentUser currentUser;
        IContextBase<sp_InsertRowToDataBase> context;
        public DatabaseUpdateService(IContextBase<sp_InsertRowToDataBase> repository,
            IMapper mapper,
            ICurrentUser user) : base()
        {
            context = repository;
            currentUser = user;
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
                        values.Add("\'" + propertyInfo.GetValue(obj) + "\'");
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

