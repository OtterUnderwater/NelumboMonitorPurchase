using MettaFramework.Classes;
using MettaFramework.Forms;
using MettaFramework.MetaDataCashe;
using MettaFramework.UPanels;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace MonitorPurchase.Helpers
{
    public static class Utility
    {
        public static UserControl GetTablePanel(Guid MOID, Hashtable inp)
        {
            // Создаем WPF UserControl вместо WinForms Table
            var userControl = new UserControl();

            try
            {
                // Временная реализация - создаем DataGrid
                var dataGrid = new DataGrid
                {
                    AutoGenerateColumns = true,
                    Margin = new Thickness(0)
                };

                // Здесь нужно загрузить данные
                userControl.Content = dataGrid;
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при создании таблицы!", error);
            }

            return userControl;
        }

        public static void OpenProductCard(int ProductID)
        {
            try
            {
                Guid objectID = new Guid();
                Hashtable inparams = new Hashtable();
                objectID = new Guid("9ec0b428-1c1b-4353-8640-3295165aecd3");
                inparams["ItemID"] = ProductID;
                OpenCard(objectID, inparams);
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при открытии карточки!", error);
            }
        }
        public static void OpenCard(Guid ObjectID, Hashtable inparams)
        {
            try
            {
                // Карточка класса
                MetaObject mo = new MetaObject(ObjectID);
                ObjectCardPanel pnl = new ObjectCardPanel();
                pnl.Init(mo, inparams, null);
                pnl.Dock = System.Windows.Forms.DockStyle.Fill;

                TabForm frm = new TabForm();
                frm.Init(pnl, mo);
                frm.MdiParent = Common.MainForm;
                frm.Show();
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при открытии карточки Объекта!", error);
            }
        }
    }
}