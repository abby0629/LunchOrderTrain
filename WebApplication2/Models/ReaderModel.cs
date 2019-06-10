using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class ReaderModel
    {
        [DisplayName("午餐A餐:")]
        public string AA { get; set; }
        [DisplayName("午餐B餐:")]
        public string AB { get; set; }
        [DisplayName("晚餐A餐:")]
        public string DA { get; set; }
        [DisplayName("午餐B餐:")]
        public string DB { get; set; }

    }
}