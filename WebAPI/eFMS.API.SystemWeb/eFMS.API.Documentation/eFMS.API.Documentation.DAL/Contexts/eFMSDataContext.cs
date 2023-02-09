using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Infrastructure.NoSql;
using Microsoft.EntityFrameworkCore;
using System;

namespace eFMS.API.Documentation.Service.Contexts
{
    public class eFMSDataContext: eFMSDataContextDefault
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
            try
            {
                var entities = ChangeTracker.Entries();
                var mongoDb = MongoDbHelper.GetDatabase(DbHelper.DbHelper.MongoDBConnectionString);
                var modifiedList = ChangeTrackerHelper.GetChangModifield(entities);
                var addedList = ChangeTrackerHelper.GetAdded(entities);
                var deletedList = ChangeTrackerHelper.GetDeleted(entities);
                var result = base.SaveChanges();
                if (result > 0)
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
                new LogHelper("SaveChangesError", ex.Message?.ToString());
                throw;
            }
            
        }
    }
}
