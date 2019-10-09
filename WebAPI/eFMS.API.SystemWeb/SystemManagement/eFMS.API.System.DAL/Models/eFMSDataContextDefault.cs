using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace eFMS.API.System.Service.Models
{
    public partial class eFMSDataContextDefault : DbContext
    {
        public eFMSDataContextDefault()
        {
        }

        public eFMSDataContextDefault(DbContextOptions<eFMSDataContextDefault> options)
            : base(options)
        {
        }

        public virtual DbSet<CatDepartment> CatDepartment { get; set; }
        public virtual DbSet<CatPlace> CatPlace { get; set; }
        public virtual DbSet<SysCompany> SysCompany { get; set; }
        public virtual DbSet<SysEmployee> SysEmployee { get; set; }
        public virtual DbSet<SysGroup> SysGroup { get; set; }
        public virtual DbSet<SysGroupRole> SysGroupRole { get; set; }
        public virtual DbSet<SysMenu> SysMenu { get; set; }
        public virtual DbSet<SysOffice> SysOffice { get; set; }
        public virtual DbSet<SysRole> SysRole { get; set; }
        public virtual DbSet<SysRoleMenu> SysRoleMenu { get; set; }
        public virtual DbSet<SysRolePermission> SysRolePermission { get; set; }
        public virtual DbSet<SysUser> SysUser { get; set; }
        public virtual DbSet<SysUserGroup> SysUserGroup { get; set; }
        public virtual DbSet<SysUserRole> SysUserRole { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=192.168.7.31; Database=eFMSTest; User ID=sa; Password=P@ssw0rd");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity<CatDepartment>(entity =>
            {
                entity.ToTable("catDepartment");

                entity.HasIndex(e => e.Code)
                    .HasName("U_catDepartment_Code")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.BranchId).HasColumnName("BranchID");

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DeptName).HasMaxLength(1600);

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.ManagerId)
                    .HasColumnName("ManagerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.CatDepartment)
                    .HasForeignKey(d => d.BranchId)
                    .HasConstraintName("FK_catDepartment_sysBranch");
            });

            modelBuilder.Entity<CatPlace>(entity =>
            {
                entity.ToTable("catPlace");

                entity.HasIndex(e => new { e.Code, e.PlaceTypeId })
                    .HasName("U_Place_Code")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Address).HasMaxLength(1600);

                entity.Property(e => e.AreaId)
                    .HasColumnName("AreaID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DisplayName).HasMaxLength(4000);

                entity.Property(e => e.DistrictId).HasColumnName("DistrictID");

                entity.Property(e => e.GeoCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.LocalAreaId)
                    .HasColumnName("LocalAreaID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModeOfTransport)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.Note).HasMaxLength(4000);

                entity.Property(e => e.PlaceTypeId)
                    .HasColumnName("PlaceTypeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProvinceId).HasColumnName("ProvinceID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysCompany>(entity =>
            {
                entity.ToTable("sysCompany");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.AccountName).HasMaxLength(4000);

                entity.Property(e => e.AccountNoOverSea)
                    .HasColumnName("AccountNo_OverSea")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AccountNoVn)
                    .HasColumnName("AccountNo_VN")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.AddressEn)
                    .HasColumnName("Address_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.AddressVn)
                    .HasColumnName("Address_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.AreaId)
                    .HasColumnName("AreaID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BankAddress).HasMaxLength(4000);

                entity.Property(e => e.BankName).HasMaxLength(4000);

                entity.Property(e => e.BunameAbbr)
                    .HasColumnName("BUName_ABBR")
                    .HasMaxLength(50);

                entity.Property(e => e.BunameEn)
                    .HasColumnName("BUName_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.BunameVn)
                    .HasColumnName("BUName_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DescriptionEn)
                    .HasColumnName("Description_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.DescriptionVn)
                    .HasColumnName("Description_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.Email).HasMaxLength(4000);

                entity.Property(e => e.Fax).HasMaxLength(1600);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Logo).HasColumnType("image");

                entity.Property(e => e.LogoPath).IsUnicode(false);

                entity.Property(e => e.ManagerId)
                    .HasColumnName("ManagerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Notes).HasMaxLength(4000);

                entity.Property(e => e.Tax)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TaxAccount).HasMaxLength(1600);

                entity.Property(e => e.Taxcode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Tel)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Website).HasMaxLength(1600);
            });

            modelBuilder.Entity<SysEmployee>(entity =>
            {
                entity.ToTable("sysEmployee");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.AccessDescription).HasMaxLength(1600);

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Birthday).HasColumnType("datetime");

                entity.Property(e => e.Bonus).HasColumnType("decimal(10, 4)");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DepartmentId)
                    .HasColumnName("DepartmentID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.EmpPhotoSize).HasMaxLength(4000);

                entity.Property(e => e.EmployeeNameEn)
                    .HasColumnName("EmployeeName_EN")
                    .HasMaxLength(1600);

                entity.Property(e => e.EmployeeNameVn)
                    .HasColumnName("EmployeeName_VN")
                    .HasMaxLength(1600);

                entity.Property(e => e.ExtNo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.HomeAddress).HasMaxLength(4000);

                entity.Property(e => e.HomePhone)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Photo).HasColumnType("image");

                entity.Property(e => e.Position).HasMaxLength(1600);

                entity.Property(e => e.SaleResource)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleTarget).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Signature).HasColumnType("image");

                entity.Property(e => e.Tel)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<SysGroup>(entity =>
            {
                entity.ToTable("sysGroup");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Decription).HasMaxLength(4000);

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.ManagerId)
                    .HasColumnName("ManagerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(3200);

                entity.Property(e => e.ParentId).HasColumnName("ParentID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.SysGroup)
                    .HasForeignKey(d => d.DepartmentId)
                    .HasConstraintName("FK_sysGroup_catDepartment");
            });

            modelBuilder.Entity<SysGroupRole>(entity =>
            {
                entity.ToTable("sysGroupRole");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.SysGroupRole)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysGroupRole_sysGroup");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.SysGroupRole)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysGroupRole_sysRole");
            });

            modelBuilder.Entity<SysMenu>(entity =>
            {
                entity.ToTable("sysMenu");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Arguments).HasMaxLength(4000);

                entity.Property(e => e.AssemplyName).HasMaxLength(4000);

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.Icon).HasMaxLength(3200);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("NameEN")
                    .HasMaxLength(4000);

                entity.Property(e => e.NameVn)
                    .HasColumnName("NameVN")
                    .HasMaxLength(4000);

                entity.Property(e => e.ParentId)
                    .HasColumnName("ParentID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_sysMenu_sysParentMenu");
            });

            modelBuilder.Entity<SysOffice>(entity =>
            {
                entity.ToTable("sysOffice");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.AddressEn)
                    .HasColumnName("Address_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.AddressVn)
                    .HasColumnName("Address_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.BankAccountUsd)
                    .HasColumnName("BankAccount_USD")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.BankAccountVnd)
                    .HasColumnName("BankAccount_VND")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.BankAddress).HasMaxLength(4000);

                entity.Property(e => e.BankName).HasMaxLength(4000);

                entity.Property(e => e.BranchNameEn)
                    .HasColumnName("BranchName_EN")
                    .HasMaxLength(4000);

                entity.Property(e => e.BranchNameVn)
                    .HasColumnName("BranchName_VN")
                    .HasMaxLength(4000);

                entity.Property(e => e.Buid).HasColumnName("BUID");

                entity.Property(e => e.Code).HasMaxLength(640);

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Fax)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Logo).HasColumnType("image");

                entity.Property(e => e.ManagerId)
                    .HasColumnName("ManagerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShortName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SwiftCode)
                    .HasMaxLength(400)
                    .IsUnicode(false);

                entity.Property(e => e.Taxcode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Tel)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Website)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Bu)
                    .WithMany(p => p.SysOffice)
                    .HasForeignKey(d => d.Buid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysBranch_sysBU");
            });

            modelBuilder.Entity<SysRole>(entity =>
            {
                entity.ToTable("sysRole");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.Inactive).HasDefaultValueSql("((1))");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(3200);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysRoleMenu>(entity =>
            {
                entity.ToTable("sysRoleMenu");

                entity.HasIndex(e => new { e.RoleId, e.MenuId })
                    .HasName("U_RoleMenu")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.AllowAccess).HasDefaultValueSql("((1))");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.MenuId)
                    .HasColumnName("MenuID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.SysRoleMenu)
                    .HasForeignKey(d => d.MenuId)
                    .HasConstraintName("FK_RoleMenu_Menu");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.SysRoleMenu)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_sysRoleMenu_sysRole");
            });

            modelBuilder.Entity<SysRolePermission>(entity =>
            {
                entity.ToTable("sysRolePermission");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.OtherIntructionId).HasColumnName("OtherIntructionID");

                entity.Property(e => e.PermissionId).HasColumnName("PermissionID");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.SysRolePermission)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysRolePermission_sysRole");
            });

            modelBuilder.Entity<SysUser>(entity =>
            {
                entity.ToTable("sysUser");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EmployeeId)
                    .HasColumnName("EmployeeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.Password).HasMaxLength(4000);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");
            });

            modelBuilder.Entity<SysUserGroup>(entity =>
            {
                entity.ToTable("sysUserGroup");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasColumnName("UserID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.SysUserGroup)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysUserGroup_sysGroup");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SysUserGroup)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_sysUserGroup_sysUser");
            });

            modelBuilder.Entity<SysUserRole>(entity =>
            {
                entity.ToTable("sysUserRole");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.BranchId)
                    .IsRequired()
                    .HasColumnName("BranchID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Buid).HasColumnName("BUID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasColumnName("UserID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });
        }
    }
}
