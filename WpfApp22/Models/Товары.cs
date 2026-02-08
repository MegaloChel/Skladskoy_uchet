using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WpfApp22.Models;

namespace WpfApp22.Models;

[Table("Товары")]
public partial class Товары
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Название { get; set; } = null!;

    [StringLength(50)]
    public string? Артикул { get; set; }

    [StringLength(50)]
    public string? Категория { get; set; }

    // --- НАВИГАЦИОННЫЕ СВОЙСТВА ---
    [InverseProperty("Товар")]
    public virtual ICollection<Приход> Приход { get; set; } = new List<Приход>();

    [InverseProperty("Товар")]
    public virtual ICollection<Расход> Расход { get; set; } = new List<Расход>();

    [InverseProperty("Товар")]
    public virtual ICollection<Остатки> Остатки { get; set; } = new List<Остатки>();
}