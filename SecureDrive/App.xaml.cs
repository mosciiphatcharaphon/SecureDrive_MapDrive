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
using SecureDrive.Helpers;
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
            try
            {
                string configSecurePath = System.IO.Path.Combine(pathKS2Drive, "configSecure.json");
                if (System.IO.File.Exists(configSecurePath))
                {
                    LogHelper.Log("SecureDrive Config Found: " + configSecurePath);
                    var configSecureJson = File.ReadAllText(configSecurePath);
                    var configSecure = JsonConvert.DeserializeObject<ConfigSecureDrive>(configSecureJson);
                    if (configSecure.AutoMount == true)
                    {
                        LogHelper.Log("AutoMount is enabled, attempting to mount drive.");
                        OnMountClicked(null, null);
                    }
                }
                else
                {
                    LogHelper.Log("SecureDrive Config Not Found");
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("Error in CheckSecureDriveConfig: " + ex.Message);
            }
        }

        private async void OnMountClicked(object sender, EventArgs e)
        {
            _notifyIcon.BalloonTipTitle = "กำลัง Mount";
            _notifyIcon.BalloonTipText = "ไดรฟ์กำลังถูก Mount กรุณารอสักครู่";
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            _notifyIcon.ShowBalloonTip(100);

            var mainWindow = new MainWindow();
            bool success = await mainWindow.Mount();

            if (success)
            {
                // status:: success
                _notifyIcon.BalloonTipTitle = "Mount เรียบร้อย";
                _notifyIcon.BalloonTipText = "ไดรฟ์ถูก Mount สำเร็จ";
                _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                _notifyIcon.ShowBalloonTip(100);

                // Mount สำเร็จ → เปลี่ยนเมนู
                var contextMenu = _notifyIcon.ContextMenuStrip;
                contextMenu.Items.Clear();
                contextMenu.Items.Add("Unmount", null, OnUnmountClicked);
                contextMenu.Items.Add("Exit", null, OnExitClicked);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Mount ไม่สำเร็จ", "เกิดข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // เปิดหน้าตั้งค่า Config
                var configWindow = new MainWindow();
                // ส่งข้อความ error
                configWindow.ShowGeneralError("ไม่สามารถเชื่อมต่อได้ กรุณาตรวจสอบ Login หรือ Password");
                configWindow.ShowDialog();
            }
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

            // status:: unmount success
            _notifyIcon.BalloonTipTitle = "UnMount เรียบร้อย";
            _notifyIcon.BalloonTipText = "ไดรฟ์ถูก UnMount สำเร็จ";
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            _notifyIcon.ShowBalloonTip(100);

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
            Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = "/F /IM KS2Drive.exe",
                CreateNoWindow = true,
                UseShellExecute = false
            });
            _notifyIcon.Dispose();
            Shutdown();
        }
    }
}

