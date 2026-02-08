using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WpfApp22.Models;

namespace WpfApp22.Models;

[Table("Склады")]
public partial class Склады
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Название { get; set; } = null!;

    [StringLength(255)]
    public string? Адрес { get; set; }

    // --- НАВИГАЦИОННЫЕ СВОЙСТВА ---
    [InverseProperty("Склад")]
    public virtual ICollection<Приход> Приход { get; set; } = new List<Приход>();

    [InverseProperty("Склад")]
    public virtual ICollection<Расход> Расход { get; set; } = new List<Расход>();

    [InverseProperty("Склад")]
    public virtual ICollection<Остатки> Остатки { get; set; } = new List<Остатки>();
}