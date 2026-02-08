using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WpfApp22.Models;

namespace WpfApp22.Models;

[Table("Поставщики")]
public partial class Поставщики
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Название { get; set; } = null!;

    [StringLength(255)]
    public string? Контакты { get; set; }

    [InverseProperty("Поставщик")]
    public virtual ICollection<Приход> Приход { get; set; } = new List<Приход>();
}