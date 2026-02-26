using WpfApp22.Models;

namespace WpfApp22.Application;

public static class ТоварыСервис
{
    public static void Подготовить(Товары товар)
    {
        товар.Название = товар.Название?.Trim() ?? string.Empty;
        товар.Артикул = string.IsNullOrWhiteSpace(товар.Артикул) ? null : товар.Артикул.Trim();
        товар.Категория = string.IsNullOrWhiteSpace(товар.Категория) ? null : товар.Категория.Trim();
    }
}
