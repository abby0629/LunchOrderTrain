using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.ViewModel
{
    public class EmployeeViewModel
    {
       

        [DisplayName("編號:")]
        [Required(ErrorMessage ="編號不可為空白")]
        public string ID { get; set; }

        [DisplayName("姓名:")]
        [Required(ErrorMessage = "姓名不可為空白")]
        public string NAME { get; set; }
    }
}