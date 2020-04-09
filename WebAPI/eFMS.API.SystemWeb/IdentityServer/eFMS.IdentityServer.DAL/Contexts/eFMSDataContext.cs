using eFMS.IdentityServer.Service.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.IdentityServer.Service.Contexts
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
                    });
            }
        }
    }
}
