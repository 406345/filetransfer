using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FileTransferClientWPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var args = Environment.GetCommandLineArgs();

            if( args.Length == 1)
            {
                this.Register();
            }
            else
            {
                base.OnStartup(e);
            }
        }
        private void RequestAdministratorAccess()
        {
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            //判断当前登录用户是否为管理员
            if (!principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                //创建启动对象
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                //startInfo.Arguments = Process.GetCurrentProcess().StartInfo.Arguments;
                startInfo.FileName = Environment.GetCommandLineArgs()[0];
                startInfo.Verb = "runas";
                try
                {
                    System.Diagnostics.Process.Start(startInfo);
                }
                catch
                {
                    return;
                }

                Process.GetCurrentProcess().Kill();
            }
        }
        private void Register()
        {
            this.RequestAdministratorAccess();

            try
            {
                Registry.ClassesRoot.DeleteSubKeyTree("*\\shell\\Send To Remote");
                Registry.ClassesRoot.DeleteSubKeyTree("Directory\\shell\\Send To Remote");
            }
            catch
            {

            }
             
            var rsg = Registry.ClassesRoot.CreateSubKey("*\\shell\\Send To Remote\\command", true); //true表可以修改
            rsg.SetValue(null, Environment.GetCommandLineArgs()[0] + " \"%1\"", RegistryValueKind.String);
            rsg.Close();

            rsg = Registry.ClassesRoot.CreateSubKey("Directory\\shell\\Send To Remote\\command", true); //true表可以修改
            rsg.SetValue(null, Environment.GetCommandLineArgs()[0] + " \"%1\"", RegistryValueKind.String);
            rsg.Close();
            MessageBox.Show("Successfully");
            Process.GetCurrentProcess().Kill();
        }
    }
}
