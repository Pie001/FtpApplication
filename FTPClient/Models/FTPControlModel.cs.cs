using System.Collections.Generic;
using System.IO;
using FTPClient.BL.Model;

namespace FTPClient.Models
{
    public class FTPControlModel
    {
        public FTPControlModel()
        {
            LocalDriveList = Directory.GetLogicalDrives();
        }

        public string LocalServerPath
        {
            get;
            set;
        }

        public string RemoteServerPath
        {
            get;
            set;
        }

        public List<ResourceInfoModel> LocalList
        {
            get;
            set;
        }

        public List<ResourceInfoModel> RemoteList
        {
            get;
            set;
        }

        public string[] LocalDriveList
        {
            get;
            set;
        }

        public string OperationLog
        {
            get;
            set;
        }

        public List<HostDetailModel> HostList
        {
            get;
            set;
        }

        public int SelectedHostID
        {
            get;
            set;
        }

        public string LocalFilterPattern
        {
            get;
            set;
        }

        public string RemoteFilterPattern
        {
            get;
            set;
        }
    }
}