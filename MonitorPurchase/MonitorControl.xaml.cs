using MonitorPurchase.Helpers;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MettaFramework.Classes;

namespace MonitorPurchase
{
    public partial class MonitorControl : UserControl
    {
        private string CString;
        private int RegID;
        private bool IsDebug = false;
        private UserControl tbl = null; // Ваш кастомный контрол
        private DataTable dtOrders = null;
        private DataTable dtData = null;

        public MonitorControl(Hashtable inp)
        {
            InitializeComponent();

            CString = inp["ConnectionString"].ToString();
            RegID = (int)inp["RegID"];
            if (inp.ContainsKey("IsDebug")) IsDebug = true;

            Loaded += SupplyMonitor_Window_Loaded;
        }

        private void SupplyMonitor_Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsDebug)
                {
                    Hashtable inp = new Hashtable();
                    tbl = Utility.GetTablePanel(new Guid("d6940d10-3d4f-4393-b0ad-6df4add7f715"), inp);
                    // Добавьте tbl в нужную панель, если необходимо
                }

                LoadOrders();
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при инициализации формы!", error);
            }
        }

        private void LoadOrders()
        {
            try
            {
                DbCmd cmd = new DbCmd(CString, "crm.ext_SupplyMonitor");
                cmd.Parameters.AddWithValue("ActionID", 1);
                DataSet ds = cmd.ExecuteDataSet();

                dtOrders = ds.Tables[0];
                gridOrders.ItemsSource = dtOrders.DefaultView;
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при загрузке заказов!", error);
            }
        }

        private void OrderFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dtOrders == null) return;

            if (string.IsNullOrEmpty(txtOrderFilter.Text))
            {
                gridOrders.ItemsSource = dtOrders.DefaultView;
            }
            else
            {
                DataView dv = new DataView(dtOrders,
                    $"DocNumber like '%{txtOrderFilter.Text}%'", "",
                    DataViewRowState.CurrentRows);
                gridOrders.ItemsSource = dv;
            }
        }

        private void IsDeficitOnly_Changed(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void gridOrders_CurrentCellChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                DbCmd cmd = new DbCmd(CString, "crm.ext_SupplyMonitor");
                cmd.Parameters.AddWithValue("ActionID", 2);
                cmd.Parameters.AddWithValue("IsDeficitOnly", chkIsDeficitOnly.IsChecked ?? false);

                var selectedOrder = gridOrders.SelectedItem as DataRowView;
                if (selectedOrder != null && selectedOrder["ClientOrderID"] != DBNull.Value)
                {
                    cmd.Parameters.AddWithValue("ClientOrderID", selectedOrder["ClientOrderID"]);
                }

                DataSet ds = cmd.ExecuteDataSet();
                dtData = ds.Tables[0];

                LoadSuppliers();
                ApplyFilters();
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при получении данных!", error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void LoadSuppliers()
        {
            if (dtData == null) return;

            var suppliers = dtData.DefaultView.ToTable(true, "ClientName");
            var list = suppliers.AsEnumerable()
                .Where(r => r["ClientName"] != DBNull.Value)
                .Select(r => r["ClientName"].ToString())
                .Where(n => !string.IsNullOrEmpty(n))
                .OrderBy(n => n)
                .ToList();

            list.Insert(0, "<Все>");
            cmbSupplier.ItemsSource = list;
            cmbSupplier.SelectedIndex = 0;
        }

        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void Supplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (dtData == null) return;

            try
            {
                var filter = new StringBuilder();

                if (!string.IsNullOrEmpty(txtCodeFilter.Text))
                    filter.Append($"ItemCode like '%{txtCodeFilter.Text}%'");

                if (!string.IsNullOrEmpty(txtNameFilter.Text))
                {
                    if (filter.Length > 0) filter.Append(" AND ");
                    filter.Append($"ItemName like '%{txtNameFilter.Text}%'");
                }

                if (!string.IsNullOrEmpty(txtSymbolFilter.Text))
                {
                    if (filter.Length > 0) filter.Append(" AND ");
                    filter.Append($"ItemSymbol like '%{txtSymbolFilter.Text}%'");
                }

                if (cmbSupplier.SelectedValue != null && cmbSupplier.SelectedValue.ToString() != "<Все>")
                {
                    if (filter.Length > 0) filter.Append(" AND ");
                    filter.Append($"ClientName = '{cmbSupplier.SelectedValue}'");
                }

                if (!string.IsNullOrEmpty(txtWrhFilter.Text))
                {
                    if (filter.Length > 0) filter.Append(" AND ");
                    filter.Append($"WrhName like '%{txtWrhFilter.Text}%'");
                }

                var dv = string.IsNullOrEmpty(filter.ToString())
                    ? dtData.DefaultView
                    : new DataView(dtData, filter.ToString(), "", DataViewRowState.CurrentRows);

                gridData.ItemsSource = dv;
            }
            catch
            {
                gridData.ItemsSource = dtData.DefaultView;
            }
        }

        private void gridData_CurrentCellChanged(object sender, EventArgs e)
        {
            var row = gridData.SelectedItem as DataRowView;
            if (row != null)
            {
                lblItem.Text = $"{row["ItemCode"]}  {row["ItemName"]}  {row["ItemSymbol"]}";

                if (tbl != null && !IsDebug)
                {
                    var inp = new Hashtable();
                    inp.Add("ItemID", row["ItemID"]);

                    var selectedOrder = gridOrders.SelectedItem as DataRowView;
                    if (selectedOrder != null && selectedOrder["ClientOrderID"] != DBNull.Value)
                    {
                        inp.Add("ClientOrderID", selectedOrder["ClientOrderID"]);
                    }

                    // tbl.ShowData(inp); // Раскомментируйте если нужен метод ShowData
                }
            }
        }

        private void lblItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = gridData.SelectedItem as DataRowView;
            if (row != null && row["ItemID"] != DBNull.Value)
            {
                try
                {
                    Utility.OpenProductCard((int)row["ItemID"]);
                }
                catch { }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            var items = gridData.ItemsSource as System.Data.DataView;
            if (items != null)
            {
                foreach (DataRowView item in items)
                {
                    item["IsSelected"] = true;
                }
            }
        }

        private void btnResetAll_Click(object sender, RoutedEventArgs e)
        {
            var items = gridData.ItemsSource as System.Data.DataView;
            if (items != null)
            {
                foreach (DataRowView item in items)
                {
                    item["IsSelected"] = false;
                }
            }
        }

        private void btnCreateOrders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Создать заказы поставщикам для выбранных строк?",
                    "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;

                var dv = new DataView(dtData, "IsSelected=1 AND ToSupply > 0", "", DataViewRowState.CurrentRows);
                var suppliers = dv.ToTable(true, "ClientName");

                foreach (DataRow row in suppliers.Rows)
                {
                    var clientName = row["ClientName"].ToString();
                    if (!string.IsNullOrEmpty(clientName))
                    {
                        int docID = CreateOrder(clientName);

                        if (docID > 0)
                        {
                            var dvRows = new DataView(dtData,
                                $"IsSelected=1 AND ClientName='{clientName}' AND ToSupply>0",
                                "", DataViewRowState.CurrentRows);

                            foreach (DataRowView rowView in dvRows)
                            {
                                decimal qty = Convert.ToDecimal(rowView["ToSupply"]);
                                if (qty > 0)
                                {
                                    AddOrderRow(docID, Convert.ToInt32(rowView["ItemID"]), qty);
                                }
                            }
                        }
                    }
                }

                MessageBox.Show("Заказы поставщикам созданы (со статусом <Новый>)!",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadData();
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при создании Заказа поставщику!", error);
            }
        }

        private int CreateOrder(string clientName)
        {
            string orders = "";
            var selectedOrder = gridOrders.SelectedItem as DataRowView;
            if (selectedOrder != null && selectedOrder["ClientOrderID"] != DBNull.Value)
            {
                orders = selectedOrder["ClientOrderID"].ToString();
            }

            DbCmd cmd = new DbCmd(CString, "crm.ext_Metall");
            cmd.Parameters.AddWithValue("ActionID", 10);
            cmd.Parameters.AddWithValue("RegID", RegID);
            cmd.Parameters.AddWithValue("ClientName", clientName);
            cmd.Parameters.AddWithValue("Orders", orders);
            cmd.Parameters.AddWithValue("DocTypeID", 1);
            cmd.Parameters.Add("Result", SqlDbType.VarChar, 150).Direction = ParameterDirection.Output;

            DataSet ds = cmd.ExecuteDataSet();

            string result = cmd.Parameters["Result"].Value.ToString();
            if (result != "OK")
            {
                MessageBox.Show(result, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return -1;
            }

            return Convert.ToInt32(ds.Tables[0].Rows[0]["DocID"]);
        }

        private void AddOrderRow(int docID, int itemID, decimal qty)
        {
            DbCmd cmd = new DbCmd(CString, "crm.ext_Metall");
            cmd.Parameters.AddWithValue("ActionID", 11);
            cmd.Parameters.AddWithValue("RegID", RegID);
            cmd.Parameters.AddWithValue("DocID", docID);
            cmd.Parameters.AddWithValue("ItemID", itemID);
            cmd.Parameters.AddWithValue("Qty", qty);
            cmd.Parameters.Add("Result", SqlDbType.VarChar, 150).Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();
        }
    }
}