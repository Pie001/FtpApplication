using Newtonsoft.Json;

namespace FTPClient.BL.Model
{
    public class HostDetailModel
    {
        public HostDetailModel()
        {
        }

        public HostDetailModel(HostDetailModel hostDetail)
        {
            this.HostID = hostDetail.HostID;
            this.HostName = hostDetail.HostName;
            this.HostAddress = hostDetail.HostAddress;
            this.Anonymous = hostDetail.Anonymous;
            this.UserName = hostDetail.UserName;
            this.Password = hostDetail.Password;
            this.PasvMode = hostDetail.PasvMode;
            this.PortNumber = hostDetail.PortNumber;
            this.Timeout = hostDetail.Timeout;
            this.LocalServerPath = hostDetail.LocalServerPath;
            this.RemoteServerPath = hostDetail.RemoteServerPath;
            this.SortNumber = hostDetail.SortNumber;
        }

        [JsonProperty("host_name")]
        public string HostName
        {
            get;
            set;
        }

        [JsonProperty("host_id")]
        public int HostID
        {
            get;
            set;
        }

        [JsonProperty("host_address")]
        public string HostAddress
        {
            get;
            set;
        }

        [JsonProperty("port_number")]
        public int PortNumber
        {
            get;
            set;
        }

        [JsonProperty("user_name")]
        public string UserName
        {
            get;
            set;
        }

        [JsonProperty("password")]
        public string Password
        {
            get;
            set;
        }

        [JsonProperty("anonymous")]
        public bool Anonymous
        {
            get;
            set;
        }

        [JsonProperty("timeout")]
        public int Timeout
        {
            get;
            set;
        }

        [JsonProperty("pasv_mode")]
        public bool PasvMode
        {
            get;
            set;
        }

        [JsonProperty("local_server_path")]
        public string LocalServerPath
        {
            get;
            set;
        }

        [JsonProperty("remote_server_path")]
        public string RemoteServerPath
        {
            get;
            set;
        }

        [JsonProperty("sort_number")]
        public int SortNumber
        {
            get;
            set;
        }
    }
}