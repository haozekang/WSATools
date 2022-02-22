using AdvancedSharpAdbClient;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WSATools.Enums;
using WSATools.ExtendMethod;
using WSATools.Models;

namespace WSATools.ViewModels
{
    public class FileTransferModel : ViewModelBase
    {
        bool load = false;
        BackgroundWorker background = null;

        private string _directoryPath = "/";
        public string DirectoryPath
        {
            get
            {
                return _directoryPath;
            }
            set
            {
                Set(ref _directoryPath, value);
            }
        }
        public ObservableCollection<PathItemModel> PathItems { get; set; } = new ObservableCollection<PathItemModel>();

        public FileTransferModel()
        {
        }

        public class FileTransferModelFileParametersModel
        {
            public string SourceFileName  { get; set; } = string.Empty;
            public string DestFileName { get; set; } = string.Empty;

            public FileTransferModelFileParametersModel() : base()
            {
            }
            public FileTransferModelFileParametersModel(string SourceFileName, string DestFileName) : base()
            {
                this.SourceFileName = SourceFileName;
                this.DestFileName = DestFileName;
            }
        }

        public class FileTransferModelMultipleFilesParametersModel
        {
            public Dictionary<string, string> SourceAndDestFileNames { get; set; } = new Dictionary<string, string>();

            public FileTransferModelMultipleFilesParametersModel() : base()
            {
            }
            public FileTransferModelMultipleFilesParametersModel(Dictionary<string, string> SourceAndDestFileNames) : base()
            {
                this.SourceAndDestFileNames = SourceAndDestFileNames;
            }
        }

        public class FileTransferModelRenameParametersModel
        {
            public string OldFileName  { get; set; } = string.Empty;
            public string NewFileName { get; set; } = string.Empty;

            public FileTransferModelRenameParametersModel() : base()
            {
            }
            public FileTransferModelRenameParametersModel(string OldFileName, string NewFileName) : base()
            {
                this.OldFileName = OldFileName;
                this.NewFileName = NewFileName;
            }
        }

