using MonitorPurchase.WinForms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TestDllWindows
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Hashtable parameters = new Hashtable();
            parameters["ConnectionString"] = "server=MB-FW5T1RUAHO2Z;database=_box001;uid=se;pwd=wd;";
            parameters["RegID"] = 174967;
            parameters["OrderID"] = 27;

            // Создаем форму и добавляем на нее панель
            Form hostForm = new Form();
            hostForm.Text = "Производственный план";
            hostForm.WindowState = FormWindowState.Maximized;
            hostForm.StartPosition = FormStartPosition.CenterScreen;

            MonitorPanel panel = new MonitorPanel(parameters);
            panel.Dock = DockStyle.Fill;
            hostForm.Controls.Add(panel);

            Application.Run(hostForm);
        }
    }
}
