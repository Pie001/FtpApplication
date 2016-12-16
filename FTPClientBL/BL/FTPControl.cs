using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using FTPClient.BL.Model;
using FTPClient.BL.Settings;

namespace FTPClient.BL
{
    public enum FileListStyle
    {
        UnixStyle,
        WindowsStyle,
        Unknown
    }

    public struct FileStruct
    {
        public string Flags;
        public string Owner;
        public string Group;
        public bool IsDirectory;
        public DateTime CreateTime;
        public string Name;
        public long length;
    }

    public class FTPControl
    {
        public static void UploadDirectory(string localFullPath, string remoteFullPath, HostDetailModel hostDetail, ref string uploadDirLog)
        {
            string remoteDirName = remoteFullPath.Substring(remoteFullPath.LastIndexOf("/") + 1, remoteFullPath.Length - (remoteFullPath.LastIndexOf("/") + 1));

            if (!FTPControl.IsExistResourceName(FTPControl.GetRemoteResourceList(remoteFullPath.Substring(0, remoteFullPath.Length - (remoteDirName.Length + 1)), hostDetail, false), remoteDirName, Constants.ResourceType.Directory))
            {
                FTPControl.MakeRemoteDirectory(remoteFullPath, hostDetail);
            }

            List<ResourceInfoModel> subDirResourceList = FTPControl.GetLocalResourceList(localFullPath);

            foreach (ResourceInfoModel subDirfile in subDirResourceList)
            {
                if (subDirfile.Type == Constants.ResourceType.File)
                {
                    string localPath = localFullPath + "\\" + subDirfile.Name;
                    string remotePath = remoteFullPath + "/" + subDirfile.Name;

                    FTPControl.UploadFile(localPath, remotePath, hostDetail, ref uploadDirLog);
                }
            }

            foreach (ResourceInfoModel subDirfile in subDirResourceList)
            {
                if (subDirfile.Type == Constants.ResourceType.Directory && subDirfile.Name != "..")
                {
                    string localPath = localFullPath + "\\" + subDirfile.Name;
                    string remotePath = remoteFullPath + "/" + subDirfile.Name;

                    FTPControl.UploadDirectory(localPath, remotePath, hostDetail, ref uploadDirLog);
                }
            }
        }

        public static void UploadFile(string localFullPath, string remoteFullPath, HostDetailModel hostDetail, ref string uploadFileLog)
        {
            var ftpReq = GetFtpWebRequest(remoteFullPath, hostDetail);
            ftpReq.Method = WebRequestMethods.Ftp.UploadFile;

            Stream reqStrm = ftpReq.GetRequestStream();
            var fs = new FileStream(localFullPath, FileMode.Open, FileAccess.Read);

            byte[] buffer = new byte[10485760];
            while (true)
            {
                int readSize = fs.Read(buffer, 0, buffer.Length);
                if (readSize == 0)
                {
                    break;
                }
                reqStrm.Write(buffer, 0, readSize);
            }
            fs.Close();
            reqStrm.Close();

            var ftpRes = (FtpWebResponse)ftpReq.GetResponse();
            ftpRes.Close();

            uploadFileLog += "[Upload] " + DateTime.Now + " " + localFullPath + " >> " + remoteFullPath + "\r\n";
        }

        public static void DownloadDirectory(string localFullPath, string remoteFullPath, HostDetailModel hostDetail, ref string downloadDirLog)
        {
            if (!Directory.Exists(localFullPath))
            {
                DirectoryInfo di = Directory.CreateDirectory(localFullPath);
            }

            List<ResourceInfoModel> subDirResourceList = FTPControl.GetRemoteResourceList(remoteFullPath, hostDetail, false);

            foreach (ResourceInfoModel subDirfile in subDirResourceList)
            {
                if (subDirfile.Type == Constants.ResourceType.File)
                {
                    string localPath = localFullPath + "\\" + subDirfile.Name;
                    string remotePath = remoteFullPath + "/" + subDirfile.Name;

                    FTPControl.DownloadFile(localPath, remotePath, hostDetail, ref downloadDirLog);
                }
            }

            foreach (ResourceInfoModel subDirfile in subDirResourceList)
            {
                if (subDirfile.Type == Constants.ResourceType.Directory)
                {
                    string localPath = localFullPath + "\\" + subDirfile.Name;
                    string remotePath = remoteFullPath + "/" + subDirfile.Name;

                    FTPControl.DownloadDirectory(localPath, remotePath, hostDetail, ref downloadDirLog);
                }
            }
        }

