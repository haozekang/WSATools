using AdvancedSharpAdbClient;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
using System.Windows.Controls;
using System.Windows.Input;
using WSATools.Enums;
using WSATools.ExtendMethod;
using WSATools.Models;

namespace WSATools.ViewModels
{
    public class ShellModel : ViewModelBase
    {
        bool load = false;
        BackgroundWorker background = null;

        private int ConsoleOutputLength = 2;
        private string _consoleOutputOld = "> ";
        private string _consoleOutput = "> ";
        public string ConsoleOutput
        {
            get
            {
                return _consoleOutput;
            }
            set
            {
                Set(ref _consoleOutput, value);
            }
        }

        private string _commandString = "";
        public string CommandString
        {
            get
            {
                return _commandString;
            }
            set
            {
                Set(ref _commandString, value);
            }
        }

        private List<string> CommandList = new List<string>();
        private int CommandShowIndex = 0;

        public ShellModel()
        {
        }

        private void DoCommand_DoWork(object sender, DoWorkEventArgs e)
        {
            string command = $"{CommandString.StringTrim()}";
            Debug.WriteLine($"Command:{command}");
            if (command.IsBlank())
            {
                return;
            }
            CommandList.Add(command);
            switch (command)
            {
                case "clear":
                    ConsoleOutput = "";
                    break;
                default:
                    ConsoleOutputReceiver outputReceiver = new ConsoleOutputReceiver();
                    App.Client.ExecuteRemoteCommand(command, App.Device, outputReceiver);
                    string output = outputReceiver.ToString();

                    if (ConsoleOutput.EndsWith(Environment.NewLine))
                    {
                        ConsoleOutput += output;
                    }
                    else
                    {
                        ConsoleOutput += $"{Environment.NewLine}{output}";
                    }
                    break;
            }

            e.Result = e.Argument;
        }

        private void DoCommand_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            background.DoWork -= DoCommand_DoWork;
            background.RunWorkerCompleted -= DoCommand_RunWorkerCompleted;
            string msg = e.Result as string;
            if (msg.IsNotBlank())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"{msg}", $"错误");
                });
                return;
            }
            if (ConsoleOutput.EndsWith(Environment.NewLine) || ConsoleOutput.IsBlank())
            {
                ConsoleOutput += $"> ";
            }
            else
            {
                ConsoleOutput += $"{Environment.NewLine}> ";
            }
            ConsoleOutputLength = ConsoleOutput.Length;
            CommandString = string.Empty;
            CommandShowIndex = CommandList.Count;
            if (e.Result is TextBox txt)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    _consoleOutputOld = ConsoleOutput;
                    txt.Text = ConsoleOutput;
                    txt.SelectionStart = txt.Text.Length;
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

        public ICommand txt_command_PreviewKeyDown
        {
            get
            {
                return new RelayCommand<KeyEventArgs>((key) =>
                {
                    if (key == null)
                    {
                        return;
                    }
                    if (background.IsBusy)
                    {
                        key.Handled = true;
                        return;
                    }
                    switch (key.Key)
                    {
                        case Key.Back:
                            if (ConsoleOutput.Length == ConsoleOutputLength)
                            {
                                key.Handled = true;
                                return;
                            }
                            break;
                        case Key.Left:
                        case Key.Right:
                            break;
                        case Key.Up:
                            if (CommandShowIndex > 0)
                            {
                                CommandShowIndex--;
                                string cmd = CommandList.ElementAt(CommandShowIndex);
                                ConsoleOutput = _consoleOutputOld + cmd;
                            }
                            key.Handled = true;
                            if (key.Source is TextBox txtl)
                            {
                                txtl.SelectionStart = txtl.Text.Length;
                            }
                            break;
                        case Key.Down:
                            if (CommandShowIndex < CommandList.Count - 1)
                            {
                                CommandShowIndex++;
                                string cmd = CommandList.ElementAt(CommandShowIndex);
                                ConsoleOutput = _consoleOutputOld + cmd;
                            }
                            else if(CommandShowIndex == CommandList.Count - 1)
                            {
                                CommandShowIndex = CommandList.Count;
                                ConsoleOutput = _consoleOutputOld;
                            }
                            key.Handled = true;
                            if (key.Source is TextBox txtr)
                            {
                                txtr.SelectionStart = txtr.Text.Length;
                            }
                            break;
                        default:
                            if (key.Source is TextBox txtd)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    txtd.SelectionStart = ConsoleOutput.Length;
                                });
                            }
                            break;
                    }
                });
            }
        }

        public ICommand txt_command_KeyUp
        {
            get
            {
                return new RelayCommand<KeyEventArgs>((key) =>
                {
                    if (key == null)
                    {
                        return;
                    }
                    CommandString = ConsoleOutput.Substring(ConsoleOutputLength);
                    if (key.Key == Key.Enter)
                    {
                        ConsoleOutputLength = ConsoleOutput.Length;
                        if (background.IsBusy)
                        {
                            return;
                        }
                        background.DoWork += DoCommand_DoWork;
                        background.RunWorkerCompleted += DoCommand_RunWorkerCompleted;
                        background.RunWorkerAsync(key.Source);
                    }
                });
            }
        }
    }
}
