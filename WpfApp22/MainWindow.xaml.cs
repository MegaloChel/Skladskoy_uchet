// C:\Users\иван челик\source\repos\Skladskoy_uchet\WpfApp22\MainWindow.xaml.cs
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
            // 1. Загружаем основные справочники (БЕЗ отслеживания для списков)
            var товары = await _context.Товары.AsNoTracking().ToListAsync();
            var склады = await _context.Склады.AsNoTracking().ToListAsync();
            var поставщики = await _context.Поставщики.AsNoTracking().ToListAsync();

            // 2. Загружаем движения и остатки (С ОТСЛЕЖИВАНИЕМ, так как их будем редактировать)
            await _context.Приход.LoadAsync();
            await _context.Расход.LoadAsync();
            await _context.Остатки
                .Include(о => о.Товар)
                .Include(о => о.Склад)
                .LoadAsync();

            // 3. Устанавливаем источники данных для гридов
            GridТовары.ItemsSource = товары; // Используем список без отслеживания
            GridСклады.ItemsSource = склады;
            GridПоставщики.ItemsSource = поставщики;

            // Для движений и остатков используем локальные коллекции с отслеживанием
            GridПриход.ItemsSource = _context.Приход.Local.ToObservableCollection();
            GridРасход.ItemsSource = _context.Расход.Local.ToObservableCollection();
            GridОстатки.ItemsSource = _context.Остатки.Local.ToObservableCollection();

            // 4. Настраиваем ComboBox'ы в гридах движений (источник - загруженные справочники)
            SetupComboBoxColumns(товары, склады, поставщики);

            StatusText.Text = "✅ Все таблицы загружены. Используйте отдельные кнопки для сохранения.";
        }
        catch (Exception ex)
        {
            StatusText.Text = "❌ Ошибка загрузки";
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SetupComboBoxColumns(List<Товары> товары, List<Склады> склады, List<Поставщики> поставщики)
    {
        // Настройка для таблицы "Приход"
        foreach (var column in GridПриход.Columns.OfType<DataGridComboBoxColumn>())
        {
            if (column.Header?.ToString()?.StartsWith("Товар") == true)
            {
                column.ItemsSource = товары;
                column.DisplayMemberPath = "ПредставлениеДляВыбора";
                column.SelectedValuePath = "Id";
            }
            else if (column.Header?.ToString() == "Склад")
            {
                column.ItemsSource = склады;
                column.DisplayMemberPath = "Название";
                column.SelectedValuePath = "Id";
            }
            else if (column.Header?.ToString() == "Поставщик")
            {
                column.ItemsSource = поставщики;
                column.DisplayMemberPath = "Название";
                column.SelectedValuePath = "Id";
            }
        }

        // Настройка для таблицы "Расход"
        foreach (var column in GridРасход.Columns.OfType<DataGridComboBoxColumn>())
        {
            if (column.Header?.ToString()?.StartsWith("Товар") == true)
            {
                column.ItemsSource = товары;
                column.DisplayMemberPath = "ПредставлениеДляВыбора";
                column.SelectedValuePath = "Id";
            }
            else if (column.Header?.ToString() == "Склад")
            {
                column.ItemsSource = склады;
                column.DisplayMemberPath = "Название";
                column.SelectedValuePath = "Id";
            }
        }
    }


    private static void ПодготовитьДанныеТоваров(Товары товар)
    {
        товар.Название = товар.Название?.Trim() ?? string.Empty;
        товар.Артикул = string.IsNullOrWhiteSpace(товар.Артикул) ? null : товар.Артикул.Trim();
        товар.Категория = string.IsNullOrWhiteSpace(товар.Категория) ? null : товар.Категория.Trim();
    }

    // --- УНИВЕРСАЛЬНЫЙ МЕТОД СОХРАНЕНИЯ ДЛЯ ТАБЛИЦ ---
    private async Task<bool> SaveChangesForEntityAsync<T>(string entityName, Func<T, bool>? validate = null) where T : class
    {
        try
        {
            // Проверяем, есть ли изменения в указанном типе сущности
            var entries = _context.ChangeTracker.Entries<T>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            if (!entries.Any())
            {
                StatusText.Text = $"ℹ️ Нет изменений в таблице '{entityName}'.";
                return true; // Не ошибка, просто нечего сохранять
            }

            // Валидация (если передана)
            if (validate != null)
            {
                foreach (var entry in entries.Where(e => e.State != EntityState.Deleted))
                {
                    if (!validate(entry.Entity))
                    {
                        return false; // Валидация не пройдена, сообщение уже показано внутри validate
                    }
                }
            }

            StatusText.Text = $"💾 Сохранение таблицы '{entityName}'...";
            int count = await _context.SaveChangesAsync();
            StatusText.Text = $"✅ Таблица '{entityName}' сохранена. Строк: {count}";

            // Если сохраняли движения, пересчитываем остатки
            if (typeof(T) == typeof(Приход) || typeof(T) == typeof(Расход))
            {
                await UpdateBalancesAsync();
            }

            // ОБНОВЛЯЕМ СПРАВОЧНИКИ В ВЫПАДАЮЩИХ СПИСКАХ, если сохраняли их
            if (typeof(T) == typeof(Товары) || typeof(T) == typeof(Склады) || typeof(T) == typeof(Поставщики))
            {
                await RefreshLookupsAsync();
            }

            return true;
        }
        catch (DbUpdateException ex)
        {
            StatusText.Text = $"❌ Ошибка сохранения таблицы '{entityName}'";
            MessageBox.Show($"Ошибка базы данных: {ex.InnerException?.Message ?? ex.Message}\n\nПроверьте связи и уникальность полей.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            _context.ChangeTracker.Clear(); // Очищаем трекер после серьезной ошибки
            LoadAllData(); // Перезагружаем данные
            return false;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"❌ Ошибка сохранения таблицы '{entityName}'";
            MessageBox.Show($"Неизвестная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    // --- ОБНОВЛЕНИЕ ВЫПАДАЮЩИХ СПИСКОВ ---
    private async Task RefreshLookupsAsync()
    {
        // Просто перезагружаем справочники без отслеживания и обновляем источники ComboBox'ов
        var товары = await _context.Товары.AsNoTracking().ToListAsync();
        var склады = await _context.Склады.AsNoTracking().ToListAsync();
        var поставщики = await _context.Поставщики.AsNoTracking().ToListAsync();

        // Обновляем сами гриды справочников, чтобы показать, например, новый ID
        GridТовары.ItemsSource = товары;
        GridСклады.ItemsSource = склады;
        GridПоставщики.ItemsSource = поставщики;

        // Перенастраиваем ComboBox'ы с новыми данными
        SetupComboBoxColumns(товары, склады, поставщики);
    }

    // --- МЕТОДЫ СОХРАНЕНИЯ ДЛЯ КАЖДОЙ ТАБЛИЦЫ ---
    private async void SaveТовары_Click(object sender, RoutedEventArgs e)
    {
        await SaveChangesForEntityAsync<Товары>("Товары", (товар) =>
        {
            ПодготовитьДанныеТоваров(товар);

            if (string.IsNullOrWhiteSpace(товар.Название))
            {
                MessageBox.Show("Название товара не может быть пустым!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(товар.Артикул))
            {
                MessageBox.Show("Артикул товара обязателен: он используется как основной идентификатор в документах.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        });
    }

    private async void SaveСклады_Click(object sender, RoutedEventArgs e)
    {
        await SaveChangesForEntityAsync<Склады>("Склады", (склад) =>
        {
            if (string.IsNullOrWhiteSpace(склад.Название))
            {
                MessageBox.Show("Название склада не может быть пустым!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        });
    }

    private async void SaveПоставщики_Click(object sender, RoutedEventArgs e)
    {
        await SaveChangesForEntityAsync<Поставщики>("Поставщики", (поставщик) =>
        {
            if (string.IsNullOrWhiteSpace(поставщик.Название))
            {
                MessageBox.Show("Название поставщика не может быть пустым!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        });
    }

    private async void SaveПриход_Click(object sender, RoutedEventArgs e)
    {
        await SaveChangesForEntityAsync<Приход>("Приход", (приход) =>
        {
            if (приход.Количество <= 0)
            {
                MessageBox.Show("Количество в приходе должно быть больше 0!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (приход.Цена < 0)
            {
                MessageBox.Show("Цена не может быть отрицательной!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (приход.ТоварId == 0)
            {
                MessageBox.Show("Выберите товар в приходе!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (приход.СкладId == 0)
            {
                MessageBox.Show("Выберите склад в приходе!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (приход.ПоставщикId == 0)
            {
                MessageBox.Show("Выберите поставщика в приходе!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        });
    }

    private async void SaveРасход_Click(object sender, RoutedEventArgs e)
    {
        await SaveChangesForEntityAsync<Расход>("Расход", (расход) =>
        {
            if (расход.Количество <= 0)
            {
                MessageBox.Show("Количество в расходе должно быть больше 0!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (расход.ТоварId == 0)
            {
                MessageBox.Show("Выберите товар в расходе!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (расход.СкладId == 0)
            {
                MessageBox.Show("Выберите склад в расходе!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        });
    }

    // --- ПЕРЕСЧЁТ ОСТАТКОВ (улучшенная версия) ---
    private async Task UpdateBalancesAsync()
    {
        try
        {
            StatusText.Text = "🔄 Пересчёт остатков...";

            // Группируем приходы и расходы прямо в базе данных для эффективности
            var приходы = await _context.Приход
                .GroupBy(x => new { x.ТоварId, x.СкладId })
                .Select(g => new { g.Key.ТоварId, g.Key.СкладId, Количество = g.Sum(x => x.Количество) })
                .ToDictionaryAsync(x => (x.ТоварId, x.СкладId), x => x.Количество);

            var расходы = await _context.Расход
                .GroupBy(x => new { x.ТоварId, x.СкладId })
                .Select(g => new { g.Key.ТоварId, g.Key.СкладId, Количество = g.Sum(x => x.Количество) })
                .ToDictionaryAsync(x => (x.ТоварId, x.СкладId), x => x.Количество);

            // Получаем все ключи (уникальные пары Товар-Склад) из обоих словарей
            var всеКлючи = приходы.Keys.Union(расходы.Keys).ToList();

            // Получаем текущие остатки из БД (чтобы знать их ID)
            var текущиеОстаткиВБд = await _context.Остатки.ToDictionaryAsync(x => (x.ТоварId, x.СкладId));

            // Словарь для новых значений остатков
            var новыеОстатки = new Dictionary<(int ТоварId, int СкладId), int>();

            foreach (var key in всеКлючи)
            {
                приходы.TryGetValue(key, out int приход);
                расходы.TryGetValue(key, out int расход);
                int итого = приход - расход;
                if (итого > 0) // Сохраняем только положительные остатки
                {
                    новыеОстатки[key] = итого;
                }
            }

            // 1. Удаляем остатки, которых больше нет
            foreach (var остаток in текущиеОстаткиВБд)
            {
                if (!новыеОстатки.ContainsKey(остаток.Key))
                {
                    _context.Остатки.Remove(остаток.Value);
                }
            }

            // 2. Обновляем существующие или добавляем новые
            foreach (var kvp in новыеОстатки)
            {
                if (текущиеОстаткиВБд.TryGetValue(kvp.Key, out var существующийОстаток))
                {
                    // Обновляем количество, резерв не трогаем
                    существующийОстаток.Количество = kvp.Value;
                }
                else
                {
                    // Добавляем новый остаток
                    _context.Остатки.Add(new Остатки
                    {
                        ТоварId = kvp.Key.ТоварId,
                        СкладId = kvp.Key.СкладId,
                        Количество = kvp.Value,
                        В_Резерве = 0
                    });
                }
            }

            await _context.SaveChangesAsync();
            StatusText.Text = "✅ Остатки пересчитаны!";
        }
        catch (Exception ex)
        {
            StatusText.Text = "❌ Ошибка пересчёта остатков";
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // *** ВОТ НЕДОСТАЮЩИЙ МЕТОД ***
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

    // --- УПРАВЛЕНИЕ РЕЗЕРВАМИ ---
    private void Зарезервировать_Click(object sender, RoutedEventArgs e)
    {
        if (GridОстатки.SelectedItem is Остатки выбранныйОстаток)
        {
            var диалог = new Window
            {
                Title = "Резервирование товара",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            stackPanel.Children.Add(new TextBlock { Text = $"Товар: {выбранныйОстаток.Товар?.Название}" });
            stackPanel.Children.Add(new TextBlock { Text = $"Склад: {выбранныйОстаток.Склад?.Название}" });
            stackPanel.Children.Add(new TextBlock { Text = $"Доступно: {выбранныйОстаток.Доступно}" });

            var textBox = new TextBox { Margin = new Thickness(0, 10, 0, 10) };
            stackPanel.Children.Add(new TextBlock { Text = "Количество для резерва:" });
            stackPanel.Children.Add(textBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "OK", Width = 60, Margin = new Thickness(0, 0, 10, 0), IsDefault = true };
            var cancelButton = new Button { Content = "Отмена", Width = 60, IsCancel = true };

            okButton.Click += (s, args) =>
            {
                if (int.TryParse(textBox.Text, out int количество) && количество > 0)
                {
                    if (количество <= выбранныйОстаток.Доступно)
                    {
                        выбранныйОстаток.В_Резерве += количество;
                        // Сохраняем изменение в остатках немедленно, так как это критично
                        try
                        {
                            _context.SaveChanges();
                            StatusText.Text = $"✅ Зарезервировано {количество} ед. товара.";
                            диалог.DialogResult = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Недостаточно доступного товара. Доступно: {выбранныйОстаток.Доступно}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректное положительное число.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            диалог.Content = stackPanel;
            диалог.ShowDialog();
        }
        else
        {
            MessageBox.Show("Выберите строку в таблице остатков.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // --- ОТЧЁТНОСТЬ ---
    private async void СформироватьОтчет_Click(object sender, RoutedEventArgs e)
    {
        DateTime? датаС = ОтчетДатаС.SelectedDate;
        DateTime? датаПо = ОтчетДатаПо.SelectedDate;

        DateTime датаСФильтра = датаС?.Date ?? DateTime.MinValue;
        DateTime датаПоФильтра = датаПо?.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;

        StatusText.Text = датаС.HasValue && датаПо.HasValue
            ? "⏳ Формирование отчёта за выбранный период..."
            : "⏳ Формирование полной истории операций...";
        try
        {
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
                    Дата = p.Дата.Value,
                    Документ = p.Поставщик.Название
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
                    Дата = r.Дата.Value,
                    Документ = r.Причина ?? "Не указана"
                })
                .ToListAsync();

            var отчет = приходы.Concat(расходы).OrderBy(x => x.Дата).ToList();

            GridОтчет.ItemsSource = отчет;
            StatusText.Text = $"✅ Отчёт сформирован. Найдено записей: {отчет.Count}" +
                (датаС.HasValue && датаПо.HasValue ? string.Empty : " (полная история)");
        }
        catch (Exception ex)
        {
            StatusText.Text = "❌ Ошибка формирования отчёта";
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // --- ФИЛЬТРАЦИЯ И ОБНОВЛЕНИЕ (исправленные методы) ---
    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        // Сохраняем состояние фильтров перед перезагрузкой
        var приходФильтрС = ПриходДатаС.SelectedDate;
        var приходФильтрПо = ПриходДатаПо.SelectedDate;
        var расходФильтрС = РасходДатаС.SelectedDate;
        var расходФильтрПо = РасходДатаПо.SelectedDate;

        _context.Dispose();
        _context = new СкладскойУчётContext();
        LoadAllData();

        // Восстанавливаем фильтры
        ПриходДатаС.SelectedDate = приходФильтрС;
        ПриходДатаПо.SelectedDate = приходФильтрПо;
        РасходДатаС.SelectedDate = расходФильтрС;
        РасходДатаПо.SelectedDate = расходФильтрПо;

        // Применяем фильтры заново
        if (приходФильтрС.HasValue || приходФильтрПо.HasValue)
            ФильтрПриход_Changed(null, null);
        if (расходФильтрС.HasValue || расходФильтрПо.HasValue)
            ФильтрРасход_Changed(null, null);

        StatusText.Text = "✅ Данные обновлены!";
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string filter = SearchBox.Text.Trim().ToLower();
        // Важно: ItemsSource может быть списком без отслеживания или ObservableCollection.
        // Получаем представление напрямую из DataGrid, если ItemsSource установлен.
        if (GridТовары.ItemsSource != null)
        {
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
                            return (product.Название?.ToLower().Contains(filter) == true) ||
                                   (product.Артикул?.ToLower().Contains(filter) == true) ||
                                   (product.Категория?.ToLower().Contains(filter) == true);
                        }
                        return false;
                    };
                }
            }
        }
    }

    private void ФильтрПриход_Changed(object sender, SelectionChangedEventArgs? e)
    {
        if (GridПриход.ItemsSource != null)
        {
            var view = CollectionViewSource.GetDefaultView(GridПриход.ItemsSource);
            if (view != null)
            {
                view.Filter = item =>
                {
                    if (item is Приход приход && приход.Дата.HasValue)
                    {
                        if (ПриходДатаС.SelectedDate.HasValue && приход.Дата < ПриходДатаС.SelectedDate)
                            return false;
                        if (ПриходДатаПо.SelectedDate.HasValue && приход.Дата > ПриходДатаПо.SelectedDate.Value.AddDays(1).AddTicks(-1))
                            return false;
                        return true;
                    }
                    return false;
                };
            }
        }
    }

    private void СброситьФильтрПриход_Click(object sender, RoutedEventArgs e)
    {
        ПриходДатаС.SelectedDate = null;
        ПриходДатаПо.SelectedDate = null;
        var view = CollectionViewSource.GetDefaultView(GridПриход.ItemsSource);
        if (view != null) view.Filter = null;
    }

    private void ФильтрРасход_Changed(object sender, SelectionChangedEventArgs? e)
    {
        if (GridРасход.ItemsSource != null)
        {
            var view = CollectionViewSource.GetDefaultView(GridРасход.ItemsSource);
            if (view != null)
            {
                view.Filter = item =>
                {
                    if (item is Расход расход && расход.Дата.HasValue)
                    {
                        if (РасходДатаС.SelectedDate.HasValue && расход.Дата < РасходДатаС.SelectedDate)
                            return false;
                        if (РасходДатаПо.SelectedDate.HasValue && расход.Дата > РасходДатаПо.SelectedDate.Value.AddDays(1).AddTicks(-1))
                            return false;
                        return true;
                    }
                    return false;
                };
            }
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