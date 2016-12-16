using System.Configuration;
using System.Web.Mvc;
using FTPClient.BL;
using FTPClient.BL.Model;
using FTPClient.Settings;

namespace FTPClient.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Session[Constants.SESSION_LOCAL_PATH] = null;
            Session[Constants.SESSION_REMOTE_PATH] = null;
            Session[Constants.SESSION_HOST_DETAIL] = null;
            Session[Constants.SESSION_LOCAL_FILE_LIST] = null;
            Session[Constants.SESSION_REMOTE_FILE_LIST] = null;

            string filePath = ConfigurationManager.AppSettings["HostList"];
            HostListModel hosts = HostAdmin.GetHostList(filePath);
            Session[Constants.SESSION_HOST_SETTING] = hosts;
            ViewBag.hostList = hosts.HostList;

            return View();
        }
    }
}