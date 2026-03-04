using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WpfApp22.Models;

#nullable disable

namespace WpfApp22.Migrations
{
    [DbContext(typeof(СкладскойУчётContext))]
    [Migration("20260226170627_Возрат")]
    partial class Возрат
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("WpfApp22.Models.Остатки", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("В_Резерве")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int>("Количество")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int>("СкладId")
                        .HasColumnType("int")
                        .HasColumnName("Склад_ID");

                    b.Property<int>("ТоварId")
                        .HasColumnType("int")
                        .HasColumnName("Товар_ID");

                    b.HasKey("Id");

                    b.HasIndex("СкладId");

                    b.HasIndex(new[] { "ТоварId", "СкладId" }, "UQ_Товар_Склад")
                        .IsUnique();

                    b.ToTable("Остатки", t =>
                        {
                            t.HasCheckConstraint("CHK_Остатки", "[Количество] >= 0 AND [В_Резерве] >= 0");
                        });
                });

            modelBuilder.Entity("WpfApp22.Models.Поставщики", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Контакты")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Название")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Поставщики");
                });

            modelBuilder.Entity("WpfApp22.Models.Приход", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("Дата")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<int>("Количество")
                        .HasColumnType("int");

                    b.Property<int>("ПоставщикId")
                        .HasColumnType("int")
                        .HasColumnName("Поставщик_ID");

                    b.Property<int>("СкладId")
                        .HasColumnType("int")
                        .HasColumnName("Склад_ID");

                    b.Property<int>("ТоварId")
                        .HasColumnType("int")
                        .HasColumnName("Товар_ID");

                    b.Property<decimal>("Цена")
                        .HasColumnType("money");

                    b.HasKey("Id");

                    b.HasIndex("ПоставщикId");

                    b.HasIndex("СкладId");

                    b.HasIndex("ТоварId");

                    b.ToTable("Приход");
                });

            modelBuilder.Entity("WpfApp22.Models.Расход", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("Дата")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<int>("Количество")
                        .HasColumnType("int");

                    b.Property<string>("Причина")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("СкладId")
                        .HasColumnType("int")
                        .HasColumnName("Склад_ID");

                    b.Property<int>("ТоварId")
                        .HasColumnType("int")
                        .HasColumnName("Товар_ID");

                    b.HasKey("Id");

                    b.HasIndex("СкладId");

                    b.HasIndex("ТоварId");

                    b.ToTable("Расход");
                });

            modelBuilder.Entity("WpfApp22.Models.Склады", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Адрес")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Название")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Название" }, "IX_Склады_Название");

                    b.ToTable("Склады");
                });

            modelBuilder.Entity("WpfApp22.Models.Товары", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Артикул")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Категория")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Название")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Артикул" }, "IX_Товары_Артикул")
                        .IsUnique()
                        .HasFilter("[Артикул] IS NOT NULL");

                    b.HasIndex(new[] { "Название" }, "IX_Товары_Название");

                    b.ToTable("Товары");
                });

            modelBuilder.Entity("WpfApp22.Models.Остатки", b =>
                {
                    b.HasOne("WpfApp22.Models.Склады", "Склад")
                        .WithMany("Остатки")
                        .HasForeignKey("СкладId")
                        .IsRequired()
                        .HasConstraintName("FK_Остатки_Склады");

                    b.HasOne("WpfApp22.Models.Товары", "Товар")
                        .WithMany("Остатки")
                        .HasForeignKey("ТоварId")
                        .IsRequired()
                        .HasConstraintName("FK_Остатки_Товары");

                    b.Navigation("Склад");

                    b.Navigation("Товар");
                });

            modelBuilder.Entity("WpfApp22.Models.Приход", b =>
                {
                    b.HasOne("WpfApp22.Models.Поставщики", "Поставщик")
                        .WithMany("Приход")
                        .HasForeignKey("ПоставщикId")
                        .IsRequired()
                        .HasConstraintName("FK_Приход_Поставщики");

                    b.HasOne("WpfApp22.Models.Склады", "Склад")
                        .WithMany("Приход")
                        .HasForeignKey("СкладId")
                        .IsRequired()
                        .HasConstraintName("FK_Приход_Склады");

                    b.HasOne("WpfApp22.Models.Товары", "Товар")
                        .WithMany("Приход")
                        .HasForeignKey("ТоварId")
                        .IsRequired()
                        .HasConstraintName("FK_Приход_Товары");

                    b.Navigation("Поставщик");

                    b.Navigation("Склад");

                    b.Navigation("Товар");
                });

            modelBuilder.Entity("WpfApp22.Models.Расход", b =>
                {
                    b.HasOne("WpfApp22.Models.Склады", "Склад")
                        .WithMany("Расход")
                        .HasForeignKey("СкладId")
                        .IsRequired()
                        .HasConstraintName("FK_Расход_Склады");

                    b.HasOne("WpfApp22.Models.Товары", "Товар")
                        .WithMany("Расход")
                        .HasForeignKey("ТоварId")
                        .IsRequired()
                        .HasConstraintName("FK_Расход_Товары");

                    b.Navigation("Склад");

                    b.Navigation("Товар");
                });

            modelBuilder.Entity("WpfApp22.Models.Поставщики", b =>
                {
                    b.Navigation("Приход");
                });

            modelBuilder.Entity("WpfApp22.Models.Склады", b =>
                {
                    b.Navigation("Остатки");

                    b.Navigation("Приход");

                    b.Navigation("Расход");
                });

            modelBuilder.Entity("WpfApp22.Models.Товары", b =>
                {
                    b.Navigation("Остатки");

                    b.Navigation("Приход");

                    b.Navigation("Расход");
                });
#pragma warning restore 612, 618
        }
    }
}
