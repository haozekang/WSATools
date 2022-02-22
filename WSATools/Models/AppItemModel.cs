using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSATools.Enums;

namespace WSATools.Models
{
    public class AppItemModel : ViewModelBase
    {
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                Set(ref _name, value);
            }
        }

        private string _iconPath;
        public string IconPath
        {
            get
            {
                return _iconPath;
            }
            set
            {
                Set(ref _iconPath, value);
            }
        }

        private string _imagePath;
        public string ImagePath
        {
            get
            {
                return _imagePath;
            }
            set
            {
                Set(ref _imagePath, value);
            }
        }

        private string _packageName;
        public string PackageName
        {
            get
            {
                return _packageName;
            }
            set
            {
                Set(ref _packageName, value);
            }
        }

        private string _version;
        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                Set(ref _version, value);
            }
        }

        private int? _pid;
        public int? Pid
        {
            get
            {
                return _pid;
            }
            set
            {
                Set(ref _pid, value);
            }
        }

        private AppRunStateEnum _runState;
        public AppRunStateEnum RunState
        {
            get
            {
                return _runState;
            }
            set
            {
                Set(ref _runState, value);
            }
        }
    }
}
