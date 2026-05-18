using MettaFramework.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorPurchase.Services
{
    public class DbService
    {
        private readonly string _connectionString;
        private readonly int _regID;

        public DbService(string connectionString, int regID)
        {
            _connectionString = connectionString;
            _regID = regID;
        }

        /// <summary>
        /// Загрузка заказов (ActionID = 1)
        /// </summary>
        public async Task<DataSet> LoadOrdersAsync(CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                if (cancellationToken.IsCancellationRequested) return null;
                DbCmd cmd = new DbCmd(_connectionString, "crm.ext_SupplyMonitor");
                cmd.Parameters.AddWithValue("ActionID", 1);
                return cmd.ExecuteDataSet();
            }, cancellationToken);
        }

        /// <summary>
        /// Загрузка основных данных мониторинга (ActionID = 2)
        /// </summary>
        public async Task<DataSet> LoadDataAsync(string orderID, bool isDeficitOnly, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                if (cancellationToken.IsCancellationRequested) return null;
                DbCmd cmd = new DbCmd(_connectionString, "crm.ext_SupplyMonitor");
                cmd.Parameters.AddWithValue("ActionID", 2);
                cmd.Parameters.AddWithValue("IsDeficitOnly", isDeficitOnly);
                if (!string.IsNullOrEmpty(orderID))
                    cmd.Parameters.AddWithValue("ClientOrderID", orderID);
                return cmd.ExecuteDataSet();
            }, cancellationToken);
        }

        /// <summary>
        /// Загрузка детальных данных по позиции (ActionID = 3)
        /// </summary>
        public async Task<DataSet> LoadDetailDataAsync(int itemID, string clientOrderID, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                if (cancellationToken.IsCancellationRequested) return null;
                DbCmd cmd = new DbCmd(_connectionString, "crm.ext_SupplyMonitor");
                cmd.Parameters.AddWithValue("ActionID", 3);
                cmd.Parameters.AddWithValue("ItemID", itemID);
                if (!string.IsNullOrEmpty(clientOrderID))
                    cmd.Parameters.AddWithValue("ClientOrderID", clientOrderID);
                return cmd.ExecuteDataSet();
            }, cancellationToken);
        }

        /// <summary>
        /// Получение списка поставщиков (ActionID = 4)
        /// </summary>
        public async Task<DataSet> GetSuppliersAsync(CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                if (cancellationToken.IsCancellationRequested) return null;
                DbCmd cmd = new DbCmd(_connectionString, "crm.ext_SupplyMonitor");
                cmd.Parameters.AddWithValue("ActionID", 4);
                return cmd.ExecuteDataSet();
            }, cancellationToken);
        }

        /// <summary>
        /// Смена основного поставщика (ActionID = 5)
        /// </summary>
        public async Task<string> ChangeMainSupplierAsync(int itemID, int newSupplierID, string clientOrderID = null)
        {
            return await Task.Run(() =>
            {
                DbCmd cmd = new DbCmd(_connectionString, "crm.ext_SupplyMonitor");
                cmd.Parameters.AddWithValue("ActionID", 5);
                cmd.Parameters.AddWithValue("ItemID", itemID);
                cmd.Parameters.AddWithValue("ClientID", newSupplierID);
                if (!string.IsNullOrEmpty(clientOrderID))
                {
                    cmd.Parameters.AddWithValue("ClientOrderID", clientOrderID);
                }
                cmd.Parameters.Add("Result", SqlDbType.VarChar, 150).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                return cmd.Parameters["Result"].Value.ToString();
            });
        }

        /// <summary>
        /// Создание заказа поставщику (ActionID = 10 из crm.ext_Metall)
        /// </summary>
        public async Task<(int DocID, string Result)> CreateOrderAsync(string clientName, string orders)
        {
            return await Task.Run(() =>
            {
                DbCmd cmd = new DbCmd(_connectionString, "crm.ext_Metall");
                cmd.Parameters.AddWithValue("ActionID", 10);
                cmd.Parameters.AddWithValue("RegID", _regID);
                cmd.Parameters.AddWithValue("ClientName", clientName);
                cmd.Parameters.AddWithValue("Orders", orders);
                cmd.Parameters.AddWithValue("DocTypeID", 1);
                cmd.Parameters.Add("Result", SqlDbType.VarChar, 150).Direction = ParameterDirection.Output;
                DataSet ds = cmd.ExecuteDataSet();
                string result = cmd.Parameters["Result"].Value.ToString();
                int docID = -1;
                if (result == "OK" && ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    docID = Convert.ToInt32(ds.Tables[0].Rows[0]["DocID"]);
                }
                return (docID, result);
            });
        }

        /// <summary>
        /// Добавление строки в заказ поставщику (ActionID = 11 из crm.ext_Metall)
        /// </summary>
        public async Task<string> AddOrderRowAsync(int docID, int itemID, decimal qty)
        {
            return await Task.Run(() =>
            {
                DbCmd cmd = new DbCmd(_connectionString, "crm.ext_Metall");
                cmd.Parameters.AddWithValue("ActionID", 11);
                cmd.Parameters.AddWithValue("RegID", _regID);
                cmd.Parameters.AddWithValue("DocID", docID);
                cmd.Parameters.AddWithValue("ItemID", itemID);
                cmd.Parameters.AddWithValue("Qty", qty);
                cmd.Parameters.Add("Result", SqlDbType.VarChar, 150).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                return cmd.Parameters["Result"].Value.ToString();
            });
        }
    }
}
