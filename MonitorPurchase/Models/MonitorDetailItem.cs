using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorPurchase.Models
{
    /// <summary>
    /// Детальная модель (содержимое элемента)
    /// </summary>
    public class MonitorDetailItem
    {
        public string ClientOrder { get; set; }
        public string ClientOrderNote { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal DemandQty { get; set; }
        public string MeasureName { get; set; }
        public string Notes { get; set; }
        public decimal DeficitQty { get; set; }
        public decimal RequestQty { get; set; }
        public decimal FixQty { get; set; }
        public decimal OutQty { get; set; }
        public string RequestName { get; set; }
    }
}
