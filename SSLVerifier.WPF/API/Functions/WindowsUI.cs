using System;
using System.Windows;

namespace SSLVerifier.API.Functions
{
    /// <summary>
    ///     Помошник для работы с окнами
    /// </summary>
    public static class WindowsUI
    {
        /// <summary>
        ///     Создать и показать окно как диалоговое
        /// </summary>
        /// <typeparam name="T">
        ///     Тип окна
        /// </typeparam>
        public static void ShowWindow<T>() where T : Window
        {
            T dlg = (T)Activator.CreateInstance(typeof(T), new object[] { });
            dlg.Owner = App.Current.MainWindow;
            dlg.Show();
        }

        /// <summary>
        ///     Создать и показать окно как диалоговое
        /// </summary>
        /// <typeparam name="T">
        ///     Тип окна
        /// </typeparam>
        /// <returns>
        ///     Результат закрытия окна (Ok, Cancel)
        /// </returns>
        public static bool? ShowWindowDialog<T>() where T : Window
        {
            T dlg = (T)Activator.CreateInstance(typeof(T), new object[] { });
            dlg.Owner = App.Current.MainWindow;
            return dlg.ShowDialog();
        }

        /// <summary>
        ///     Создать и показать окно, которому передаётся один дополнительный параметр, как диалоговое
        /// </summary>
        /// <typeparam name="T">
        ///     Тип окна
        /// </typeparam>
        /// <param name="item">
        ///     Параметр
        /// </param>
        /// <returns>
        ///     Ссылка на окно, для возможности получить данные из окна после того как оператор завершенил работу с ним
        /// </returns>
        public static T ShowWindowDialog<T>(object item) where T : Window
        {
            T dlg = (T)Activator.CreateInstance(typeof(T), new object[] { item });
            dlg.Owner = App.Current.MainWindow;
            dlg.ShowDialog();
            return dlg;
        }
    }
}
