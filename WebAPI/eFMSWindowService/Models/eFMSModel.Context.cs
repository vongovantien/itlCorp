﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace eFMSWindowService.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class eFMSTestEntities : DbContext
    {
        public eFMSTestEntities()
            : base("name=eFMSTestEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<catDepartment> catDepartments { get; set; }
        public virtual DbSet<sysSentEmailHistory> sysSentEmailHistories { get; set; }
    
        public virtual ObjectResult<Nullable<int>> sp_QueryAndUpdateCurrentStatusOfJob()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("sp_QueryAndUpdateCurrentStatusOfJob");
        }
    
        public virtual int sp_AutoUpdateExchangeRate()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_AutoUpdateExchangeRate");
        }
    
        public virtual ObjectResult<sp_GetShipmentInThreeDayToSendARDept_Result> sp_GetShipmentInThreeDayToSendARDept()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_GetShipmentInThreeDayToSendARDept_Result>("sp_GetShipmentInThreeDayToSendARDept");
        }
    }
}
