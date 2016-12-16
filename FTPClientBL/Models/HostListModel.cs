using System.Collections.Generic;
using Newtonsoft.Json;

namespace FTPClient.BL.Model
{
    public class HostListModel
    {
        [JsonProperty("host_list")]
        public List<HostDetailModel> HostList
        {
            get;
            set;
        }

        [JsonProperty("max_host_id")]
        public int MaxHostId
        {
            get;
            set;
        }
    }
}