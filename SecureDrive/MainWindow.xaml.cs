using Microsoft.Win32;
using Newtonsoft.Json;
using SecureDrive.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using static SecureDrive.Models.OcsResponseModel;


namespace SecureDrive
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string PathConfig = "";
        private string ServerURL = $"";
        private string ServerLogin = "";
        private string ServerPassword = "";
        private string PathPermission = "";
        private string pathKS2Drive;
        private string configSecurePath;
        private Configuration config = new Configuration();
        public MainWindow()
        {
            InitializeComponent();
            pathKS2Drive = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KS2Drive");
            if (!Directory.Exists(pathKS2Drive))
            {
                Directory.CreateDirectory(pathKS2Drive);
            }
            configSecurePath = System.IO.Path.Combine(pathKS2Drive, "configSecure.json");
            if (File.Exists(configSecurePath)) 
            {
                var configSecureJson = File.ReadAllText(configSecurePath);
                var configSecure = JsonConvert.DeserializeObject<ConfigSecureDrive>(configSecureJson);
                if (configSecure != null)
                {
                    LoginTextBox.Text = configSecure.ServerLogin;
                    PasswordBox.Password = configSecure.ServerPassword;
                    AutoMountCheckBox.IsChecked = configSecure.AutoMount;
                    StartWithWindowsCheckBox.IsChecked = configSecure.StartWithWindows;
                }

            }
            
        }

        public async Task<bool> Mount()
        {
            try
            {
                if (!File.Exists(configSecurePath))
                {
                    this.Show();
                    return false;
                }

                PathPermission = System.IO.Path.Combine(pathKS2Drive, "Permission");
                if (!Directory.Exists(PathPermission))
                {
                    Directory.CreateDirectory(PathPermission);
                }

                PathConfig = System.IO.Path.Combine(pathKS2Drive, "config.json");
                if (File.Exists(PathConfig))
                {
                    File.Delete(PathConfig);
                }

                var configSecureJson = File.ReadAllText(configSecurePath);
                var configSecure = JsonConvert.DeserializeObject<ConfigSecureDrive>(configSecureJson);
                string Server = configSecure.ServerURL;
                ServerLogin = configSecure.ServerLogin;
                ServerPassword = configSecure.ServerPassword;

                OcsResponse res = await GetGroupFolderXml(Server, ServerLogin, ServerPassword);
                if (res?.Data?.Elements == null)
                {
                    // ถ้าไม่สามารถดึงข้อมูลได้
                    return false;
                }

                string driveLetter = "E";
                foreach (var folder in res.Data.Elements)
                {
                    var IsDrive = new List<string>();
                    var groups = folder.GetGroups();
                    if (groups.Count == 0)
                    {
                        config.Permission = new List<string> { "31" };
                    }
                    else
                    {
                        var permissionList = new List<string>();
                        foreach (var group in groups)
                        {
                            IsDrive.Add(group.Value.IsDrive.ToString());
                            int permission = group.Value.Permissions;
                            permissionList.Add(permission.ToString());
                        }
                        config.Permission = permissionList.Distinct().ToList();
                    }

                    if (folder.Id != -1 && !string.IsNullOrEmpty(folder.ParentsPath))
                    {
                        config.ServerURL = $"{ServerURL.TrimEnd('/')}/{folder.ParentsPath.TrimStart('/')}/{folder.MountPoint.TrimStart('/')}";
                        config.VolumeLabel = folder.MountPoint;
                    }
                    else if (folder.Id != -1 && string.IsNullOrEmpty(folder.ParentsPath))
                    {
                        config.ServerURL = $"{ServerURL.TrimEnd('/')}/{folder.MountPoint.TrimStart('/')}";
                        config.VolumeLabel = folder.MountPoint;
                    }
                    else if (folder.Id == -1)
                    {
                        config.ServerURL = ServerURL;
                        config.VolumeLabel = $"Drive is {ServerLogin}";
                    }

                    if (IsDrive.Contains("1") || folder.Id == -1)
                    {
                        config.DriveLetter = GetNextDriveLetter(ref driveLetter);
                        config.quota = ulong.Parse(folder.Quota.ToString());
                        config.size = ulong.Parse(folder.Size.ToString());
                        config.ServerLogin = ServerLogin;
                        config.ServerPassword = ServerPassword;

                            byte[] databyte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(config));
                            string data = Convert.ToBase64String(databyte);
                            File.WriteAllText(PathConfig, data);
                            string filePermission = System.IO.Path.Combine(PathPermission, $"{config.DriveLetter}.json");
                            var permis = new Permission
                            {
                                URLPath = config.ServerURL,
                                PermissionFolder = config.Permission,
                                Drive = config.DriveLetter
                            };
                            File.WriteAllText(filePermission, JsonConvert.SerializeObject(permis));
                            string filepath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "k2sdrive");
                            string filename = "KS2Drive.exe";
                            string fullExePath = System.IO.Path.Combine(filepath, filename);
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
                                //MessageBox.Show($"ไม่พบไฟล์: {fullExePath}", "ไม่พบไฟล์", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                            Thread.Sleep(10000);
                            File.Delete(PathConfig);
                        }
                    }
                }
                else 
                {
                    this.Show();
                }

                return true;
            }
            catch (Exception ex)
            {
                // log error
                return false;
            }
        }


        private string GetNextDriveLetter(ref string currentLetter)
        {
            var usedDriveLetters = new HashSet<char>(
                DriveInfo.GetDrives()
                         .Select(d => char.ToUpper(d.Name[0]))
            );
            char letter;
            if (currentLetter == null)
            {
                letter = 'E';
            }
            else
            {
                letter = char.ToUpper(currentLetter[0]);
            }

            while (letter <= 'Z')
            {
                if (!usedDriveLetters.Contains(letter))
                {
                    currentLetter = $"{letter}:";
                    return currentLetter.Replace(":", "");
                }
                letter++;
            }
            throw new InvalidOperationException("No available drive letters from A to Z.");
        }
        private async Task<OcsResponse> GetGroupFolderXml(string Server, string ServerLogin, string ServerPassword)
        {
            try
            {
                ServerURL = $"{Server}{ServerLogin}/";
                var uri = new Uri(ServerURL);
                var baseUrl = $"{uri.Scheme}://{uri.Host}";
                if (!uri.IsDefaultPort)
                {
                    baseUrl += $":{uri.Port}";
                }
                string username = ServerLogin;
                string password = ServerPassword;
                string url = $"{baseUrl}/ocs/v2.php/cloud/users/{username}/groupFolder";

                using (var client = new HttpClient())
                {
                    var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    client.DefaultRequestHeaders.Add("OCS-APIRequest", "true");

                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var xml = await response.Content.ReadAsStringAsync();
                    var serializer = new XmlSerializer(typeof(OcsResponse));
                    using (var reader = new StringReader(xml))
                    {
                        return (OcsResponse)serializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            bool hasError = false;
            HideGeneralError();

            // เช็ค Login
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
                LoginErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }
            else
            {
                LoginErrorText.Visibility = Visibility.Collapsed;
            }

            // เช็ค Password
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                PasswordErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }
            else
            {
                PasswordErrorText.Visibility = Visibility.Collapsed;
            }

            // ถ้าเจอ error ไม่ต้อง save
            if (hasError)
            {
                return;
            }

            // ถ้าไม่เจอ error ทำการ save ต่อ
            var configSecure = new ConfigSecureDrive
            {
                ServerURL = "http://192.168.3.113/remote.php/dav/files/",
                ServerLogin = LoginTextBox.Text,
                ServerPassword = PasswordBox.Password,
                AutoMount = (bool)AutoMountCheckBox.IsChecked,
                StartWithWindows = (bool)StartWithWindowsCheckBox.IsChecked
            };

            ShowSuccessPopup();
            File.WriteAllText(System.IO.Path.Combine(pathKS2Drive, "configSecure.json"), JsonConvert.SerializeObject(configSecure, Formatting.Indented));
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (StartWithWindowsCheckBox.IsChecked == true)
            {
                rkApp.SetValue("SecureDriveAutoMap", System.Reflection.Assembly.GetEntryAssembly().Location);
            }
            else 
            {
                rkApp.DeleteValue("SecureDriveAutoMap", false);
            }
            rkApp.Close();
        }

        private void ShowSuccessPopup()
        {
            var popup = new Border
            {
                Width = 300,
                Height = 100,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCFFCC")),
                CornerRadius = new CornerRadius(5),
                VerticalAlignment = VerticalAlignment.Center,
                // HorizontalAlignment = HorizontalAlignment.Center,
                Child = new TextBlock
                {
                    Text = "บันทึกข้อมูลสำเร็จ",
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.Green,
                    // HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                }
            };

            PopupContainer.Children.Clear();
            PopupContainer.Children.Add(popup);
            PopupContainer.Visibility = Visibility.Visible;

            Task.Delay(2000).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    PopupContainer.Visibility = Visibility.Collapsed;
                    // ปิดหน้าต่างหลังจากปิด Popup
                    this.Close();
                });
            });
        }

        // เมื่อเริ่มพิมพ์ใน LoginTextBox
        private void LoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
                LoginErrorText.Visibility = Visibility.Collapsed;
            }
        }

        // เมื่อเริ่มพิมพ์ใน PasswordBox
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                PasswordErrorText.Visibility = Visibility.Collapsed;
            }
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

        public void ShowGeneralError(string message)
        {
            GeneralErrorText.Text = message;
            GeneralErrorText.Visibility = Visibility.Visible;
        }

        public void HideGeneralError()
        {
            GeneralErrorText.Visibility = Visibility.Collapsed;
        }

    }
    public static class GroupElementsExtensions
    {
        public static Dictionary<string, GroupInfo> GetGroups(this GroupElements element)
        {
            return element.Groups?.ToGroupDictionary() ?? new Dictionary<string, GroupInfo>();
        }

        public static GroupInfo GetGroup(this GroupElements element, string groupName)
        {
            return element.GetGroups().TryGetValue(groupName, out var group) ? group : null;
        }


    }

    
}
