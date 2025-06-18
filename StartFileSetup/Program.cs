using System;
using System.Diagnostics;
using System.IO;

namespace StartFileSetup
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "install")
            {
                string filepath = AppContext.BaseDirectory;
                string filename = "SecureDrive.exe";
                string fullExePath = Path.Combine(filepath, filename);

                if (File.Exists(fullExePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = fullExePath,
                        UseShellExecute = true,
                        WorkingDirectory = filepath
                    });
                }
                else
                {
                    Console.WriteLine("ไม่พบไฟล์: " + fullExePath);
                }
            }
        }
    }
}
