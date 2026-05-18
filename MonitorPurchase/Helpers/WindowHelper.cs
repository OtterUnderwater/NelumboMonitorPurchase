using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MonitorPurchase.Helpers
{
    public static class WindowHelper
    {
        /// <summary>
        /// Получение окна-владельца для элемента
        /// </summary>
        public static Window GetOwnerWindow(FrameworkElement element)
        {
            try
            {
                // Пытаемся получить родительское окно через визуальное дерево
                var owner = Window.GetWindow(element);
                if (owner != null) return owner;

                // Если не получилось, пробуем через Application.Current
                if (Application.Current != null)
                {
                    owner = Application.Current.MainWindow;
                    if (owner != null) return owner;

                    // Ищем активное окно
                    owner = Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive);
                    if (owner != null) return owner;
                }
            }
            catch
            {
                // Игнорируем ошибки
            }

            return null;
        }


        /// <summary>
        /// Вспомогательный метод для поиска визуального дочернего элемента указанного типа
        /// </summary>
        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

    }
}