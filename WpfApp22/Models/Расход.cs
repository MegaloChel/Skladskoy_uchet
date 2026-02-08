using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WpfApp22.Models;

namespace WpfApp22.Models;

[Table("Расход")]
public partial class Расход
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Товар_ID")]
    public int ТоварId { get; set; }

    [Column("Склад_ID")]
    public int СкладId { get; set; }

    public int Количество { get; set; }

    [StringLength(255)]
    public string? Причина { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Дата { get; set; }

    [ForeignKey("СкладId")]
    [InverseProperty("Расход")]
    public virtual Склады Склад { get; set; } = null!;

    [ForeignKey("ТоварId")]
    [InverseProperty("Расход")]
    public virtual Товары Товар { get; set; } = null!;
}