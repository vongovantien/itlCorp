using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Accounting.Service.ViewModels;
using eFMS.API.Common.Helpers;
using eFMS.API.Infrastructure.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.EF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace eFMS.API.Accounting.DL.Services
{
    public class DatabaseUpdateService: IDatabaseUpdateService
    {
        private readonly ICurrentUser currentUser;
        private IContextBase<sp_InsertRowToDataBase> context;
        public DatabaseUpdateService(IContextBase<sp_InsertRowToDataBase> repository,
            IMapper mapper,
            IContextBase<CsShipmentSurcharge> csShipmentSurcharge,
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
            var mongoDb = MongoDbHelper.GetDatabase(DbHelper.DbHelper.MongoDBConnectionString);
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

        /// <summary>
        /// Update Surcharge Data To Settlement
        /// </summary>
        /// <param name="surcharges"></param>
        /// <param name="settleCode"></param>
        /// <param name="kickBackExcRate"></param>
        /// <param name="action"></param>
        public void UpdateSurchargeSettleDataToDB(List<CsShipmentSurcharge> surcharges, string settleCode, decimal kickBackExcRate, string action)
        {
            try
            {
                var parameters = new[]{
                new SqlParameter()
                {
                    Direction = ParameterDirection.Input,
                    ParameterName = "@surcharges",
                    Value = DataHelper.ToDataTable(surcharges),
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[CsShipmentSurchargeTable]"
                },
                new SqlParameter(){ ParameterName = "@settleCode", Value = settleCode},
                new SqlParameter(){ ParameterName = "@kickBackExcRate", Value = kickBackExcRate},
                new SqlParameter(){ ParameterName = "@userCurrent", Value = currentUser.UserID},
                new SqlParameter(){ ParameterName = "@curOfficeID", Value = currentUser.OfficeID},
                new SqlParameter(){ ParameterName = "@curCompanyID", Value = currentUser.CompanyID},
                new SqlParameter(){ ParameterName = "@action", Value = action}
            };
                var hs = context.DC.ExecuteProcedure<sp_UpdateSurchargeSettlement>(parameters);
                new LogHelper("eFMS_Log_UpdateSurchargeSettle-" + settleCode, "UserModified: " + currentUser.UserID + "\nAction: " + action + "\n Charges:" + JsonConvert.SerializeObject(surcharges));
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_Log_UpdateSurchargeSettle-" + settleCode, "UserModified: " + currentUser.UserID + "\nAction: " + action + "\nError " + ex.ToString() + "\n Settle: " + settleCode + "\n Charges:" + JsonConvert.SerializeObject(surcharges));
            }
        }

        /// <summary>
        /// Update surcharge when synce settle/soa/voucher/cdnote
        /// </summary>
        /// <param name="type"></param>
        /// <param name="code"></param>
        public void UpdateSurchargeAfterSynced(string type, string code)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@type", Value = type},
                new SqlParameter(){ ParameterName = "@code", Value = code},
                new SqlParameter(){ ParameterName = "@currentUser", Value = currentUser.UserID}
            };
            var hs = context.DC.ExecuteProcedure<sp_UpdateSurchargeSynceToAccountant>(parameters);
        }
    }
}

