using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class EditModel
    {

        [DisplayName("編號:")]
        [Required(ErrorMessage = "編號不可為空白")]
        public string ID { get; set; }

        [DisplayName("姓名:")]
        [Required(ErrorMessage = "姓名不可為空白")]
        public string NAME { get; set; }
    }
}