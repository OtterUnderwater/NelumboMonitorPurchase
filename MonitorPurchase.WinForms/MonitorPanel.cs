using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Windows.Forms.Integration;

namespace MonitorPurchase.WinForms
{
    public partial class MonitorPanel : UserControl
    {
        private ElementHost _elementHost;
        private MonitorControl _wpfControl;

        public MonitorPanel(Hashtable parameters)
        {
            try
            {
                InitializeComponent();
                _elementHost = new ElementHost
                {
                    Dock = DockStyle.Fill,
                    Name = "wpfHost"
                };
                _wpfControl = new MonitorControl(parameters);
                _elementHost.Child = _wpfControl;
                Controls.Add(_elementHost);
                ConfigurePanel();
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
        private void ConfigurePanel()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.AutoScaleMode = AutoScaleMode.None;
        }
    }
}
