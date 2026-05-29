using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MonitorPurchase.Converters
{
    public class NumFormatConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return "";

            try
            {
                if (System.Convert.ToDecimal(values[0]) == 0)
                    return string.Empty;
            }
            catch (InvalidCastException)
            {
                return string.Empty;
            }

            if (!double.TryParse(values[0]?.ToString(), out double number))
                return values[0]?.ToString() ?? "";

            string unit = values[1]?.ToString() ?? "";
            string unitLower = unit.ToLower();
            bool isIntegerUnit = unitLower.Contains("шт") ||
                                 unitLower.Contains("рул") ||
                                 unitLower.Contains("уп") ||
                                 unitLower.Contains("компл");

            if (isIntegerUnit)
                return Math.Round(number, 0).ToString("F0");
            else
                return number.ToString("F2");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
