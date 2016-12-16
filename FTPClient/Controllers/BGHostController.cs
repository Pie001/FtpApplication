using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Web.Mvc;
using FTPClient.Settings;
using FTPClient.Models;
using FTPClient.BL;
using FTPClient.BL.Model;

namespace FTPClient.Controllers
{
    public class BGHostController : Controller
    {
        private string HostListFilePath;

        public BGHostController()
        {
            this.HostListFilePath = ConfigurationManager.AppSettings["HostList"];
        }

        public ActionResult ViewHostList()
        {
            var model = new FTPControlModel();

            try
            {
                string filePath = ConfigurationManager.AppSettings["HostList"];
                HostListModel hosts = HostAdmin.GetHostList(filePath);
                model.HostList = hosts.HostList;
            }
            catch (Exception e)
            {
                model.OperationLog = string.Format("[Exception] {0},\r\n{1}", e.Message, e.StackTrace) + "\r\n";
            }

            return PartialView("_HostMenu", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddHost(HostDetailModel newHostDetail)
        {
            var result = new JsonResponse();

            try
            {
                HostListModel hostList = getHostList();

                if (!HostAdmin.IsExistHostName(hostList.HostList, newHostDetail.HostName))
                {
                    int newHostId = hostList.MaxHostId + 1;

                    newHostDetail.HostID = newHostId;
                    newHostDetail.SortNumber = hostList.HostList.Count;
                    hostList.HostList.Add(newHostDetail);
                    hostList.MaxHostId = newHostId;

                    if (HostAdmin.SetHostList(hostList, HostListFilePath))
                    {
                        Session[Constants.SESSION_HOST_SETTING] = hostList;
                        result.SetResult(true);
                        result.AddData("hostId", newHostId.ToString());
                        result.AddData("hostName", newHostDetail.HostName);
                    }
                    else
                    {
                        result.AddErrorInfo("errorMessage", "新規追加に失敗！");
                    }
                }
                else
                {
                    result.AddErrorInfo("errorMessage", "既に登録されているホスト名です！");
                }
            }
            catch (Exception)
            {
                result.AddErrorInfo("errorMessage", "予期せぬエラー");
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditHost(HostDetailModel editHostDetail)
        {
            var result = new JsonResponse();

            try
            {
                var hostList = getHostList();

                var newHosts = new HostListModel();
                var hostDetailList = new List<HostDetailModel>();

                bool isEdit = false;

                if (HostAdmin.GetHostName(hostList.HostList, editHostDetail.HostID) == editHostDetail.HostName)
                {
                    isEdit = true;
                }
                else if (!HostAdmin.IsExistHostName(hostList.HostList, editHostDetail.HostName))
                {
                    isEdit = true;
                }

                if (isEdit)
                {
                    foreach (HostDetailModel hostDetail in hostList.HostList)
                    {
                        if (hostDetail.HostID.Equals(editHostDetail.HostID))
                        {
                            hostDetailList.Add(editHostDetail);
                        }
                        else
                        {
                            hostDetailList.Add(hostDetail);
                        }
                    }
                    newHosts.HostList = hostDetailList;
                    newHosts.MaxHostId = hostList.MaxHostId;

                    if (HostAdmin.SetHostList(newHosts, HostListFilePath))
                    {
                        Session[Constants.SESSION_HOST_SETTING] = newHosts;
                        result.SetResult(true);
                        result.AddData("hostId", editHostDetail.HostID.ToString());
                        result.AddData("hostName", editHostDetail.HostName);
                    }
                    else
                    {
                        result.AddErrorInfo("errorMessage", "ホスト設定の修正に失敗！");
                    }
                }
                else
                {
                    result.AddErrorInfo("errorMessage", "既に登録されているホスト名です！");
                }
            }
            catch (Exception)
            {
                result.AddErrorInfo("errorMessage", "予期せぬエラー");
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.DenyGet);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ConnectTest(HostDetailModel newHostDetail)
        {
            var result = new JsonResponse();
            try
            {
                if (FTPControl.IsRemoteConnected(newHostDetail))
                {
                    result.SetResult(true);
                }
                else
                {
                    result.AddErrorInfo("errorMessage", "接続テストに失敗しました！");
                }
            }
            catch (Exception)
            {
                result.AddErrorInfo("errorMessage", "予期せぬエラー");
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CopyHost(int selectHostId, string NewHostName)
        {
            var result = new JsonResponse();

            try
            {
                HostListModel hosts = getHostList();

                if (!HostAdmin.IsExistHostID(hosts.HostList, selectHostId))
                {
                    result.AddErrorInfo("errorMessage", "コピー元のホストが存在しません！");
                }
                else
                {
                    int newHostId = hosts.MaxHostId + 1;
                    string newHostName = string.Empty;

                    if (NewHostName == string.Empty || HostAdmin.IsExistHostName(hosts.HostList, NewHostName))
                    {
                        newHostName = "Untitled_" + newHostId;
                    }
                    else
                    {
                        newHostName = NewHostName;
                    }

                    foreach (HostDetailModel hostDetail in hosts.HostList)
                    {
                        if (hostDetail.HostID == selectHostId)
                        {
                            var newHostDetail = new HostDetailModel(hostDetail);
                            newHostDetail.HostName = newHostName;
                            newHostDetail.HostID = newHostId;
                            newHostDetail.SortNumber = hosts.HostList.Count;
                            hosts.HostList.Add(newHostDetail);
                            hosts.MaxHostId = newHostId;

                            break;
                        }
                    }

                    if (HostAdmin.SetHostList(hosts, HostListFilePath))
                    {
                        Session[Constants.SESSION_HOST_SETTING] = hosts;
                        result.SetResult(true);
                        result.AddData("hostId", newHostId.ToString());
                        result.AddData("hostName", newHostName);
                    }
                    else
                    {
                        result.AddErrorInfo("errorMessage", "新規追加に失敗！");
                    }
                }
            }
            catch (Exception)
            {
                result.AddErrorInfo("errorMessage", "予期せぬエラー");
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveHost(int selectHostId)
        {
            var result = new JsonResponse();

            try
            {
                HostListModel hosts = getHostList();

                var newHosts = new HostListModel();
                var hostDetailList = new List<HostDetailModel>();

                if (!HostAdmin.IsExistHostID(hosts.HostList, selectHostId))
                {
                    result.AddErrorInfo("errorMessage", "ホストは既に削除されています！");
                }
                else
                {
                    foreach (HostDetailModel hostDetail in hosts.HostList)
                    {
                        if (hostDetail.HostID != selectHostId)
                        {
                            hostDetailList.Add(hostDetail);
                        }
                    }
                    newHosts.HostList = hostDetailList;
                    newHosts.MaxHostId = hosts.MaxHostId;

                    if (HostAdmin.SetHostList(newHosts, HostListFilePath))
                    {
                        Session[Constants.SESSION_HOST_SETTING] = newHosts;
                        result.SetResult(true);
                        result.AddData("hostId", selectHostId.ToString());
                    }
                    else
                    {
                        result.AddErrorInfo("errorMessage", "ホスト削除に失敗！");
                    }
                }
            }
            catch (Exception)
            {
                result.AddErrorInfo("errorMessage", "予期せぬエラー");
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// ホスト情報取得
        /// </summary>
        [HttpGet]
        public JsonResult HostItem(int hostId)
        {
            var result = new JsonResponse();

            try
            {
                HostListModel hosts = getHostList();

                if (!HostAdmin.IsExistHostID(hosts.HostList, hostId))
                {
                    result.AddErrorInfo("errorMessage", "該当ホスト情報が存在しません！");
                }
                else
                {
                    var hostItemDetail = new HostDetailModel();

                    foreach (HostDetailModel hostDetail in hosts.HostList)
                    {
                        if (hostDetail.HostID == hostId)
                        {
                            result.SetResult(true);
                            result.AddData("hostId", hostDetail.HostID.ToString());
                            result.AddData("hostName", hostDetail.HostName);
                            result.AddData("hostAddress", hostDetail.HostAddress);
                            result.AddData("anonymous", hostDetail.Anonymous.ToString());
                            result.AddData("userName", hostDetail.UserName);
                            result.AddData("password", hostDetail.Password);
                            result.AddData("pasvMode", hostDetail.PasvMode.ToString());
                            result.AddData("portNumber", hostDetail.PortNumber.ToString());
                            result.AddData("timeout", hostDetail.Timeout.ToString());
                            result.AddData("localServerPath", hostDetail.LocalServerPath);
                            result.AddData("remoteServerPath", hostDetail.RemoteServerPath);
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                result.AddErrorInfo("errorMessage", "予期せぬエラー");
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// ホストの順番変更
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SortHost(string hostList)
        {
            var result = new JsonResponse();

            try
            {
                HostListModel currentHostList = getHostList();
                var newHostList = new List<HostDetailModel>();
                string[] hosts = hostList.Split(':');

                foreach (HostDetailModel currentHost in currentHostList.HostList)
                {
                    foreach (string host in hosts)
                    {
                        string[] hostDetail = host.Split(',');
                        if (Convert.ToInt32(hostDetail[0]) == currentHost.HostID)
                        {
                            var newHostDetail = new HostDetailModel(currentHost);
                            newHostDetail.SortNumber = Convert.ToInt32(hostDetail[1]);
                            newHostList.Add(newHostDetail);
                            break;
                        }
                    }
                }
                currentHostList.HostList = newHostList;

                if (HostAdmin.SetHostList(currentHostList, HostListFilePath))
                {
                    Session[Constants.SESSION_HOST_SETTING] = currentHostList;
                    result.SetResult(true);
                }
                else
                {
                    result.AddErrorInfo("errorMessage", "ホスト一覧のソートに失敗！");
                }
            }
            catch (Exception)
            {
                result.AddErrorInfo("errorMessage", "予期せぬエラー");
            }

            return Json(result, "text/json", Encoding.UTF8, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// jsonからホスト一覧を取得
        /// </summary>
        public HostListModel getHostList()
        {
            var hostList = (HostListModel)Session[Constants.SESSION_HOST_SETTING];

            if (hostList == null)
            {
                hostList = HostAdmin.GetHostList(HostListFilePath);
            }

            return hostList;
        }
    }
}