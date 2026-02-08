using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WpfApp22.Models;

namespace WpfApp22.Models;

[Table("Остатки")]
public partial class Остатки
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Товар_ID")]
    public int ТоварId { get; set; }

    [Column("Склад_ID")]
    public int СкладId { get; set; }

    public int Количество { get; set; }

    public int В_Резерве { get; set; }

    [ForeignKey("СкладId")]
    [InverseProperty("Остатки")]
    public virtual Склады Склад { get; set; } = null!;

    [ForeignKey("ТоварId")]
    [InverseProperty("Остатки")]
    public virtual Товары Товар { get; set; } = null!;
}