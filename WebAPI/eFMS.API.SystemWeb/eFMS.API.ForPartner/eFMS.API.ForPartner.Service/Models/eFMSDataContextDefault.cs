using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace eFMS.API.ForPartner.Service.Models
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

        public virtual DbSet<AccAccountingManagement> AccAccountingManagement { get; set; }
        public virtual DbSet<AcctAdvancePayment> AcctAdvancePayment { get; set; }
        public virtual DbSet<SysPartnerApi> SysPartnerApi { get; set; }
        public virtual DbSet<SysUser> SysUser { get; set; }

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
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<AccAccountingManagement>(entity =>
            {
                entity.ToTable("accAccountingManagement");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountNo).HasMaxLength(250);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.Currency)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PartnerId)
                    .IsRequired()
                    .HasColumnName("PartnerID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentDatetimeUpdated).HasColumnType("datetime");

                entity.Property(e => e.PaymentDueDate).HasColumnType("datetime");

                entity.Property(e => e.PaymentMethod).HasMaxLength(50);

                entity.Property(e => e.PaymentNote).HasMaxLength(500);

                entity.Property(e => e.PaymentStatus).HasMaxLength(50);

                entity.Property(e => e.PaymentTerm).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ServiceType).HasMaxLength(10);

                entity.Property(e => e.SourceCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalExchangeRate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.UnpaidAmount).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VoucherId).HasColumnName("VoucherID");

                entity.Property(e => e.VoucherType).HasMaxLength(50);
            });

            modelBuilder.Entity<AcctAdvancePayment>(entity =>
            {
                entity.ToTable("acctAdvancePayment");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.AdvanceCurrency).HasMaxLength(10);

                entity.Property(e => e.AdvanceNo)
                    .IsRequired()
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.DeadlinePayment).HasColumnType("date");

                entity.Property(e => e.Department)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.GroupId).HasColumnName("GroupID");

                entity.Property(e => e.OfficeId).HasColumnName("OfficeID");

                entity.Property(e => e.PaymentMethod)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentTerm).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.RequestDate).HasColumnType("date");

                entity.Property(e => e.Requester)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StatusApproval)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VoucherDate).HasColumnType("datetime");

                entity.Property(e => e.VoucherNo)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SysPartnerApi>(entity =>
            {
                entity.ToTable("sysPartnerAPI");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Company).HasMaxLength(100);

                entity.Property(e => e.DatetimeCreated).HasColumnType("datetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(10);

                entity.Property(e => e.Environment).HasMaxLength(150);

                entity.Property(e => e.ExpiredDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            modelBuilder.Entity<SysUser>(entity =>
            {
                entity.ToTable("sysUser");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.CreditLimit).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CreditRate).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.DatetimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DatetimeModified)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.EmployeeId)
                    .HasColumnName("EmployeeID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InactiveOn).HasColumnType("datetime");

                entity.Property(e => e.IsLdap).HasColumnName("IsLDAP");

                entity.Property(e => e.Password).HasMaxLength(4000);

                entity.Property(e => e.PasswordLdap).HasColumnName("PasswordLDAP");

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkPlaceId).HasColumnName("WorkPlaceID");

                entity.Property(e => e.WorkingStatus)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });
        }
    }
}
