using MonitorPurchase.Helpers;
using MonitorPurchase.Services;
using MettaFramework.Classes;
using System;
using System.Collections;
using System.Data;
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
        private DataTable dtOrders = null;
        private DataTable dtData = null;
        private DataTable dtDetailData = null;
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
            if (inp.ContainsKey("IsDebug")) IsDebug = true;

            _dbService = new DbService(CString, RegID);
            _dialogsService = new DialogsService();
            Loaded += SupplyMonitor_Window_Loaded;
        }

        private async void SupplyMonitor_Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsDebug)
                {
                    var inp = new Hashtable();
                    tbl = Utility.GetTablePanel(new Guid("d6940d10-3d4f-4393-b0ad-6df4add7f715"), inp);
                }
                await LoadOrdersAsync();
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при инициализации формы!", error);
            }
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var result = await _dbService.LoadOrdersAsync();
                dtOrders = result?.Tables[0];
                Dispatcher.Invoke(() => gridOrders.ItemsSource = dtOrders?.DefaultView);
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
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var selectedOrder = gridOrders.SelectedItem as DataRowView;
                var orderID = selectedOrder?["ClientOrderID"] != DBNull.Value ? selectedOrder["ClientOrderID"]?.ToString() : null;
                var isDeficitOnly = chkIsDeficitOnly.IsChecked ?? false;

                var result = await _dbService.LoadDataAsync(orderID, isDeficitOnly, cancellationToken);
                if (result == null || cancellationToken.IsCancellationRequested) return;

                dtData = result.Tables[0];
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

        private async void LoadDetailData(int itemID, string clientOrderID = null)
        {
            _detailCancellationTokenSource?.Cancel();
            _detailCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _detailCancellationTokenSource.Token;

            try
            {
                Dispatcher.Invoke(() => gridDetailData.ItemsSource = null);
                var result = await _dbService.LoadDetailDataAsync(itemID, clientOrderID, cancellationToken);
                if (result == null || cancellationToken.IsCancellationRequested) return;

                dtDetailData = result.Tables[0];
                Dispatcher.Invoke(() =>
                {
                    gridDetailData.ItemsSource = dtDetailData?.Rows.Count > 0 ? dtDetailData.DefaultView : null;
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
            if (dtData == null) return;
            var list = dtData.AsEnumerable()
                .Where(r => r["ClientName"] != DBNull.Value && !string.IsNullOrEmpty(r["ClientName"].ToString()))
                .Select(r => r["ClientName"].ToString())
                .Distinct().OrderBy(n => n).ToList();
            list.Insert(0, "<Все>");
            cmbSupplier.ItemsSource = list;
            cmbSupplier.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            if (dtData == null) return;
            try
            {
                var filter = BuildFilter();
                gridData.ItemsSource = string.IsNullOrEmpty(filter)
                    ? dtData.DefaultView
                    : new DataView(dtData, filter, "", DataViewRowState.CurrentRows);
            }
            catch { gridData.ItemsSource = dtData.DefaultView; }
        }

        private string BuildFilter()
        {
            var filter = new StringBuilder();
            AddFilterCondition(filter, "ItemCode", txtCodeFilter.Text);
            AddFilterCondition(filter, "ItemName", txtNameFilter.Text);
            AddFilterCondition(filter, "ItemSymbol", txtSymbolFilter.Text);
            AddFilterCondition(filter, "WrhName", txtWrhFilter.Text);

            if (cmbSupplier.SelectedValue != null && cmbSupplier.SelectedValue.ToString() != "<Все>")
            {
                AddFilterCondition(filter, "ClientName", cmbSupplier.SelectedValue.ToString(), true);
            }

            return filter.ToString();
        }

        private void AddFilterCondition(StringBuilder filter, string field, string value, bool exactMatch = false)
        {
            if (string.IsNullOrEmpty(value)) return;
            if (filter.Length > 0) filter.Append(" AND ");

            filter.Append(exactMatch
                ? $"{field} = '{value}'"
                : $"{field} like '%{value}%'");
        }

        private void ClearDetails()
        {
            gridDetailData.ItemsSource = null;
            dtDetailData = null;
        }

        private void OrderFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dtOrders == null) return;
            gridOrders.ItemsSource = string.IsNullOrEmpty(txtOrderFilter.Text)
                ? dtOrders.DefaultView
                : new DataView(dtOrders, $"DocNumber like '%{txtOrderFilter.Text}%'", "", DataViewRowState.CurrentRows);
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
            if (gridData.ItemsSource is DataView dv)
                foreach (DataRowView row in dv) row["IsSelected"] = true;
        }

        private void btnResetAll_Click(object sender, RoutedEventArgs e)
        {
            if (gridData.ItemsSource is DataView dv)
                foreach (DataRowView row in dv) row["IsSelected"] = false;
        }

        private async void gridData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = gridData.SelectedItem as DataRowView;
            if (row != null)
            {
                lblItem.Text = $"{row["ItemCode"]}  {row["ItemName"]}  {row["ItemSymbol"]}";

                if (row["ItemID"] != DBNull.Value)
                {
                    var itemID = Convert.ToInt32(row["ItemID"]);
                    var selectedOrder = gridOrders.SelectedItem as DataRowView;
                    var clientOrderID = selectedOrder?["ClientOrderID"] != DBNull.Value
                        ? selectedOrder["ClientOrderID"].ToString()
                        : null;
                    LoadDetailData(itemID, clientOrderID);
                }
            }
            else
            {
                lblItem.Text = string.Empty;
                ClearDetails();
            }
        }

        private void lblItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = gridData.SelectedItem as DataRowView;
            if (row != null && row["ItemID"] != DBNull.Value)
                try { Utility.OpenProductCard((int)row["ItemID"]); } catch { }
        }

        private async void btnCreateOrders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Создать заказы поставщикам для выбранных строк?", "Внимание",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

                Mouse.OverrideCursor = Cursors.Wait;
                await Task.Run(async () =>
                {
                    var dv = new DataView(dtData, "IsSelected=1 AND ToSupply > 0", "", DataViewRowState.CurrentRows);
                    var suppliers = dv.ToTable(true, "ClientName");
                    var selectedOrder = gridOrders.SelectedItem as DataRowView;
                    var orders = selectedOrder?["ClientOrderID"] != DBNull.Value ? selectedOrder["ClientOrderID"].ToString() : "";

                    foreach (DataRow row in suppliers.Rows)
                    {
                        var clientName = row["ClientName"].ToString();
                        if (string.IsNullOrEmpty(clientName)) continue;

                        var (docID, result) = await _dbService.CreateOrderAsync(clientName, orders);
                        if (result != "OK" || docID <= 0) continue;

                        var dvRows = new DataView(dtData, $"IsSelected=1 AND ClientName='{clientName}' AND ToSupply>0", "", DataViewRowState.CurrentRows);
                        foreach (DataRowView rowView in dvRows)
                        {
                            var qty = Convert.ToDecimal(rowView["ToSupply"]);
                            if (qty > 0)
                                await _dbService.AddOrderRowAsync(docID, Convert.ToInt32(rowView["ItemID"]), qty);
                        }
                    }
                });

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
                var selectedRows = GetSelectedRows();
                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("Не выбрано ни одной позиции!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newSupplier = await SelectSupplierAsync();
                if (newSupplier == null) return;

                if (MessageBox.Show($"Сменить основного поставщика на '{newSupplier.ClientName}' для {selectedRows.Count} позиций?",
                    "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

                Mouse.OverrideCursor = Cursors.Wait;
                var selectedOrder = gridOrders.SelectedItem as DataRowView;
                var clientOrderID = selectedOrder?["ClientOrderID"] != DBNull.Value ? selectedOrder["ClientOrderID"].ToString() : null;

                foreach (var row in selectedRows)
                {
                    if (row["ItemID"] != DBNull.Value)
                    {
                        var result = await _dbService.ChangeMainSupplierAsync(Convert.ToInt32(row["ItemID"]), newSupplier.ClientID, clientOrderID);

                        if (result != "OK")
                            throw new Exception($"Ошибка при смене поставщика: {result}");
                    }
                }

                MessageBox.Show($"Для {selectedRows.Count} позиций основной поставщик успешно изменен!", "Информация",
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
                var result = await _dbService.GetSuppliersAsync();
                if (result == null || result.Tables.Count == 0 || result.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("Список поставщиков пуст!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                var suppliers = new List<SupplierInfo>();
                foreach (DataRow row in result.Tables[0].Rows)
                {
                    suppliers.Add(new SupplierInfo
                    {
                        ClientID = Convert.ToInt32(row["ClientID"]),
                        ClientName = row["ClientName"].ToString()
                    });
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

        private List<DataRowView> GetSelectedRows()
        {
            return gridData.ItemsSource is DataView dv
                ? dv.Cast<DataRowView>().Where(row => Convert.ToBoolean(row["IsSelected"])).ToList()
                : new List<DataRowView>();
        }
    }
}