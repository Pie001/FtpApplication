using System.ComponentModel.DataAnnotations;

namespace FTPClient.Models
{
    public class ChangeResourceNameModel
    {
        public ChangeResourceNameModel()
        {
            NewName = null;
        }

        [Required(ErrorMessage = "変更後のファイル名を入力してください。")]
        public string NewName
        {
            get;
            set;
        }
    }
}