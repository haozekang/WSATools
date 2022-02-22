using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSATools.ViewModels;

namespace WSATools.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            SimpleIoc.Default.Register<HomeModel>();
            SimpleIoc.Default.Register<InstallApkModel>();
            SimpleIoc.Default.Register<AppManagerModel>();
            SimpleIoc.Default.Register<FileTransferModel>();
            SimpleIoc.Default.Register<ShellModel>();
        }

        public static void Cleanup()
        {
        }

        public HomeModel Home => SimpleIoc.Default.GetInstance<HomeModel>();
        public InstallApkModel InstallApk => SimpleIoc.Default.GetInstance<InstallApkModel>();
        public AppManagerModel AppManager => SimpleIoc.Default.GetInstance<AppManagerModel>();
        public FileTransferModel FileTransfer => SimpleIoc.Default.GetInstance<FileTransferModel>();
        public ShellModel Shell => SimpleIoc.Default.GetInstance<ShellModel>();
    }
}