        public static void DownloadFile(string localFullPath, string remoteFullPath, HostDetailModel hostDetail, ref string downloadFileLog)
        {
            var ftpReq = GetFtpWebRequest(remoteFullPath, hostDetail);
            ftpReq.Method = WebRequestMethods.Ftp.DownloadFile;

            var ftpRes = (FtpWebResponse)ftpReq.GetResponse();
            Stream resStrm = ftpRes.GetResponseStream();
            var fs = new FileStream(localFullPath, FileMode.Create, FileAccess.Write);

            byte[] buffer = new byte[10485760];
            while (true)
            {
                int readSize = resStrm.Read(buffer, 0, buffer.Length);
                if (readSize == 0)
                {
                    break;
                }
                fs.Write(buffer, 0, readSize);
            }
            fs.Close();
            resStrm.Close();
            ftpRes.Close();

            downloadFileLog += "[Download] " + DateTime.Now + " " + remoteFullPath + " >> " + localFullPath + "\r\n";
        }

        public static List<ResourceInfoModel> GetLocalResourceList(string path)
        {
            var localResourceList = new List<ResourceInfoModel>();

            try
            {
                if (path.IndexOf("\\") + 1 != path.Length)
                {
                    localResourceList.Add(Common.GetBackwardDirectoryInfo());
                }

                foreach (string dicPath in Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly))
                {
                    ResourceInfoModel resourceInfo = Common.GetLocalResourceInfo(dicPath, Constants.ResourceType.Directory);

                    if (resourceInfo != null)
                    {
                        localResourceList.Add(resourceInfo);
                    }
                }

                foreach (string filePath in Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly))
                {
                    ResourceInfoModel resourceInfo = Common.GetLocalResourceInfo(filePath, Constants.ResourceType.File);

                    if (resourceInfo != null)
                    {
                        localResourceList.Add(resourceInfo);
                    }
                }
            }
            catch (Exception)
            {
            }

