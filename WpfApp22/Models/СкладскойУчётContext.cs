using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WpfApp22.Models;

namespace WpfApp22.Models;

public partial class СкладскойУчётContext : DbContext
{
    public СкладскойУчётContext()
    {
    }

    public СкладскойУчётContext(DbContextOptions<СкладскойУчётContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Поставщики> Поставщики { get; set; } = null!;
    public virtual DbSet<Приход> Приход { get; set; } = null!;
    public virtual DbSet<Расход> Расход { get; set; } = null!;
    public virtual DbSet<Склады> Склады { get; set; } = null!;
    public virtual DbSet<Товары> Товары { get; set; } = null!;
    public virtual DbSet<Остатки> Остатки { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#pragma warning disable CS8604
        => optionsBuilder.UseSqlServer("Server=VANYACOMP;Database=Складской учёт;User Id=413_11;Password=надежныйпароль;TrustServerCertificate=True;");
#pragma warning restore CS8604

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Приход>(entity =>
        {
            entity.Property(e => e.Дата).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Поставщик).WithMany(p => p.Приход)
                .HasForeignKey(d => d.ПоставщикId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Приход_Поставщики");

            entity.HasOne(d => d.Склад).WithMany(p => p.Приход)
                .HasForeignKey(d => d.СкладId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Приход_Склады");

            entity.HasOne(d => d.Товар).WithMany(p => p.Приход)
                .HasForeignKey(d => d.ТоварId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Приход_Товары");
        });

        modelBuilder.Entity<Расход>(entity =>
        {
            entity.Property(e => e.Дата).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Склад).WithMany(p => p.Расход)
                .HasForeignKey(d => d.СкладId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Расход_Склады");

            entity.HasOne(d => d.Товар).WithMany(p => p.Расход)
                .HasForeignKey(d => d.ТоварId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Расход_Товары");
        });

        modelBuilder.Entity<Остатки>(entity =>
        {

            entity.HasIndex(e => new { e.ТоварId, e.СкладId }, "UQ_Товар_Склад").IsUnique();

            entity.HasCheckConstraint("CHK_Остатки", "[Количество] >= 0 AND [В_Резерве] >= 0");

            entity.HasOne(d => d.Склад).WithMany(p => p.Остатки)
                .HasForeignKey(d => d.СкладId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Остатки_Склады");

            entity.HasOne(d => d.Товар).WithMany(p => p.Остатки)
                .HasForeignKey(d => d.ТоварId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Остатки_Товары");

            entity.Property(e => e.Количество).HasDefaultValue(0);
            entity.Property(e => e.В_Резерве).HasDefaultValue(0);
        });

        modelBuilder.Entity<Склады>(entity =>
        {
            entity.HasIndex(e => e.Название, "IX_Склады_Название");
        });

        modelBuilder.Entity<Товары>(entity =>
        {
            entity.HasIndex(e => e.Артикул, "IX_Товары_Артикул").IsUnique();
            entity.HasIndex(e => e.Название, "IX_Товары_Название");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}