using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace MonitorPurchase.WinForms
{
    public partial class MonitorForm : Form
    {
        private ElementHost _elementHost;
        public MonitorForm(Hashtable parameters)
        {
            try
            {
                InitializeComponent();
                _elementHost = new ElementHost
                {
                    Dock = DockStyle.Fill,
                    Name = "wpfHost"
                };
                _elementHost.Child = new MonitorControl(parameters);
                Controls.Add(_elementHost);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке диаграммы:\n\n{ex.Message}\n\n",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                throw;
            }
        }
    }
}