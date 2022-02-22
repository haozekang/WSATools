using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSATools.Enums;

namespace WSATools.Models
{
    public class PathItemModel : ViewModelBase
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

        private string _fullName;
        public string FullName
        {
            get
            {
                return _fullName;
            }
            set
            {
                Set(ref _fullName, value);
            }
        }

        private string _extension;
        public string Extension
        {
            get
            {
                return _extension;
            }
            set
            {
                Set(ref _extension, value);
            }
        }

        private string _reallyPath;
        public string ReallyPath
        {
            get
            {
                return _reallyPath;
            }
            set
            {
                Set(ref _reallyPath, value);
            }
        }

        private PathTypeEnum _type;
        public PathTypeEnum Type
        {
            get
            {
                return _type;
            }
            set
            {
                Set(ref _type, value);
            }
        }

        private DateTime? _lastModifyTime;
        public DateTime? LastModifyTime
        {
            get
            {
                return _lastModifyTime;
            }
            set
            {
                Set(ref _lastModifyTime, value);
            }
        }

        private int _sizeByte;
        public int SizeByte
        {
            get
            {
                return _sizeByte;
            }
            set
            {
                Set(ref _sizeByte, value);
            }
        }

        public double SizeKilobyte
        {
            get
            {
                return _sizeByte / 1024.0;
            }
        }

        public double SizeMByte
        {
            get
            {
                return _sizeByte / 1024.0 / 1024.0;
            }
        }

        public double SizeGigabyte
        {
            get
            {
                return _sizeByte / 1024.0 / 1024.0 / 1024.0;
            }
        }
    }
}
