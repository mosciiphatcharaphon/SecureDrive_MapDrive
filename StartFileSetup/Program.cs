using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace StartFileSetup
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args[0] == "install")
            {
                try
                {
                    string pathKS2Drive = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KS2Drive");
                    if (Directory.Exists(pathKS2Drive))
                    {
                        Directory.Delete(pathKS2Drive);
                    }
                    string exePath = Process.GetCurrentProcess().MainModule.FileName;
                    exePath = $"\"{exePath}\"";
                    using (var rkApp = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", writable: true))
                    {
                        rkApp.DeleteValue("SecureDriveAutoMap", false);
                    }
                }
                catch (Exception e) 
                {
                    
                }
            }
        }
    }
}
