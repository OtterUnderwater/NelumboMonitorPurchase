using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorPurchase.Models
{
    /// <summary>
    /// Основная модель мониторинга (таблица данных)
    /// </summary>
    public class MonitorItem
    {
        public int ItemID { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemSymbol { get; set; }
        public string MeasureName { get; set; }
        public decimal DemandQty { get; set; }
        public decimal OutQty { get; set; }
        public decimal FixQty { get; set; }
        public decimal NetQty { get; set; }
        public int ClientOrderCount { get; set; }
        public decimal RestQty { get; set; }
        public string RestString { get; set; }
        public decimal MinRest { get; set; }
        public decimal NotReceivedQty { get; set; }
        public decimal ToSupply { get; set; }
        public string ClientName { get; set; }
        public string WrhName { get; set; }
        public bool IsSelected { get; set; }
    }
}
