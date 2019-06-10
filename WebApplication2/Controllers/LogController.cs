using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;
using Dapper;
using System.Configuration;
using System.Linq;

namespace WebApplication2.Controllers
{
    public class LogController : Controller
    {
        SqlConnection em = new SqlConnection();
        static string account;

        // GET: Log
        readonly string contr=@"Server=a604\sqlexpress;database=test1;uid=sa;pwd=98236754" ; //資料庫連接字串
    

            public ActionResult Index()
        {
            if (Session["account"] == null || string.IsNullOrWhiteSpace(Session["account"].ToString())) //如果還沒有登入帳號就直接顯示Index View
            {
                return RedirectToAction("Login", "Log");
            }
            else
            {
                 //將Session[account]的資料存到字串account中
                account = Session["account"].ToString();
                if (account == "sa") //判斷是不是管理員帳號
                {
                    return RedirectToAction("SaIndex"); //如果是管理員帳號就轉到管理員Index
                }

                else//本月
                {
                    string month = DateTime.Today.Month.ToString(); //把現在是幾月存到month                  
                    List<OrderModel> OrderList = new List<OrderModel>(); //新建一個以OrderModel為基礎的List              
                    using (var cn = new SqlConnection(em.ConnectionString = contr)) //使用Dapper來顯示資料
                    {
                        OrderList = cn.Query<OrderModel>("SELECT * FROM Orderday2 WHERE account =@account AND month =@month ORDER BY orderday ", new { account, month }).ToList();
                    }
                    return View(OrderList);//將DataTable傳到View 
                };
            }
        }

        public ActionResult IndexNextMonth() //顯示下個月資料的Index,omonth為下個月的月份
        {
            string month = DateTime.Today.AddMonths(1).Month.ToString();
            if (Session["account"] == null || string.IsNullOrWhiteSpace(Session["account"].ToString())) //如果還沒有登入帳號就直接顯示Login view
            {
                return View("Login");
            }
            else
            {
                account = Session["account"].ToString();
                if (account == "sa") 
                {
                    return RedirectToAction("SaIndex");
                }
                    DataTable dtt = QuerySql("SELECT * FROM Orderday2 WHERE account ='" + account + "' AND month ='" + month + "' ORDER BY orderday ");
                    return View(dtt);  
            }
        }
        
        public ActionResult SaIndex() //管理員畫面
        {       
            DataTable dtt = QuerySql("SELECT account FROM Account WHERE rank ='normal'");
            return View(dtt);     
        }

        public ActionResult NormalReader(string account)
        {
            DataTable dtt = QuerySql("SELECT * FROM Orderday2 WHERE account ='" + account + "'ORDER BY orderday ");
            TempData["account"] = account;
            return View(dtt);
        }

        private void Loginsql()//連接Sql server
        {
            em.ConnectionString = @"Server=a604\sqlexpress;database=test1;uid=sa;pwd=98236754";
            em.Open();
        }
        private void ExecuteSql(String sql)//執行Sql語法
        {
            Loginsql();
            SqlCommand cmd = new SqlCommand(sql, em);
            cmd.ExecuteNonQuery();
            em.Close();
        }

        private DataTable QuerySql(string sql) //取得資料
        {
            Loginsql();
            SqlDataAdapter adp = new SqlDataAdapter(sql, em);
            DataSet ds = new DataSet();
            adp.Fill(ds);
            return ds.Tables[0];
        }

       

        public ActionResult Register()
        {
            RegisterModel register = new RegisterModel(); 
            return View();
        }
        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            bool isExist = false;
            Loginsql();

            SqlCommand cmd = new SqlCommand//檢查帳號是否已存在
            {
                Connection = em,
                CommandText = "SELECT COUNT(*) FROM Account WHERE account = '" + model.Account + "'"
            };
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Read();              
                isExist = reader.GetInt32(0) > 0;
                em.Close();
            }
            if (isExist == true)
            {
                TempData["ResultMessage"] = String.Format("失敗");
                ViewBag.ResultMessage = "此帳號已存在，請重新輸入";
                return View(model);
            }
            else if (ModelState.IsValid) //如果資料驗證成功，就新增資料到資料庫
            {
                string sql = "INSERT INTO Account(account,password,rank)VALUES('" + model.Account + "','" + model.Password + "','normal')";
                ExecuteSql(sql);
                em.Close();

            }
            else return View(model); //如果資料驗證失敗，就留在原頁面

