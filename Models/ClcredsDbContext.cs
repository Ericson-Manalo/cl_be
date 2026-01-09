using System;
using System.Collections.Generic;
using cl_be.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace cl_be.Models;

public partial class ClcredsDbContext : DbContext
{
    public ClcredsDbContext()
    {
    }

    public ClcredsDbContext(DbContextOptions<ClcredsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<UserLogin> UserLogins { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<LogErrorActivities> LogErrorActivities { get; set; }

  //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
  //        => optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=CLCredsDb;Trusted_Connection=True;TrustServerCertificate=True;");

  protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserLogin>(entity =>
        {
            entity.HasKey(e => e.CustomerId);

            entity.Property(e => e.CustomerId)
                .ValueGeneratedNever()
                .HasColumnName("CustomerID");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.PasswordSalt)
                .HasMaxLength(10)
                .IsUnicode(false);
        });
        modelBuilder.Entity<LogErrorActivities>(entity =>
        {
          entity.HasKey(e => e.LogId);

          entity.Property(e => e.LogId)
              .ValueGeneratedOnAdd()
              .HasColumnName("LogId");

          entity.Property(e => e.Timestamp)
              .HasDefaultValueSql("SYSUTCDATETIME()");

          entity.Property(e => e.CustomerID)
              .IsRequired(false); // Nullable per utenti anonimi

          entity.Property(e => e.Email)
              .HasMaxLength(50)
              .IsRequired(false);

          entity.Property(e => e.Role)
              .IsRequired();


          entity.Property(e => e.IpAddress)
              .HasMaxLength(50)
              .IsRequired(false);

          entity.Property(e => e.UserAgent)
              .HasMaxLength(250)
              .IsRequired(false);

          entity.Property(e => e.EventType)
              .HasMaxLength(100)
              .IsRequired();

          entity.Property(e => e.EventSource)
              .HasMaxLength(150)
              .IsRequired(false);

          entity.Property(e => e.ActionName)
              .HasMaxLength(150)
              .IsRequired(false);

          entity.Property(e => e.Message)
              .HasMaxLength(500)
              .IsRequired();

          entity.Property(e => e.ExceptionDetail)
              .IsRequired(false);

          entity.Property(e => e.RequestData)
              .IsRequired(false);

          entity.Property(e => e.Url)
              .HasMaxLength(250)
              .IsRequired(false);

          // Relazione opzionale con UserLogin
          entity.HasOne<UserLogin>()
                .WithMany() // Un utente può avere molte log
                .HasForeignKey(e => e.CustomerID)
                .HasConstraintName("FK_Log_User")
                .IsRequired(false); // Nullable per anonimi
        });


    OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
