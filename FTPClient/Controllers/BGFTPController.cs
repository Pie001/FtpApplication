using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web.Mvc;
using FTPClient.BL;
using FTPClient.BL.Model;
using FTPClient.Models;
using FTPClient.Settings;

namespace FTPClient.Controllers
{
    public class BGFTPController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Connect(int selectHostId)
        {
            var result = new JsonResponse();
            HostDetailModel hostDetail = null;

            try
            {
                var hosts = (HostListModel)Session[Constants.SESSION_HOST_SETTING];

                if (hosts == null)
                {
                    hosts = HostAdmin.GetHostList(ConfigurationManager.AppSettings["HostList"]);
                }

                if (!HostAdmin.IsExistHostID(hosts.HostList, selectHostId))
                {
                    result.AddErrorInfo("errorMessage", "接続先のホスト情報が存在しません！");
                }
                else
                {
                    foreach (HostDetailModel host in hosts.HostList)
                    {
                        if (host.HostID == selectHostId)
                        {
                            hostDetail = new HostDetailModel(host);
                            break;
                        }
                    }

                    if (hostDetail != null && FTPControl.IsRemoteConnected(hostDetail))
                    {
                        Session[Constants.SESSION_LOCAL_PATH] = hostDetail.LocalServerPath != "" && hostDetail.LocalServerPath != null ? hostDetail.LocalServerPath : "C:\\forwork\\localFolder";
                        Session[Constants.SESSION_REMOTE_PATH] = hostDetail.HostAddress;

                        List<ResourceInfoModel> localResourceList = FTPControl.GetLocalResourceList((string)Session[Constants.SESSION_LOCAL_PATH]);
                        List<ResourceInfoModel> remoteResourceList = FTPControl.GetRemoteResourceList(hostDetail.HostAddress, hostDetail, true);

                        Session[Constants.SESSION_LOCAL_FILE_LIST] = localResourceList;
                        Session[Constants.SESSION_REMOTE_FILE_LIST] = remoteResourceList;

                        Session[Constants.SESSION_HOST_DETAIL] = hostDetail;
                        string log = (string)Session[Constants.SESSION_OLD_LOG] + "ホスト" + hostDetail.HostAddress + "に接続しました。\r\n";
                        Session[Constants.SESSION_LOG] = log;
                        Session[Constants.SESSION_OLD_LOG] = log;

                        result.SetResult(true);
                    }
                    else
                    {
                        result.AddErrorInfo("errorMessage", "FTP接続に失敗しました。");
                    }
                }
            }
            catch (Exception)
            {
                result.AddErrorInfo("errorMessage", "接続時にエラーが発生しました。");
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Reconnect(int selectHostId)
        {
            var result = new JsonResponse();

            try
            {
                var bgHost = new BGHostController();
                HostListModel hosts = bgHost.getHostList();

                if (!HostAdmin.IsExistHostID(hosts.HostList, selectHostId))
                {
                    result.AddErrorInfo("errorMessage", "接続先のホスト情報が存在しません！");
                }

                result.SetResult(true);
            }
            catch (Exception)
            {
                result.AddErrorInfo("errorMessage", "接続時にエラーが発生しました。");
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Disconnect()
        {
            var result = new JsonResponse();

            try
            {
                Session[Constants.SESSION_LOCAL_PATH] = null;
                Session[Constants.SESSION_REMOTE_PATH] = null;
                Session[Constants.SESSION_HOST_DETAIL] = null;
                Session[Constants.SESSION_LOCAL_FILE_LIST] = null;
                Session[Constants.SESSION_REMOTE_FILE_LIST] = null;
                Session[Constants.SESSION_LOG] = null;
                Session[Constants.SESSION_OLD_LOG] = null;

                result.SetResult(true);
            }
            catch (Exception)
            {
                result.AddErrorInfo("errorMessage", "切断時にエラーが発生しました。");
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Upload(string resList)
        {
            var result = new JsonResponse();

            HostDetailModel hostDetail = (HostDetailModel)Session[Constants.SESSION_HOST_DETAIL];

            string sessionLocalPath = (string)Session[Constants.SESSION_LOCAL_PATH];
            string sessionRemotePath = (string)Session[Constants.SESSION_REMOTE_PATH];

            string[] uploadResourceList = resList.Split(':');
            string uploadFileLog = string.Empty;
            string uploadDirLog = string.Empty;

            try
            {
                foreach (string file in uploadResourceList)
                {
                    // {fileName, isDir}
                    string[] uploadFile = file.Split(',');
                    string localFileFullPath = string.Empty;
                    string remoteFileFullPath = sessionRemotePath + "/" + uploadFile[0];

                    if (sessionLocalPath.IndexOf("\\") == sessionLocalPath.LastIndexOf("\\"))
                    {
                        localFileFullPath = sessionLocalPath;
                    }
                    else
                    {
                        localFileFullPath = sessionLocalPath + "\\" + uploadFile[0];
                    }

                    // ファイルの場合
                    if (uploadFile[1] == "false")
                    {
                        FTPControl.UploadFile(localFileFullPath, remoteFileFullPath, hostDetail, ref uploadFileLog);
                    }
                    else
                    {
                        FTPControl.UploadDirectory(localFileFullPath, remoteFileFullPath, hostDetail, ref uploadDirLog);
                    }
                }

                Session[Constants.SESSION_LOG] = uploadFileLog + uploadDirLog + "ファイルのアップロードが正常終了しました。\r\n";

                result.SetResult(true);
            }
            catch (Exception e)
            {
                result.AddErrorInfo("errorMessage", "アップロード時にエラーが発生しました。");
                Session[Constants.SESSION_LOG] = string.Format("[Exception] {0},\r\n{1}", e.Message, e.StackTrace) + "\r\n";
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Download(string resList)
        {
            var result = new JsonResponse();

            HostDetailModel hostDetail = (HostDetailModel)Session[Constants.SESSION_HOST_DETAIL];

            string sessionLocalPath = (string)Session[Constants.SESSION_LOCAL_PATH];
            string sessionRemotePath = (string)Session[Constants.SESSION_REMOTE_PATH];

            string[] downloadResourceList = resList.Split(':');
            string downloadFileLog = string.Empty;
            string downloadDirLog = string.Empty;

            try
            {
                if (downloadResourceList.Length > 0)
                {
                    foreach (string file in downloadResourceList)
                    {
                        // {fileName, isDir}
                        string[] downloadFile = file.Split(',');
                        string localFileFullPath = string.Empty;
                        string remoteFileFullPath = sessionRemotePath + "/" + downloadFile[0];

                        if (sessionLocalPath.IndexOf("\\") == sessionLocalPath.LastIndexOf("\\"))
                        {
                            localFileFullPath = sessionLocalPath;
                        }
                        else
                        {
                            localFileFullPath = sessionLocalPath + "\\" + downloadFile[0];
                        }

                        // ファイルの場合
                        if (downloadFile[1] == "false")
                        {
                            FTPControl.DownloadFile(localFileFullPath, remoteFileFullPath, hostDetail, ref downloadFileLog);
                        }
                        else
                        {
                            FTPControl.DownloadDirectory(localFileFullPath, remoteFileFullPath, hostDetail, ref downloadDirLog);
                        }
                    }
                }
                Session[Constants.SESSION_LOG] = downloadFileLog + downloadDirLog + "ファイルのダウンロードが正常終了しました。\r\n";

                result.SetResult(true);
            }
            catch (Exception e)
            {
                result.AddErrorInfo("errorMessage", "ダウンロード時にエラーが発生しました。");
                Session[Constants.SESSION_LOG] = string.Format("[Exception] {0},\r\n{1}", e.Message, e.StackTrace) + "\r\n";
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveResource(string env, string resList)
        {
            var result = new JsonResponse();

            HostDetailModel hostDetail = (HostDetailModel)Session[Constants.SESSION_HOST_DETAIL];

            string sessionLocalPath = (string)Session[Constants.SESSION_LOCAL_PATH];
            string sessionRemotePath = (string)Session[Constants.SESSION_REMOTE_PATH];
            string fullPath = string.Empty;

            string[] removeResourceList = resList.Split(':');
            string removeLocalLog = string.Empty;
            string removeFileLog = string.Empty;
            string removeDirlog = string.Empty;

            try
            {
                if (env == Constants.ENV_LOCAL)
                {
                    foreach (string file in removeResourceList)
                    {
                        string[] removeFile = file.Split(',');
                        fullPath = sessionLocalPath + "\\" + removeFile[0];

                        if (removeFile[1] == "true" && Directory.Exists(fullPath))
                        {
                            Directory.Delete(fullPath, true);
                            result.SetResult(true);
                        }
                        else if (removeFile[1] != "true" && System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                            result.SetResult(true);
                        }
                        removeLocalLog += "[Remove] [" + env + "] " + DateTime.Now + " " + fullPath + "\r\n";
                    }
                }
                else
                {
                    foreach (string file in removeResourceList)
                    {
                        string[] removeFile = file.Split(',');
                        fullPath = sessionRemotePath + "/" + removeFile[0];

                        List<ResourceInfoModel> remoteResourceList = FTPControl.GetRemoteResourceList(sessionRemotePath, hostDetail, false);

                        if (removeFile[1] == "true" && FTPControl.IsExistResourceName(remoteResourceList, removeFile[0], FTPClient.BL.Settings.Constants.ResourceType.Directory))
                        {
                            FTPControl.RemoveRemoteDirectory(fullPath, hostDetail, ref removeDirlog);
                            result.SetResult(true);
                        }
                        else if (removeFile[1] != "true" && FTPControl.IsExistResourceName(remoteResourceList, removeFile[0], FTPClient.BL.Settings.Constants.ResourceType.File))
                        {
                            FTPControl.RemoveRemoteFile(fullPath, hostDetail, ref removeFileLog);
                            result.SetResult(true);
                        }
                    }
                }

                if (result.Result != "success")
                {
                    result.AddErrorInfo("errorMessage", "削除するファイルが存在しません。");
                }
                else if (env == Constants.ENV_LOCAL)
                {
                    Session[Constants.SESSION_LOG] = removeLocalLog + "ファイルの削除が正常終了しました。\r\n";
                }
                else if (env == Constants.ENV_REMOTE)
                {
                    Session[Constants.SESSION_LOG] = removeFileLog + removeDirlog + "ファイルの削除が正常終了しました。\r\n";
                }
            }
            catch (Exception e)
            {
                result.AddErrorInfo("errorMessage", "ファイル削除時にエラーが発生しました。");
                Session[Constants.SESSION_LOG] = string.Format("[Exception] {0},\r\n{1}", e.Message, e.StackTrace) + "\r\n";
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangeResourceName(string NewName, string env, bool isDir, string beforeName)
        {
            var result = new JsonResponse();

            HostDetailModel hostDetail = (HostDetailModel)Session[Constants.SESSION_HOST_DETAIL];

            string sessionLocalPath = (string)Session[Constants.SESSION_LOCAL_PATH];
            string sessionRemotePath = (string)Session[Constants.SESSION_REMOTE_PATH];
            string nextFullPath = string.Empty;
            string previousFullPath = string.Empty;
            result.SetResult(true);

            try
            {
                if (env == Constants.ENV_LOCAL)
                {
                    nextFullPath = sessionLocalPath + "\\" + NewName.Trim();
                    previousFullPath = sessionLocalPath + "\\" + beforeName;

                    if (isDir && Directory.Exists(previousFullPath) && !Directory.Exists(nextFullPath))
                    {
                        Directory.Move(previousFullPath, nextFullPath);
                        result.SetResult(true);
                    }
                    else if (!isDir && System.IO.File.Exists(previousFullPath) && !System.IO.File.Exists(nextFullPath))
                    {
                        System.IO.File.Move(previousFullPath, nextFullPath);
                        result.SetResult(true);
                    }
                }
                else
                {
                    previousFullPath = sessionRemotePath + "/" + beforeName;

                    List<ResourceInfoModel> remoteResourceList = FTPControl.GetRemoteResourceList(sessionRemotePath, hostDetail, false);

                    FTPControl.RenameRemoteResource(previousFullPath, NewName.Trim(), hostDetail);
                    result.SetResult(true);
                }

                if (result.Result != "success")
                {
                    result.AddErrorInfo("errorMessage", "名前を変更するファイルが存在しません。");
                }
                else
                {
                    Session[Constants.SESSION_LOG] = "[ChangeResourceName] " + DateTime.Now + " " + previousFullPath + " >> " + NewName.Trim() + "\r\n名前変更が正常終了しました。\r\n";
                }
            }
            catch (Exception e)
            {
                result.AddErrorInfo("errorMessage", "名前変更時にエラーが発生しました。");
                Session[Constants.SESSION_LOG] = string.Format("[Exception] {0},\r\n{1}", e.Message, e.StackTrace) + "\r\n";
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult MakeDirectory(string DirectoryName, string env)
        {
            var result = new JsonResponse();

            HostDetailModel hostDetail = (HostDetailModel)Session[Constants.SESSION_HOST_DETAIL];

            string sessionLocalPath = (string)Session[Constants.SESSION_LOCAL_PATH];
            string sessionRemotePath = (string)Session[Constants.SESSION_REMOTE_PATH];
            string newDirectory = string.Empty;

            try
            {
                if (env == Constants.ENV_LOCAL)
                {
                    if (!Directory.Exists(sessionLocalPath + "\\" + DirectoryName.Trim()))
                    {
                        newDirectory = sessionLocalPath + "\\" + DirectoryName.Trim();
                        DirectoryInfo di = Directory.CreateDirectory(newDirectory);
                        result.SetResult(true);
                    }
                }
                else
                {
                    if (!FTPControl.IsExistResourceName(FTPControl.GetRemoteResourceList(sessionRemotePath, hostDetail, false), DirectoryName.Trim(), FTPClient.BL.Settings.Constants.ResourceType.Directory))
                    {
                        newDirectory = sessionRemotePath + "/" + DirectoryName.Trim();
                        FTPControl.MakeRemoteDirectory(sessionRemotePath + "/" + DirectoryName.Trim(), hostDetail);
                        result.SetResult(true);
                    }
                }

                if (result.Result != "success")
                {
                    result.AddErrorInfo("errorMessage", "既に同じ名前のディレクトリが存在します。");
                }
                else
                {
                    Session[Constants.SESSION_LOG] = "[MakeDirectory] " + DateTime.Now + " " + "New Directory >> " + env + " " + newDirectory + "\r\nディレクトリ作成が正常終了しました。\r\n";
                }
            }
            catch (Exception e)
            {
                result.AddErrorInfo("errorMessage", "ディレクトリ作成時にエラーが発生しました。");
                Session[Constants.SESSION_LOG] = string.Format("[Exception] {0},\r\n{1}", e.Message, e.StackTrace) + "\r\n";
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.DenyGet);
        }

        public ActionResult ViewServerInfo(string path, string env = "", string filterPattern = "")
        {
            var result = new JsonResponse();
            var model = new FTPControlModel();

            HostDetailModel hostDetail = (HostDetailModel)Session[Constants.SESSION_HOST_DETAIL];

            string sessionLocalPath = (string)Session[Constants.SESSION_LOCAL_PATH];
            string sessionRemotePath = (string)Session[Constants.SESSION_REMOTE_PATH];
            string viewName = string.Empty;

            try
            {
                string localPath = string.Empty;
                string remotePath = string.Empty;
                List<ResourceInfoModel> localResourceList = new List<ResourceInfoModel>();
                List<ResourceInfoModel> remoteResourceList = new List<ResourceInfoModel>();

                if (env == Constants.ENV_LOCAL)
                {
                    viewName = "_LocalServer";

                    // 一つ前のディレクトリに戻る場合
                    if (path.Equals(".."))
                    {
                        localPath = sessionLocalPath.Substring(0, sessionLocalPath.LastIndexOf("\\"));
                        // ex)c:\\などのドライブまで上がった場合は最後に\\を付ける
                        if (localPath.IndexOf("\\") == -1)
                        {
                            localPath = localPath + "\\";
                        }
                    }
                    else if (path.Equals("*"))
                    {
                        // リフレッシュ、フィルターをリセットする場合
                        localPath = sessionLocalPath;
                    }
                    else
                    {
                        localPath = path;
                    }

                    localResourceList = FTPControl.GetLocalResourceList(localPath);

                    if (filterPattern != string.Empty)
                    {
                        localResourceList = FTPControl.GetFilteredList(localResourceList, filterPattern);
                        model.LocalFilterPattern = filterPattern;
                    }

                    remotePath = (string)Session[Constants.SESSION_REMOTE_PATH];
                    remoteResourceList = (List<ResourceInfoModel>)Session[Constants.SESSION_REMOTE_FILE_LIST];
                }
                else if (env == Constants.ENV_REMOTE)
                {
                    viewName = "_RemoteServer";

                    // 一つ前のディレクトリに戻る場合
                    if (path.Equals(".."))
                    {
                        if (sessionRemotePath.IndexOf("/") != -1)
                        {
                            remotePath = sessionRemotePath.Substring(0, sessionRemotePath.LastIndexOf("/"));
                        }
                        else
                        {
                            remotePath = sessionRemotePath;
                        }
                    }
                    else if (path.Equals("*"))
                    {
                        // リフレッシュ、フィルターをリセットする場合
                        remotePath = sessionRemotePath;
                    }
                    else
                    {
                        remotePath = path;
                    }

                    remoteResourceList = FTPControl.GetRemoteResourceList(remotePath, hostDetail, true);

                    if (filterPattern != string.Empty)
                    {
                        remoteResourceList = FTPControl.GetFilteredList(remoteResourceList, filterPattern);
                        model.RemoteFilterPattern = filterPattern;
                    }

                    localPath = (string)Session[Constants.SESSION_LOCAL_PATH];
                    localResourceList = (List<ResourceInfoModel>)Session[Constants.SESSION_LOCAL_FILE_LIST];
                }
                else if (env == string.Empty && path.Equals("*"))
                {
                    // ローカル＆リモート両方とも初期化状態に戻す
                    viewName = "_Server";

                    localPath = sessionLocalPath;
                    remotePath = sessionRemotePath;

                    localResourceList = FTPControl.GetLocalResourceList(localPath);
                    remoteResourceList = FTPControl.GetRemoteResourceList(remotePath, hostDetail, true);
                }

                model.LocalList = localResourceList;
                model.LocalServerPath = localPath;
                model.RemoteList = remoteResourceList;
                model.RemoteServerPath = remotePath;
                Session[Constants.SESSION_LOCAL_PATH] = localPath;
                Session[Constants.SESSION_LOCAL_FILE_LIST] = localResourceList;
                Session[Constants.SESSION_REMOTE_PATH] = remotePath;
                Session[Constants.SESSION_REMOTE_FILE_LIST] = remoteResourceList;
            }
            catch (Exception e)
            {
                Session[Constants.SESSION_LOG] = string.Format("[Exception] {0},\r\n{1}", e.Message, e.StackTrace) + "\r\n";
            }

            return PartialView(viewName, model);
        }

        public ActionResult ViewLog()
        {
            var model = new FTPControlModel();

            try
            {
                string log = (string)Session[Constants.SESSION_OLD_LOG] + (string)Session[Constants.SESSION_LOG];
                model.OperationLog = log;
                Session[Constants.SESSION_OLD_LOG] = log;
            }
            catch (Exception e)
            {
                model.OperationLog = string.Format("[Exception] {0},\r\n{1}", e.Message, e.StackTrace) + "\r\n";
            }

            return PartialView("_OperationLog", model);
        }

        [HttpGet]
        public FileContentResult DownloadLog()
        {
            string log = string.Empty;

            log = (string)Session[Constants.SESSION_OLD_LOG] + (string)Session[Constants.SESSION_LOG];
            var encoding = new System.Text.UTF8Encoding();
            byte[] returnContent = encoding.GetBytes(log);
            string logFileName = "LOG_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";

            return File(returnContent, "application/txt", logFileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveLog()
        {
            var result = new JsonResponse();

            try
            {
                Session[Constants.SESSION_OLD_LOG] = string.Empty;
                Session[Constants.SESSION_LOG] = string.Empty;

                result.SetResult(true);
            }
            catch (Exception)
            {
                result.AddErrorInfo("errorMessage", "ログ削除時にエラーが発生しました。");
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.DenyGet);
        }
    }
}