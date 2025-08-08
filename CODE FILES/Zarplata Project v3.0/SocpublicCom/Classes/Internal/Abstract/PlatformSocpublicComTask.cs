using Microsoft.AspNetCore.SignalR.Client;
using System.Net;
using AngleSharp;
using CommonModels.UserAgentClasses;
using CommonModels;
using CommonModels.ClientLibraries.ProjectTask;
using CommonModels.ProjectTask.Platform.SocpublicCom;
using CommonModels.ProjectTask.EarningSite.SocpublicCom;
using CommonModels.Client;
using CommonModels.ProjectTask.EarningSite.SocpublicCom.TaskType.GSelectiveTaskWithAuth.SelectiveTask.WithdrawMoney;

namespace SocpublicCom.Classes.Internal.Abstract
{
    internal abstract class EarningSiteSocpublicComTask : WebClientTask
    {
        internal EarningSiteWorkBotClient Client { get; set; }
        internal SocpublicComTask? Task { get; set; }
        internal SocpublicComAutoregTask? TaskAutoreg { get; set; }
        internal WithdrawMoneyGroupSelectiveTaskWithAuth? TaskWithdrawMoney { get; set; }
        internal HubConnection ServerHubConnection { get; set; }
        internal HttpClient ServerHttpConnection { get; set; }
        Random random1 = new Random();

        private protected EarningSiteSocpublicComTask(
            EarningSiteWorkBotClient client,
            HubConnection serverHubConnection,
            HttpClient serverHttpConnection,
            SocpublicComTask? task = null,
            SocpublicComAutoregTask? taskAutoreg = null,
            WithdrawMoneyGroupSelectiveTaskWithAuth? taskWithdrawMoney = null)
        {
            Client = client;
            Task = task;
            ServerHubConnection = serverHubConnection;
            ServerHttpConnection = serverHttpConnection;
            TaskAutoreg = taskAutoreg;
            TaskWithdrawMoney = taskWithdrawMoney;
        }

