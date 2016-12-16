using System.ComponentModel.DataAnnotations;

namespace FTPClient.Models
{
    public class MakeDirectoryModel
    {
        public MakeDirectoryModel()
        {
        }

        [Required(ErrorMessage = "新しいディレクトリ名を入力してください。")]
        public string DirectoryName
        {
            get;
            set;
        }
    }
}