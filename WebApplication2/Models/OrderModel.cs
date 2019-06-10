using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class OrderModel
    {
        [DisplayName("您有訂購便當的日期:")]
        public string Orderday { get; set; }
        [DisplayName("餐別:")]
        public string Meal { get; set; }
        [DisplayName("時間:")]
        public string ADmeal { get; set; }  
        public string Account { get; set; }
        public string Month { get; set; }
        public string Oid { get; set; }
       
    }
 
}