using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp22.Models
{

    public class ОтчетПоДвижению
    {
        public string Товар { get; set; } = null!;
        public string Склад { get; set; } = null!;
        public string Тип { get; set; } = null!; // "Приход" или "Расход"
        public int Количество { get; set; }
        public decimal? Цена { get; set; } // Цена есть только у прихода
        public DateTime Дата { get; set; }
        public string Документ { get; set; } = null!; // Причина расхода или поставщик
    }
}