using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using SharpAdbClient;
using SharpAdbClient.DeviceCommands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WSATools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public ObservableCollection<MenuItem> LanguageItems { get; } = new ObservableCollection<MenuItem>();

        public MainWindow()
        {
            InitializeComponent();
            InitControls();
        }

        public void InitControls()
        {
            LanguageItems.Add(new MenuItem (){ Header = "中文(简体)", Command = Language_Click, CommandParameter = LanguageEnum.中文简体 });
            LanguageItems.Add(new MenuItem (){ Header = "中文(繁體)", Command = Language_Click, CommandParameter = LanguageEnum.中文繁體 });
            LanguageItems.Add(new MenuItem (){ Header = "English", Command = Language_Click, CommandParameter = LanguageEnum.English });
            btn_language.ItemsSource = LanguageItems;

            AdbServer server = new AdbServer();
            var result = server.StartServer(@"adb\adb.exe", false);

            var monitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
            monitor.DeviceConnected += this.OnDeviceConnected;
            monitor.Start();
        }

        private void wsa_Loaded(object sender, RoutedEventArgs e)
        {
        }

        void OnDeviceConnected(object sender, DeviceDataEventArgs e)
        {
            Console.WriteLine($"The device {e.Device.Name} has connected to this PC");
        }

        public ICommand Language_Click
        {
            get
            {
                return new RelayCommand<LanguageEnum>((language) =>
                {
                    ResourceDictionary langRd = null;
                    try
                    {
                        switch (language)
                        {
                            case LanguageEnum.中文简体:
                                langRd = Application.LoadComponent(new Uri(@"Language\zh_CN.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary;
                                break;
                            case LanguageEnum.中文繁體:
                                langRd = Application.LoadComponent(new Uri(@"Language\zh_TW.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary;
                                break;
                            case LanguageEnum.English:
                                langRd = Application.LoadComponent(new Uri(@"Language\en_US.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"{DateTime.Now}:{ex}");
                    }
                    if (langRd != null)
                    {
                        Application.Current.Resources.MergedDictionaries.Add(langRd);
                    }
                    else
                    {
                        MessageBox.Show($"未发现【{language}】语言资源文件", $"错误", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                });
            }
        }

        private void btn_language_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_feedback_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MetroWindow_Drop(object sender, DragEventArgs e)
        {
            if (sender == null || e == null)
            {
                return;
            }
            if (e.Data == null)
            {
                return;
            }
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null)
            {
                return;
            }
        }
    }
}
