using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using SystemLog.ProjectClass;

namespace SystemLog.Controllers
{
    public class HomeController : ProjectBase
    {
        public ActionResult Index()
        {
            // 記錄方法名稱
            this.logUtil.AppendMethod(MethodBase.GetCurrentMethod().DeclaringType.FullName + "." + MethodBase.GetCurrentMethod().Name);

            //呼叫資料庫
            DataTable dt = this.CallDatabase();

            // 回傳 Model
            Hashtable outModel = new Hashtable(); //測試 Model
            outModel.Add("Result", dt);

            return View(outModel);
        }

        /// <summary>
        /// 呼叫資料庫
        /// </summary>
        private DataTable CallDatabase()
        {
            // 記錄方法名稱
            this.logUtil.AppendMethod(MethodBase.GetCurrentMethod().DeclaringType.FullName + "." + MethodBase.GetCurrentMethod().Name);

            string connStr = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ConnDB"].ConnectionString;
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = connStr;
            conn.Open();
            string sql = "select UserName from Member where UserID = @UserID "; //測試用 sql
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = sql;
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@UserID", "Test");

            // 在執行 SQL 之前先記錄指令
            string sqlLog = sql;
            foreach (SqlParameter param in cmd.Parameters)
            {
                sqlLog = sqlLog.Replace(param.ParameterName.ToString(), "'" + param.Value.ToString() + "'");
            }
            this.logUtil.AppendMessage("SQL", sqlLog);

            SqlDataAdapter adpt = new SqlDataAdapter();
            adpt.SelectCommand = cmd;
            DataSet ds = new DataSet();
            adpt.Fill(ds);
            DataTable dt = ds.Tables[0];
            // 省略...
            conn.Close();

            return dt;
        }

        public ActionResult Divide(int x, int y)
        {
            // 記錄方法名稱
            this.logUtil.AppendMethod(MethodBase.GetCurrentMethod().DeclaringType.FullName + "." + MethodBase.GetCurrentMethod().Name);

            int z = x / y;

            Hashtable outModel = new Hashtable();
            outModel.Add("Result", z.ToString());

            return View(outModel);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}