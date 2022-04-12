using eFMS.API.Report.Service.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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
                    });
            }
        }
    }
}
