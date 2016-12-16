using System.ComponentModel.DataAnnotations;

namespace FTPClient.Models
{
    public class OpenServerModel
    {
        public OpenServerModel()
        {
        }

        [Required(ErrorMessage = "移動先パスを入力してください。")]
        public string Path
        {
            get;
            set;
        }
    }
}