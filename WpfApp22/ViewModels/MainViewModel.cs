using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace WpfApp22.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private string _statusMessage = "Готово";

    public MainViewModel(
        Action refreshAction,
        Action buildReportAction,
        Action saveProductsAction,
        Action saveWarehousesAction,
        Action saveSuppliersAction,
        Action saveIncomeAction,
        Action saveOutcomeAction,
        Action reserveAction,
        Action unreserveAction,
        Action fullRecountAction,
        Action resetIncomeFilterAction,
        Action resetOutcomeFilterAction,
        Action undoAction,
        Action redoAction)
    {
        RefreshCommand = new RelayCommand(refreshAction);
        BuildReportCommand = new RelayCommand(buildReportAction);
        SaveProductsCommand = new RelayCommand(saveProductsAction);
        SaveWarehousesCommand = new RelayCommand(saveWarehousesAction);
        SaveSuppliersCommand = new RelayCommand(saveSuppliersAction);
        SaveIncomeCommand = new RelayCommand(saveIncomeAction);
        SaveOutcomeCommand = new RelayCommand(saveOutcomeAction);
        ReserveCommand = new RelayCommand(reserveAction);
        UnreserveCommand = new RelayCommand(unreserveAction);
        FullRecountCommand = new RelayCommand(fullRecountAction);
        ResetIncomeFilterCommand = new RelayCommand(resetIncomeFilterAction);
        ResetOutcomeFilterCommand = new RelayCommand(resetOutcomeFilterAction);
        UndoCommand = new RelayCommand(undoAction);
        RedoCommand = new RelayCommand(redoAction);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand BuildReportCommand { get; }
    public ICommand SaveProductsCommand { get; }
    public ICommand SaveWarehousesCommand { get; }
    public ICommand SaveSuppliersCommand { get; }
    public ICommand SaveIncomeCommand { get; }
    public ICommand SaveOutcomeCommand { get; }
    public ICommand ReserveCommand { get; }
    public ICommand UnreserveCommand { get; }
    public ICommand FullRecountCommand { get; }
    public ICommand ResetIncomeFilterCommand { get; }
    public ICommand ResetOutcomeFilterCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
