using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WpfApp22.Models; // Убедитесь, что пространство имён указано правильно

namespace WpfApp22.Models;

[Table("Приход")]
public partial class Приход
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Товар_ID")]
    public int ТоварId { get; set; }

    [Column("Склад_ID")]
    public int СкладId { get; set; }

    [Column("Поставщик_ID")]
    public int ПоставщикId { get; set; }

    public int Количество { get; set; }

    [Column(TypeName = "money")]
    public decimal Цена { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Дата { get; set; }

    [ForeignKey("ПоставщикId")]
    [InverseProperty("Приход")]
    public virtual Поставщики Поставщик { get; set; } = null!;

    [ForeignKey("СкладId")]
    [InverseProperty("Приход")]
    public virtual Склады Склад { get; set; } = null!;

    [ForeignKey("ТоварId")]
    [InverseProperty("Приход")]
    public virtual Товары Товар { get; set; } = null!;
}