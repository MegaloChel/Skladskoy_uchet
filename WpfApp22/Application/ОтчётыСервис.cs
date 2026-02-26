using Microsoft.EntityFrameworkCore;
using WpfApp22.Models;

namespace WpfApp22.Application;

public sealed class ОтчётыСервис
{
    private readonly СкладскойУчётContext _context;

    public ОтчётыСервис(СкладскойУчётContext context)
    {
        _context = context;
    }

    public async Task<List<ОтчетПоДвижению>> СформироватьОтчетПоДвижениюAsync(DateTime? датаС, DateTime? датаПо)
    {
        DateTime датаСФильтра = датаС?.Date ?? DateTime.MinValue;
        DateTime датаПоФильтра = датаПо?.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;

        var приходы = await _context.Приход
            .Include(p => p.Товар)
            .Include(p => p.Склад)
            .Include(p => p.Поставщик)
            .Where(p => p.Дата >= датаСФильтра && p.Дата <= датаПоФильтра)
            .Select(p => new ОтчетПоДвижению
            {
                Товар = p.Товар.Название,
                Склад = p.Склад.Название,
                Тип = "Приход",
                Количество = p.Количество,
                Цена = p.Цена,
                Дата = p.Дата ?? DateTime.MinValue,
                Контрагент = p.Поставщик.Название,
                ОснованиеОперации = "Поступление от поставщика"
            })
            .ToListAsync();

        var расходы = await _context.Расход
            .Include(r => r.Товар)
            .Include(r => r.Склад)
            .Where(r => r.Дата >= датаСФильтра && r.Дата <= датаПоФильтра)
            .Select(r => new ОтчетПоДвижению
            {
                Товар = r.Товар.Название,
                Склад = r.Склад.Название,
                Тип = "Расход",
                Количество = r.Количество,
                Цена = null,
                Дата = r.Дата ?? DateTime.MinValue,
                Контрагент = "—",
                ОснованиеОперации = string.IsNullOrWhiteSpace(r.Причина) ? "Не указана" : r.Причина
            })
            .ToListAsync();

        return приходы.Concat(расходы).OrderBy(x => x.Дата).ToList();
    }
}
