using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using FTPClient.BL.Model;
using Newtonsoft.Json;

namespace FTPClient.BL
{
    public class HostAdmin
    {
        public static HostListModel GetHostList(string filePath)
        {
            var sr = new StreamReader(filePath, Encoding.GetEncoding("utf-8"));
            string text = sr.ReadToEnd();
            sr.Close();

            HostListModel hostList = JsonConvert.DeserializeObject<HostListModel>(text);

            hostList.HostList.Sort(delegate(HostDetailModel x, HostDetailModel y)
            {
                return x.SortNumber - y.SortNumber;
            });

            return hostList;
        }

        public static bool SetHostList(HostListModel hostList, string filePath)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                string json = JsonConvert.SerializeObject(hostList);

                var utf8 = Encoding.GetEncoding("utf-8");
                // ファイルを全て上書き
                File.WriteAllText(filePath, json, utf8);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool IsExistHostName(List<HostDetailModel> hostList, string hostName)
        {
            foreach (HostDetailModel hostDetail in hostList)
            {
                if (hostDetail.HostName.Equals(hostName))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsExistHostID(List<HostDetailModel> hostList, int hostId)
        {
            foreach (HostDetailModel hostDetail in hostList)
            {
                if (hostDetail.HostID == hostId)
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetHostName(List<HostDetailModel> hostList, int hostId)
        {
            foreach (HostDetailModel hostDetail in hostList)
            {
                if (hostDetail.HostID.Equals(hostId))
                {
                    return hostDetail.HostName;
                }
            }

            return string.Empty;
        }
    }
}