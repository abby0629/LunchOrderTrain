using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
namespace WebApplication2.Models
{
    public class RegisterModel
    {
        [DisplayName("帳號:")]
        [Required(ErrorMessage = "帳號不可為空白")]
        public string Account { get; set; }



        [System.Web.Mvc.Compare("Password2", ErrorMessage = "密碼必須相同")]
        [DisplayName("密碼:")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "密碼不可為空白")]
        public string Password { get; set; }

        [System.Web.Mvc.Compare("Password", ErrorMessage = "密碼必須相同")]
        [DisplayName("再次確認:")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "密碼不可為空白")]
        public string Password2 { get; set; }
    }
}