        private void RefreshFiles_DoWork(object sender, DoWorkEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                PathItems.Clear();
            });
            using (SyncService service = new SyncService(new AdbSocket(App.Client.EndPoint), App.Device))
            {
                var _fileList = service.GetDirectoryListing(DirectoryPath).OrderBy(x => x.Path).ToList();
                foreach (var file in _fileList)
                {
                    var item = new PathItemModel
                    {
                        Name = $"{Path.GetFileName(file.Path)}",
                        Extension = $"{Path.GetExtension(file.Path)}",
                        Type = GetPathTypeEnum(file.FileMode),
                        SizeByte = file.Size,
                        LastModifyTime = file.Time.DateTime,
                    };
                    if (DirectoryPath.EndsWith("/"))
                    {
                        item.FullName = $"{DirectoryPath}{file.Path}";
                    }
                    else
                    {
                        item.FullName = $"{DirectoryPath}/{file.Path}";
                    }
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        PathItems.Add(item);
                    });
                }
            }
        }

        private void RefreshFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= RefreshFiles_DoWork;
            background.RunWorkerCompleted -= RefreshFiles_RunWorkerCompleted;
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

        private void DownloadFile_DoWork(object sender, DoWorkEventArgs e)
        {
            FileTransferModelFileParametersModel args = e.Argument as FileTransferModelFileParametersModel;
            if (args == null)
            {
                e.Result = $"文件传输参数不能为空！";
                return;
            }
            try
            {
                using (SyncService service = new SyncService(new AdbSocket(App.Client.EndPoint), App.Device))
                {
                    using (Stream stream = File.OpenWrite(args.DestFileName))
                    {
                        service.Pull(args.SourceFileName, stream, null, CancellationToken.None);
                    }
                }
            }
            catch(Exception ex)
            {
                e.Result = $"{ex}";
            }
        }

        private void DownloadFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= DownloadFile_DoWork;
            background.RunWorkerCompleted -= DownloadFile_RunWorkerCompleted;
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
                    MessageBox.Show($"下载文件完成！", $"提示");
                });
            }
        }

        private void UploadFile_DoWork(object sender, DoWorkEventArgs e)
        {
            FileTransferModelFileParametersModel args = e.Argument as FileTransferModelFileParametersModel;
            if (args == null)
            {
                e.Result = $"文件传输参数不能为空！";
                return;
            }
            try
            {
                using (SyncService service = new SyncService(new AdbSocket(App.Client.EndPoint), App.Device))
                {
                    using (Stream stream = File.OpenRead(args.SourceFileName))
                    {
                        service.Push(stream, args.DestFileName, 777, DateTimeOffset.Now, null, CancellationToken.None);
                    }
                }
            }
            catch(Exception ex)
            {
                e.Result = $"{ex}";
            }
        }

        private void UploadFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= UploadFile_DoWork;
            background.RunWorkerCompleted -= UploadFile_RunWorkerCompleted;
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
                background.DoWork += RefreshFiles_DoWork;
                background.RunWorkerCompleted += RefreshFiles_RunWorkerCompleted;
                background.RunWorkerAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"上传文件完成！", $"提示");
                });
            }
        }

        private void UploadMultipleFiles_DoWork(object sender, DoWorkEventArgs e)
        {
            FileTransferModelMultipleFilesParametersModel args = e.Argument as FileTransferModelMultipleFilesParametersModel;
            if (args == null)
            {
                e.Result = $"文件传输参数不能为空！";
                return;
            }
            try
            {
                foreach (var filename in args.SourceAndDestFileNames.Keys)
                {
                    using (SyncService service = new SyncService(new AdbSocket(App.Client.EndPoint), App.Device))
                    {
                        using (Stream stream = File.OpenRead(filename))
                        {
                            service.Push(stream, args.SourceAndDestFileNames[filename], 777, DateTimeOffset.Now, null, CancellationToken.None);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                e.Result = $"{ex}";
            }
        }

        private void UploadMultipleFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= UploadMultipleFiles_DoWork;
            background.RunWorkerCompleted -= UploadMultipleFiles_RunWorkerCompleted;
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
                background.DoWork += RefreshFiles_DoWork;
                background.RunWorkerCompleted += RefreshFiles_RunWorkerCompleted;
                background.RunWorkerAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"上传文件完成！", $"提示");
                });
            }
        }

        private void Mkdir_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string dirName = e.Argument as string;
                if (dirName.IsBlank())
                {
                    e.Result = $"目录名称不能为空！";
                    return;
                }
                string command = string.Empty;
                if (DirectoryPath.EndsWith("/"))
                {
                    command = $"mkdir {DirectoryPath}{dirName}";
                }
                else
                {
                    command = $"mkdir {DirectoryPath}/{dirName}";
                }
                Debug.WriteLine($"Command:{command}");
                ConsoleOutputReceiver outputReceiver = new ConsoleOutputReceiver();
                App.Client.ExecuteRemoteCommand(command, App.Device, outputReceiver);
                string output = outputReceiver.ToString();
                if (output.IsNotBlank())
                {
                    e.Result = output;
                }
            }
            catch(Exception ex)
            {
                e.Result = $"{ex}";
            }
        }

        private void Mkdir_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= Mkdir_DoWork;
            background.RunWorkerCompleted -= Mkdir_RunWorkerCompleted;
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
                background.DoWork += RefreshFiles_DoWork;
                background.RunWorkerCompleted += RefreshFiles_RunWorkerCompleted;
                background.RunWorkerAsync();
            }
        }

        private void RenameFile_DoWork(object sender, DoWorkEventArgs e)
        {
            FileTransferModelRenameParametersModel args = e.Argument as FileTransferModelRenameParametersModel;
            if (args == null)
            {
                e.Result = $"文件命名参数不能为空！";
                return;
            }
            try
            {
                string command = string.Empty;
                if (DirectoryPath.EndsWith("/"))
                {
                    command = $"mv {DirectoryPath}{args.OldFileName} {DirectoryPath}{args.NewFileName}";
                }
                else
                {
                    command = $"mv {DirectoryPath}/{args.OldFileName} {DirectoryPath}/{args.NewFileName}";
                }
                Debug.WriteLine($"Command:{command}");
                ConsoleOutputReceiver outputReceiver = new ConsoleOutputReceiver();
                App.Client.ExecuteRemoteCommand(command, App.Device, outputReceiver);
                string output = outputReceiver.ToString();
                if (output.IsNotBlank())
                {
                    e.Result = output;
                }
            }
            catch(Exception ex)
            {
                e.Result = $"{ex}";
            }
        }

        private void RenameFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= RenameFile_DoWork;
            background.RunWorkerCompleted -= RenameFile_RunWorkerCompleted;
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
                background.DoWork += RefreshFiles_DoWork;
                background.RunWorkerCompleted += RefreshFiles_RunWorkerCompleted;
                background.RunWorkerAsync();
            }
        }

        private void DeleteFile_DoWork(object sender, DoWorkEventArgs e)
        {
            PathItemModel path = e.Argument as PathItemModel;
            if (path == null)
            {
                e.Result = $"文件参数不能为空！";
                return;
            }
            try
            {
                string command = string.Empty;
                if (DirectoryPath.EndsWith("/"))
                {
                    command = $"rm -rf \"{DirectoryPath}{path.Name}\"";
                }
                else
                {
                    command = $"rm -rf \"{DirectoryPath}/{path.Name}\"";
                }
                Debug.WriteLine($"Command:{command}");
                ConsoleOutputReceiver outputReceiver = new ConsoleOutputReceiver();
                App.Client.ExecuteRemoteCommand(command, App.Device, outputReceiver);
                string output = outputReceiver.ToString();
                if (output.IsNotBlank())
                {
                    e.Result = output;
                }
            }
            catch(Exception ex)
            {
                e.Result = $"{ex}";
            }
        }

        private void DeleteFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= DeleteFile_DoWork;
            background.RunWorkerCompleted -= DeleteFile_RunWorkerCompleted;
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
                background.DoWork += RefreshFiles_DoWork;
                background.RunWorkerCompleted += RefreshFiles_RunWorkerCompleted;
                background.RunWorkerAsync();
            }
        }

        private PathTypeEnum GetPathTypeEnum(string typeString)
        {
            if (typeString.IsBlank())
            {
                return PathTypeEnum.None;
            }
            switch (typeString[0])
            {
                case 'd':
                    return PathTypeEnum.文件夹;
                case '-':
                    return PathTypeEnum.文件;
                case 'l':
                    return PathTypeEnum.链接;
            }
            return PathTypeEnum.None;
        }

        private PathTypeEnum GetPathTypeEnum(UnixFileMode fileMode)
        {
            switch ((int)fileMode)
            {
                case 16384:
                case 16749:
                case 16832:
                case 16840:
                case 16877:
                case 16889:
                case 17913:
                    return PathTypeEnum.文件夹;
                case 33152:
                    return PathTypeEnum.文件;
                case 41344:
                case 41380:
                    return PathTypeEnum.链接;
            }
            return PathTypeEnum.None;
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
                    background.DoWork += RefreshFiles_DoWork;
                    background.RunWorkerCompleted += RefreshFiles_RunWorkerCompleted;
                    background.RunWorkerAsync();
                });
            }
        }

        public ICommand btn_download_file_Click
        {
            get
            {
                return new RelayCommand<PathItemModel>((path) =>
                {
                    if (path.Type != PathTypeEnum.文件)
                    {
                        return;
                    }
                    SaveFileDialog dialog = new SaveFileDialog();
                    dialog.Title = $"另存为";
                    dialog.FileName = $"{path.Name}";
                    dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    var flag = dialog.ShowDialog();
                    if (flag != true)
                    {
                        return;
                    }
                    FileTransferModelFileParametersModel parameters = new FileTransferModelFileParametersModel (){
                        SourceFileName = path.FullName,
                        DestFileName = dialog.FileName
                    };
                    background.DoWork += DownloadFile_DoWork;
                    background.RunWorkerCompleted += DownloadFile_RunWorkerCompleted;
                    background.RunWorkerAsync(parameters);
                });
            }
        }

        public ICommand btn_upload_file_Click
        {
            get
            {
                return new RelayCommand(() =>
                {
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.Filter = "所有文件|*.*";
                    dialog.Title = $"选择文件";
                    dialog.Multiselect = false;
                    dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    var flag = dialog.ShowDialog();
                    if (flag != true)
                    {
                        return;
                    }
                    FileTransferModelFileParametersModel parameters = new FileTransferModelFileParametersModel()
                    {
                        SourceFileName = dialog.FileName,
                    };
                    if (DirectoryPath.EndsWith("/"))
                    {
                        parameters.DestFileName = $"{DirectoryPath}{Path.GetFileName(dialog.FileName)}";
                    }
                    else
                    {
                        parameters.DestFileName = $"{DirectoryPath}/{Path.GetFileName(dialog.FileName)}";
                    }
                    background.DoWork += UploadFile_DoWork;
                    background.RunWorkerCompleted += UploadFile_RunWorkerCompleted;
                    background.RunWorkerAsync(parameters);
                });
            }
        }

        public ICommand btn_refresh_Click
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (background.IsBusy)
                    {
                        return;
                    }
                    background.DoWork += RefreshFiles_DoWork;
                    background.RunWorkerCompleted += RefreshFiles_RunWorkerCompleted;
                    background.RunWorkerAsync();
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
                    FileTransferModelMultipleFilesParametersModel parameters = new FileTransferModelMultipleFilesParametersModel();

                    files.ToList().ForEach(x =>
                    {
                        if (DirectoryPath.EndsWith("/"))
                        {
                            parameters.SourceAndDestFileNames.Add(x, $"{DirectoryPath}{Path.GetFileName(x)}");
                        }
                        else
                        {
                            parameters.SourceAndDestFileNames.Add(x, $"{DirectoryPath}/{Path.GetFileName(x)}");
                        }
                    });
                    background.DoWork += UploadMultipleFiles_DoWork;
                    background.RunWorkerCompleted += UploadMultipleFiles_RunWorkerCompleted;
                    background.RunWorkerAsync(parameters);
                });
            }
        }

        public ICommand txt_path_KeyDown
        {
            get
            {
                return new RelayCommand<KeyEventArgs>((key) =>
                {
                    if (key == null)
                    {
                        return;
                    }
                    if (key.Key != Key.Enter)
                    {
                        return;
                    }
                    if (background.IsBusy)
                    {
                        return;
                    }
                    background.DoWork += RefreshFiles_DoWork;
                    background.RunWorkerCompleted += RefreshFiles_RunWorkerCompleted;
                    background.RunWorkerAsync();
                });
            }
        }

        public ICommand btn_mkdir_Click
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (background.IsBusy)
                    {
                        return;
                    }
                    string dirName = string.Empty;
                    if (App.Current.MainWindow is MetroWindow mw)
                    {
                        dirName = mw.ShowModalInputExternal("新建目录", "请输入需要新建目录的名称：", new MetroDialogSettings
                        {
                            AffirmativeButtonText = "确认",
                            NegativeButtonText = "取消"
                        });
                    }
                    if (dirName.IsBlank())
                    {
                        return;
                    }
                    background.DoWork += Mkdir_DoWork;
                    background.RunWorkerCompleted += Mkdir_RunWorkerCompleted;
                    background.RunWorkerAsync(dirName);
                });
            }
        }

        public ICommand btn_delete_file_Click
        {
            get
            {
                return new RelayCommand<PathItemModel>((path) =>
                {
                    if (background.IsBusy)
                    {
                        return;
                    }
                    MessageDialogResult result = MessageDialogResult.Negative;
                    if (App.Current.MainWindow is MetroWindow mw)
                    {
                        result = mw.ShowModalMessageExternal("警告", "兄弟，我用的是rm -rf，你想好了再点！", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                        {
                            AffirmativeButtonText = "真的想好了",
                            NegativeButtonText = "算了，我再想想"
                        });
                    }
                    if (result != MessageDialogResult.Affirmative)
                    {
                        return;
                    }
                    background.DoWork += DeleteFile_DoWork;
                    background.RunWorkerCompleted += DeleteFile_RunWorkerCompleted;
                    background.RunWorkerAsync(path);
                });
            }
        }

        public ICommand btn_rename_file_Click
        {
            get
            {
                return new RelayCommand<PathItemModel>((path) =>
                {
                    if (background.IsBusy)
                    {
                        return;
                    }
                    string newName = string.Empty;
                    if (App.Current.MainWindow is MetroWindow mw)
                    {
                        newName = mw.ShowModalInputExternal("新建目录", "请输入需要新建目录的名称：", new MetroDialogSettings
                        {
                            AffirmativeButtonText = "确认",
                            NegativeButtonText = "取消"
                        });
                    }
                    if (newName.IsBlank())
                    {
                        return;
                    }
                    if (newName.Equals(path.Name))
                    {
                        MessageBox.Show($"名称不能和旧名称相同");
                        return;
                    }
                    FileTransferModelRenameParametersModel parameters = new FileTransferModelRenameParametersModel()
                    {
                        OldFileName = path.Name,
                        NewFileName = newName,
                    };
                    background.DoWork += RenameFile_DoWork;
                    background.RunWorkerCompleted += RenameFile_RunWorkerCompleted;
                    background.RunWorkerAsync(parameters);
                });
            }
        }

        public ICommand btn_path_item_DoubleClick
        {
            get
            {
                return new RelayCommand<PathItemModel>((path) =>
                {
                    if (path == null)
                    {
                        return;
                    }
                    if (background.IsBusy)
                    {
                        return;
                    }
                    if (".".Equals(path.Name))
                    {
                        btn_refresh_Click.Execute(null);
                        return;
                    }
                    if ("..".Equals(path.Name))
                    {
                        if (DirectoryPath.Count(x => x == '/') > 1)
                        {
                            DirectoryPath = DirectoryPath.Substring(0, DirectoryPath.LastIndexOf("/"));
                        }
                        else if (DirectoryPath.Count(x => x == '/') == 1)
                        {
                            DirectoryPath = "/";
                        }
                        btn_refresh_Click.Execute(null);
                        return;
                    }
                    if (path.Type == PathTypeEnum.文件)
                    {
                        btn_download_file_Click.Execute(path);
                        return;
                    }
                    if (path.Type == PathTypeEnum.链接 || path.Type == PathTypeEnum.文件夹)
                    {
                        if (DirectoryPath.IsBlank() || "/".Equals(DirectoryPath.StringTrim()))
                        {
                            DirectoryPath = $"/{path.Name}";
                        }
                        else
                        {
                            if (!DirectoryPath.EndsWith("/"))
                            {
                                DirectoryPath += "/";
                            }
                            DirectoryPath += $"{path.Name}";
                        }
                        btn_refresh_Click.Execute(null);
                        return;
                    }
                });
            }
        }
    }
}
