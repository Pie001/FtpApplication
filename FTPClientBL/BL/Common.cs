using System;
using System.IO;
using FTPClient.BL.Model;

namespace FTPClient.BL
{
    public class Common
    {
        public static ResourceInfoModel GetLocalResourceInfo(string path, Settings.Constants.ResourceType type)
        {
            ResourceInfoModel ResourceInfo = null;

            FileSystemInfo fileInfo = null;

            switch (type)
            {
                case Settings.Constants.ResourceType.Directory:
                    fileInfo = new DirectoryInfo(path);
                    break;

                case Settings.Constants.ResourceType.File:
                    fileInfo = new FileInfo(path);
                    break;
            }

            if (fileInfo.Exists)
            {
                if ((fileInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden &&
                (fileInfo.Attributes & FileAttributes.System) != FileAttributes.System &&
                (fileInfo.Attributes & FileAttributes.Encrypted) != FileAttributes.Encrypted)
                {
                    ResourceInfo = new ResourceInfoModel();

                    ResourceInfo.Name = fileInfo.Name;
                    ResourceInfo.LastWrittenTime = fileInfo.LastAccessTime;
                    ResourceInfo.FullPath = fileInfo.FullName;
                    ResourceInfo.Extension = fileInfo.Extension;
                    ResourceInfo.Length = type == Settings.Constants.ResourceType.File ? ((FileInfo)fileInfo).Length : 0;
                    ResourceInfo.Type = type;
                }
            }

            return ResourceInfo;
        }

        public static ResourceInfoModel GetBackwardDirectoryInfo()
        {
            ResourceInfoModel fileInfo = null;

            fileInfo = new ResourceInfoModel();

            fileInfo.Name = "..";
            fileInfo.Type = Settings.Constants.ResourceType.Directory;

            return fileInfo;
        }

        public static string GetFileSizeTextFormat(long fileSize, int rounding = 1)
        {
            if (fileSize >= Math.Pow(2, 30))
            {
                return Math.Round(fileSize / Math.Pow(2, 30), rounding).ToString() + " GB";
            }
            if (fileSize >= Math.Pow(2, 20))
            {
                return Math.Round(fileSize / Math.Pow(2, 20), rounding).ToString() + " MB";
            }
            if (fileSize >= Math.Pow(2, 10))
            {
                return Math.Round(fileSize / Math.Pow(2, 10), rounding).ToString() + " KB";
            }

            return fileSize.ToString() + " Bytes";
        }
    }
}