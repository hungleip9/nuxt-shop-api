using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace nuxt_shop.Models;

public partial class NuxtShopApiDbContext : DbContext
{
    public NuxtShopApiDbContext()
    {
    }

    public NuxtShopApiDbContext(DbContextOptions<NuxtShopApiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TokenLogin> TokenLogins { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserLog> UserLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=192.168.2.117;Database=NuxtShopApi;User Id=sa;Password=hungle;Trusted_Connection=False;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TokenLogin>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("TokenLogin");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("userId");
            entity.Property(e => e.AccessToken).HasColumnName("accessToken");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(200)
                .HasColumnName("refreshToken");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IdNumber).HasMaxLength(50);
            entity.Property(e => e.Otp).HasMaxLength(50);
            entity.Property(e => e.OtpExpired).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Photo).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(150);
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UserLog>(entity =>
        {
            entity.ToTable("UserLog");

            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.ActionDate).HasColumnType("datetime");
            entity.Property(e => e.IpAddress).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
