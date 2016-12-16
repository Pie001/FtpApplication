using System.ComponentModel.DataAnnotations;

namespace FTPClient.Models
{
    public class FilterServerModel
    {
        public FilterServerModel()
        {
        }

        [Required(ErrorMessage = "フィルター条件を入力してください。")]
        public string FilterPattern
        {
            get;
            set;
        }
    }
}