using AdvancedSharpAdbClient;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WSATools.Enums;
using WSATools.ExtendMethod;
using WSATools.Models;

namespace WSATools.ViewModels
{
    public class AppManagerModel : ViewModelBase
    {
        bool load = false;
        BackgroundWorker background = null;
        public bool IsBusy 
        {
            get
            {
                return background?.IsBusy ?? false;
            }
        }

        public ObservableCollection<AppItemModel> AppItems { get; set; } = new ObservableCollection<AppItemModel>();

        public class AppManagerModelMultipleApkFilesParametersModel
        {
            public List<string> ApkFiles { get; set; } = new List<string>();

            public AppManagerModelMultipleApkFilesParametersModel() : base()
            {
            }
            public AppManagerModelMultipleApkFilesParametersModel(List<string> ApkFiles) : base()
            {
                this.ApkFiles = ApkFiles;
            }
        }

        public AppManagerModel()
        {
        }

        private void RefreshApp_DoWork(object sender, DoWorkEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                AppItems.Clear();
            });
            ConsoleOutputReceiver outputReceiver = null;
            AppRunStateEnum runState = AppRunStateEnum.未运行;

            outputReceiver = new ConsoleOutputReceiver();
            var command = $"pm list packages -3";
            Debug.WriteLine($"Command:{command}");
            App.Client.ExecuteRemoteCommand(command, App.Device, outputReceiver);
            string output = outputReceiver.ToString().StringTrim();
            if (output.IsBlank())
            {
                return;
            }
            var packlist = output.Split(new string[] { "package:", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            command = $"ps -ef|egrep \"{string.Join("|", packlist.Select(x => x + "$"))}\"";
            outputReceiver = new ConsoleOutputReceiver();
            App.Client.ExecuteRemoteCommand(command, App.Device, outputReceiver);
            output = outputReceiver.ToString().StringTrim();
            var packrunlist = new string[] { };
            if (output.IsNotBlank())
            {
                packrunlist = output.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }

            foreach (var packName in packlist)
            {
                using (var appx = Registry.CurrentUser.OpenSubKey($"Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{packName}"))
                {
                    var appName = (appx?.GetValue("DisplayName") as string).StringTrim();
                    var packageName = packName;
                    var version = (appx?.GetValue("DisplayVersion") as string).StringTrim();
                    string packageRun = packrunlist.Where(x => x.Contains(packageName)).FirstOrDefault();
                    int? pid = null;
                    if (packageRun.IsNotBlank())
                    {
                        runState = AppRunStateEnum.正在运行;
                        var runArgs = packageRun.Split(new char[] { ' ', '?', ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        int _pid = 0;
                        if (int.TryParse(runArgs[1], out _pid))
                        {
                            pid = _pid;
                        }
                    }
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        AppItems.Add(new AppItemModel()
                        {
                            Name = appName,
                            IconPath = Path.Combine(App.IconAndImageDirPath, $"{packageName}.ico"),
                            ImagePath = Path.Combine(App.IconAndImageDirPath, $"{packageName}.png"),
                            PackageName = packageName,
                            Version = version,
                            RunState = runState,
                            Pid = pid,
                        });
                    });
                }
            }
        }

        private void RefreshApp_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= RefreshApp_DoWork;
            background.RunWorkerCompleted -= RefreshApp_RunWorkerCompleted;
            string msg = e.Result as string;
            if (msg.IsNotBlank())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"{msg}", $"错误");
                });
                return;
            }
        }

        private void MultipleInstallApp_DoWork(object sender, DoWorkEventArgs e)
        {
            if (App.PackageManager == null)
            {
                e.Result = $"PackageManager未实例化！";
                return;
            }
            AppManagerModelMultipleApkFilesParametersModel parameters = e.Argument as AppManagerModelMultipleApkFilesParametersModel;
            if (parameters == null)
            {
                e.Result = $"参数不能为空！";
                return;
            }
            foreach (var path in parameters.ApkFiles)
            {
                try
                {
                    App.PackageManager.InstallPackage(path, true);
                }
                catch(Exception ex)
                {
                    e.Result += $"【{path}】安装失败：{ex}{Environment.NewLine}";
                }
            }
        }

        private void MultipleInstallApp_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= MultipleInstallApp_DoWork;
            background.RunWorkerCompleted -= MultipleInstallApp_RunWorkerCompleted;
            string msg = e.Result as string;
            if (msg.IsNotBlank())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"{msg}", $"错误");
                });
                return;
            }
            background.DoWork += RefreshApp_DoWork;
            background.RunWorkerCompleted += RefreshApp_RunWorkerCompleted;
            background.RunWorkerAsync();
        }

        private void StartApp_DoWork(object sender, DoWorkEventArgs e)
        {
            if (App.Client == null)
            {
                e.Result = $"Client未实例化！";
                return;
            }
            if (App.Device == null)
            {
                e.Result = $"Device未实例化！";
                return;
            }
            AppItemModel app = e.Argument as AppItemModel;
            if (app == null)
            {
                e.Result = $"App参数不能为空！";
                return;
            }
            App.Client.StartApp(App.Device, app.PackageName);
        }

        private void StartApp_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= StartApp_DoWork;
            background.RunWorkerCompleted -= StartApp_RunWorkerCompleted;
            string msg = e.Result as string;
            if (msg.IsNotBlank())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"启动App失败：{msg}", $"错误");
                });
                return;
            }
            background.DoWork += RefreshApp_DoWork;
            background.RunWorkerCompleted += RefreshApp_RunWorkerCompleted;
            background.RunWorkerAsync();
        }

        private void StopApp_DoWork(object sender, DoWorkEventArgs e)
        {
            if (App.Client == null)
            {
                e.Result = $"Client未实例化！";
                return;
            }
            if (App.Device == null)
            {
                e.Result = $"Device未实例化！";
                return;
            }
            AppItemModel app = e.Argument as AppItemModel;
            if (app == null)
            {
                e.Result = $"App参数不能为空！";
                return;
            }
            App.Client.StopApp(App.Device, app.PackageName);
        }

        private void StopApp_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= StopApp_DoWork;
            background.RunWorkerCompleted -= StopApp_RunWorkerCompleted;
            string msg = e.Result as string;
            if (msg.IsNotBlank())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"{msg}", $"错误");
                });
                return;
            }
            background.DoWork += RefreshApp_DoWork;
            background.RunWorkerCompleted += RefreshApp_RunWorkerCompleted;
            background.RunWorkerAsync();
        }

        private void UninstallApp_DoWork(object sender, DoWorkEventArgs e)
        {
            AppItemModel app = e.Argument as AppItemModel;
            if (app == null)
            {
                e.Result = $"App参数不能为空！";
                return;
            }
            if (App.WsaClientPath.IsBlank())
            {
                return;
            }
            if (!System.IO.File.Exists(App.WsaClientPath))
            {
                return;
            }
            var uri = $"{App.WsaClientPath} /uninstall {app.PackageName}";
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = uri,
            };
            process.Start();
        }

        private void UninstallApp_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= UninstallApp_DoWork;
            background.RunWorkerCompleted -= UninstallApp_RunWorkerCompleted;
            string msg = e.Result as string;
            if (msg.IsNotBlank())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"{msg}", $"错误");
                });
                return;
            }
            background.DoWork += RefreshApp_DoWork;
            background.RunWorkerCompleted += RefreshApp_RunWorkerCompleted;
            background.RunWorkerAsync();
        }

        private void CreateAppShortcut_DoWork(object sender, DoWorkEventArgs e)
        {
            if (App.WsaClientPath.IsBlank())
            {
                return;
            }
            AppItemModel app = e.Argument as AppItemModel;
            if (app == null)
            {
                e.Result = $"App参数不能为空！";
                return;
            }
            if (!System.IO.File.Exists(App.WsaClientPath))
            {
                return;
            }
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{app.Name}.lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = shell.CreateShortcut(path) as IWshShortcut;
            shortcut.Arguments = $"/launch wsa://{app.PackageName}";
            shortcut.Description = "WsaClient.exe";
            shortcut.TargetPath = App.WsaClientPath;
            shortcut.IconLocation = app.IconPath;
            shortcut.Save();
        }

        private void CreateAppShortcut_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= CreateAppShortcut_DoWork;
            background.RunWorkerCompleted -= CreateAppShortcut_RunWorkerCompleted;
            string msg = e.Result as string;
            if (msg.IsNotBlank())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"{msg}", $"错误");
                });
                return;
            }
        }

        public ICommand Loaded
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (background == null)
                    {
                        background = new BackgroundWorker();
                    }
                    if (background.IsBusy)
                    {
                        return;
                    }
                    background.DoWork += RefreshApp_DoWork;
                    background.RunWorkerCompleted += RefreshApp_RunWorkerCompleted;
                    background.RunWorkerAsync();
                });
            }
        }

        public ICommand btn_start_apk_Click
        {
            get
            {
                return new RelayCommand<AppItemModel>((app) =>
                {
                    if (app == null)
                    {
                        return;
                    }
                    if (background.IsBusy)
                    {
                        return;
                    }
                    background.DoWork += StartApp_DoWork;
                    background.RunWorkerCompleted += StartApp_RunWorkerCompleted;
                    background.RunWorkerAsync(app);
                });
            }
        }

        public ICommand btn_stop_apk_Click
        {
            get
            {
                return new RelayCommand<AppItemModel>((app) =>
                {
                    if (app == null)
                    {
                        return;
                    }
                    if (background.IsBusy)
                    {
                        return;
                    }
                    background.DoWork += StopApp_DoWork;
                    background.RunWorkerCompleted += StopApp_RunWorkerCompleted;
                    background.RunWorkerAsync(app);
                });
            }
        }

        public ICommand btn_uninstall_apk_Click
        {
            get
            {
                return new RelayCommand<AppItemModel>((app) =>
                {
                    if (app == null)
                    {
                        return;
                    }
                    background.DoWork += UninstallApp_DoWork;
                    background.RunWorkerCompleted += UninstallApp_RunWorkerCompleted;
                    background.RunWorkerAsync(app);
                });
            }
        }

        public ICommand btn_create_apk_icon_Click
        {
            get
            {
                return new RelayCommand<AppItemModel>((app) =>
                {
                    if (app == null)
                    {
                        return;
                    }
                    background.DoWork += UninstallApp_DoWork;
                    background.RunWorkerCompleted += UninstallApp_RunWorkerCompleted;
                    background.RunWorkerAsync(app);
                });
            }
        }

        public ICommand datagrid_Drop
        {
            get
            {
                return new RelayCommand<DragEventArgs>((args) =>
                {
                    if (args == null)
                    {
                        return;
                    }
                    if (args.Data == null)
                    {
                        return;
                    }
                    string[] files = (string[])args.Data.GetData(DataFormats.FileDrop);
                    if (files == null)
                    {
                        return;
                    }
                    if (files.Length <= 0)
                    {
                        return;
                    }
                    if (background.IsBusy)
                    {
                        return;
                    }
                    AppManagerModelMultipleApkFilesParametersModel parameters = new AppManagerModelMultipleApkFilesParametersModel();
                    files.ToList().ForEach(x =>
                    {
                        parameters.ApkFiles.Add(x);
                    });
                    background.DoWork += MultipleInstallApp_DoWork;
                    background.RunWorkerCompleted += MultipleInstallApp_RunWorkerCompleted;
                    background.RunWorkerAsync(parameters);
                });
            }
        }
    }
}
