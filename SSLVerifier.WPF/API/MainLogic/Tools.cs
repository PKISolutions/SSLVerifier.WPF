using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using SSLVerifier.API.ModelObjects;

namespace SSLVerifier.API.MainLogic {
    static class Tools {
        public static MessageBoxResult MsgBox(String header, String message, MessageBoxImage image = MessageBoxImage.Error,
            MessageBoxButton button = MessageBoxButton.OK) {
            WindowCollection windows = Application.Current.Windows;
            Window hwnd = null;
            if (windows.Count > 0) {
                hwnd = App.Current.MainWindow;
            }

            return hwnd == null
                ? MessageBox.Show(message, header, button, image)
                : MessageBox.Show(hwnd, message, header, button, image);
        }
        public static void SaveArrayAsCSV(IEnumerable<ServerObject> servers, String fileName) {
            using (StreamWriter file = new StreamWriter(fileName)) {
                file.WriteLine("Name,Port,Subject,NotBefore,NotAfter,Status");
                foreach (ServerObject server in servers) {
                    file.WriteLine(server.ToString());
                }
            }
        }
    }
}
