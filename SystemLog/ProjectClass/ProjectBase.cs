using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SystemLog.ProjectClass
{
    public class ProjectBase : Controller
    {
        #region 屬性
        public LogUtil logUtil = null;
        #endregion

        #region MVC 事件
        /// <summary>
        /// 在執行 Action 之前執行
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Log 初始化
            string FunctionName = Request.CurrentExecutionFilePath; //執行程式
            string HostIP = GetClientIP(); //呼叫來源主機
            string BrowserVersion = this.GetClientBrowserVersion(); //瀏覽器版本

            this.logUtil = new LogUtil(FunctionName, HostIP, BrowserVersion);

            // 取得輸入參數轉為 Json 
            Hashtable param = new Hashtable();
            // Post 參數
            foreach (string key in base.Request.Form.Keys)
            {
                if (key == null)
                {
                    continue;
                }
                if (key.StartsWith("__"))
                {
                    continue;
                }
                param.Add(key, base.Request[key]);
            }
            // Get 參數
            foreach (string key in base.Request.QueryString.Keys)
            {
                param.Add(key, base.Request.QueryString[key]);
            }
            string paramJson = JsonConvert.SerializeObject(param);
            if (paramJson.Length > 2)
            {
                this.logUtil.AppendMessage("傳入參數", paramJson);
            }

            // 登入者帳號
            if (Session["UserID"] != null && Session["UserID"].ToString() != "")
            {
                this.logUtil.AppendMessage("登入帳號", Session["UserID"].ToString());
            }

            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// 在執行 Action Result 之前執行
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            // 自動記錄輸出 Model
            if (((System.Web.Mvc.ViewResultBase)filterContext.Result).Model != null)
            {
                this.logUtil.AppendMessage("輸出 Model", JsonConvert.SerializeObject(((System.Web.Mvc.ViewResultBase)filterContext.Result).Model));
            }

            this.logUtil.OutputLog(); //輸出Log
            base.OnResultExecuting(filterContext);
        }

        /// <summary>
        /// 錯誤事件
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnException(ExceptionContext filterContext)
        {
            //儲存錯誤訊息
            StringBuilder buff = new StringBuilder();
            buff.Append(string.Concat(new object[] { "Exception.Type : ", filterContext.Exception.GetType().Name, "\r\nException.Message : ", filterContext.Exception.Message, "\r\nException.TargetSite: ", filterContext.Exception.TargetSite, "\r\nException.StackTrace: \r\n", filterContext.Exception.StackTrace }));
            this.logUtil.AppendMessage("系統發生錯誤", buff.ToString());

            //輸出 Log
            this.logUtil.OutputLog();

            base.OnException(filterContext);
        }
        #endregion

        #region 共用方法
        /// <summary>
        /// 取得遠端呼叫者ip
        /// </summary>
        /// <returns></returns>
        public string GetClientIP()
        {
            string ClientIP = "";
            if (Request.ServerVariables["HTTP_VIA"] == null)
            {
                ClientIP = Request.ServerVariables["REMOTE_ADDR"].ToString();
            }
            else
            {
                ClientIP = Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            }
            ClientIP = ClientIP.Replace("::1", "127.0.0.1");
            return ClientIP;
        }

        /// <summary>
        /// 取得遠端呼叫者瀏覽器版本
        /// </summary>
        /// <returns></returns>
        public string GetClientBrowserVersion()
        {
            HttpBrowserCapabilitiesBase bc = Request.Browser;
            string brow_ver = bc.Browser + " " + bc.Version;
            return brow_ver;
        }
        #endregion
    }
}