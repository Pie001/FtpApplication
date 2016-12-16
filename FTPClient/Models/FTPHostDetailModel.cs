using System.ComponentModel.DataAnnotations;

namespace FTPClient.Models
{
    public class FTPHostDetailModel
    {
        public FTPHostDetailModel()
        {
            this.PortNumber = 21;
            this.Timeout = 6000;
            this.PasvMode = true;
        }

        [Required(ErrorMessage = "ホストのラベル名は必須です。")]
        [StringLength(200, ErrorMessage = "ホストのラベル名が長すぎます。")]
        public string HostName
        {
            get;
            set;
        }

        public int HostID
        {
            get;
            set;
        }

        [Required(ErrorMessage = "ホストアドレスは必須です。")]
        public string HostAddress
        {
            get;
            set;
        }

        [Required(ErrorMessage = "ポート番号は必須です。")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "ポート番号は数字のみ入力可能です")]
        [StringLength(6, ErrorMessage = "ポート番号が長すぎます。")]
        public int PortNumber
        {
            get;
            set;
        }

        [Required(ErrorMessage = "ユーザー名は必須です。")]
        public string UserName
        {
            get;
            set;
        }

        [Required(ErrorMessage = "パスワードは必須です。")]
        [DataType(DataType.Password)]
        public string Password
        {
            get;
            set;
        }

        [Required]
        public bool Anonymous
        {
            get;
            set;
        }

        /// <summary>
        /// タイムアウト制限時間(秒)
        /// </summary>
        [RegularExpression("^[0-9]+$", ErrorMessage = "タイムアウトは数字のみ入力可能です")]
        [StringLength(4)]
        public int Timeout
        {
            get;
            set;
        }

        public bool PasvMode
        {
            get;
            set;
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
    }
}