using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace WSATools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("kernel32.dll", EntryPoint = "WinExec")]
        public static extern int RunWinExec(string exeName, int operType);

        public static AdvancedAdbClient Client = null;
        public static DeviceData Device = null;
        public static PackageManager PackageManager = null;
        public static string PackageRoot = string.Empty;
        public static string WsaClientPath = string.Empty;
        public static string IconAndImageDirPath = string.Empty;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            using (var appx = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Appx"))
            {
                App.PackageRoot = appx.GetValue("PackageRoot") as string;
            }
            WsaClientPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                , "Microsoft"
                , "WindowsApps"
                , "MicrosoftCorporationII.WindowsSubsystemForAndroid_8wekyb3d8bbwe"
                , "WsaClient.exe");
            IconAndImageDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                , "Packages"
                , "MicrosoftCorporationII.WindowsSubsystemForAndroid_8wekyb3d8bbwe"
                , "LocalState");
            Debug.WriteLine($"WsaClientPath: {WsaClientPath}");
            Debug.WriteLine($"IconAndImageDirPath: {IconAndImageDirPath}");
            Client = new AdvancedAdbClient();
            Client.Connect(new System.Net.DnsEndPoint("127.0.0.1", 58526)); 
            
            ResourceDictionary langRd = null;
            try
            {
                langRd = Application.LoadComponent(new Uri(@"Language\zh_CN.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{DateTime.Now}:{ex}");
            }
            if (langRd != null)
            {
                Application.Current.Resources.MergedDictionaries.Add(langRd);
            }

            Environment.SetEnvironmentVariable("PATH", Path.Combine(Environment.CurrentDirectory, "adb"), EnvironmentVariableTarget.Process);
            App.RunWinExec(WsaClientPath, 1);

            MainWindow mw = new MainWindow();
            this.MainWindow = mw;
            this.MainWindow.Show();
        }
    }
}
