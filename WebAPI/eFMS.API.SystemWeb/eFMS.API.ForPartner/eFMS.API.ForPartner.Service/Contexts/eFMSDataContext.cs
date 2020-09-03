using eFMS.API.ForPartner.Service.Models;
using eFMS.API.Infrastructure.NoSql;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.ForPartner.Service.Contexts
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
                    });
            }
        }
        public override int SaveChanges()
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
    }
}
