using MonitorPurchase.Helpers;
using MonitorPurchase.Services;
using MettaFramework.Classes;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MonitorPurchase.Models;
using System.Collections.Generic;
using System.Windows.Media;

namespace MonitorPurchase
{
    public partial class MonitorControl : UserControl
    {
        #region [Переменные и свойства]
        private readonly string CString;
        private readonly int RegID;
        private readonly bool IsDebug = false;
        private readonly DbService _dbService;
        private readonly DialogsService _dialogsService;
        private UserControl tbl = null;
        private List<Wrh> _wrhs = null;
        private List<OrderInfo> _orders = null;
        private List<MonitorItem> _items = null;
        private List<MonitorDetailItem> _details = null;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _detailCancellationTokenSource;
        private void IsDeficitOnly_Changed(object sender, RoutedEventArgs e) => LoadData();
        private void gridOrders_SelectionChanged(object sender, EventArgs e) { LoadData(); ClearDetails(); ResetScroll(); }
        private void Filter_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void Supplier_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        private async void btnRefresh_Click(object sender, RoutedEventArgs e) { await LoadOrdersAsync(); LoadData(); }
        #endregion

        public MonitorControl(Hashtable inp)
        {
            InitializeComponent();
            CString = inp["ConnectionString"].ToString();
            RegID = (int)inp["RegID"];

            _dbService = new DbService(CString, RegID);
            _dialogsService = new DialogsService();
            Loaded += SupplyMonitor_Window_Loaded;
        }
        private async void SupplyMonitor_Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var inp = new Hashtable();
                tbl = Utility.GetTablePanel(new Guid("d6940d10-3d4f-4393-b0ad-6df4add7f715"), inp);
                await LoadOrdersAsync();
                await LoadWrhsAsync();

