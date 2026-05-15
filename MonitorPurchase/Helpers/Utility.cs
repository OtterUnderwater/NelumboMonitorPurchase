using MettaFramework.Classes;
using Syncfusion.Windows.PropertyGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MonitorPurchase.Helpers
{
    public static class Utility
    {
        // WPF метод для изменения размера последней колонки DataGrid
        public static void ResizeLastColumnToFill(DataGrid grid, string lastColumnName)
        {
            if (grid == null || string.IsNullOrEmpty(lastColumnName)) return;

            grid.Loaded += (sender, e) => AdjustLastColumnWidth(grid, lastColumnName);
            grid.SizeChanged += (sender, e) => AdjustLastColumnWidth(grid, lastColumnName);
        }

        private static void AdjustLastColumnWidth(DataGrid grid, string lastColumnName)
        {
            try
            {
                var lastColumn = grid.Columns.FirstOrDefault(c => c.Header?.ToString() == lastColumnName);
                if (lastColumn == null) return;

                lastColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
            catch { }
        }

        // WPF методы для изменения размеров колонок DataGrid
        public static void GridResize(DataGrid grid, string columnName)
        {
            if (grid == null || string.IsNullOrEmpty(columnName) || grid.Columns.Count == 0) return;

            try
            {
                double totalWidth = 0;
                foreach (var column in grid.Columns)
                {
                    if (column.Visibility == Visibility.Visible && column.Header?.ToString() != columnName)
                    {
                        totalWidth += column.ActualWidth;
                    }
                }

                var targetColumn = grid.Columns.FirstOrDefault(c => c.Header?.ToString() == columnName);
                if (targetColumn != null)
                {
                    double newWidth = grid.ActualWidth - 22 - totalWidth;
                    if (newWidth < 100) newWidth = 100;
                    targetColumn.Width = newWidth;
                }
            }
            catch { }
        }

        public static void GridResize(DataGrid grid, string columnName, double widthMinus)
        {
            if (grid == null || string.IsNullOrEmpty(columnName) || grid.Columns.Count == 0) return;

            try
            {
                double totalWidth = 0;
                foreach (var column in grid.Columns)
                {
                    if (column.Visibility == Visibility.Visible && column.Header?.ToString() != columnName)
                    {
                        totalWidth += column.ActualWidth;
                    }
                }

                var targetColumn = grid.Columns.FirstOrDefault(c => c.Header?.ToString() == columnName);
                if (targetColumn != null)
                {
                    double newWidth = grid.ActualWidth - 22 - totalWidth - widthMinus;
                    if (newWidth < 100) newWidth = 100;
                    targetColumn.Width = newWidth;
                }
            }
            catch { }
        }

        public static string CheckSeparator(string value)
        {
            string Separator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            string BadSeparator = ".";
            if (Separator == ".") BadSeparator = ",";

            return value.Replace(BadSeparator, Separator);
        }

        public static bool PrintReport(string ReportFileName, Guid SPID, Hashtable inp, int Copies)
        {
            bool Result = true;

            try
            {
                MettaFramework.UPanels.ReportPanel rpt = new MettaFramework.UPanels.ReportPanel();
                rpt.External_Print(ReportFileName, SPID, inp, Copies);
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка печати документа!", error);
                Result = false;
            }

            return Result;
        }

        public static void ViewHelp(Guid ObjectID)
        {
            // Для WPF создаем новое окно вместо WinForms формы
            var helpWindow = new Window
            {
                Title = "Помощь",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            try
            {
                // Здесь нужно создать WPF версию HelpForm
                // Пока просто показываем окно с сообщением
                var textBlock = new TextBlock
                {
                    Text = "Помощь в разработке",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                helpWindow.Content = textBlock;

                if (Application.Current.MainWindow != null)
                    helpWindow.Owner = Application.Current.MainWindow;

                helpWindow.Show();
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при открытии справки!", error);
            }
        }

        public static void OpenCard(Guid ObjectID, Hashtable inparams)
        {
            try
            {
                // Для WPF создаем новое окно
                var cardWindow = new Window
                {
                    Title = "Карточка объекта",
                    Width = 900,
                    Height = 700,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                // Здесь нужно создать WPF версию ObjectCardPanel
                // Пока заглушка
                var stackPanel = new StackPanel();
                var textBlock = new TextBlock
                {
                    Text = $"Карточка объекта: {ObjectID}",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    Margin = new Thickness(20)
                };
                stackPanel.Children.Add(textBlock);

                if (inparams != null)
                {
                    foreach (DictionaryEntry entry in inparams)
                    {
                        var paramText = new TextBlock
                        {
                            Text = $"{entry.Key}: {entry.Value}",
                            Margin = new Thickness(20, 5, 20, 5)
                        };
                        stackPanel.Children.Add(paramText);
                    }
                }

                cardWindow.Content = stackPanel;

                if (Application.Current.MainWindow != null)
                    cardWindow.Owner = Application.Current.MainWindow;

                cardWindow.Show();
            }
            catch (Exception error)
            {
                MettaFramework.Classes.Common.MsgBox("Ошибка при открытии карточки Объекта!", error);
            }
        }

        public static void OpenActionForm(Guid ObjectID, Actions Action, Hashtable inparams, Refiller refill)
        {
            try
            {
                var actionWindow = new Window
                {
                    Title = "Форма действия",
                    Width = 800,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                var textBlock = new TextBlock
                {
                    Text = $"Действие: {Action}\nОбъект: {ObjectID}",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                actionWindow.Content = textBlock;

                if (Application.Current.MainWindow != null)
                    actionWindow.Owner = Application.Current.MainWindow;

                actionWindow.Show();
            }
            catch (Exception error)
            {
                MettaFramework.Classes.Common.MsgBox("Ошибка при открытии формы!", error);
            }
        }

        public static void OpenBrowserForm(Guid ObjectID, Hashtable inparams)
        {
            try
            {
                var browserWindow = new Window
                {
                    Title = "Просмотр",
                    Width = 1024,
                    Height = 768,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                var textBlock = new TextBlock
                {
                    Text = $"Просмотр объекта: {ObjectID}",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                browserWindow.Content = textBlock;

                if (Application.Current.MainWindow != null)
                    browserWindow.Owner = Application.Current.MainWindow;

                browserWindow.Show();
            }
            catch (Exception error)
            {
                MettaFramework.Classes.Common.MsgBox("Ошибка при открытии формы!", error);
            }
        }

        public class ListValue
        {
            public object ID;
            public string Value;
        }

        public static ListValue OpenList(Guid LstID, Hashtable inparams)
        {
            ListValue val = new ListValue();

            try
            {
                // Для WPF создаем диалоговое окно
                var listWindow = new Window
                {
                    Title = "Выбор из списка",
                    Width = 400,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                var listBox = new ListBox();
                listBox.Items.Add("Элемент 1");
                listBox.Items.Add("Элемент 2");
                listBox.Items.Add("Элемент 3");

                var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal };
                var okButton = new Button { Content = "OK", Width = 75, Margin = new Thickness(5) };
                var cancelButton = new Button { Content = "Отмена", Width = 75, Margin = new Thickness(5) };

                buttonPanel.Children.Add(okButton);
                buttonPanel.Children.Add(cancelButton);

                var mainPanel = new StackPanel();
                mainPanel.Children.Add(listBox);
                mainPanel.Children.Add(buttonPanel);

                listWindow.Content = mainPanel;

                bool? result = false;
                okButton.Click += (s, e) => {
                    if (listBox.SelectedItem != null)
                    {
                        val.Value = listBox.SelectedItem.ToString();
                        val.ID = listBox.SelectedIndex;
                    }
                    listWindow.DialogResult = true;
                    result = true;
                };
                cancelButton.Click += (s, e) => { listWindow.DialogResult = false; result = false; };

                if (listWindow.ShowDialog() == true && result == true)
                    return val;

                return null;
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при открытии списка!", error);
            }
            return val;
        }

        // WPF версии GetTablePanel - возвращают UserControl
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

        public static UserControl GetTablePanel(Guid MOID, Guid TableViewID, Hashtable inp)
        {
            var userControl = new UserControl();

            try
            {
                var dataGrid = new DataGrid
                {
                    AutoGenerateColumns = true,
                    Margin = new Thickness(0)
                };

                userControl.Content = dataGrid;
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при создании таблицы!", error);
            }

            return userControl;
        }

        public static UserControl GetPropGrid(Guid MOID, Actions mode, Hashtable inp)
        {
            // WPF версия PropertyGrid
            var userControl = new UserControl();

            try
            {
                // Используем встроенный PropertyGrid из WPF
                var propertyGrid = new PropertyGrid();
                userControl.Content = propertyGrid;
            }
            catch
            {
                var textBlock = new TextBlock
                {
                    Text = "Property Grid (требуется сборка System.Windows.Controls.PropertyGrid)",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                userControl.Content = textBlock;
            }

            return userControl;
        }

        public static UserControl GetUCard(Guid MOID, Actions mode, Hashtable inp, Refiller refill)
        {
            var userControl = new UserControl();

            var textBlock = new TextBlock
            {
                Text = $"UCard для объекта: {MOID}",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            userControl.Content = textBlock;

            return userControl;
        }

        public static UserControl GetFindPanel(Guid MOID, Guid? TableViewID, Hashtable inp)
        {
            var userControl = new UserControl();

            var textBlock = new TextBlock
            {
                Text = $"FindPanel для объекта: {MOID}",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            userControl.Content = textBlock;

            return userControl;
        }

        public static void OpenProductCard(int ProductID)
        {
            try
            {
                Hashtable inp = new Hashtable();
                inp.Add("ItemID", ProductID);

                OpenCard(new Guid("9ec0b428-1c1b-4353-8640-3295165aecd3"), inp);
            }
            catch (Exception error)
            {
                Common.MsgBox("Ошибка при открытии карточки!", error);
            }
        }
    }
}