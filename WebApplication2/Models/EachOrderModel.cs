using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class EachOrderModel
    {


        [DisplayName("日期:")]
        public string Eachday { get; set; }
        [DisplayName("餐別:")]
        public string Meal { get; set; }
        [DisplayName("時間:")]
        public string ADmeal { get; set; }

        public int Oid { get; set; }
    }
}