using Newtonsoft.Json;
using SecureDrive.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace SecureDrive
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private NotifyIcon _notifyIcon;
        private string pathKS2Drive = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KS2Drive");

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            InitTrayIcon();
            if (!Directory.Exists(pathKS2Drive))
            {
                Directory.CreateDirectory(pathKS2Drive);
            }
            CheckSecureDriveConfig();

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
            contextMenu.Items.Add("Configuration", null, OnConfigClicked);
            contextMenu.Items.Add("Exit", null, OnExitClicked);

            _notifyIcon.ContextMenuStrip = contextMenu;
        }
        private void CheckSecureDriveConfig()
        {
            string configSecurePath = System.IO.Path.Combine(pathKS2Drive, "configSecure.json");
            if (System.IO.File.Exists(configSecurePath))
            {
                var configSecureJson = File.ReadAllText(configSecurePath);
                var configSecure = JsonConvert.DeserializeObject<ConfigSecureDrive>(configSecureJson);
                if (configSecure?.StartWithWindows == true && configSecure.AutoMount == true)
                {
                    OnMountClicked(null, null);
                }
            }
            else 
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }

        private void OnMountClicked(object sender, EventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Mount();
            //System.Windows.MessageBox.Show("Mount Drive!");

            // เปลี่ยนเมนู
            var contextMenu = _notifyIcon.ContextMenuStrip;
            contextMenu.Items.Clear();
            contextMenu.Items.Add("Unmount", null, OnUnmountClicked);
            contextMenu.Items.Add("Exit", null, OnExitClicked);

        }

        private void OnUnmountClicked(object sender, EventArgs e)
        {
            //System.Windows.MessageBox.Show("Unmount Drive!");
            Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = "/F /IM KS2Drive.exe",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            // เปลี่ยนเมนู
            var contextMenu = _notifyIcon.ContextMenuStrip;
            contextMenu.Items.Clear();
            contextMenu.Items.Add("Mount", null, OnMountClicked);
            contextMenu.Items.Add("Configuration", null, OnConfigClicked);
            contextMenu.Items.Add("Exit", null, OnExitClicked);

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

