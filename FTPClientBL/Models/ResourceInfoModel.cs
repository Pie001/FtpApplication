using System;
using FTPClient.BL.Settings;

namespace FTPClient.BL.Model
{
    public class ResourceInfoModel
    {
        public string Name
        {
            get;
            set;
        }

        public DateTime LastWrittenTime
        {
            get;
            set;
        }

        public long Length
        {
            get;
            set;
        }

        public string Extension
        {
            get;
            set;
        }

        public string FullPath
        {
            get;
            set;
        }

        public Constants.ResourceType Type
        {
            get;
            set;
        }
    }
}