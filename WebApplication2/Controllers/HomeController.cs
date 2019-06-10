using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using WebApplication2.Models;
using WebApplication2.ViewModel;

namespace WebApplication2.Controllers
{


    public class HomeController : Controller
    {
        SqlConnection em = new SqlConnection();
        private void Loginsql()//連接Sql server
        {
            em.ConnectionString = @"Server=a604\sqlexpress;database=test1;uid=sa;pwd=98236754";
            em.Open();
        }
        private void ExecuteSql(String sql)//執行Sql語法
        {
            Loginsql();
            SqlCommand cmd = new SqlCommand(sql, em);
            int a = cmd.ExecuteNonQuery();
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




        public ActionResult Index()//主頁面
        {
            DataTable dt = QuerySql("SELECT * FROM test_teble ORDER BY empid");
            return View(dt);
        }

        public ActionResult Create()//初始化
        {

            EmployeeViewModel model = new EmployeeViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(EmployeeViewModel model)//新增
        {
            bool isExist = false;
            Loginsql();

            SqlCommand cmd = new SqlCommand
            {
                Connection = em,
                CommandText = "SELECT COUNT(*) FROM test_teble WHERE empid = '" + model.ID + "'"
            };
            if (em.State == ConnectionState.Closed)
            {
                em.Open();
            }

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Read();
                isExist = reader.GetInt32(0) > 0;
                em.Close();
            }
            if (isExist == true)
            {
                TempData["ResultMessage"] = String.Format("失敗");
                ViewBag.ResultMessage = "ID已存在，請重新輸入";
                return View(model);
            }
            else if (ModelState.IsValid) //如果資料驗證成功，就新增資料到資料庫
            {
                string sql = "INSERT INTO test_teble(empid,empname)VALUES('" + model.ID + "','" + model.NAME + "')";
                ExecuteSql(sql);
                em.Close();
            }
            else return View(model); //如果資料驗證失敗，就留在原頁面
            return RedirectToAction("Index"); //回到Index



        }


        public ActionResult Edit(string empid, string empname)
        {

            EditModel editModel = new EditModel
            {
                ID = empid,
                NAME = empname
            };
            return View(editModel);
        }

        [HttpPost]
        public ActionResult Edit(EditModel editModel)//編輯
        {

            if (ModelState.IsValid)
            {
                string sql = "UPDATE test_teble SET empname='" + editModel.NAME + "' WHERE empid ='" + editModel.ID + "'";
                ExecuteSql(sql);
                return RedirectToAction("Index");
            }
            else
                return View(editModel);
        }




        public ActionResult Delete(string empid)
        {
            string sql = "DELETE FROM test_teble WHERE empid='" + empid + "'";
            ExecuteSql(sql);
            return RedirectToAction("Index");
        }

       
       
    }
}