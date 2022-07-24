using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
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

namespace eFMS.API.Documentation.DL.Services
{
    public class DatabaseUpdateService : IDatabaseUpdateService
    {
        private readonly ICurrentUser currentUser;
        private string typeApproval = "Settlement";
        private decimal _decimalNumber = Constants.DecimalNumber;
        IContextBase<sp_InsertRowToDataBase> context;
        public DatabaseUpdateService(IContextBase<sp_InsertRowToDataBase> repository) : base()
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
            try
            {
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
            catch (Exception ex)
            {
                new LogHelper("EFMS_InsertDataToDB", "Error : " + ex.ToString() + "\nAction: " + currentUser.Action + "\nValue: Table " + obj.GetType().Name.Replace("Model", "") + " Value: " + sql);
                return null;
            }
        }

        /// <summary>
        /// Log add object to mongo
        /// </summary>
        /// <param name="entity"></param>
        public void LogAddEntity(object entity)
        {
            var mongoDb = MongoDbHelper.GetDatabase(DbHelper.DbHelper.MongoDBConnectionString);
            var addedList = ChangeTrackerHelper.GetAdded((new List<object>() { entity }).AsEnumerable());
            ChangeTrackerHelper.InsertToMongoDb(addedList);
        }

        /// <summary>
        /// Insert Charges + Link charges AutoRate To DB
        /// </summary>
        /// <param name="surcharges"></param>
        /// <param name="linkCharges"></param>
        /// <returns></returns>
        public sp_InsertChargesAutoRate InsertChargesAutoRateToDB(List<CsShipmentSurcharge> surcharges, List<CsLinkCharge> linkCharges)
        {
            try
            {
                var parameters = new[]{
                new SqlParameter()
                {
                    Direction = ParameterDirection.Input,
                    ParameterName = "@CsShipmentSurcharge",
                    Value = DataHelper.ToDataTable(surcharges),
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[CsShipmentSurchargeTable]"
                },
                new SqlParameter()
                {
                    Direction = ParameterDirection.Input,
                    ParameterName = "@CslinkCharge",
                    Value = DataHelper.ToDataTable(linkCharges),
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[CsLinkChargeTable]"
                }
            };
                var hs = context.DC.ExecuteProcedure<sp_InsertChargesAutoRate>(parameters);
                LogSurchargeSettlement(null, surcharges, "Add");
                LogSurchargeSettlement(null, linkCharges, "Add");
                return hs.FirstOrDefault();
            }
            catch (Exception ex)
            {
                new LogHelper("EFMS_InsertChargesAutoRateToDB", "Error : " + ex.ToString() + "\nAction: " + currentUser.Action + "\nValue: Surcharges " + JsonConvert.SerializeObject(surcharges)
                    + " \n LinkCharges: " + JsonConvert.SerializeObject(linkCharges));
                return null;
            }
        }

        /// <summary>
        /// Log mongo when update database after SP/Function
        /// </summary>
        /// <param name="oldObjects"></param>
        /// <param name="newObjects"></param>
        /// <param name="action"></param>
        private void LogSurchargeSettlement(IEnumerable<object> oldObjects, IEnumerable<object> newObjects, string action)
        {
            var mongoDb = MongoDbHelper.GetDatabase(DbHelper.DbHelper.MongoDBConnectionString);
            var idsOld = new List<Guid>();
            if (newObjects != null && newObjects.Count() > 0)
            {
                var addList = ChangeTrackerHelper.GetAdded(newObjects);
                ChangeTrackerHelper.InsertToMongoDb(addList);
            }
            if (oldObjects != null && oldObjects.Count() > 0)
            {
                if (action != "Delete")
                {
                    var editList = ChangeTrackerHelper.GetChangeModifield(oldObjects, newObjects);
                    ChangeTrackerHelper.InsertToMongoDb(editList);
                }
                else
                {
                    var deleteList = ChangeTrackerHelper.GetDeleted(oldObjects);
                    ChangeTrackerHelper.InsertToMongoDb(deleteList);
                }
            }
        }
    }
}

