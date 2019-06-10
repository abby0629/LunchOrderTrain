using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class LoginModel
    {
        [DisplayName("帳號:")]
        [Required(ErrorMessage = "帳號不可為空白")]
        public string Account { get; set; }


        [DisplayName("密碼:")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "密碼不可為空白")]
        public string Password { get; set; }
    }
}