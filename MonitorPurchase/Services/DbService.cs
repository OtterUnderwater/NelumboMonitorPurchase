using Dapper;
using MettaFramework.Classes;
using MonitorPurchase.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
        /// Загрузка заказов
        /// [crm.ext_SupplyMonitor ActionID = 1]
        /// </summary>
        public async Task<List<OrderInfo>> LoadOrdersAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var multi = await connection.QueryMultipleAsync( "crm.ext_SupplyMonitor", new { ActionID = 1 }, commandType: CommandType.StoredProcedure))
                    {
                        var orders = (await multi.ReadAsync<OrderInfo>()).ToList();
                        return orders;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                throw;
            }
        }

        /// <summary>
        /// Загрузка основных данных мониторинга
        /// [crm.ext_SupplyMonitor ActionID = 2]
        /// </summary>
        public async Task<List<MonitorItem>> LoadDataAsync(int? orderID, bool isDeficitOnly)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var parameters = new DynamicParameters();
                    parameters.Add("ActionID", 2);
                    parameters.Add("IsDeficitOnly", isDeficitOnly);
                    if (orderID != null && orderID != 0)
                    {
                        parameters.Add("ClientOrderID", orderID);
                    }
                    using (var multi = await connection.QueryMultipleAsync("crm.ext_SupplyMonitor", parameters, commandType: CommandType.StoredProcedure))
                    {
                        var items = (await multi.ReadAsync<MonitorItem>()).ToList();
                        return items;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                throw;
            }
        }

        /// <summary>
        /// Загрузка детальных данных по позиции
        /// [crm.ext_SupplyMonitor ActionID = 3]
        /// </summary>
        public async Task<List<MonitorDetailItem>> LoadDetailDataAsync(int itemID, int? clientOrderID)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var parameters = new DynamicParameters();
                    parameters.Add("ActionID", 3);
                    parameters.Add("ItemID", itemID);
                    if (clientOrderID != 0) parameters.Add("ClientOrderID", clientOrderID);

                    using (var multi = await connection.QueryMultipleAsync("crm.ext_SupplyMonitor", parameters, commandType: CommandType.StoredProcedure))
                    {
                        var items = (await multi.ReadAsync<MonitorDetailItem>()).ToList();
                        return items;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                throw;
            }
        }

        /// <summary>
        /// Получение списка поставщиков
        /// [crm.ext_SupplyMonitor ActionID = 4]
        /// </summary>
        public async Task<List<SupplierInfo>> GetSuppliersAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var multi = await connection.QueryMultipleAsync("crm.ext_SupplyMonitor", new { ActionID = 4 }, commandType: CommandType.StoredProcedure))
                    {
                        var suppliers = (await multi.ReadAsync<SupplierInfo>()).ToList();
                        return suppliers;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                throw;
            }
        }

        /// <summary>
        /// Получение списка Складов
        /// [crm.ext_SupplyMonitor ActionID = 6]
        /// </summary>
        public async Task<List<Wrh>> GetWrhAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var multi = await connection.QueryMultipleAsync("crm.ext_SupplyMonitor", new { ActionID = 6 }, commandType: CommandType.StoredProcedure))
                    {
                        var wrhs = (await multi.ReadAsync<Wrh>()).ToList();
                        return wrhs;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                throw;
            }
        }

        /// <summary>
        /// Смена основного поставщика
        /// [crm.ext_SupplyMonitor ActionID = 5]
        /// </summary>
        public async Task<bool> ChangeMainSupplierAsync(int itemID, int newSupplierID, int? clientOrderID = null)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("ActionID", 5);
                    parameters.Add("ItemID", itemID);
                    parameters.Add("ClientID", newSupplierID);
                    if (clientOrderID != null)
                    {
                        parameters.Add("ClientOrderID", clientOrderID);
                    }
                    await connection.ExecuteAsync("crm.ext_SupplyMonitor", parameters, commandType: CommandType.StoredProcedure);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Создание заказа поставщику (ActionID = 10 из crm.ext_Metall)
        /// [crm.ext_Metall ActionID = 10]
        /// </summary>
        public async Task<(int DocID, bool Success)> CreateOrderAsync(string clientName, string orders)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("ActionID", 10);
                    parameters.Add("RegID", _regID);
                    parameters.Add("ClientName", clientName);
                    parameters.Add("Orders", orders);
                    parameters.Add("DocTypeID", 1);
                    parameters.Add("DocID", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                    await connection.ExecuteAsync("mes.ext_Metall", parameters, commandType: CommandType.StoredProcedure);

                    int docID = parameters.Get<int?>("DocID") ?? -1;
                    bool success = docID > 0;
                    return (docID, success);
                }
            }
            catch
            {
                return (-1, false);
            }
        }

        /// <summary>
        /// Добавление строки в заказ поставщику
        /// [crm.ext_Metall ActionID = 11]
        /// </summary>
        public async Task<bool> AddOrderRowAsync(int docID, int itemID, decimal qty)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("ActionID", 11);
                    parameters.Add("RegID", _regID);
                    parameters.Add("DocID", docID);
                    parameters.Add("ItemID", itemID);
                    parameters.Add("Qty", qty);

                    await connection.ExecuteAsync("mes.ext_Metall", parameters, commandType: CommandType.StoredProcedure);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}