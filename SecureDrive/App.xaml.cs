using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace SecureDrive
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private NotifyIcon _notifyIcon;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            InitTrayIcon();
        }

        private void InitTrayIcon()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Text = "SecureDrive";
            _notifyIcon.Icon = new Icon(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resource/securedrive_logo_new.ico")).Stream);
            _notifyIcon.Visible = true;

            // สร้าง Context Menu
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Mount", null, OnMountClicked);
            contextMenu.Items.Add("Unmount", null, OnUnmountClicked);
            contextMenu.Items.Add("Configuration", null, OnConfigClicked);
            contextMenu.Items.Add("Exit", null, OnExitClicked);

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void OnMountClicked(object sender, EventArgs e)
        {
            System.Windows.MessageBox.Show("Mount Drive!");
        }

        private void OnUnmountClicked(object sender, EventArgs e)
        {
            System.Windows.MessageBox.Show("Unmount Drive!");
        }

        private void OnConfigClicked(object sender, EventArgs e)
        {
            // เปิด MainWindow ตอนกด Configuration
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            _notifyIcon.Dispose();
            Shutdown();
        }
    }
}