        internal abstract Task StartWork();
        protected virtual bool InitWebContextData()
        {
            // set or create UserAgent
            UserAgent? userAgent = null;
            //bool useragent_is_unique = false;
            //int max_check_count = 10;
            //int current_check_count = 0;

            //while(useragent_is_unique == false)
            //{
            //    if (current_check_count >= max_check_count)
            //    {
            //        throw new Exception($"class PlatformSocpublicComTask(), method InitWebContextData(), current_check_count >= max_check_count({max_check_count}), error unique useragent ({userAgent?.Useragent})");
            //    }

            //    userAgent = CreateNewRandomUserAgent();

            //    if (userAgent == null)
            //    {
            //        throw new Exception($"class PlatformSocpublicComTask(), method InitWebContextData(), error userAgent == null");
            //    }

            //    // check user agent on unique
            //    try
            //    {
            //        useragent_is_unique = ServerHubConnection.InvokeAsync<bool>("check_useragent_on_unique", userAgent.Useragent).Result; // жесткий костыль без освобождения "потока"
            //    }
            //    catch { }

            //    current_check_count++;
            //}

            //get ua file
            //string[] ua_dump = File.ReadAllLines(@"C:\Users\glebg\Desktop\Zarplata Project v3.0\ua.txt");
            string[] ua_dump = File.ReadAllLines(@"ua.txt");
            userAgent = new UserAgent()
            {
                Useragent = ua_dump[random1.Next(0, ua_dump.Length)]
            };

            // set or create Cookies
            CookieContainer cookieContainer = new CookieContainer();
            var collection = CreateNewRandomCookieCollection();
            if (collection == null)
            {
                throw new Exception($"class PlatformSocpublicComTask(), method InitWebContextData(), error collection == null");
            }
            if (TaskAutoreg != null)
            {
                cookieContainer.Add(new Uri(TaskAutoreg.Url), collection);
            }
            else if (TaskWithdrawMoney != null)
            {
                cookieContainer.Add(new Uri(TaskWithdrawMoney.Url), collection);
            }
            else
            {
                cookieContainer.Add(new Uri(Task.Url), collection);
            }

            // set or create Headers
            var headers = CreateNewRandomHeaderCollection(userAgent!);
            if (headers == null)
            {
                throw new Exception($"class PlatformSocpublicComTask(), method InitWebContextData(), error h == null");
            }

            // create BrowsingContext for parse
            BrowsingContext ParseContext = new BrowsingContext();

            // init this data
            var init = base.SetWebContextData(
                contextUserA: userAgent,
                cookieC: cookieContainer,
                contextH: headers,
                htmlParseC: ParseContext);
            if (init == false)
            {
                throw new Exception($"class PlatformSocpublicComTask(), method InitWebContextData(), error init == false");
            }

            return true;
        }
        protected override UserAgent? CreateNewRandomUserAgent()
        {
            UserAgent? userAgent = null;

            try
            {
                userAgent = new UserAgent(UserAgent.CreateRandom()!);
            }
            catch { }

            return userAgent;
        }
        protected override CookieCollection? CreateNewRandomCookieCollection()
        {
            CookieCollection? cookieCollection = null;
            Random random = new Random();

            string _ym_d = random.Next(11111, 99999).ToString() + random.Next(11111, 99999).ToString();
            string _ym_uid = _ym_d + random.Next(111111111, 999999999).ToString();
            string _ga = "GA1.1." + random.Next(11111, 99999).ToString() + random.Next(11111, 99999).ToString() + "." + _ym_d;
            string _gid = "GA1.2." + random.Next(111111111, 999999999).ToString() + _ym_d;

            string _part_ga_635HGNP8QH = random.Next(1111111, 9999999).ToString();
            string _ga_635HGNP8QH = "GS1.1.168" + _part_ga_635HGNP8QH + "." + random.Next(0, 50).ToString() + "." + random.Next(0, 50).ToString() + "." + _part_ga_635HGNP8QH + "60.0.0";

            string _gcl_au = "1.1." + random.Next(11111, 99999).ToString() + random.Next(11111, 99999).ToString() + "." + _ym_d;
            string secret = Guid.NewGuid().ToString();
            string session_id = Guid.NewGuid().ToString();
            string supportOnlineTalkID = Guid.NewGuid().ToString("N");
            string user_data = "a%3A0%3A%7B%7D";
            string _ym_isad = "2";
            string _ym_visorc = "w";
            string _gat_gtag_UA_186037441_1 = "1";

            try
            {
                cookieCollection = new CookieCollection()
                {
                    new Cookie()
                    {
                        Name = "_ga",
                        Value = _ga,
                        Domain = "socpublic.com",
                        Path = "/",
                        Expired = false,
                        Expires = DateTime.UtcNow,
                        Secure = false,
                        HttpOnly = false
                    },
                    new Cookie()
                    {
                        Name = "_ga_635HGNP8QH",
                        Value = _ga_635HGNP8QH,
                        Domain = "socpublic.com",
                        Path = "/",
                        Expired = false,
                        Expires = DateTime.UtcNow,
                        Secure = false,
                        HttpOnly = false
                    },
                    new Cookie()
                    {
                        Name = "_gat_gtag_UA_186037441_1",
                        Value = _gat_gtag_UA_186037441_1,
                        Domain = "socpublic.com",
                        Path = "/",
                        Expired = false,
                        Expires = DateTime.UtcNow,
                        Secure = false,
                        HttpOnly = false
                    },
                    new Cookie()
                    {
                        Name = "_gcl_au",
                        Value = _gcl_au,
                        Domain = "socpublic.com",
                        Path = "/",
                        Expired = false,
                        Expires = DateTime.UtcNow,
                        Secure = false,
                        HttpOnly = false
                    },
                    new Cookie()
                    {
                        Name = "_gid",
                        Value = _gid,
                        Domain = "socpublic.com",
                        Path = "/",
                        Expired = false,
                        Expires = DateTime.UtcNow,
                        Secure = false,
                        HttpOnly = false
                    },
                    new Cookie()
                    {
                        Name = "_ym_d",
                        Value = _ym_d,
                        Domain = "socpublic.com",
                        Path = "/",
                        Expired = false,
                        Expires = DateTime.UtcNow,
                        Secure = true,
                        HttpOnly = false
                    },
                    new Cookie()
                    {
                        Name = "_ym_isad",
                        Value = _ym_isad,
                        Domain = "socpublic.com",
                        Path = "/",
                        Expired = false,
                        Expires = DateTime.UtcNow,
                        Secure = true,
                        HttpOnly = false
                    },
                    new Cookie()
                    {
                        Name = "_ym_uid",
                        Value = _ym_uid,
                        Domain = "socpublic.com",
                        Path = "/",
                        Expired = false,
                        Expires = DateTime.UtcNow,
                        Secure = true,
                        HttpOnly = false
                    },
                    new Cookie()
                    {
                        Name = "_ym_visorc",
                        Value = _ym_visorc,
                        Domain = "socpublic.com",
                        Path = "/",
                        Expired = false,
                        Expires = DateTime.UtcNow,
                        Secure = true,
                        HttpOnly = false
                    },
                    new Cookie()
                    {
                        Name = "secret",
                        Value = secret,
                        Domain = "socpublic.com",
                        Path = "/",
                        Expired = false,
                        Expires = DateTime.UtcNow,
                        Secure = false,
                        HttpOnly = false
                    },
                    new Cookie()
                    {
                        Name = "session_id",
                        Value = session_id,
                        Domain = "socpublic.com",
                        Path = "/",
                        Expired = false,
                        Expires = DateTime.UtcNow,
                        Secure = false,
                        HttpOnly = true
                    },
                    new Cookie()
                    {
                        Name = "supportOnlineTalkID",
                        Value = supportOnlineTalkID,
                        Domain = "socpublic.com",
                        Path = "/",
                        Expired = false,
                        Expires = DateTime.UtcNow,
                        Secure = false,
                        HttpOnly = false
                    },
                    new Cookie()
                    {
                        Name = "user_data",
                        Value = user_data,
                        Domain = "socpublic.com",
                        Path = "/",
                        Expired = false,
                        Expires = DateTime.UtcNow,
                        Secure = false,
                        HttpOnly = false
                    }
                };
            }
            catch { }

            return cookieCollection;
        }
        protected override Dictionary<string, List<string>>? CreateNewRandomHeaderCollection(UserAgent userAgent)
        {
            Dictionary<string, List<string>> headerCollection = new Dictionary<string, List<string>>();

            try
            {
                headerCollection.Add("accept", new List<string>() { "text/html", "text/plain", "application/json", "application/xhtml+xml", "application/xml;q=0.9", "image/webp", "image/apng", "*/*;q=0.8", "application/signed-exchange;v=b3;q=0.7" });
                headerCollection.Add("accept-encoding", new List<string>() { "gzip", "deflate", "br" });
                headerCollection.Add("accept-language", new List<string>() { "ru", "en;q=0.9", "en-GB;q=0.8", "en-US;q=0.7" });
                headerCollection.Add("sec-fetch-dest", new List<string>() { "document" });
                headerCollection.Add("sec-fetch-mode", new List<string>() { "navigate" });
                headerCollection.Add("sec-fetch-site", new List<string>() { "same-origin" });
                headerCollection.Add("sec-fetch-user", new List<string>() { "?1" });
                headerCollection.Add("upgrade-insecure-requests", new List<string>() { "1" });
                headerCollection.Add("user-agent", new List<string>() { userAgent.Useragent! });
            }
            catch
            {
                return null;
            }

            return headerCollection;
        }
        private protected async Task SendTaskChangedResultStatusReport()
        {
            try
            {
                await ServerHubConnection.SendAsync("platform_socpubliccom_task_status_changed", Task);
            }
            catch (Exception ex)
            {
            }
        }
        private protected void Dispose_PlatformSocpublicComTask_Data()
        {
            Task = null;
            TaskAutoreg = null;
            TaskWithdrawMoney = null;

            base.DisposeData();
        }
    }
}
