using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace SystemLog.ProjectClass
{
    public class LogUtil
    {
        #region 屬性
        long StartTimeTicks; //開始時間(毫秒)
        List<string> MessageContent = new List<string>();//訊息
        int MethodLevel = 0; //呼叫方法順序
        #endregion

        #region 建構子
        public LogUtil(string FunctionName, string HostIP, string BrowserVersion)
        {
            this.StartTimeTicks = DateTime.Now.Ticks / 0x2710L;//開始時間(毫秒)

            // Log 表頭
            this.MessageContent.Add("[執行時間:" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] [執行程式：" + FunctionName + "] [來源 IP：" + HostIP + "] [瀏覽器版本：" + BrowserVersion + "]");
        }
        #endregion

        #region 方法
        /// <summary>
        /// 記錄方法名稱
        /// </summary>
        /// <param name="text"></param>
        public void AppendMethod(string text)
        {
            this.MethodLevel = this.MethodLevel + 1;
            this.MessageContent.Add(this.MethodLevel + ". " + text);
        }

        /// <summary>
        /// 記錄訊息內容
        /// </summary>
        /// <param name="text"></param>
        public void AppendMessage(string tag, string text)
        {
            this.MessageContent.Add("[" + tag + "] " + text);
        }

        /// <summary>
        /// 輸出 Log
        /// </summary>
        public void OutputLog()
        {
            string LogSaveType = ConfigurationManager.AppSettings["LogSaveType"]; //Log 儲存方式
            string LogPath = ConfigurationManager.AppSettings["LogPath"]; //Log 檔案儲存路徑 

            // 執行時間
            long runTimeMillisecond = (DateTime.Now.Ticks / 0x2710L) - this.StartTimeTicks;
            this.AppendMessage("執行時間", runTimeMillisecond.ToString("N0") + " ms");

            string[] saveTypes = LogSaveType.Split(',');
            foreach (string type in saveTypes)
            {
                if (type == "HTML")
                {
                    // 輸出為 HTML
                    if (System.IO.Directory.Exists(LogPath) == false)
                    {
                        System.IO.Directory.CreateDirectory(LogPath);
                    }

                    string logFile = LogPath + DateTime.Now.ToString("yyyyMMdd") + ".htm";

                    //寫入檔案
                    StreamWriter writer = new StreamWriter(logFile, true, Encoding.GetEncoding("UTF-8"));
                    foreach (string message in this.MessageContent)
                    {
                        writer.WriteLine(message.Replace("\n", "<br>") + "<br>");
                    }
                    writer.WriteLine("<hr>");
                    writer.Flush();
                    writer.Close();
                }
                if (type == "DB")
                {
                    //輸出至資料庫

                    // 取得資料庫連線字串
                    string connStr = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ConnDB"].ConnectionString;

                    // 當程式碼離開 using 區塊時，會自動關閉連接
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        // 資料庫連線
                        conn.Open();

                        // 寫入 Log 資料表
                        string sql = @"INSERT INTO SystemLog(LogTime ,LogMessage) VALUES (@LogTime, @LogMessage)";
                        SqlCommand cmd = new SqlCommand(sql, conn);

                        // 使用參數化填值
                        string errorContent = string.Join("\n", this.MessageContent.ToArray());
                        cmd.Parameters.AddWithValue("@LogTime", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@LogMessage", errorContent);

                        // 執行資料庫更新動作
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        #endregion
    }
}