            return localResourceList;
        }

        public static bool IsRemoteConnected(HostDetailModel hostDetail)
        {
            try
            {
                var ftpReq = GetFtpWebRequest(hostDetail.HostAddress, hostDetail);
                ftpReq.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                var ftpRes = (FtpWebResponse)ftpReq.GetResponse();
                var sr = new StreamReader(ftpRes.GetResponseStream());
                string res = sr.ReadToEnd();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static List<ResourceInfoModel> GetRemoteResourceList(string path, HostDetailModel hostDetail, bool needBackward)
        {
            var remoteList = new List<ResourceInfoModel>();

            var ftpReq = GetFtpWebRequest(path, hostDetail);
            ftpReq.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            var ftpRes = (FtpWebResponse)ftpReq.GetResponse();

            var sr = new StreamReader(ftpRes.GetResponseStream());
            string res = sr.ReadToEnd();
            FileStruct[] list = GetRemoteList(res);

            var dicList = new List<ResourceInfoModel>();
            var fileList = new List<ResourceInfoModel>();

            if (needBackward && path.IndexOf("/") > -1)
            {
                remoteList.Add(Common.GetBackwardDirectoryInfo());
            }

            foreach (FileStruct file in list)
            {
                var fileInfo = new ResourceInfoModel();

                fileInfo.Name = file.Name;
                fileInfo.FullPath = path + "/" + file.Name;
                fileInfo.LastWrittenTime = file.CreateTime;

                if (file.IsDirectory)
                {
                    fileInfo.Type = Settings.Constants.ResourceType.Directory;

                    dicList.Add(fileInfo);
                }
                else
                {
                    fileInfo.Type = Settings.Constants.ResourceType.File;
                    fileInfo.Length = file.length;
                    if (file.Name.IndexOf(".") > -1)
                    {
                        fileInfo.Extension = file.Name.Substring(file.Name.LastIndexOf("."), file.Name.Length - file.Name.LastIndexOf("."));
                    }
                    else
                    {
                        fileInfo.Extension = file.Name;
                    }
                    fileList.Add(fileInfo);
                }
            }

            foreach (ResourceInfoModel dic in dicList)
            {
                remoteList.Add(dic);
            }

            foreach (ResourceInfoModel file in fileList)
            {
                remoteList.Add(file);
            }

            sr.Close();

            return remoteList;
        }

        public static bool IsExistResourceName(List<ResourceInfoModel> resourceList, string resourceName, Constants.ResourceType type)
        {
            foreach (ResourceInfoModel resource in resourceList)
            {
                if (resource.Type == type && resource.Name.Equals(resourceName))
                {
                    return true;
                }
            }

            return false;
        }

        public static FileListStyle GuessFileSystemStyle(string[] recordList)
        {
            foreach (string s in recordList)
            {
                if (s.Length > 10
                 && Regex.IsMatch(s.Substring(0, 10), "(-|d)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)"))
                {
                    return FileListStyle.UnixStyle;
                }
                else if (s.Length > 8
                 && Regex.IsMatch(s.Substring(0, 8), "[0-9][0-9]-[0-9][0-9]-[0-9][0-9]"))
                {
                    return FileListStyle.WindowsStyle;
                }
            }
            return FileListStyle.Unknown;
        }

        public static List<ResourceInfoModel> GetFilteredList(List<ResourceInfoModel> beforeList, string filterPattern)
        {
            var afterList = new List<ResourceInfoModel>();

            try
            {
                foreach (ResourceInfoModel beforeFile in beforeList)
                {
                    if (Regex.IsMatch(beforeFile.Name, filterPattern, RegexOptions.ECMAScript))
                    {
                        afterList.Add(beforeFile);
                    }
                }
            }
            catch
            {
                return beforeList;
            }

            return afterList;
        }

        public static void MakeRemoteDirectory(string path, HostDetailModel hostDetail)
        {
            var ftpReq = GetFtpWebRequest(path, hostDetail);
            ftpReq.Method = WebRequestMethods.Ftp.MakeDirectory;

            var ftpRes = (FtpWebResponse)ftpReq.GetResponse();
            ftpRes.Close();
        }

        public static void RemoveRemoteFile(string path, HostDetailModel hostDetail, ref string removeFileLog)
        {
            var ftpReq = GetFtpWebRequest(path, hostDetail);
            ftpReq.Method = WebRequestMethods.Ftp.DeleteFile;

            var ftpRes = (FtpWebResponse)ftpReq.GetResponse();
            ftpRes.Close();

            removeFileLog += "[Remove] [remote] " + DateTime.Now + " " + path + "\r\n";
        }

        public static void RemoveRemoteDirectory(string path, HostDetailModel hostDetail, ref string removeDirLog)
        {
            List<ResourceInfoModel> remoteResList = GetRemoteResourceList(path, hostDetail, false);

            foreach (ResourceInfoModel subDirfile in remoteResList)
            {
                if (subDirfile.Type == Constants.ResourceType.File)
                {
                    string removePath = path + "/" + subDirfile.Name;
                    RemoveRemoteFile(removePath, hostDetail, ref removeDirLog);
                }
            }

            foreach (ResourceInfoModel subDirFile in remoteResList)
            {
                if (subDirFile.Type == Constants.ResourceType.Directory)
                {
                    string removePath = path + "/" + subDirFile.Name;
                    RemoveRemoteDirectory(removePath, hostDetail, ref removeDirLog);
                }
            }

            var ftpReq = GetFtpWebRequest(path, hostDetail);
            ftpReq.Method = WebRequestMethods.Ftp.RemoveDirectory;

            var ftpRes = (FtpWebResponse)ftpReq.GetResponse();
            ftpRes.Close();
        }

        public static void RenameRemoteResource(string previousFullPath, string nextName, HostDetailModel hostDetail)
        {
            var ftpReq = GetFtpWebRequest(previousFullPath, hostDetail);
            ftpReq.Method = WebRequestMethods.Ftp.Rename;
            ftpReq.RenameTo = nextName;

            var ftpRes = (FtpWebResponse)ftpReq.GetResponse();
            ftpRes.Close();
        }

        private static FileStruct[] GetRemoteList(string dataString)
        {
            var myListArray = new List<FileStruct>();
            string[] dataRecords = dataString.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            FileListStyle directoryListStyle = GuessFileSystemStyle(dataRecords);
            foreach (string s in dataRecords)
            {
                if (directoryListStyle != FileListStyle.Unknown && s != "")
                {
                    var f = new FileStruct();
                    f.Name = "..";
                    switch (directoryListStyle)
                    {
                        case FileListStyle.UnixStyle:
                            f = ParseFileStructFromUnixStyleRecord(s);
                            break;

                        case FileListStyle.WindowsStyle:
                            f = ParseFileStructFromWindowsStyleRecord(s);
                            break;
                    }

                    if (!(f.Name == "." || f.Name == ".."))
                    {
                        myListArray.Add(f);
                    }
                }
            }
            return myListArray.ToArray();
        }

        private static FileStruct ParseFileStructFromWindowsStyleRecord(string record)
        {
            var f = new FileStruct();
            string processstr = record.Trim();
            string dateStr = processstr.Substring(0, 8);
            processstr = (processstr.Substring(8, processstr.Length - 8)).Trim();
            string timeStr = processstr.Substring(0, 7);
            processstr = (processstr.Substring(7, processstr.Length - 7)).Trim();

            string month = dateStr.Substring(0, 2);
            string day = dateStr.Substring(3, 2);
            string year = "20" + dateStr.Substring(6, 2);

            f.CreateTime = DateTime.Parse(year + "/" + month + "/" + day + " " + timeStr);
            if (processstr.Substring(0, 5) == "<DIR>")
            {
                f.IsDirectory = true;
                processstr = (processstr.Substring(5, processstr.Length - 5)).Trim();
            }
            else
            {
                string[] strs = processstr.Split(' ');
                f.length = long.Parse(strs[0].Trim());
                if (strs.Length > 2)
                {
                    int i = 0;
                    processstr = string.Empty;
                    foreach (string str in strs)
                    {
                        if (i != 0)
                        {
                            processstr += strs[i] + " ";
                        }
                        i++;
                    }
                    processstr = processstr.Trim();
                }
                else
                {
                    processstr = strs[1].Trim();
                }
                f.IsDirectory = false;
            }
            f.Name = processstr;
            return f;
        }

        private static FileStruct ParseFileStructFromUnixStyleRecord(string record)
        {
            var f = new FileStruct();
            string processstr = record.Trim();
            f.Flags = processstr.Substring(0, 9);
            f.IsDirectory = (f.Flags[0] == 'd');
            processstr = (processstr.Substring(11)).Trim();
            CutSubstringFromStringWithTrim(ref processstr, ' ', 0);
            f.Owner = CutSubstringFromStringWithTrim(ref processstr, ' ', 0);
            f.Group = CutSubstringFromStringWithTrim(ref processstr, ' ', 0);
            CutSubstringFromStringWithTrim(ref processstr, ' ', 0);
            f.CreateTime = DateTime.Parse(CutSubstringFromStringWithTrim(ref processstr, ' ', 8));
            f.Name = processstr;
            return f;
        }

        private static string CutSubstringFromStringWithTrim(ref string s, char c, int startIndex)
        {
            int pos1 = s.IndexOf(c, startIndex);
            string retString = s.Substring(0, pos1);
            s = (s.Substring(pos1)).Trim();
            return retString;
        }

        private static FtpWebRequest GetFtpWebRequest(string path, HostDetailModel hostDetail)
        {
            int index = path.IndexOf('/');
            Uri u = null;
            if (index == -1)
            {
                u = new Uri("ftp://" + path + ":" + hostDetail.PortNumber);
            }
            else
            {
                u = new Uri("ftp://" + path.Substring(0, index) + ":" + hostDetail.PortNumber + path.Substring(index, path.Length - index));
            }

            var ftpReq = (FtpWebRequest)WebRequest.Create(u);

            if (!hostDetail.Anonymous)
            {
                ftpReq.Credentials = new NetworkCredential(hostDetail.UserName, hostDetail.Password);
            }
            else
            {
                ftpReq.Credentials = new NetworkCredential("anonymous", hostDetail.Password);
            }
            ftpReq.Timeout = hostDetail.Timeout;
            ftpReq.UsePassive = hostDetail.PasvMode;

            return ftpReq;
        }
    }
}