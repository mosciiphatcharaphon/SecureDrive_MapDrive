using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Security.Cryptography;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;


namespace SecureDrive
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var pathConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KS2Drive");
            try
            {
                if (!Directory.Exists(pathConfig))
                {
                    Directory.CreateDirectory(pathConfig);
                }
                var configFilePath = Path.Combine(pathConfig, "config.json");
                var configContent = "AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAbjVeSaso/kOw+efA/39WlQAAAAACAAAAAAAQZgAAAAEAACAAAACXXV3yfv0gvstvyhsDny6HIG2jOMUn+3R0xMKdaCQHhAAAAAAOgAAAAAIAACAAAAAV0jI+2l+JnHpFk1DJqVUzKGb0AD6+JJcPvAIJ/5ncWzACAACQ4vMTytFZ92KFLdNzQdJv08qX5DRQxgvTb2le7GhsWAc37I1MMjQd3hLjG7FkADTj2RwH1Lg7Dp3nlWsb59tX9Xa/OqL3/TpkiL1labyBp6JQJE4qCZIBZXeNaWdPviWxfN6Hadqh/DCeHStJeSNKEQiA4OqBlkY+9fSBr8AdWXE0bwAFjFF+RRO7TxBAxxa7d9zdS4t21TxZSq1cMjj4Nkl8IrbFkYT2jfZdqwcxEWg56VPAyRseIkcrdpGnxBjhtbrJSb/nwyN0n+yxnCaJY6uLo56ZIqEqogUb0z4wgdoVPmC3e8pyPgHDJ4bDTcwv3qLStu7uojCkEU4xV6roDiFWEzFoVJ3i3dL4+beaLNlRU6ZCKVaf+AaTuaX99SfUb5oBrRPesxBDIskkhuhj3CZ3OxlcOpWAwX+heF1YS6XWc6pHv0bed1eTfmXhTVLfIoVVst7yDAGBdMzgWCuf2UI03EPZvVKEVQp+8auZOeQEF8ObfzcdS55DLMMWJm0hDjPn46QseajZCDp+fD22/SJQKlUGhXbheLyLyFbNY7H1Fe0u5/ierYMo+5K9ncFxWBFNjREP7zYZCuX+YHVjj4mEeViJ2Ab+1+l5YrIE3Elm5IK2YOkStM7+082MOItfEOdLec5g2TDkzzCDB7xSpYkDZxfaYXf31lmIXG9ri5FBujZFV+4qT/ap+naFrdf9sAzeflYl9n4+kp+FXtB7GFYrhuvM0PIzt8nDZzVBbUAAAABITgQsnvP97oC1BM2yh9VV6zHeeJvpNZao1ybWSY/apxbk9U26myC+6azTfKT6KvlKbg1CT4bSa2PhIpPCWDH6";
                var xx = Unprotect(configContent);
                File.WriteAllText(configFilePath, configContent);
                string filepath = Path.Combine(Directory.GetCurrentDirectory(), "k2sdrive");
                string filename = "KS2Drive.exe";
                string fullExePath = Path.Combine(filepath, filename);

                if (File.Exists(fullExePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = fullExePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    System.Windows.MessageBox.Show($"ไม่พบไฟล์: {fullExePath}", "ไม่พบไฟล์", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error initializing configuration:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static string Protect(string str)
        {
            byte[] entropy = Encoding.ASCII.GetBytes(Assembly.GetExecutingAssembly().FullName);
            byte[] data = Encoding.ASCII.GetBytes(str);
            string protectedData = Convert.ToBase64String(ProtectedData.Protect(data, entropy, DataProtectionScope.CurrentUser));
            return protectedData;
        }

        public static string Unprotect(string str)
        {
            byte[] protectedData = Convert.FromBase64String(str);
            var xx = Assembly.GetExecutingAssembly().FullName;
            byte[] entropy = Encoding.ASCII.GetBytes(Assembly.GetExecutingAssembly().FullName);
            string data = Encoding.ASCII.GetString(ProtectedData.Unprotect(protectedData, entropy, DataProtectionScope.CurrentUser));
            return data;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;
            bool autoMount = AutoMountCheckBox.IsChecked == true;
            bool startWithWindows = StartWithWindowsCheckBox.IsChecked == true;

            System.Windows.MessageBox.Show($"Login: {login}\nPassword: {password}\nAuto-mount: {autoMount}\nStart with Windows: {startWithWindows}");
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

       
    }
}
