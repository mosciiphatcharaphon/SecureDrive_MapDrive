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
using System.Xml.Serialization;
using static SecureDrive.Models.OcsResponseModel;
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
        private string PathConfig = "";
        private string ServerURL = $"";
        private string ServerLogin = "";
        private string ServerPassword = "";
        private string PathPermission = "";
        private string pathKS2Drive;
        private Configuration config = new Configuration();
        public MainWindow()
        {
            InitializeComponent();
            pathKS2Drive = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KS2Drive");
            if (!Directory.Exists(pathKS2Drive))
            {
                Directory.CreateDirectory(pathKS2Drive);
            }
            string configSecurePath = System.IO.Path.Combine(pathKS2Drive, "configSecure.json");
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

        public async void Mount()
        {
            
            try
            {
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
                string Server = "http://192.168.3.113/remote.php/dav/files/";
                ServerLogin = "user1";
                ServerPassword = "P@ssw0rdm1s";
                OcsResponse res = await GetGroupFolderXml(Server, ServerLogin, ServerPassword);
                if (res?.Data?.Elements == null)
                {
                    //เดี๋ยวทำ Log ไว้
                    return;
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
                    // folder ID  = -1 ตือ Main Drive
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
                        //File.WriteAllText(PathConfig, Protect(JsonConvert.SerializeObject(config)));
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
            catch (Exception ex)
            {
                //MessageBox.Show($"Error initializing configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            //string ServerURL = "http://192.168.3.113/remote.php/dav/files/";
            //string ServerLogin = LoginTextBox.Text;
            //string ServerPassword = PasswordBox.Password;
            //bool autoMount = AutoMountCheckBox.IsChecked == true;
            //bool startWithWindows = StartWithWindowsCheckBox.IsChecked == true;
            var configSecure = new ConfigSecureDrive
            {
                ServerURL = "http://192.168.3.113/remote.php/dav/files/",
                ServerLogin = LoginTextBox.Text,
                ServerPassword = PasswordBox.Password,
                AutoMount = (bool)AutoMountCheckBox.IsChecked,
                StartWithWindows = (bool)StartWithWindowsCheckBox.IsChecked
            };
            File.WriteAllText(System.IO.Path.Combine(pathKS2Drive, "configSecure.json"), JsonConvert.SerializeObject(configSecure, Formatting.Indented));
            this.Close();

            //System.Windows.MessageBox.Show($"Login: {login}\nPassword: {password}\nAuto-mount: {autoMount}\nStart with Windows: {startWithWindows}");
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
