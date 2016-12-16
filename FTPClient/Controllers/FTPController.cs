using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using FTPClient.Models;
using FTPClient.Settings;
using FTPClient.BL;
using FTPClient.BL.Model;

namespace FTPClient.Controllers
{
    public class FTPController : Controller
    {
        public ActionResult Index()
        {
            var model = new FTPControlModel();

            if (Session[Constants.SESSION_LOCAL_FILE_LIST] == null || Session[Constants.SESSION_REMOTE_FILE_LIST] == null)
            {
                return new RedirectToRouteResult(new RouteValueDictionary(
               new {
                   action = "Index",
                   controller = "Home"
               }));
            }

            model.LocalServerPath = (string)Session[Constants.SESSION_LOCAL_PATH];
            model.LocalList = (List<ResourceInfoModel>)Session[Constants.SESSION_LOCAL_FILE_LIST];
            model.RemoteList = (List<ResourceInfoModel>)Session[Constants.SESSION_REMOTE_FILE_LIST];
            model.RemoteServerPath = (string)Session[Constants.SESSION_REMOTE_PATH];

            string filePath = ConfigurationManager.AppSettings["HostList"];
            HostListModel hosts = HostAdmin.GetHostList(filePath);
            model.HostList = hosts.HostList;
            model.SelectedHostID = ((HostDetailModel)Session[Constants.SESSION_HOST_DETAIL]).HostID;
            model.OperationLog = (string)Session[Constants.SESSION_LOG];


            return View(model);
        }
    }
}