            return RedirectToAction("Index", "Log");
        }


        public ActionResult Login() //登入畫面
        {
            LoginModel login = new LoginModel();
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginModel data) //登入驗證
        {        
            Loginsql();       
            SqlCommand cmd = new SqlCommand
            {
                Connection = em,
                CommandText = "SELECT *FROM Account WHERE Account = '" + data.Account + "'"
            };
            SqlDataReader reader = cmd.ExecuteReader(); //執行CommandText
            reader.Read();               
            if (data.Account == reader.GetString(1) && data.Password == reader.GetString(2))//確認輸入的帳密是否正確
            {
                ViewBag.ResultMessage = "登入成功";
                string account = reader.GetString(1);//將帳號取出來存到Session[account]
                Session["account"] = account;
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.ResultMessage = "帳號密碼錯誤"; //如果帳密錯誤就顯示帳號密碼錯誤
                return View("Login");
            }
        }
        public ActionResult Logout() //把Session[account]清空
        {
            Session["account"] = "";
            return View();
        }

        public ActionResult Order()//批量訂購
        {
            if (Session["account"] == null || string.IsNullOrWhiteSpace(Session["account"].ToString())) //如果還沒有登入帳號就直接顯示Login view
            {
                return View("Login");
            }
            else
            { 
                account = Session["account"].ToString();
            }

            DateTime today = DateTime.Today;   //抓今天的日期
            DateTime LastDay = DateTime.Now.AddMonths(1).AddDays(-DateTime.Now.AddMonths(1).Day); //抓這個月最後一天的日期
            TimeSpan span = LastDay.Subtract(today); //最後一天與今天相減，算出這個月剩幾天
            int dayDiff = span.Days; //把天數存到dayDiff
            String[] date = new String[dayDiff]; //定義一個Date陣列，大小為dayDiff
            today= today.AddDays(1); //將today日期+1
            bool isExist = false;
            
            int j = 0;
            for (int i = 0; i < dayDiff; i++)
                {

                string day = today.DayOfWeek.ToString();  //抓today的日期是星期幾
                string datetime = string.Format("{0:yyyy/MM/dd}", today); //將日期轉成資料庫格式
                Loginsql();
                SqlCommand cmd = new SqlCommand//檢查此日期資料否已存在
                {
                    Connection = em,
                    CommandText = "SELECT COUNT(*) FROM Orderday2 WHERE orderday = '" + datetime + "'AND account='" + account + "'"
                };            

                using (SqlDataReader reader = cmd.ExecuteReader()) //如果該日期訂購紀錄大於1，isExist就是true，那個日期就不顯示
                {
                    reader.Read();
                    isExist = reader.GetInt32(0) > 1;
                    em.Close();
                }           
               if (isExist == false) //如果isExist是false且不是Sunday、Saturday就把日期加入到date陣列
                {
                    if(day != "Sunday" && day != "Saturday")
                    {
                        date[j] = datetime;
                        j += 1;
                    }
                }           
                today = today.AddDays(1); //將today日期+1
            }           
            ViewBag.date = date; //將date陣列裝到ViewVag.date
            ViewBag.length = j;
            return View();           
        }

        [HttpPost]
        public ActionResult Order(FormCollection collection, string[] date)
        {
          
                account = Session["account"].ToString();
            int mount = (collection.Count+1)/2;    
            //int mount = date.Length;    //將date陣列的長度存到mount
            for (int j = 0; j < mount; j++)
                {
                    if(collection["ADmeal[" + j + "]"] != null && collection["meal[" + j + "]"]!=null) {
                    bool isExist = false;
                    Loginsql();
                    SqlCommand cmd = new SqlCommand//在資料庫搜尋此日期資料否已存在
                    {
                        Connection = em,
                        CommandText = "SELECT COUNT(*) FROM Orderday2 WHERE orderday = '" + date[j] + "'AND account='" + account  + "'AND ADmeal ='" + collection["ADmeal[" + j + "]"] + "'"
                    };                   
                    using (SqlDataReader reader = cmd.ExecuteReader()) //如果該日期訂購紀錄大於0就不新增
                    {
                        reader.Read();
                        isExist = reader.GetInt32(0) > 0;
                        em.Close();
                    }
                    if (collection["meal[" + j + "]"] != null&& isExist == false)
                    {
                    /*   DateTime orderdate = Convert.ToDateTime(date[j]);
                         string month = orderdate.Month.ToString();*/
                         string month = Convert.ToDateTime(date[j]).Month.ToString();  //抓資料的的月份                 
                         string sql = "INSERT INTO Orderday2(account,orderday,meal,ADmeal,month )VALUES('" + account + "','" + date[j] + "','" + collection["meal[" + j + "]"] + "','" + collection["ADmeal[" + j + "]"] + "','" + month + "')";
                         ExecuteSql(sql);      
                    }
                }
            }     
            return RedirectToAction("Index");
        }

        public ActionResult OrderNextWeek() //訂下個月
        {
            if (Session["account"] == null || string.IsNullOrWhiteSpace(Session["account"].ToString())) //如果還沒有登入帳號就直接顯示Login view
            {
                return View("Login");
            }
            else
            {
                account = Session["account"].ToString();
            }
            DateTime FirstDay = DateTime.Now.AddMonths(1).AddDays(-DateTime.Now.Day + 1);
            DateTime LastDay = DateTime.Now.AddMonths(2).AddDays(-DateTime.Now.AddMonths(1).Day);
            TimeSpan span = LastDay.Subtract(FirstDay);
            int dayDiff = span.Days;
            String[] date = new String[dayDiff];
            FirstDay = FirstDay.AddDays(1);
            bool isExist = false;
           
            int j = 0;

            for (int i = 0; i < dayDiff; i++)
            {
                string day = FirstDay.DayOfWeek.ToString();
                string datetime = string.Format("{0:yyyy/MM/dd}", FirstDay); //將日期轉成資料庫格式
                Loginsql();
                SqlCommand cmd = new SqlCommand//檢查此日期資料否已存在
                {
                    Connection = em,
                    CommandText = "SELECT COUNT(*) FROM Orderday2 WHERE orderday = '" + datetime + "'AND account='" + account + "'"
                };
                

                using (SqlDataReader reader = cmd.ExecuteReader()) //如果該日期訂購紀錄大於1就不顯示
                {
                    reader.Read();
                    isExist = reader.GetInt32(0) > 1;
                    em.Close();
                }
                if (isExist == false) //如果isExist是false且不是Sunday、Saturday就把日期加入到date陣列
                {
                    if (day != "Sunday" && day != "Saturday")
                    {
                        date[j] = datetime;
                        j += 1;
                    }
                }
                FirstDay = FirstDay.AddDays(1);
            }
            ViewBag.date = date;
            ViewBag.length = j;
            return View();
        }

        [HttpPost]
        public ActionResult OrderNextWeek(FormCollection collection, string[] date)
        {
            account = Session["account"].ToString();
            int mount = (collection.Count + 1) / 2;
            //int mount = date.Length;    //將date陣列的長度存到mount
            for (int j = 0; j < mount; j++)
            {
                if (collection["ADmeal[" + j + "]"] != null && collection["meal[" + j + "]"] != null)
                {
                    bool isExist = false;
                    Loginsql();
                    SqlCommand cmd = new SqlCommand//在資料庫搜尋此日期資料否已存在
                    {
                        Connection = em,
                        CommandText = "SELECT COUNT(*) FROM Orderday2 WHERE orderday = '" + date[j] + "'AND account='" + account + "'AND ADmeal ='" + collection["ADmeal[" + j + "]"] + "'"
                    };
                    using (SqlDataReader reader = cmd.ExecuteReader()) //如果該日期訂購紀錄大於0就不新增
                    {
                        reader.Read();
                        isExist = reader.GetInt32(0) > 0;
                        em.Close();
                    }
                    if (collection["meal[" + j + "]"] != null && isExist == false)
                    {
                        /*   DateTime orderdate = Convert.ToDateTime(date[j]);
                             string month = orderdate.Month.ToString();*/
                        string month = Convert.ToDateTime(date[j]).Month.ToString();  //抓資料的的月份                 
                        string sql = "INSERT INTO Orderday2(account,orderday,meal,ADmeal,month )VALUES('" + account + "','" + date[j] + "','" + collection["meal[" + j + "]"] + "','" + collection["ADmeal[" + j + "]"] + "','" + month + "')";
                        ExecuteSql(sql);
                    }
                }
            }
            return RedirectToAction("IndexNextMonth");
        }

        public ActionResult EachOrder()//單筆訂購
        {
            EachOrderModel EachModel = new EachOrderModel();
            return View();
        }
        [HttpPost]
        public ActionResult EachOrder(EachOrderModel EachModel)
        {
            
            int deal = 0;
            if (Session["account"] == null || string.IsNullOrWhiteSpace(Session["account"].ToString())) //如果還沒有登入帳號就直接顯示Login view
            {
                return View("Login");
            }
            else
            {
                account = Session["account"].ToString();
            }
              
            DateTime today2 = DateTime.Now;//取得今天的日期  
            DateTime today = DateTime.Today;
            string std= string.Format("{0:yyyy/MM/dd}", today2); //將today2日期轉為字串std
            DateTime compareday = Convert.ToDateTime(EachModel.Eachday); //將Eachday轉為日期格式
            DateTime end = Convert.ToDateTime(EachModel.Eachday+"上午 10:00:00"); //10點
            String month = compareday.Month.ToString(); //抓Eachday的月份
 
                       
            string day = compareday.DayOfWeek.ToString();

            if (day == "Sunday" || day == "Saturday")
            {
                ViewBag.dayoff = "假日不可以訂購";
                return View(EachModel);
            }
            if (std == EachModel.Eachday)
            {
                if (today2 > end)
                {
                    ViewBag.tenoff = "請在當日十點以前訂購";
                    return View(EachModel);
                }
            }
            if (today > compareday)
            {
                ViewBag.dayoff = "不可以訂購"+ std + "以前的日期";
                return View(EachModel);
            }
            
            bool isExist = false;
            Loginsql();
            SqlCommand cmd = new SqlCommand//檢查日期是否已存在
            {
                Connection = em,
                CommandText = "SELECT COUNT(*) FROM Orderday2 WHERE orderday = '" + EachModel.Eachday + "'AND account='" + account+"'"
            };
            if (em.State == ConnectionState.Closed)
            {
                em.Open();
            }

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Read();
                deal = reader.GetInt32(0); //將訂單數存到deal
                em.Close();
            }
            if(deal >= 2)
            {
                ViewBag.deal2 = "此日期已訂購完畢";
                return View(EachModel);
            }
            else if (deal == 1)
            {//如果有一筆資料，查是午餐還晚餐，並顯示您已訂購過
                Loginsql();
                cmd.CommandText = "SELECT COUNT(*) FROM Orderday2 WHERE orderday = '" + EachModel.Eachday + "'AND account='" + account + "'AND ADmeal ='午餐'" ;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    isExist = reader.GetInt32(0)>0; //午餐訂單是否存在
                    em.Close();
                }
                if (isExist == true && EachModel.ADmeal == "0")
                {
                    ViewBag.ResultMessage = "您已訂購午餐，請重新輸入";
                    return View(EachModel);
                }
                else if(isExist ==false && EachModel.ADmeal =="1")
                {
                    ViewBag.ResultMessage = "您已訂購晚餐請重新輸入";
                    return View(EachModel);
                }

            }
          
            
            string ADmeal = "";
            string Meal = "";
            if (EachModel.ADmeal == "0")
                ADmeal = "午餐";
            if (EachModel.ADmeal == "1")
                ADmeal = "晚餐";
            if (EachModel.Meal == "0")
                Meal = "A餐";
            if (EachModel.Meal == "1")
                Meal = "B餐";
          
            string sql = "INSERT INTO Orderday2(account,orderday,meal,ADmeal,month)VALUES('" + account + "','" + EachModel.Eachday +"','"+ Meal + "','"+ ADmeal + "','"+month+"')";
            ExecuteSql(sql);
            em.Close();
            return RedirectToAction("Index");
        }
        public ActionResult Delete(string oid)
        {           
            string sql = "DELETE FROM Orderday2 WHERE oid='" + oid + "'";
            ExecuteSql(sql);           
            return RedirectToAction("Index");
        }
        public ActionResult Delete2(string oid)
        {
            string sql = "DELETE FROM Orderday2 WHERE oid='" + oid + "'";
            ExecuteSql(sql);
            return RedirectToAction("IndexNextMonth");
        }
        public ActionResult EditOrder(string orderday,string ordermeal,string orderADmeal,int orderid) //編輯訂單
        {
            EachOrderModel editModel = new EachOrderModel();
            editModel.Oid = orderid;
            editModel.Eachday = orderday;
            if (ordermeal == "A餐")
                editModel.Meal = "0";
            else
                editModel.Meal = "1";
            if (orderADmeal == "午餐")
                editModel.ADmeal = "0";
            else
                editModel.ADmeal = "1";
            TempData["ordertime"] = orderday;
            return View(editModel);
        }

        [HttpPost]
        public ActionResult EditOrder(EachOrderModel editModel)//編輯訂單
        {
            bool isExist = false;
            string account = "";
            account = Session["account"].ToString();
            string ADmeal = "";
            string Meal = "";
            string date = editModel.Eachday;
            if (editModel.ADmeal == "0")
                ADmeal = "午餐";
            if (editModel.ADmeal == "1")
                ADmeal = "晚餐";
            if (editModel.Meal == "0")
                Meal = "A餐";
            if (editModel.Meal == "1")
                Meal = "B餐";
            Loginsql();
            SqlCommand cmd = new SqlCommand//檢查日期是否已存在
            {
                Connection = em,
                CommandText = "SELECT COUNT(*) FROM Orderday2 WHERE orderday = '" + editModel.Eachday + "' AND ADmeal ='"+ADmeal+"'"
            };
            if (em.State == ConnectionState.Closed)
            {
                em.Open();
            }

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Read();
                int i = reader.GetInt32(0);
                isExist = reader.GetInt32(0) > 0;
                em.Close();
            }
            if (isExist == true)
            {
                TempData["dontrepeat"] = "不可以重複購買"+editModel.Eachday+ ""+ ADmeal+ "，請重新修改";               
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                string sql = "UPDATE Orderday2 SET meal='" + Meal + "',ADmeal='"+ADmeal+ "' WHERE oid ='" + editModel.Oid + "'";
                ExecuteSql(sql);
               
                return RedirectToAction("Index");

            }
            else
                return View(editModel);
        }
        public ActionResult Reader()
        {
            ReaderModel rmodel = new ReaderModel();
            
            DateTime today = DateTime.Today;
            Loginsql();
            string day = today.DayOfWeek.ToString();
            string datetime = string.Format("{0:d}", today);
            TempData["datetime"] = datetime;
            SqlCommand cmd = new SqlCommand//計算午餐A餐數量

            {
                Connection = em,
                CommandText = "SELECT COUNT(*) FROM Orderday2 WHERE orderday = '" + datetime + "'AND meal='A餐' AND ADmeal ='午餐'" //午餐A餐
            };
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Read();
                String AA = reader.GetInt32(0).ToString(); //人數存入到AA變數
                TempData["AA"] = AA;
                em.Close();
            }
          
            if (em.State == ConnectionState.Closed)
            {
                em.Open();
            }
           
            cmd.CommandText = "SELECT COUNT(*) FROM Orderday2 WHERE orderday = '" + datetime + "'AND meal='B餐' AND ADmeal ='午餐'"; //午餐B餐
            if (em.State == ConnectionState.Closed)
            {
                em.Open();
            }
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Read();
                String AB = reader.GetInt32(0).ToString() ;
                TempData["AB"] = AB;
                em.Close();
            }

            cmd.CommandText = "SELECT COUNT(*) FROM Orderday2 WHERE orderday = '" + datetime + "'AND meal='A餐' AND ADmeal ='晚餐'"; //晚餐A餐
            if (em.State == ConnectionState.Closed)
            {
                em.Open();
            }
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Read();
                String DA = reader.GetInt32(0).ToString();
                TempData["DA"] = DA;
                em.Close();
            }

            cmd.CommandText = "SELECT COUNT(*) FROM Orderday2 WHERE orderday = '" + datetime + "'AND meal='B餐' AND ADmeal ='晚餐'"; //晚餐B餐
            if (em.State == ConnectionState.Closed)
            {
                em.Open();
            }
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Read();
                String DB = reader.GetInt32(0).ToString();
                TempData["DB"] = DB;
                em.Close();
            }
            

            return View();
        }
        public ActionResult Photoupload() //上傳菜單
        {
            return View();
        }
        [HttpPost]
        public ActionResult Photoupload(HttpPostedFileBase photoFile, string month)
        {
           
            ViewBag.path = TempData["id"];
            TempData["id"] = month;
            //如果有上傳成功
            if (photoFile != null)
            {
                //圖片結尾是否為gif|png|jpg|bmp
                if (!isPicture(photoFile.FileName))
                {
                    TempData["ErrorMessage"] = "內容不為圖片";
                    return RedirectToAction("Upload");
                }
                //檔案是否為圖片
                if (IsImage(photoFile) == null)
                {
                    TempData["ErrorMessage"] = "檔案不為圖片";
                    return RedirectToAction("Upload");
                }
                //大小>0byte
                if (photoFile.ContentLength > 0)
                {
                    //檔案名
                    var fileName = "photo.jpg";
                    //路徑
                    var path = Path.Combine(Server.MapPath("~/FileUploads/" + month));
                    //路徑加檔案名
                    var pathName = Path.Combine(Server.MapPath("~/FileUploads/" + month), fileName);
                    //資料夾不存在的話創一個
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    //有此檔名的話把他刪了
                    if (System.IO.File.Exists(pathName))
                    {
                        System.IO.File.Delete(pathName);
                    }
                    Image photo = Image.FromStream(photoFile.InputStream);
                    string sql = "INSERT INTO Menu(month,menu)VALUES('" + month + "','" + "~/FileUploads/" + month + "')";
                    ExecuteSql(sql);
                    em.Close();
                    photo.Save(pathName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return RedirectToAction("SaIndex");

                }
            }
            return RedirectToAction("Photoupload");
        }
        protected bool isPicture(string fileName)
        {
            string extensionName = fileName.Substring(fileName.LastIndexOf('.') + 1);
            var reg = new Regex("^(gif|png|jpg|bmp)$", RegexOptions.IgnoreCase);
            return reg.IsMatch(extensionName);
        }

        //檔案是否為圖片
        private Image IsImage(HttpPostedFileBase photofile)
        {
            try
            {
                Image img = Image.FromStream(photofile.InputStream);
                return img;
            }
            catch
            {
                return null;
            }
        }
        public ActionResult MenuIndex()
        {
            Loginsql();

            int[] check = new int[13];
            SqlCommand cmd = new SqlCommand
            {
                Connection = em,           
            };

            for (int i = 1; i < 13; i++)
            {   

                cmd.CommandText = "SELECT COUNT(*) FROM Menu WHERE month = " + i;
                bool isExist = false;
                if (em.State == ConnectionState.Closed)
                {
                    em.Open();
                }
                SqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                isExist = reader.GetInt32(0) > 0;
                em.Close();
                if (isExist == true)
                {
                    check[i] = i;
                }
                else
                    check[i] = 0;
              
              
            }
            ViewBag.check = check;
          
            return View();
        }
        public ActionResult DeletePhoto(string check)
        {
            var fileName = "photo.jpg";
            //路徑        
            var path = Path.Combine(Server.MapPath("~/FileUploads/" + check));
            var pathName = Path.Combine(Server.MapPath("~/FileUploads/" + check), fileName);
            System.IO.File.Delete(pathName);
            string sql = "DELETE FROM Menu WHERE month='" + check + "'";
            ExecuteSql(sql);
            return RedirectToAction("MenuIndex");
        }
    }
 }
