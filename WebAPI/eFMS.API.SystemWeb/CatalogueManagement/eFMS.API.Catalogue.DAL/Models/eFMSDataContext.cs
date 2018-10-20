using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class eFMSDataContext : DbContext
    {
        public eFMSDataContext()
        {
        }

        public eFMSDataContext(DbContextOptions<eFMSDataContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CatCountry> CatCountry { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=192.168.7.88;Database=eFMSTest;User ID=sa;Password=P@ssw0rd;",
                    options =>
                    {
                        options.UseRowNumberForPaging();
                    });
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CatCountry>(entity =>
            {
                entity.ToTable("catCountry");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DatetimeCreated).HasColumnType("smalldatetime");

                entity.Property(e => e.DatetimeModified).HasColumnType("smalldatetime");

                entity.Property(e => e.InactiveOn).HasColumnType("smalldatetime");

                entity.Property(e => e.NameEn)
                    .HasColumnName("Name_EN")
                    .HasMaxLength(300);

                entity.Property(e => e.NameVn)
                    .HasColumnName("Name_VN")
                    .HasMaxLength(300);

                entity.Property(e => e.UserCreated)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserModified)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });
            
        }
    }
}
