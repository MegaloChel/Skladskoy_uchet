using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.EntityFrameworkCore;
using WpfApp22.Models;

namespace WpfApp22;

public partial class MainWindow : Window
{
    private СкладскойУчётContext _context;

    public MainWindow()
    {
        InitializeComponent();
        _context = new СкладскойУчётContext();
        DataContext = this;
        LoadAllData();
    }

    private async void LoadAllData()
    {
        StatusText.Text = "⏳ Загрузка таблиц...";
        try
        {
            await _context.Товары.LoadAsync();
            await _context.Склады.LoadAsync();
            await _context.Поставщики.LoadAsync();
            await _context.Приход.LoadAsync();
            await _context.Расход.LoadAsync();
            await _context.Остатки
                .Include(о => о.Товар)
                .Include(о => о.Склад)
                .LoadAsync();

            GridТовары.ItemsSource = _context.Товары.Local.ToObservableCollection();
            GridСклады.ItemsSource = _context.Склады.Local.ToObservableCollection();
            GridПоставщики.ItemsSource = _context.Поставщики.Local.ToObservableCollection();
            GridПриход.ItemsSource = _context.Приход.Local.ToObservableCollection();
            GridРасход.ItemsSource = _context.Расход.Local.ToObservableCollection();
            GridОстатки.ItemsSource = _context.Остатки.Local.ToObservableCollection();

            SetupComboBoxColumns();

            StatusText.Text = "✅ Все таблицы загружены. Доступно редактирование.";
        }
        catch (Exception ex)
        {
            StatusText.Text = "❌ Ошибка загрузки";
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SetupComboBoxColumns()
    {
        var товары = _context.Товары.Local.ToObservableCollection();
        var склады = _context.Склады.Local.ToObservableCollection();
        var поставщики = _context.Поставщики.Local.ToObservableCollection();

        foreach (var column in GridПриход.Columns.OfType<DataGridComboBoxColumn>())
        {
            if (column.Header.ToString() == "Товар")
                column.ItemsSource = товары;
            else if (column.Header.ToString() == "Склад")
                column.ItemsSource = склады;
            else if (column.Header.ToString() == "Поставщик")
                column.ItemsSource = поставщики;
        }

        foreach (var column in GridРасход.Columns.OfType<DataGridComboBoxColumn>())
        {
            if (column.Header.ToString() == "Товар")
                column.ItemsSource = товары;
            else if (column.Header.ToString() == "Склад")
                column.ItemsSource = склады;
        }
    }

    private async void SaveChanges_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!ВалидироватьДанные())
            {
                return;
            }

            StatusText.Text = "💾 Сохранение изменений...";

            bool естьИзмененияВДвижениях = _context.ChangeTracker.Entries()
                .Any(entry => (entry.Entity is Приход || entry.Entity is Расход) &&
                         (entry.State == EntityState.Added ||
                          entry.State == EntityState.Modified ||
                          entry.State == EntityState.Deleted));

            int count = await _context.SaveChangesAsync();
            StatusText.Text = $"✅ Сохранено строк: {count}";

            if (естьИзмененияВДвижениях)
            {
                StatusText.Text = "🔄 Пересчёт остатков...";
                await UpdateBalancesAsync();
                StatusText.Text = "✅ Данные сохранены, остатки обновлены!";
            }

            MessageBox.Show($"Успешно сохранено строк: {count}" +
                (естьИзмененияВДвижениях ? "\nОстатки пересчитаны." : ""),
                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusText.Text = "❌ Ошибка сохранения";
            MessageBox.Show($"Ошибка: {ex.Message}\n\nДетали:\n{ex.InnerException?.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool ВалидироватьДанные()
    {
        foreach (var приход in _context.ChangeTracker.Entries<Приход>()
            .Where(entry => entry.State == EntityState.Added || entry.State == EntityState.Modified)
            .Select(entry => entry.Entity))
        {
            if (приход.Количество <= 0)
            {
                MessageBox.Show("Количество в приходе должно быть больше 0!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (приход.Цена < 0)
            {
                MessageBox.Show("Цена не может быть отрицательной!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (приход.ТоварId == 0)
            {
                MessageBox.Show("Выберите товар в приходе!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (приход.СкладId == 0)
            {
                MessageBox.Show("Выберите склад в приходе!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (приход.ПоставщикId == 0)
            {
                MessageBox.Show("Выберите поставщика в приходе!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        foreach (var расход in _context.ChangeTracker.Entries<Расход>()
            .Where(entry => entry.State == EntityState.Added || entry.State == EntityState.Modified)
            .Select(entry => entry.Entity))
        {
            if (расход.Количество <= 0)
            {
                MessageBox.Show("Количество в расходе должно быть больше 0!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (расход.ТоварId == 0)
            {
                MessageBox.Show("Выберите товар в расходе!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (расход.СкладId == 0)
            {
                MessageBox.Show("Выберите склад в расходе!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        return true;
    }

    private async Task UpdateBalancesAsync()
    {
        try
        {
            var приходы = await _context.Приход
                .AsNoTracking()
                .GroupBy(x => new { x.ТоварId, x.СкладId })
                .Select(g => new {
                    g.Key.ТоварId,
                    g.Key.СкладId,
                    Количество = g.Sum(x => x.Количество)
                })
                .ToListAsync();

            var расходы = await _context.Расход
                .AsNoTracking()
                .GroupBy(x => new { x.ТоварId, x.СкладId })
                .Select(g => new {
                    g.Key.ТоварId,
                    g.Key.СкладId,
                    Количество = g.Sum(x => x.Количество)
                })
                .ToListAsync();

            var остаткиDict = new Dictionary<(int ТоварId, int СкладId), int>();

            foreach (var приход in приходы)
            {
                var key = (приход.ТоварId, приход.СкладId);
                if (!остаткиDict.ContainsKey(key))
                    остаткиDict[key] = 0;
                остаткиDict[key] += приход.Количество;
            }

            foreach (var расход in расходы)
            {
                var key = (расход.ТоварId, расход.СкладId);
                if (!остаткиDict.ContainsKey(key))
                    остаткиDict[key] = 0;
                остаткиDict[key] -= расход.Количество;
            }

            var текущиеОстатки = _context.Остатки.Local.ToList();

            foreach (var остаток in текущиеОстатки)
            {
                _context.Остатки.Remove(остаток);
            }
            await _context.SaveChangesAsync();

            foreach (var kvp in остаткиDict)
            {
                if (kvp.Value > 0)
                {
                    var новыйОстаток = new Остатки
                    {
                        ТоварId = kvp.Key.ТоварId,
                        СкладId = kvp.Key.СкладId,
                        Количество = kvp.Value,
                        В_Резерве = 0
                    };
                    _context.Остатки.Add(новыйОстаток);
                }
            }

            await _context.SaveChangesAsync();

            foreach (var остаток in _context.Остатки.Local)
            {
                await _context.Entry(остаток).Reference(о => о.Товар).LoadAsync();
                await _context.Entry(остаток).Reference(о => о.Склад).LoadAsync();
            }

            GridОстатки.ItemsSource = null;
            GridОстатки.ItemsSource = _context.Остатки.Local.ToObservableCollection();
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка обновления остатков: {ex.Message}", ex);
        }
    }

    private async void ПолныйПересчётОстатков_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            StatusText.Text = "🔄 Полный пересчёт остатков...";
            await UpdateBalancesAsync();
            StatusText.Text = "✅ Остатки пересчитаны!";
            MessageBox.Show("Остатки успешно пересчитаны из Прихода и Расхода!",
                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusText.Text = "❌ Ошибка пересчёта";
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _context.Dispose();
        _context = new СкладскойУчётContext();
        LoadAllData();
        StatusText.Text = "✅ Данные обновлены!";
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string filter = SearchBox.Text.Trim().ToLower();
        var view = CollectionViewSource.GetDefaultView(GridТовары.ItemsSource);
        if (view != null)
        {
            if (string.IsNullOrEmpty(filter))
            {
                view.Filter = null;
            }
            else
            {
                view.Filter = item =>
                {
                    if (item is Товары product)
                    {
                        bool matchName = product.Название?.ToLower().Contains(filter) == true;
                        bool matchArt = product.Артикул?.ToLower().Contains(filter) == true;
                        bool matchCategory = product.Категория?.ToLower().Contains(filter) == true;
                        return matchName || matchArt || matchCategory;
                    }
                    return false;
                };
            }
        }
    }

    private void ФильтрПриход_Changed(object sender, SelectionChangedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(GridПриход.ItemsSource);
        if (view != null)
        {
            view.Filter = item =>
            {
                if (item is Приход приход)
                {
                    if (ПриходДатаС.SelectedDate.HasValue && приход.Дата < ПриходДатаС.SelectedDate)
                        return false;
                    if (ПриходДатаПо.SelectedDate.HasValue && приход.Дата > ПриходДатаПо.SelectedDate.Value.AddDays(1))
                        return false;
                    return true;
                }
                return false;
            };
        }
    }
    private void СброситьФильтрПриход_Click(object sender, RoutedEventArgs e)
    {
        ПриходДатаС.SelectedDate = null;
        ПриходДатаПо.SelectedDate = null;
        var view = CollectionViewSource.GetDefaultView(GridПриход.ItemsSource);
        if (view != null) view.Filter = null;
    }
    private void ФильтрРасход_Changed(object sender, SelectionChangedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(GridРасход.ItemsSource);
        if (view != null)
        {
            view.Filter = item =>
            {
                if (item is Расход расход)
                {
                    if (РасходДатаС.SelectedDate.HasValue && расход.Дата < РасходДатаС.SelectedDate)
                        return false;
                    if (РасходДатаПо.SelectedDate.HasValue && расход.Дата > РасходДатаПо.SelectedDate.Value.AddDays(1))
                        return false;
                    return true;
                }
                return false;
            };
        }
    }

    private void СброситьФильтрРасход_Click(object sender, RoutedEventArgs e)
    {
        РасходДатаС.SelectedDate = null;
        РасходДатаПо.SelectedDate = null;
        var view = CollectionViewSource.GetDefaultView(GridРасход.ItemsSource);
        if (view != null) view.Filter = null;
    }

    protected override void OnClosed(EventArgs e)
    {
        _context?.Dispose();
        base.OnClosed(e);
    }
}