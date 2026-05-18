using MonitorPurchase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MonitorPurchase.Services
{
    public class DialogsService
    {
        /// <summary>
        /// Диалог выбора поставщика
        /// </summary>
        public SupplierInfo ShowSupplierSelectionDialog(Window owner, List<SupplierInfo> suppliers)
        {
            if (suppliers == null || !suppliers.Any())
            {
                MessageBox.Show("Список поставщиков пуст!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            var dialog = CreateDialog(owner, suppliers);
            var result = dialog.ShowDialog();

            return result == true ? dialog.Tag as SupplierInfo : null;
        }

        private Window CreateDialog(Window owner, List<SupplierInfo> suppliers)
        {
            var dialog = new Window
            {
                Title = "Выбор основного поставщика",
                Width = 450,
                Height = 400,
                ResizeMode = ResizeMode.CanResize,
                ShowInTaskbar = true,
                Tag = null // Здесь будет храниться выбранный поставщик
            };

            if (owner != null)
            {
                dialog.Owner = owner;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            var grid = CreateDialogGrid();

            // Заголовок
            grid.Children.Add(CreateTitleBlock());

            // Поиск
            var searchBox = CreateSearchBox();
            grid.Children.Add(searchBox);

            // Список
            var listBox = CreateListBox(suppliers);
            grid.Children.Add(listBox);

            // Фильтрация
            searchBox.TextChanged += (s, e) => FilterListBox(listBox, suppliers, searchBox.Text);

            // Кнопки
            var buttonPanel = CreateButtonPanel(listBox, dialog);
            grid.Children.Add(buttonPanel);

            dialog.Content = grid;
            return dialog;
        }

        private Grid CreateDialogGrid()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.Margin = new Thickness(10);
            return grid;
        }

        private TextBlock CreateTitleBlock()
        {
            var title = new TextBlock
            {
                Text = "Выберите нового основного поставщика:",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(title, 0);
            return title;
        }

        private TextBox CreateSearchBox()
        {
            var searchBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 5),
                Text = ""
            };
            Grid.SetRow(searchBox, 1);
            return searchBox;
        }

        private ListBox CreateListBox(List<SupplierInfo> suppliers)
        {
            var listBox = new ListBox
            {
                DisplayMemberPath = "ClientName",
                Margin = new Thickness(0, 5, 0, 10)
            };

            foreach (var supplier in suppliers)
            {
                listBox.Items.Add(supplier);
            }

            Grid.SetRow(listBox, 2);
            return listBox;
        }

        private void FilterListBox(ListBox listBox, List<SupplierInfo> suppliers, string searchText)
        {
            listBox.Items.Clear();
            var text = searchText.ToLower();
            var filtered = string.IsNullOrEmpty(text)
                ? suppliers
                : suppliers.Where(sp => sp.ClientName.ToLower().Contains(text)).ToList();

            foreach (var supplier in filtered)
            {
                listBox.Items.Add(supplier);
            }
        }

        private StackPanel CreateButtonPanel(ListBox listBox, Window dialog)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var btnOk = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 30,
                Margin = new Thickness(5, 0, 5, 0),
                IsDefault = true
            };

            var btnCancel = new Button
            {
                Content = "Отмена",
                Width = 80,
                Height = 30,
                Margin = new Thickness(0, 0, 5, 0),
                IsCancel = true
            };

            btnOk.Click += (s, e) =>
            {
                if (listBox.SelectedItem != null)
                {
                    dialog.Tag = listBox.SelectedItem as SupplierInfo;
                    dialog.DialogResult = true;
                }
                else
                {
                    MessageBox.Show(dialog, "Пожалуйста, выберите поставщика из списка!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            btnCancel.Click += (s, e) => dialog.DialogResult = false;

            panel.Children.Add(btnOk);
            panel.Children.Add(btnCancel);
            Grid.SetRow(panel, 3);

            return panel;
        }
    }
}