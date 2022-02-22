using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using WSATools.ExtendMethod;

namespace WSATools.ViewModels
{
    public class HomeModel : ViewModelBase
    {
        bool load = false;
        BackgroundWorker background = null;

        public Visibility _connectedVisibility = Visibility.Collapsed;
        public Visibility ConnectedVisibility
        {
            get
            {
                return _connectedVisibility;
            }
            set
            {
                Set(ref _connectedVisibility, value);
            }
        }

        public Visibility _failedConnectedVisibility = Visibility.Collapsed;
        public Visibility FailedConnectedVisibility
        {
            get
            {
                return _failedConnectedVisibility;
            }
            set
            {
                Set(ref _failedConnectedVisibility, value);
            }
        }

        public Visibility _waitingConnectedVisibility = Visibility.Collapsed;
        public Visibility WaitingConnectedVisibility
        {
            get
            {
                return _waitingConnectedVisibility;
            }
            set
            {
                Set(ref _waitingConnectedVisibility, value);
            }
        }

        public Visibility _noWSAVisibility = Visibility.Visible;
        public Visibility NoWSAVisibility
        {
            get
            {
                return _noWSAVisibility;
            }
            set
            {
                Set(ref _noWSAVisibility, value);
            }
        }

        public string _adbVersion;
        public string AdbVersion
        {
            get
            {
                return _adbVersion;
            }
            set
            {
                Set(ref _adbVersion, value);
            }
        }

        public string _model;
        public string Model 
        {
            get 
            {
                return _model;
            }
            set 
            {
                Set(ref _model, value);
            }
        }
        public string _androidVersion;
        public string AndroidVersion
        {
            get
            {
                return _androidVersion;
            }
            set
            {
                Set(ref _androidVersion, value);
            }
        }

        public string _linuxKernel;
        public string LinuxKernel
        {
            get
            {
                return _linuxKernel;
            }
            set
            {
                Set(ref _linuxKernel, value);
            }
        }

        public string _totalMemory;
        public string TotalMemory
        {
            get
            {
                return _totalMemory;
            }
            set
            {
                Set(ref _totalMemory, value);
            }
        }

        public HomeModel()
        {
        }

        private void Connect_DoWork(object sender, DoWorkEventArgs e)
        {
            int i = 0;
            if (!File.Exists(App.WsaClientPath))
            {
                MessageBox.Show($"未查询到WSA系统", $"警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            NoWSAVisibility = Visibility.Collapsed;
            WaitingConnectedVisibility = Visibility.Visible;

            i = 0;
            App.Device = App.Client.GetDevices().Where(x => x.Serial == "127.0.0.1:58526").FirstOrDefault();
            if (App.Device == null)
            {
                return;
            }
            while (i++ < 50)
            {
                if (App.Device.State == DeviceState.Online)
                {
                    break;
                }
                Thread.Sleep(100);
                continue;
            }
            WaitingConnectedVisibility = Visibility.Collapsed;
            if (i >= 50)
            {
                FailedConnectedVisibility = Visibility.Visible;
                return;
            }

            ConnectedVisibility = Visibility.Visible;
            App.PackageManager = new PackageManager(App.Client, App.Device);
            Model = App.Device?.Model;
            AdbVersion = $"1.0.{App.Client.GetAdbVersion()}";
            ConsoleOutputReceiver outputReceiver = new ConsoleOutputReceiver();
            App.Client.ExecuteRemoteCommandAsync("getprop ro.build.version.release && uname -sr && head -qn 1 /proc/meminfo", App.Device, outputReceiver, CancellationToken.None).ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    return;
                }
                string output = outputReceiver.ToString();
                Debug.WriteLine(output);
                var arg = output?.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (arg.Length == 3) 
                {
                    AndroidVersion = $"Android {arg[0]}";
                    LinuxKernel = $"{arg[1]}";
                    string result = System.Text.RegularExpressions.Regex.Replace(arg[2], @"[^0-9]+", "");
                    TotalMemory = $"{(int.Parse(result) / 1024d).ToString("F0")} MB";
                }
            });
        }

        private void Connect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= Connect_DoWork;
            background.RunWorkerCompleted -= Connect_RunWorkerCompleted;
        }

        public ICommand Loaded
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (load)
                    {
                        return;
                    }
                    load = true;
                    background = new BackgroundWorker();
                    background.DoWork += Connect_DoWork;
                    background.RunWorkerCompleted += Connect_RunWorkerCompleted;
                    background.RunWorkerAsync();
                });
            }
        }

        public ICommand btn_open_wsa_setting_Click
        {
            get
            {
                return new RelayCommand(() =>
                {
                    
                    if (App.WsaClientPath.IsNotBlank())
                    {
                        Process process = new Process();
                        ProcessStartInfo processStartInfo2 = (process.StartInfo = new ProcessStartInfo
                        {
                            UseShellExecute = true,
                            FileName = "wsa-settings://",
                        });
                        process.Start();
                    }
                    else
                    {
                        MessageBox.Show($"未查询到WSA系统", $"警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                });
            }
        }

        public ICommand btn_open_android_setting_Click
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (App.Device == null)
                    {
                        MessageBox.Show($"未连接至WSA系统", $"警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    ConsoleOutputReceiver outputReceiver = new ConsoleOutputReceiver();
                    App.Client.ExecuteRemoteCommandAsync("am start com.android.settings/com.android.settings.Settings", App.Device, outputReceiver, CancellationToken.None).ContinueWith(x =>
                    {
                        if (x.IsFaulted)
                        {
                            return;
                        }
                    });
                });
            }
        }

        public ICommand btn_open_wsa_baidupan_page_Click
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Process.Start("explorer.exe", $"https://pan.baidu.com/s/1CuGFIQTvLsVNUkaj06pq1Q");
                });
            }
        }

        public ICommand btn_scan_qrcode_Click
        {
            get
            {
                return new RelayCommand(() =>
                {
                });
            }
        }
    }
}