                chkIsDeficitOnly.IsChecked = true; 
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при инициализации формы!", error);
            }
        }
        private async Task LoadWrhsAsync()
        {
            try
            {
                _wrhs = await _dbService.GetWrhAsync();
                Dispatcher.Invoke(() =>
                {
                    var list = _wrhs?.Select(w => w.WrhName).Distinct().OrderBy(n => n).ToList() ?? new List<string>();
                    list.Insert(0, "<Все>");
                    cmbWrh.ItemsSource = list;
                    cmbWrh.SelectedIndex = 0;
                });
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при загрузке складов!", error);
            }
        }
        private async Task LoadOrdersAsync()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                _orders = await _dbService.LoadOrdersAsync();
                Dispatcher.Invoke(() => gridOrders.ItemsSource = _orders);
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при загрузке заказов!", error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        private async void LoadData()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var selectedOrder = gridOrders.SelectedItem as OrderInfo;
                var orderID = selectedOrder?.ClientOrderID;
                var isDeficitOnly = chkIsDeficitOnly.IsChecked ?? false;

                _items = await _dbService.LoadDataAsync(orderID, isDeficitOnly);
                if (_items == null) return;

                Dispatcher.Invoke(() =>
                {
                    LoadSuppliers();
                    ApplyFilters();
                    ClearDetails();
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при получении данных!", error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        private async void LoadDetailData(int itemID, int? clientOrderID = null)
        {
            _detailCancellationTokenSource?.Cancel();
            _detailCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _detailCancellationTokenSource.Token;

            try
            {
                Dispatcher.Invoke(() => gridDetailData.ItemsSource = null);
                _details = await _dbService.LoadDetailDataAsync(itemID, clientOrderID);
                if (_details == null || cancellationToken.IsCancellationRequested) return;

                Dispatcher.Invoke(() =>
                {
                    gridDetailData.ItemsSource = _details?.Count > 0 ? _details : null;
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception error)
            {
                Dispatcher.Invoke(() => Common.MsgBox("Ошибка при загрузке деталей!", error));
            }
        }
        private void LoadSuppliers()
        {
            if (_items == null) return;
            var list = _items
                .Where(r => !string.IsNullOrEmpty(r.ClientName))
                .Select(r => r.ClientName)
                .Distinct().OrderBy(n => n).ToList();
            list.Insert(0, "<Все>");
            cmbSupplier.ItemsSource = list;
            cmbSupplier.SelectedIndex = 0;
        }
        private void ApplyFilters()
        {
            if (_items == null) return;
            try
            {
                var filteredItems = _items.AsEnumerable();

                if (!string.IsNullOrEmpty(txtCodeFilter.Text))
                    filteredItems = filteredItems.Where(i => i.ItemCode?.ToLower().Contains(txtCodeFilter.Text.ToLower()) == true);

                if (!string.IsNullOrEmpty(txtNameFilter.Text))
                    filteredItems = filteredItems.Where(i => i.ItemName?.ToLower().Contains(txtNameFilter.Text.ToLower()) == true);

                if (!string.IsNullOrEmpty(txtSymbolFilter.Text))
                    filteredItems = filteredItems.Where(i => i.ItemSymbol?.ToLower().Contains(txtSymbolFilter.Text.ToLower()) == true);

                if (cmbWrh.SelectedValue != null && cmbWrh.SelectedValue.ToString() != "<Все>")
                    filteredItems = filteredItems.Where(i => i.WrhName == cmbWrh.SelectedValue.ToString());

                if (cmbSupplier.SelectedValue != null && cmbSupplier.SelectedValue.ToString() != "<Все>")
                    filteredItems = filteredItems.Where(i => i.ClientName == cmbSupplier.SelectedValue.ToString());

                gridData.ItemsSource = filteredItems.ToList();
            }
            catch { gridData.ItemsSource = _items; }
        }
        private void ClearDetails()
        {
            gridDetailData.ItemsSource = null;
            _details = null;
        }
        private void OrderFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_orders == null) return;

            var filteredOrders = string.IsNullOrEmpty(txtOrderFilter.Text)
                ? _orders
                : _orders.Where(o => o.DocNumber?.ToLower().Contains(txtOrderFilter.Text.ToLower()) == true).ToList();

            gridOrders.ItemsSource = filteredOrders;
        }

        /// <summary>
        /// Сбрасывает вертикальный и горизонтальный скролл DataGrid в начало
        /// </summary>
        private void ResetScroll()
        {
            if (gridData == null) return;

            var scrollViewer = WindowHelper.FindVisualChild<ScrollViewer>(gridData);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToTop();
                scrollViewer.ScrollToLeftEnd();
            }

            gridData.SelectedItem = null;
            gridData.UpdateLayout();
        }
        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (gridData.ItemsSource is List<MonitorItem> items)
            {
                foreach (var item in items)
                    item.IsSelected = true;

                gridData.ItemsSource = null;
                gridData.ItemsSource = items;
            }
        }
        private void btnResetAll_Click(object sender, RoutedEventArgs e)
        {
            if (gridData.ItemsSource is List<MonitorItem> items)
            {
                foreach (var item in items)
                    item.IsSelected = false;

                gridData.ItemsSource = null;
                gridData.ItemsSource = items;
            }
        }
        private void gridData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = gridData.SelectedItem as MonitorItem;
            if (item != null)
            {
                lblItem.Text = $"{item.ItemCode}  {item.ItemName}  {item.ItemSymbol}";

                var selectedOrder = gridOrders.SelectedItem as OrderInfo;
                var clientOrderID = selectedOrder?.ClientOrderID;
                LoadDetailData(item.ItemID, clientOrderID);
            }
            else
            {
                lblItem.Text = string.Empty;
                ClearDetails();
            }
        }
        private void lblItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = gridData.SelectedItem as MonitorItem;
            if (item != null)
                try { Utility.OpenProductCard(item.ItemID); } catch { }
        }
        private async void btnCreateOrders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Создать заказы поставщикам для выбранных строк?", "Внимание",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

                Mouse.OverrideCursor = Cursors.Wait;

                var selectedItems = _items?.Where(i => i.IsSelected && i.ToSupply > 0).ToList();
                if (selectedItems == null || selectedItems.Count == 0)
                {
                    MessageBox.Show("Нет выбранных позиций с потребностью в закупке!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var suppliers = selectedItems.Select(i => i.ClientName).Distinct().ToList();
                var selectedOrder = gridOrders.SelectedItem as OrderInfo;
                var orders = selectedOrder?.ClientOrderID?.ToString() ?? "";

                foreach (var clientName in suppliers)
                {
                    if (string.IsNullOrEmpty(clientName)) continue;

                    var result = await _dbService.CreateOrderAsync(clientName, orders);
                    if (!result.Success) continue;

                    var itemsForSupplier = selectedItems.Where(i => i.ClientName == clientName && i.ToSupply > 0).ToList();
                    foreach (var item in itemsForSupplier)
                    {
                        if (item.ToSupply > 0)
                            await _dbService.AddOrderRowAsync(result.DocID, item.ItemID, item.ToSupply);
                    }
                }

                MessageBox.Show("Заказы поставщикам созданы (со статусом <Новый>)!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception error) { Common.MsgBox("Ошибка при создании Заказа поставщику!", error); }
            finally { Mouse.OverrideCursor = null; }
        }
        private async void btnChangeMainSupplier_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedItems = _items?.Where(i => i.IsSelected).ToList();
                if (selectedItems == null || selectedItems.Count == 0)
                {
                    MessageBox.Show("Не выбрано ни одной позиции!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var newSupplier = await SelectSupplierAsync();
                if (newSupplier == null) return;

                if (MessageBox.Show($"Сменить основного поставщика на '{newSupplier.ClientName}' для {selectedItems.Count} позиций?",
                    "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

                Mouse.OverrideCursor = Cursors.Wait;
                var selectedOrder = gridOrders.SelectedItem as OrderInfo;
                var clientOrderID = selectedOrder?.ClientOrderID;

                foreach (var item in selectedItems)
                {
                    var result = await _dbService.ChangeMainSupplierAsync(item.ItemID, newSupplier.ClientID, clientOrderID);
                    if (!result)
                        throw new Exception($"Ошибка при смене поставщика для {item.ItemCode}: {result}");
                }

                MessageBox.Show($"Для {selectedItems.Count} позиций основной поставщик успешно изменен!", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception error) { Common.MsgBox("Ошибка при смене основного поставщика!", error); }
            finally { Mouse.OverrideCursor = null; }
        }
        private async Task<SupplierInfo> SelectSupplierAsync()
        {
            try
            {
                var suppliers = await _dbService.GetSuppliersAsync();
                if (suppliers == null || suppliers.Count == 0)
                {
                    MessageBox.Show("Список поставщиков пуст!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                SupplierInfo selectedSupplier = null;
                await Dispatcher.InvokeAsync(() =>
                {
                    var owner = WindowHelper.GetOwnerWindow(this);
                    selectedSupplier = _dialogsService.ShowSupplierSelectionDialog(owner, suppliers);
                });

                return selectedSupplier;
            }
            catch (Exception ex)
            {
                Common.MsgBox("Ошибка при загрузке списка поставщиков!", ex);
                return null;
            }
        }
    }
}

