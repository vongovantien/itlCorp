using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.NoSql;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.Service.Contexts
{
    public partial class eFMSDataContext: eFMSDataContextDefault
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
            var mongoDb = MongoDbHelper.GetDatabase();
            var modifiedList = ChangeTrackerHelper.GetChangModifield(entities);
            var addedList = ChangeTrackerHelper.GetAdded(entities);
            var deletedList = ChangeTrackerHelper.GetDeleted(entities);
            var result = base.SaveChanges();
            if (result > 0)
            {
                if (addedList != null)
                {
                    ChangeTrackerHelper.InsertToMongoDb(addedList, EntityState.Added);
                }
                if (modifiedList != null)
                {
                    ChangeTrackerHelper.InsertToMongoDb(modifiedList, EntityState.Modified);
                }
                if (deletedList != null)
                {
                    ChangeTrackerHelper.InsertToMongoDb(deletedList, EntityState.Deleted);
                }
            }
            return result;
        }
    }
}
