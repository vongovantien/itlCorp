using eFMS.API.Common.Helpers;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.NoSql;
using eFMS.ConsoleService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace eFMS.ConsoleService.Contexts
{
    class eFMSDataContext: eFMSDataContextDefault
    {
        private readonly ConnectionStrings _connectionStrings;
        private ConnectionStrings connectionStrings;

        public eFMSDataContext(ConnectionStrings connectionStrings)
        {
            this.connectionStrings = connectionStrings;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                
                optionsBuilder.UseSqlServer(connectionStrings.eFMSConnection,
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
                var mongoDb = MongoDbHelper.GetDatabase(connectionStrings.MongoConnection);
                var modifiedList = ChangeTrackerHelper.GetChangModifield(entities);
                var addedList = ChangeTrackerHelper.GetAdded(entities);
                var deletedList = ChangeTrackerHelper.GetDeleted(entities);
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
                Console.WriteLine(log);
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
