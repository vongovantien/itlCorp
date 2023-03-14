using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.NoSql;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;

namespace eFMS.API.Accounting.Service.Contexts
{
    public class eFMSDataContext : eFMSDataContextDefault
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(DbHelper.DbHelper.ConnectionString,
                    options =>
                    {
                        options.UseRowNumberForPaging();
                    })
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging();
            }
        }
        public override int SaveChanges()
        {
            int result = -1;
            try
            {
                var entities = ChangeTracker.Entries();
                var mongoDb = MongoDbHelper.GetDatabase(DbHelper.DbHelper.MongoDBConnectionString);
                var modifiedList = ChangeTrackerHelper.GetChangModifield(entities);
                var addedList = ChangeTrackerHelper.GetAdded(entities);
                var deletedList = ChangeTrackerHelper.GetDeleted(entities);
                if (addedList != null)
                {
                    if (addedList != null)
                    {
                        ChangeTrackerHelper.InsertToMongoDb(addedList);
                    }
                    if (modifiedList != null)
                    {
                        ChangeTrackerHelper.InsertToMongoDb(modifiedList);
                    }
                    if (deletedList != null)
                    {
                        ChangeTrackerHelper.InsertToMongoDb(deletedList);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                ResponseExModel log = new ResponseExModel
                {
                    Code = 500,
                    Message = ex.Message?.ToString(),
                    Exception = ex.InnerException?.Message?.ToString(),
                    Success = false,
                    Source = ex.Source,
                    Name = ex.GetType().Name,
                    Body = null,
                    Path = null,
                };
                new LogHelper("SaveChangesError", JsonConvert.SerializeObject(log));
                throw;
            }
            finally
            {
                result = base.SaveChanges();
            }
            return result;
        }
    }
}
