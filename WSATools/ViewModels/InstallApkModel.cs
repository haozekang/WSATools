using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WSATools.ExtendMethod;

namespace WSATools.ViewModels
{
    public class InstallApkModel : ViewModelBase
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

        public string _apkFilePath;
        public string ApkFilePath
        {
            get 
            {
                return _apkFilePath;
            }
            set 
            {
                Set(ref _apkFilePath, value);
            }
        }

        public InstallApkModel()
        {
        }

        private void InstallApk_DoWork(object sender, DoWorkEventArgs e)
        {
            if (App.PackageManager == null)
            {
                e.Result = $"PackageManager未实例化！";
                return;
            }
            string filepath = e.Argument as string;
            if (!File.Exists(filepath))
            {
                e.Result = $"文件不存在！";
                return;
            }
            App.PackageManager.InstallPackage(ApkFilePath, true);
        }

        private void InstallApk_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= InstallApk_DoWork;
            background.RunWorkerCompleted -= InstallApk_RunWorkerCompleted;
            string msg = e.Result as string;
            if (msg.IsNotBlank())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"{msg}", $"错误");
                });
                return;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"安装完成！", $"提示");
                });
            }
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
                });
            }
        }

        public ICommand btn_show_select_apk_dialog_Click
        {
            get
            {
                return new RelayCommand(() =>
                {
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.Filter = "APK文件|*.apk";
                    dialog.Title = $"选择APK文件";
                    dialog.Multiselect = false;
                    dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    var flag = dialog.ShowDialog();
                    if (flag != true)
                    {
                        return;
                    }
                    else
                    {
                        ApkFilePath = dialog.FileName;
                    }
                });
            }
        }

        public ICommand btn_install_apk_Click
        {
            get
            {
                return new RelayCommand(() =>
                {
                    background.DoWork += InstallApk_DoWork;
                    background.RunWorkerCompleted += InstallApk_RunWorkerCompleted;
                    background.RunWorkerAsync(ApkFilePath);
                });
            }
        }
    }
}
