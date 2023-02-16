using eFMS.API.Report.Service.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace eFMS.API.Report.Service.Context
{
    public class eFMSDataContext: eFMSDataContextDefault
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(DbHelper.DBHelper.ConnectionString,
                    options =>
                    {
                        options.UseRowNumberForPaging();
                        options.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
                        options.CommandTimeout(180);
                    })
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
            }
        }
    }
}
