using AngleSharp.Dom;
using AngleSharp;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.PortableExecutable;
using System.Net;
using Microsoft.AspNetCore.WebUtilities;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels;
using CommonModels.ProjectTask.Platform.SocpublicCom;
using static System.Net.Mime.MediaTypeNames;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models;
using CommonModels.UserAgentClasses;
using CommonModels.Captcha;
using CommonModels.ProjectTask.EarningSite.SocpublicCom;
using CommonModels.Client;

namespace SocpublicCom.Classes.Internal.Abstract
{
    internal abstract class WithAccountAuthTask : EarningSiteSocpublicComTask
    {
        private protected OneLogAccounResult? oneLogAccount { get; private set; } = null;
        private protected ReCaptchaV2? platformCaptchaReCaptchaV2 { get; private set; } = null;
        private protected CloudflareTurnstile? platformCaptchaCloudflareTurnstile { get; private set; } = null;
        private protected string? secretPageCode { get; private set; } = null;
        private protected WithAccountAuthTask(EarningSiteWorkBotClient client, SocpublicComTask task, HubConnection serverHubConnection, HttpClient serverHttpConnection) : base(client, serverHubConnection, serverHttpConnection, task: task) { }

        protected override bool InitWebContextData()
        {
            // set or create UserAgent
            //UserAgent? userAgent = base.Task.Account!.RegedUseragent ?? CreateNewRandomUserAgent();
            UserAgent? userAgent = base.Task.Account!.RegedUseragent;
            if (userAgent == null)
            {
                throw new Exception($"class WithAccountAuthTask(), method InitWebContextData(), error userAgent == null");
            }

            // set or create Cookies
            CookieContainer cookieContainer = new CookieContainer();
            if (base.Task.Account!.LastCookies != null && base.Task.Account!.LastCookies.Count > 0)
            {
                var collection = new CookieCollection();
                base.Task.Account!.LastCookies.ForEach(cookie =>
                {
                    //remove bad cookie
                    if (cookie != null && cookie.Domain.Contains("socpublic.com"))
                    {
                        collection.Add(cookie);
                    }
                });

                cookieContainer.Add(new Uri(base.Task.Url), collection);
            }
            else
            {
                var collection = CreateNewRandomCookieCollection();

                if (collection == null)
                {
                    throw new Exception($"class WithAccountAuthTask(), method InitWebContextData(), error collection == null");
                }

                cookieContainer.Add(new Uri(base.Task.Url), collection);
            }

            // set or create Headers
            var headers = new Dictionary<string, List<string>>();
            if (base.Task.Account!.LastHeaders != null && base.Task.Account!.LastHeaders.Count > 0)
            {
                foreach (var header in base.Task.Account!.LastHeaders)
                {
                    headers.Add(header.Key, header.Value);
                }
            }
            else
            {
                var h = CreateNewRandomHeaderCollection(userAgent!);

                if (h == null)
                {
                    throw new Exception($"class WithAccountAuthTask(), method InitWebContextData(), error h == null");
                }

                headers = h;
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
                throw new Exception($"class WithAccountAuthTask(), method InitWebContextData(), error init == false");
            }

            return true;
        }
        private protected async Task<bool> CheckNeedPlatformAccountAuth(string target_url, string referer)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, target_url))
            {
                // set headers to request
                foreach (var header in contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                request.Headers.Add("referer", $"{referer}");

                var responce = await workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                if (responce.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"{responce.StatusCode}\n\n{responce.Content}");
                }

                string htmlPage = await responce.Content.ReadAsStringAsync();

                responce.Dispose();

                bool NeedToAuthorize = true;

                if(htmlPage.Contains("Вход") && htmlPage.Contains("Этот раздел доступен только для зарегистрированных пользователей") && htmlPage.Contains("войдите в аккаунт"))
                {
                    NeedToAuthorize = true;
                }
                else if (htmlPage.Contains("Выход") && htmlPage.Contains("Основной баланс") && htmlPage.Contains("Мой аккаунт"))
                {
                    NeedToAuthorize = false;
                }
                else if (htmlPage.Contains("REMOTE_ADDR") && htmlPage.Contains("REMOTE_PORT") && htmlPage.Contains("HTTP_HOST"))
                {
                    throw new Exception($"htmlPage.Contains(\"REMOTE_ADDR\") && htmlPage.Contains(\"REMOTE_PORT\") && htmlPage.Contains(\"HTTP_HOST\")\nBad Proxy, headers responce\n\n{htmlPage}");
                }
                else
                {
                    throw new Exception($"Bad Proxy, Bad NeedToAuthorize responce\n\n{htmlPage}");
                }

                if (!NeedToAuthorize)
                {
                    SetStartTimeWorkOneLogAccount(DateTime.UtcNow);
                }

                return NeedToAuthorize;
            }
        }


        private protected async Task GetCaptchaReCaptchaV2(string url)
        {
            string? htmlPage = null;
            IDocument? htmlPageDocument = null;

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                var responce = await workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                responce.EnsureSuccessStatusCode();

                htmlPage = await responce.Content.ReadAsStringAsync();
            }

            htmlPageDocument = await htmlParseContext!.OpenAsync(req => req.Content(htmlPage));
            var siteKey = htmlPageDocument.QuerySelector("div[data-sitekey]")!.GetAttribute("data-sitekey");

            // get "secret page code" -_-
            bool auth_secret_is_found = false;
            foreach(var script in htmlPageDocument.Scripts)
            {
                if (script.Text.Contains("auth_secret"))
                {
                    secretPageCode = script.Text.Split("'")[1];
                    auth_secret_is_found = true;
                    break;
                }
            }
            if (!auth_secret_is_found)
            {
                throw new Exception("'auth_secret' is not found");
            }

            platformCaptchaReCaptchaV2 = new ReCaptchaV2(url, siteKey!, base.cookieContainer!.GetCookieHeader(new Uri(base.Task.Url)), base.contextUserAgent!.Useragent!, 8);
        }
        private protected async Task SolveCaptchaReCaptchaV2()
        {
            await ServerHubConnection.SendAsync("solve_captcha_recaptchav2", platformCaptchaReCaptchaV2);
        }
        private protected async Task CheckStatusSolveCaptchaReCaptchaV2()
        {
            await ServerHubConnection.SendAsync("check_status_solve_captcha_recaptchav2", platformCaptchaReCaptchaV2);
        }


        private protected async Task GetCaptchaCloudflareTurnstile(string url)
        {
            string? htmlPage = null;
            IDocument? htmlPageDocument = null;

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                var responce = await workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                responce.EnsureSuccessStatusCode();

                htmlPage = await responce.Content.ReadAsStringAsync();
            }

            htmlPageDocument = await htmlParseContext!.OpenAsync(req => req.Content(htmlPage));
            var siteKey = htmlPageDocument.QuerySelector("div[data-sitekey]")!.GetAttribute("data-sitekey");

            // get "secret page code" -_-
            bool auth_secret_is_found = false;
            foreach (var script in htmlPageDocument.Scripts)
            {
                if (script.Text.Contains("auth_secret"))
                {
                    secretPageCode = script.Text.Split("'")[1];
                    auth_secret_is_found = true;
                    break;
                }
            }
            if (!auth_secret_is_found)
            {
                throw new Exception("'auth_secret' is not found");
            }

            platformCaptchaCloudflareTurnstile = new CloudflareTurnstile(url, siteKey!, 8);
        }
        private protected async Task SolveCaptchaCloudflareTurnstile()
        {
            await ServerHubConnection.SendAsync("solve_captcha_cloudflare_turnstile", platformCaptchaCloudflareTurnstile);
        }
        private protected async Task CheckStatusSolveCaptchaCloudflareTurnstile()
        {
            await ServerHubConnection.SendAsync("check_status_solve_captcha_cloudflare_turnstile", platformCaptchaCloudflareTurnstile);
        }


        private protected async Task AuthorizeAccount(string url)
        {
            var query = new Dictionary<string, string>()
            {
                ["act"] = "enter"
            };

            var uri = QueryHelpers.AddQueryString(url, query);

            using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                // set headers to request
                foreach (var header in contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                request.Headers.Add("referer", $"{url}");

                Dictionary<string, string> requestContent = new Dictionary<string, string>();
                requestContent.Add("name", Task.Account!.Login ?? Task.Account!.Email!.Address);
                requestContent.Add("password", Task.Account!.Password!);
                requestContent.Add("g-recaptcha-response", platformCaptchaReCaptchaV2!.resolvedCaptchaHash!);
                //requestContent.Add("cf-turnstile-response", platformCaptchaCloudflareTurnstile!.resolvedCaptchaHash!);
                requestContent.Add("secret", secretPageCode!);

                request.Content = new FormUrlEncodedContent(requestContent);

                var responce = await workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                var content = await responce.Content.ReadAsStringAsync();

                if (responce.StatusCode == HttpStatusCode.OK && (content.Contains("заблокирован") || content.Contains("Заблокирован") || content.Contains("Забанен") || content.Contains("забанен") || content.Contains("Заблок") || content.Contains("заблок")))
                {
                    base.Task.Account.IsAlive = false;
                    throw new Exception($"Не удалось войти в аккаунт!\nАККАУНТ ЗАБЛОКИРВОАН\nStatusCode={responce.StatusCode}\n\n{content}");
                }

                if (responce.StatusCode == HttpStatusCode.OK && !content.Contains("Основной баланс"))
                {
                    throw new Exception($"Не удалось войти в аккаунт!\nresponce.StatusCode == HttpStatusCode.OK && !content.Contains(\"Основной баланс\")\nStatusCode={responce.StatusCode}\n\n{content}");
                }

                if (responce.StatusCode != HttpStatusCode.OK && responce.StatusCode != HttpStatusCode.Found && responce.StatusCode != HttpStatusCode.Redirect && responce.Headers.Location == null && !responce.Headers.Location!.ToString().Contains("account"))
                {
                    throw new Exception($"Не удалось войти в аккаунт!\n responce.StatusCode != HttpStatusCode.OK && responce.StatusCode != HttpStatusCode.Found && responce.StatusCode != HttpStatusCode.Redirect && responce.Headers.Location == null && !responce.Headers.Location!.ToString().Contains(\"account\")\n {responce.StatusCode}\n\n{content}");
                }

                SetStartTimeWorkOneLogAccount(DateTime.UtcNow);
            }
        }

        private protected void AddOperationToLogAccount(Operation operation)
        {
            if(!createOneLogAccount())
            {
                throw new Exception("createOneLogAccount() == false");
            }
             
            oneLogAccount!.Operations!.Add(operation);
            oneLogAccount.MoneyMainBalancePlus = operation.MoneyMainBalancePlus;
        }
        private protected void AddCareerLadderData(Status status, double statusPoints, int statusProgress, int careerLevel)
        {
            if (!createOneLogAccount())
            {
                throw new Exception("createOneLogAccount() == false");
            }

            if(base.Task!.Account!.CareerLadder == null)
            {
                base.Task.Account!.CareerLadder = new CareerLadder()
                {
                    Status = status,
                    StatusPoints = statusPoints,
                    StatusProgress = statusProgress,
                    CareerLevel = careerLevel
                };
            }
            else
            {
                base.Task.Account!.CareerLadder.Status = status;
                base.Task.Account!.CareerLadder.StatusPoints = statusPoints;
                base.Task.Account!.CareerLadder.StatusProgress = statusProgress;
                base.Task.Account!.CareerLadder.CareerLevel = careerLevel;
            }

            //oneLogAccount!.CareerLadder = new CareerLadder()
            //{
            //    Status = status,
            //    StatusPoints = statusPoints,
            //    StatusProgress = statusProgress,
            //    CareerLevel = careerLevel
            //};

            //oneLogAccount.CareerLadderIsChanged = false;

            //if(base.Task.Account!.CareerLadder != null)
            //{
            //    if (oneLogAccount.CareerLadder.Status != base.Task.Account!.CareerLadder!.Status)
            //        oneLogAccount.CareerLadderIsChanged = true;
            //    else if (oneLogAccount.CareerLadder!.StatusPoints != base.Task.Account!.CareerLadder!.StatusPoints)
            //        oneLogAccount.CareerLadderIsChanged = true;
            //    else if (oneLogAccount.CareerLadder!.StatusProgress != base.Task.Account!.CareerLadder!.StatusProgress)
            //        oneLogAccount.CareerLadderIsChanged = true;
            //    else if (oneLogAccount.CareerLadder!.CareerLevel != base.Task.Account!.CareerLadder!.CareerLevel)
            //        oneLogAccount.CareerLadderIsChanged = true;
            //}
        }
        private protected void SetStartTimeWorkOneLogAccount(DateTime dateTime)
        {
            if (!createOneLogAccount())
            {
                throw new Exception("createOneLogAccount() == false");
            }

            oneLogAccount!.DateTimeStart = dateTime;
        }
        private protected void SetEndTimeWorkOneLogAccount(DateTime dateTime)
        {
            if (!createOneLogAccount())
            {
                throw new Exception("createOneLogAccount() == false");
            }

            oneLogAccount!.DateTimeEnd = dateTime;

            oneLogAccount.SetTimeOfWorkInSeconds();
        }
        private bool createOneLogAccount()
        {
            if (oneLogAccount == null)
            {
                try
                {
                    oneLogAccount = new OneLogAccounResult(
                    proxy: base.proxy,
                    useragent: base.contextUserAgent,
                    headers: base.contextHeaders,
                    cookies: base.cookieContainer!.GetAllCookies().ToList(),
                    dateTimeStart: DateTime.UtcNow,
                    loginCaptchaReCaptchaV2: platformCaptchaReCaptchaV2,
                    loginCaptchaCloudflareTurnstile: platformCaptchaCloudflareTurnstile,
                    operations: new List<Operation>()
                    );

                    base.Task.Account!.History!.Add(oneLogAccount);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
    }
}
