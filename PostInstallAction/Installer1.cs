using System.Diagnostics;
using System.ComponentModel;
using System.Configuration.Install;

[RunInstaller(true)]
public class PostInstallAction : Installer
{
    public override void Commit(System.Collections.IDictionary savedState)
    {
        base.Commit(savedState);

        string targetPath = Context.Parameters["targetdir"];
        string exePath = System.IO.Path.Combine(targetPath, "SecureDrive.exe"); // เปลี่ยนชื่อ exe

        Process.Start(exePath); // เรียกโปรแกรมขึ้นมา
    }
}
