using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication;
using System.Threading;
using Newtonsoft.Json.Linq;
using CommonLibrary;
using HslCommunication.BasicFramework;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ClientsLibrary
{

    /*********************************************************************************
     * 
     *    统一的账户登录模型
     *    包含的功能有
     *    1、维护状态检查
     *    2、账户检查
     *    3、框架版本检查、
     *    4、系统版本号检查
     *    5、初始化数据
     * 
     *********************************************************************************/


    /// <summary>
    /// 用户客户端使用的统一登录的逻辑
    /// </summary>
    public class AccountLogin
    {
        /// <summary>
        /// 系统统一的登录模型
        /// </summary>
        /// <param name="message_show">信息提示方法</param>
        /// <param name="start_update">启动更新方法</param>
        /// <param name="thread_finish">线程结束后的复原方法</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="remember">是否记住登录密码</param>
        /// <param name="clientType">客户端登录类型</param>
        /// <returns></returns>
        public static bool AccountLoginServer(
            Action<string> message_show,
            Action start_update,
            Action thread_finish,
            string userName,
            string password,
            bool remember,
            string clientType
            )
        {
            // 包装数据
            JObject json = new JObject
            {
                { UserAccount.UserNameText, new JValue(userName) },                                    // 用户名
                { UserAccount.PasswordText, new JValue(password) },                                    // 密码
            };
            User user = new User();
            user.userName = userName;
            user.password = password;
            UserClient.JsonSettings.LoginName = userName;
            UserClient.JsonSettings.Password = remember ? password : "";
            UserClient.JsonSettings.LoginTime = DateTime.Now;
            try
            {
                UserClient.JsonSettings.Token = login(user).Result;
                UserClient.JsonSettings.SaveToFile();
                return true;
            }
            catch
            {
                message_show.Invoke("无效的用户名或密码");
                return false;
            }
            finally
            {
                thread_finish.Invoke();
            }
        }

        static async Task<string> login(User user)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://10.0.2.2:10081/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.PostAsJsonAsync(
      "users/login", user);
            response.EnsureSuccessStatusCode();

            return response.Headers.GetValues("token").First();
        }

        private class User
        {
            public String userName { get; set; }
            public String password { get; set; }
        }
    }
}
