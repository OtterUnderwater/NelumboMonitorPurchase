using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorPurchase.Models
{
    /// <summary>
    /// Модель заказа
    /// </summary>
    public class OrderInfo
    {
        public string DocNumber { get; set; }
        public DateTime DocDate { get; set; }
        public string ClientOrderID { get; set; }
    }
}
