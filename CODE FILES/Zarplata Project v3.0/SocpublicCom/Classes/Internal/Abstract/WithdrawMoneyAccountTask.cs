using AngleSharp.Dom;
using AngleSharp;
using CommonModels.Captcha;
using CommonModels.Client;
using CommonModels.ProjectTask.EarningSite.SocpublicCom;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels;
using CommonModels.UserAgentClasses;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.WebUtilities;
using SocpublicCom.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using AngleSharp.Text;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.SelectiveTaskWithAuth;
using CommonModels.ProjectTask.Platform.SocpublicCom;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.Models;
using CommonModels.ProjectTask.EarningSite.SocpublicCom.TaskType.GSelectiveTaskWithAuth.SelectiveTask.WithdrawMoney;
using Newtonsoft.Json;
using CommonModels.ProjectTask.EarningSite.SocpublicCom.Models.WithdrawalOfMoneyModels;
using CommonModels.ProjectTask.EarningSite.SocpublicCom.Models.ConfirmWithdrawalOfMoneyModels;
using System.Globalization;

namespace SocpublicCom.Classes.Internal.Abstract
{
    internal class WithdrawMoneyAccountTask : EarningSiteSocpublicComTask, IProxySetable, IDisposable
    {
        private bool disposedValue;

        private protected OneLogAccounResult? oneLogAccount { get; private set; } = null;
        private protected ReCaptchaV2? platformCaptchaReCaptchaV2 { get; private set; } = null;
        private protected CloudflareTurnstile? platformCaptchaCloudflareTurnstile { get; private set; } = null;
        private protected string? secretPageCode { get; private set; } = null;
        private double MoneyEarnedByMain = 0;

        public WithdrawMoneyAccountTask(EarningSiteWorkBotClient client, WithdrawMoneyGroupSelectiveTaskWithAuth task, HubConnection serverHubConnection, HttpClient serverHttpConnection) : base(client, serverHubConnection, serverHttpConnection, taskWithdrawMoney: task) { }


        #region собранное из класса WithAccountAuthTask, вместо наследования!
        protected override bool InitWebContextData()
        {
            // set or create UserAgent
            //UserAgent? userAgent = base.TaskWithdrawMoney.Account!.RegedUseragent ?? CreateNewRandomUserAgent();
            UserAgent? userAgent = base.TaskWithdrawMoney.Account!.RegedUseragent;
            if (userAgent == null)
            {
                throw new Exception($"class WithAccountAuthTask(), method InitWebContextData(), error userAgent == null");
            }

            // set or create Cookies
            CookieContainer cookieContainer = new CookieContainer();
            if (base.TaskWithdrawMoney.Account!.LastCookies != null && base.TaskWithdrawMoney.Account!.LastCookies.Count > 0)
            {
                var collection = new CookieCollection();
                base.TaskWithdrawMoney.Account!.LastCookies.ForEach(cookie =>
                {
                    //remove bad cookie
                    if (cookie != null && cookie.Domain.Contains("socpublic.com"))
                    {
                        collection.Add(cookie);
                    }
                });

                cookieContainer.Add(new Uri(base.TaskWithdrawMoney.Url), collection);
            }
            else
            {
                var collection = CreateNewRandomCookieCollection();

                if (collection == null)
                {
                    throw new Exception($"class WithAccountAuthTask(), method InitWebContextData(), error collection == null");
                }

                cookieContainer.Add(new Uri(base.TaskWithdrawMoney.Url), collection);
            }

            // set or create Headers
            var headers = new Dictionary<string, List<string>>();
            if (base.TaskWithdrawMoney.Account!.LastHeaders != null && base.TaskWithdrawMoney.Account!.LastHeaders.Count > 0)
            {
                foreach (var header in base.TaskWithdrawMoney.Account!.LastHeaders)
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
            //if (base.TaskWithdrawMoney.Account.IsMain.HasValue && base.TaskWithdrawMoney.Account.IsMain.Value)
            //{
            //    try
            //    {
            //        headers.Remove("Connection");
            //    }
            //    catch { }

            //    try
            //    {
            //        headers.Remove("connection");
            //    }
            //    catch { }

            //    headers.Add("connection", new List<string>() { "keep-alive" });
            //}

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

                if (htmlPage.Contains("Вход") && htmlPage.Contains("Этот раздел доступен только для зарегистрированных пользователей") && htmlPage.Contains("войдите в аккаунт"))
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

            platformCaptchaReCaptchaV2 = new ReCaptchaV2(url, siteKey!, base.cookieContainer!.GetCookieHeader(new Uri(base.TaskWithdrawMoney.Url)), base.contextUserAgent!.Useragent!, 8);
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
            CancellationTokenSource loginCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(60));

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
                requestContent.Add("name", TaskWithdrawMoney!.Account!.Login ?? TaskWithdrawMoney.Account!.Email!.Address);
                requestContent.Add("password", TaskWithdrawMoney.Account!.Password!);
                requestContent.Add("g-recaptcha-response", platformCaptchaReCaptchaV2!.resolvedCaptchaHash!);
                //requestContent.Add("cf-turnstile-response", platformCaptchaCloudflareTurnstile!.resolvedCaptchaHash!);
                requestContent.Add("secret", secretPageCode!);

                request.Content = new FormUrlEncodedContent(requestContent);

                var responce = await workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, loginCancellationToken.Token);

                var content = await responce.Content.ReadAsStringAsync();

                if (responce.StatusCode == HttpStatusCode.OK && (content.Contains("заблокирован") || content.Contains("Заблокирован") || content.Contains("Забанен") || content.Contains("забанен") || content.Contains("Заблок") || content.Contains("заблок")))
                {
                    TaskWithdrawMoney.Account.IsAlive = false;
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
            if (!createOneLogAccount())
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

            oneLogAccount!.CareerLadder = new CareerLadder()
            {
                Status = status,
                StatusPoints = statusPoints,
                StatusProgress = statusProgress,
                CareerLevel = careerLevel
            };

            oneLogAccount.CareerLadderIsChanged = false;

            if (base.TaskWithdrawMoney.Account!.CareerLadder != null)
            {
                if (oneLogAccount.CareerLadder.Status != base.TaskWithdrawMoney.Account!.CareerLadder!.Status)
                    oneLogAccount.CareerLadderIsChanged = true;
                else if (oneLogAccount.CareerLadder!.StatusPoints != base.TaskWithdrawMoney.Account!.CareerLadder!.StatusPoints)
                    oneLogAccount.CareerLadderIsChanged = true;
                else if (oneLogAccount.CareerLadder!.StatusProgress != base.TaskWithdrawMoney.Account!.CareerLadder!.StatusProgress)
                    oneLogAccount.CareerLadderIsChanged = true;
                else if (oneLogAccount.CareerLadder!.CareerLevel != base.TaskWithdrawMoney.Account!.CareerLadder!.CareerLevel)
                    oneLogAccount.CareerLadderIsChanged = true;
            }
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
                    //oneLogAccount = new OneLogAccounResult(
                    //proxy: base.proxy,
                    //useragent: base.contextUserAgent,
                    //headers: base.contextHeaders,
                    //cookies: base.cookieContainer!.GetAllCookies().ToList(),
                    //dateTimeStart: DateTime.UtcNow,
                    //loginCaptcha: platformCaptchaReCaptchaV2,
                    //operations: new List<Operation>()
                    //);

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

                    base.TaskWithdrawMoney.Account!.History!.Add(oneLogAccount);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
        #endregion


        internal override async Task StartWork()
        {
            // clear old history onelogaccount
            try
            {
                if (base.TaskWithdrawMoney!.Account!.History!.Count() > 5)
                {
                    base.TaskWithdrawMoney!.Account!.History!.RemoveRange(0, (base.TaskWithdrawMoney!.Account!.History!.Count() - 5));
                }

                base.TaskWithdrawMoney!.Account!.History!.RemoveAll(onelog_acc => onelog_acc == null || onelog_acc.Proxy == null);
            }
            catch (Exception e)
            {
                throw new Exception($"class WithdrawMoneyAccountTask(), method StartWork(), error {e.Message}");
            }

            try
            {
                RegisterHubMethods();
            }
            catch (Exception e)
            {
                throw new Exception($"class WithdrawMoneyAccountTask(), method StartWork(), error {e.Message}");
            }

            try
            {
                FirstStep_InitWebContextData();
            }
            catch (Exception e)
            {
                throw new Exception($"class WithdrawMoneyAccountTask(), method StartWork(), error {e.Message}");
            }

            try
            {
                if (base.TaskWithdrawMoney!.Account!.IsMain.HasValue && base.TaskWithdrawMoney!.Account.IsMain.Value && base.TaskWithdrawMoney!.Account!.PersonalPrivateProxy != null)
                {
                    try
                    {
                        ThirdStep_InitHttpClientDataWithPersonalProxy(base.TaskWithdrawMoney.Account.PersonalPrivateProxy);

                        await ChooseBetweenSteps_FiveStep_or_SevenStep();
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            await TaskChangeResultStatus_SendServerReport(
                                TaskResultStatus.Error,
                                new Exception($"class WithdrawMoneyAccountTask(), method StartWork(), error {e.Message}")
                                );
                        }
                        catch { }
                    }
                }
                else
                {
                    await SecondStep_CheckPlatformAccountHistoryProxy_Or_GetNewProxy();
                }
            }
            catch (Exception e)
            {
                throw new Exception($"class WithdrawMoneyAccountTask(), method StartWork(), error {e.Message}");
            }
        }

        #region подготовка, получение прокси для аккаунта, авторизация и прочее
        private void RegisterHubMethods()
        {
            base.ServerHubConnection.On<List<CheckedProxy>>("checked_platform_account_history_proxy", async (proxies) =>
            {
                CheckedProxy? bestWorkProxy = null;

                try
                {
                    bestWorkProxy = IProxySetable.CheckCheckedProxy(proxies, base.TaskWithdrawMoney!.Account!.History!, base.TaskWithdrawMoney!.Account);
                }
                catch (Exception e)
                {
                    try
                    {
                        await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class WithdrawMoneyAccountTask(), method checked_platform_account_history_proxy(), error {e.Message}")
                            );
                    }
                    catch { }
                }

                if (bestWorkProxy == null)
                {
                    try
                    {
                        await ServerHubConnection.SendAsync("set_task_status_to_waiting_for_new_proxy", base.TaskWithdrawMoney!.Id);

                        await IProxySetable.GetNewProxy(base.Client, base.TaskWithdrawMoney.Account!, base.ServerHubConnection);
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class WithdrawMoneyAccountTask(), method checked_platform_account_history_proxy(), error {e.Message}")
                            );
                        }
                        catch { }
                    }
                }
                else
                {
                    try
                    {
                        ThirdStep_InitHttpClientDataWithProxy(bestWorkProxy);

                        await ChooseBetweenSteps_FiveStep_or_SevenStep();
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class WithdrawMoneyAccountTask(), method checked_platform_account_history_proxy(), error {e.Message}")
                            );
                        }
                        catch { }
                    }
                }
            });

            base.ServerHubConnection.On<CheckedProxy?>("new_platform_account_proxy", async (proxy) =>
            {
                try
                {
                    if (proxy != null)
                    {
                        ThirdStep_InitHttpClientDataWithProxy(proxy);

                        await ChooseBetweenSteps_FiveStep_or_SevenStep();
                    }
                    else
                    {
                        await IProxySetable.GetNewProxy(base.Client, base.TaskWithdrawMoney.Account!, base.ServerHubConnection);
                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class WithdrawMoneyAccountTask(), method new_platform_account_proxy(), error {e.Message}")
                            );
                    }
                    catch { }
                }
            });

            base.ServerHubConnection.On<ReCaptchaV2>("changed_status_solve_captcha_recaptchav2", async (captcha) =>
            {
                if (captcha.status == CaptchaStatus.Created)
                {
                    try
                    {
                        platformCaptchaReCaptchaV2!.status = CaptchaStatus.Created;
                        platformCaptchaReCaptchaV2!.id = captcha.id;

                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(platformCaptchaReCaptchaV2!.waitTime!.Value));

                        await CheckStatusSolveCaptchaReCaptchaV2();
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class WithdrawMoneyAccountTask(), method changed_status_solve_captcha_recaptchav2(), error {e.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}\n\nLocal captcha object info:\n{JsonConvert.SerializeObject(platformCaptchaReCaptchaV2)}")
                            );
                        }
                        catch { }
                    }
                }

                if (captcha.status == CaptchaStatus.inProgress)
                {
                    try
                    {
                        platformCaptchaReCaptchaV2!.status = CaptchaStatus.inProgress;

                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(platformCaptchaReCaptchaV2!.waitTime!.Value));

                        await CheckStatusSolveCaptchaReCaptchaV2();

                        platformCaptchaReCaptchaV2.waitTime += 1;
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class WithdrawMoneyAccountTask(), method changed_status_solve_captcha_recaptchav2(), error {e.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
                            );
                        }
                        catch { }
                    }
                }

                if (captcha.status == CaptchaStatus.Solved)
                {
                    try
                    {
                        await ServerHubConnection.SendAsync("set_task_status_to_executing", base.TaskWithdrawMoney!.Id);
                    }
                    catch { }

                    platformCaptchaReCaptchaV2!.status = CaptchaStatus.Solved;
                    platformCaptchaReCaptchaV2!.resolvedCaptchaHash = captcha.resolvedCaptchaHash;

                    await SixStep_LoginIntoAccount().ContinueWith(async login =>
                    {
                        if (login.IsCompletedSuccessfully)
                        {
                            try
                            {
                                await SevenStep_SolvePlatformTasks_Boot();
                            }
                            catch (Exception e)
                            {
                                try
                                {
                                    await TaskChangeResultStatus_SendServerReport(
                                    TaskResultStatus.Error,
                                    new Exception($"class WithdrawMoneyAccountTask(), method changed_status_solve_captcha_recaptchav2(), error {e.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
                                    );
                                }
                                catch { }
                            }
                        }
                        else
                        {
                            try
                            {
                                await TaskChangeResultStatus_SendServerReport(
                                TaskResultStatus.Error,
                                new Exception($"class WithdrawMoneyAccountTask(), method changed_status_solve_captcha_recaptchav2(), error {login.Exception?.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
                                );
                            }
                            catch { }
                        }
                    });
                }

                if (captcha.status == CaptchaStatus.Error)
                {
                    platformCaptchaReCaptchaV2!.status = CaptchaStatus.Error;

                    try
                    {
                        await TaskChangeResultStatus_SendServerReport(
                        TaskResultStatus.Error,
                        new Exception($"class WithdrawMoneyAccountTask(), method changed_status_solve_captcha_recaptchav2(), error captcha.status == CaptchaStatus.Error\n\n\nCaptcha Info:\n{JsonConvert.SerializeObject(captcha)}")
                        );
                    }
                    catch { }
                }
            });

            base.ServerHubConnection.On<CloudflareTurnstile>("changed_status_solve_captcha_cloudflare_turnstile", async (captcha) =>
            {
                if (captcha.status == CaptchaStatus.Created)
                {
                    try
                    {
                        platformCaptchaCloudflareTurnstile!.status = CaptchaStatus.Created;
                        platformCaptchaCloudflareTurnstile!.id = captcha.id;

                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(platformCaptchaCloudflareTurnstile!.waitTime!.Value));

                        await CheckStatusSolveCaptchaCloudflareTurnstile();
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class WithdrawMoneyAccountTask(), method changed_status_solve_captcha_recaptchav2(), error {e.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}\n\nLocal captcha object info:\n{JsonConvert.SerializeObject(platformCaptchaCloudflareTurnstile)}")
                            );
                        }
                        catch { }
                    }
                }

                if (captcha.status == CaptchaStatus.inProgress)
                {
                    try
                    {
                        platformCaptchaCloudflareTurnstile!.status = CaptchaStatus.inProgress;

                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(platformCaptchaCloudflareTurnstile!.waitTime!.Value));

                        await CheckStatusSolveCaptchaCloudflareTurnstile();

                        platformCaptchaCloudflareTurnstile.waitTime += 1;
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class WithdrawMoneyAccountTask(), method changed_status_solve_captcha_recaptchav2(), error {e.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
                            );
                        }
                        catch { }
                    }
                }

                if (captcha.status == CaptchaStatus.Solved)
                {
                    try
                    {
                        await ServerHubConnection.SendAsync("set_task_status_to_executing", base.TaskWithdrawMoney!.Id);
                    }
                    catch { }

                    platformCaptchaCloudflareTurnstile!.status = CaptchaStatus.Solved;
                    platformCaptchaCloudflareTurnstile!.resolvedCaptchaHash = captcha.resolvedCaptchaHash;

                    await SixStep_LoginIntoAccount().ContinueWith(async login =>
                    {
                        if (login.IsCompletedSuccessfully)
                        {
                            try
                            {
                                await SevenStep_SolvePlatformTasks_Boot();
                            }
                            catch (Exception e)
                            {
                                try
                                {
                                    await TaskChangeResultStatus_SendServerReport(
                                    TaskResultStatus.Error,
                                    new Exception($"class WithdrawMoneyAccountTask(), method changed_status_solve_captcha_recaptchav2(), error {e.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
                                    );
                                }
                                catch { }
                            }
                        }
                        else
                        {
                            try
                            {
                                await TaskChangeResultStatus_SendServerReport(
                                TaskResultStatus.Error,
                                new Exception($"class WithdrawMoneyAccountTask(), method changed_status_solve_captcha_recaptchav2(), error {login.Exception?.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
                                );
                            }
                            catch { }
                        }
                    });
                }

                if (captcha.status == CaptchaStatus.Error)
                {
                    platformCaptchaReCaptchaV2!.status = CaptchaStatus.Error;

                    try
                    {
                        await TaskChangeResultStatus_SendServerReport(
                        TaskResultStatus.Error,
                        new Exception($"class WithdrawMoneyAccountTask(), method changed_status_solve_captcha_recaptchav2(), error captcha.status == CaptchaStatus.Error\n\n\nCaptcha Info:\n{JsonConvert.SerializeObject(captcha)}")
                        );
                    }
                    catch { }
                }
            });
        }
        private void FirstStep_InitWebContextData()
        {
            // init this data
            var init = InitWebContextData();
            if (init == false)
            {
                throw new Exception($"class WithdrawMoneyAccountTask(), method FirstStep_InitWebContextData(), error init == false");
            }
        }
        private async Task SecondStep_CheckPlatformAccountHistoryProxy_Or_GetNewProxy()
        {
            bool NeedToGetNewProxy = true;

            try
            {
                NeedToGetNewProxy = await IProxySetable.CheckPlatformAccountHistoryProxy(base.Client, base.TaskWithdrawMoney.Account!, base.ServerHubConnection);
            }
            catch (Exception e)
            {
                throw new Exception($"class WithdrawMoneyAccountTask(), method SecondStep_CheckPlatformAccountHistoryProxy_Or_GetNewProxy(), error {e.Message}");
            }

            // get new proxy
            if (NeedToGetNewProxy)
            {
                try
                {
                    await ServerHubConnection.SendAsync("set_task_status_to_waiting_for_new_proxy", base.TaskWithdrawMoney!.Id);

                    await IProxySetable.GetNewProxy(base.Client, base.TaskWithdrawMoney.Account!, base.ServerHubConnection);
                }
                catch (Exception e)
                {
                    throw new Exception($"class WithdrawMoneyAccountTask(), method SecondStep_CheckPlatformAccountHistoryProxy_Or_GetNewProxy(), error {e.Message}");
                }
            }
            else
            {
                await ServerHubConnection.SendAsync("set_task_status_to_waiting_for_check_account_history_proxy", base.TaskWithdrawMoney!.Id);
            }
        }
        private void ThirdStep_InitHttpClientDataWithProxy(CheckedProxy proxy)
        {
            try
            {
                base.proxy = proxy;

                WebProxy webProxy = new WebProxy
                {
                    Address = proxy.FullProxyAddressUri,
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = true
                };

                var initRes = base.SetWebContextData(webP: webProxy);

                if (!initRes)
                {
                    throw new Exception("initRes == false");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"class WithdrawMoneyAccountTask(), method ThirdStep_InitHttpClientDataWithProxy(), error {e.Message}");
            }

            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler
                {
                    UseProxy = true,
                    Proxy = base.webProxy,
                    UseCookies = true,
                    CookieContainer = base.cookieContainer!,
                    AutomaticDecompression = DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | DecompressionMethods.Brotli,
                    //ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                HttpClient httpClient = new HttpClient(clientHandler, true);

                var initRes = base.SetWebContextData(httpConn: httpClient);

                if (!initRes)
                {
                    throw new Exception("initRes == false");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"class WithdrawMoneyAccountTask(), method ThirdStep_InitHttpClientDataWithProxy(), error {e.Message}");
            }
        }
        private void ThirdStep_InitHttpClientDataWithPersonalProxy(EnvironmentProxy proxy)
        {
            try
            {
                WebProxy webProxy = new WebProxy
                {
                    Address = proxy.FullProxyAddressUri,
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(proxy.Username, proxy.Password)
                };

                var initRes = base.SetWebContextData(webP: webProxy);

                if (!initRes)
                {
                    throw new Exception("initRes == false");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"class WithdrawMoneyAccountTask(), method ThirdStep_InitHttpClientDataWithPersonalProxy(), error {e.Message}");
            }

            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler
                {
                    UseProxy = true,
                    Proxy = base.webProxy,
                    UseCookies = true,
                    CookieContainer = base.cookieContainer!,
                    AutomaticDecompression = DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | DecompressionMethods.Brotli,
                    SslProtocols = System.Security.Authentication.SslProtocols.None
                };

                HttpClient httpClient = new HttpClient(clientHandler, true);
                httpClient.Timeout = TimeSpan.FromSeconds(90);
                httpClient.DefaultRequestHeaders.ConnectionClose = true;

                var initRes = base.SetWebContextData(httpConn: httpClient);

                if (!initRes)
                {
                    throw new Exception("initRes == false");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"class WithdrawMoneyAccountTask(), method ThirdStep_InitHttpClientDataWithPersonalProxy(), error {e.Message}");
            }
        }
        private async Task ChooseBetweenSteps_FiveStep_or_SevenStep()
        {
            bool needAuth = false;
            try
            {
                needAuth = await FourthStep_CheckNeedPlatformAccountAuth();
            }
            catch (Exception e)
            {
                throw new Exception($"class WithdrawMoneyAccountTask(), method ChooseBetweenSteps_FiveStep_or_SevenStep(), error {e.Message}");
            }

            if (needAuth)
            {
                try
                {
                    await ServerHubConnection.SendAsync("set_task_status_to_waiting_for_captcha", base.TaskWithdrawMoney!.Id);

                    await FiveStep_GetAndSolvePlatformCaptcha();
                }
                catch (Exception e)
                {
                    throw new Exception($"class WithdrawMoneyAccountTask(), method ChooseBetweenSteps_FiveStep_or_SevenStep(), error {e.Message}");
                }
            }
            else
            {
                try
                {
                    await ServerHubConnection.SendAsync("set_task_status_to_executing", base.TaskWithdrawMoney!.Id);

                    await SevenStep_SolvePlatformTasks_Boot();
                }
                catch (Exception e)
                {
                    throw new Exception($"class WithdrawMoneyAccountTask(), method ChooseBetweenSteps_FiveStep_or_SevenStep(), error {e.Message}");
                }
            }
        }
        private async Task<bool> FourthStep_CheckNeedPlatformAccountAuth()
        {
            bool needAuth = false;

            try
            {
                needAuth = await CheckNeedPlatformAccountAuth($"{base.TaskWithdrawMoney.Url}/account/", base.TaskWithdrawMoney.Url);
            }
            catch (Exception e)
            {
                throw new Exception($"class WithdrawMoneyAccountTask(), method FourthStep_CheckNeedPlatformAccountAuth(), error {e.Message}");
            }

            return needAuth;
        }
        private async Task FiveStep_GetAndSolvePlatformCaptcha()
        {
            // get captcha
            try
            {
                await GetCaptchaReCaptchaV2($"{base.TaskWithdrawMoney.Url}/auth_login.html");
                //await GetCaptchaCloudflareTurnstile($"{base.TaskWithdrawMoney.Url}/auth_login.html");
            }
            catch (Exception e)
            {
                throw new Exception($"class WithdrawMoneyAccountTask(), method FiveStep_GetAndSolvePlatformCaptcha(), error {e.Message}");
            }

            // solve captcha
            try
            {
                await SolveCaptchaReCaptchaV2();
                //await SolveCaptchaCloudflareTurnstile();
            }
            catch (Exception e)
            {
                throw new Exception($"class WithdrawMoneyAccountTask(), method FiveStep_GetAndSolvePlatformCaptcha(), error {e.Message}");
            }
        }
        private async Task SixStep_LoginIntoAccount()
        {
            try
            {
                await AuthorizeAccount($"{base.TaskWithdrawMoney.Url}/auth_login.html");
            }
            catch (Exception e)
            {
                throw new Exception($"class WithdrawMoneyAccountTask(), method SixStep_LoginIntoAccount(), error {e.Message}");
            }
        }
        private async Task SevenStep_SolvePlatformTasks_Boot()
        {
            //  НАЧАЛО ВЫПОЛНЕНИЯ ЗАДАНИЙ НА ПЛАТФОРМЕ

            bool success = true;
            Exception? ex = null;

            try
            {
                foreach (var task_subtype in (base.TaskWithdrawMoney as WithdrawMoneyGroupSelectiveTaskWithAuth)!.InternalSubtype!)
                {
                    if (task_subtype == SocpublicComTaskEnums.SocpublicComTaskSubtype.WithdrawalOfMoney)
                    {
                        // Withdrawal Of Money
                        if (base.TaskWithdrawMoney!.Account!.IsMain.HasValue && base.TaskWithdrawMoney!.Account.IsMain.Value)
                        {
                            // work with Account - Operation Start
                            var operation = new Operation()
                            {
                                DateTimeStart = DateTime.UtcNow,
                                Type = OperationType.withdrawalOfMoneyByMain,
                                Action = "Вывод денег мейном"
                            };

                            try
                            {
                                // тут основной код
                                await SelectiveTaskWithdrawalOfMoneyByMain();

                                operation.Status = OperationStatus.Success;

                                operation.StatusMessage = "Задание успешно запущено на платформе слейвом.";
                            }
                            catch (Exception e)
                            {
                                operation.Status = OperationStatus.Failure;
                                operation.StatusMessage = e.Message;
                                throw;
                            }
                            finally
                            {
                                operation.DateTimeEnd = DateTime.UtcNow;
                                operation.SetTimeOfWorkInSeconds();

                                AddOperationToLogAccount(operation);

                                //if(operation.Status == OperationStatus.Failure)
                                //{
                                //    throw new Exception(operation.StatusMessage);
                                //}
                            }
                        }
                        else if (base.TaskWithdrawMoney!.Account.IsMain.HasValue && !base.TaskWithdrawMoney!.Account.IsMain.Value)
                        {
                            // work with Account - Operation Start
                            var operation = new Operation()
                            {
                                DateTimeStart = DateTime.UtcNow,
                                Type = OperationType.withdrawalOfMoneyBySlave,
                                Action = "Вывод денег слейвом"
                            };

                            try
                            {
                                // тут основной код
                                await SelectiveTaskWithdrawalOfMoneyBySlave();

                                operation.Status = OperationStatus.Success;

                                operation.StatusMessage = "Задания успешно выполнены на платформе мейном.";
                            }
                            catch (Exception e)
                            {
                                operation.Status = OperationStatus.Failure;
                                operation.StatusMessage = e.Message;
                                throw;
                            }
                            finally
                            {
                                operation.DateTimeEnd = DateTime.UtcNow;
                                operation.SetTimeOfWorkInSeconds();

                                AddOperationToLogAccount(operation);

                                //if (operation.Status == OperationStatus.Failure)
                                //{
                                //    throw new Exception(operation.StatusMessage);
                                //}
                            }
                        }
                        else
                        {
                            throw new Exception("base.TaskWithdrawMoney!.Account.IsMain.HasValue == false");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                success = false;
                ex = e;
            }
            finally
            {
                try
                {
                    if (success)
                    {
                        await TaskChangeResultStatus_SendServerReport(TaskResultStatus.Success);
                    }
                    else
                    {
                        await TaskChangeResultStatus_SendServerReport(TaskResultStatus.Error, ex);
                    }
                }
                catch { }
            }
        }
        #endregion

        #region Selective Tasks

        private async Task SelectiveTaskWithdrawalOfMoneyBySlave()
        {
            Random randomDelay = new Random();
            Dictionary<string, string>? queryParams;
            string? uri = "";
            HttpResponseMessage? responce = null;
            string content = "";
            IDocument? htmlPageDocument = null;
            IElement? elementPlatformTask = null;
            List<IElement>? balances = null;
            double? main_balance = null;
            double? advert_balance = null;
            PlatformInternalAccountTaskModel? InternalSlavePlatformTask = null;
            string last_content = "";

            #region 0-й шаг: получение из бд задания привязанного к этому слейву

            InternalSlavePlatformTask = await ServerHubConnection.InvokeAsync<PlatformInternalAccountTaskModel?>("get_slave_internal_account_task", base.TaskWithdrawMoney!.Account!.Login);

            if (InternalSlavePlatformTask == null)
            {
                throw new Exception("InternalSlavePlatformTask == null --- 0-й шаг: получение из бд задания привязанного к этому слейву");
            }

            #endregion

            #region 1-й шаг: получение точного главного и рекламного балансов (переход на "Вывести средства" tab)

            if (!base.TaskWithdrawMoney!.NeedToRemovePlatformTaskForSlave.HasValue && !base.TaskWithdrawMoney.NeedToEditPlatformTaskForSlave.HasValue)
            {
                try
                {
                    uri = $"{base.TaskWithdrawMoney!.Url}/account/payout.html";

                    using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                    {
                        // set headers to request
                        foreach (var header in base.contextHeaders!)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }

                        if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                        if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                        request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/index.html");

                        // request
                        responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                        if (responce.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception($"responce.StatusCode != HttpStatusCode.OK --- 1-й шаг: получение главного и рекламного балансов (переход на \"Вывести средства\" tab)\n\n\n{content}");
                        }

                        content = await responce.Content.ReadAsStringAsync();
                    }

                    htmlPageDocument = await base.htmlParseContext!.OpenAsync(req => req.Content(content));

                    balances = htmlPageDocument.QuerySelectorAll("div.collapse.navbar-collapse > ul.page-sidebar-menu > li.balance-box > div.inner > div").ToList();

                    if (balances.Count != 2)
                    {
                        throw new Exception($"Ошибка при получении 'var balances = htmlPageDocument.QuerySelectorAll(\"div.collapse.navbar-collapse > ul.page-sidebar-menu > li.balance-box > div.inner > div\").ToList();' --- 1-й шаг: получение главного и рекламного балансов (переход на \"Вывести средства\" tab)\n\n\n{content}");
                    }

                    // main_balance будм получать ниже, более точное значение
                    //main_balance = balances[0]!.QuerySelector("span.balance_rub")!.TextContent.ToDouble();
                    advert_balance = balances[1]!.QuerySelector("span.balance_rub_advert")!.TextContent.ToDouble();

                    var div_with_parts = htmlPageDocument.QuerySelector("form[action=\"/account/payout.html?act=payout\"]")?.QuerySelectorAll("div.form-group")[2]?.QuerySelector("div > div"); ;

                    if (div_with_parts == null)
                    {
                        throw new Exception($"div_with_parts == null --- 1-й шаг: получение главного и рекламного балансов (переход на \"Вывести средства\" tab)\n\n\n{content}");
                    }

                    // get exact main_balance value
                    string? first_main_balance_value_part = div_with_parts.QuerySelector("span.font-size-16.color-grey")!.TextContent;
                    string? second_main_balance_value_part = div_with_parts.QuerySelector("span.font-size-16.color-mid-grey")!.TextContent;

                    main_balance = $"{first_main_balance_value_part}{second_main_balance_value_part}".ToDouble();

                    if (main_balance == null)
                    {
                        throw new Exception($"main_balance == null --- 1-й шаг: получение главного и рекламного балансов (переход на \"Вывести средства\" tab)\n\n\n{content}");
                    }

                    if (advert_balance == null)
                    {
                        throw new Exception($"advert_balance == null --- 1-й шаг: получение главного и рекламного балансов (переход на \"Вывести средства\" tab)\n\n\n{content}");
                    }

                    base.TaskWithdrawMoney.Account!.MoneyMainBalance = main_balance;
                    base.TaskWithdrawMoney.Account.MoneyAdvertisingBalance = advert_balance;

                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(randomDelay.Next(5, 31)));
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message + "\n\n\nНеизвестная ошибка: 1-й шаг: получение главного и рекламного балансов (переход на \"Вывести средства\" tab)");
                } 
            }

            #endregion

            #region 2-й шаг (по надобности): перевод денег с главного на рекламный баланс

            if (main_balance >= 1.0000 && !base.TaskWithdrawMoney!.NeedToRemovePlatformTaskForSlave.HasValue && !base.TaskWithdrawMoney.NeedToEditPlatformTaskForSlave.HasValue)
            {
                try
                {
                    bool need_to_check_transfer = true;

                    #region 2.1 перевод денег на рекламный баланс

                    queryParams = new Dictionary<string, string>()
                    {
                        ["act"] = $"payin"
                    };

                    uri = QueryHelpers.AddQueryString($"{base.TaskWithdrawMoney!.Url}/account/payin.html", queryParams);

                    using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                    {
                        // set headers to request
                        foreach (var header in base.contextHeaders!)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }

                        if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                        if (request.Headers.Contains("origin")) request.Headers.Remove("origin");
                        if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                        request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/payin.html");
                        request.Headers.Add("origin", $"{base.TaskWithdrawMoney.Url}");

                        // set content
                        Dictionary<string, string> startExecuteSlaveTaskRequestContent = new Dictionary<string, string>();
                        startExecuteSlaveTaskRequestContent.Add("session", "");
                        startExecuteSlaveTaskRequestContent.Add("amount", $"{main_balance.Value.ToString("0.0000").Replace(',', '.')}"); /// баланс отсылать через ".", пример 1.5 рубля remove after!!!!
                        startExecuteSlaveTaskRequestContent.Add("provider", "account");
                        startExecuteSlaveTaskRequestContent.Add("pin", $"{base.TaskWithdrawMoney.Account!.Pincode}");
                        request.Content = new FormUrlEncodedContent(startExecuteSlaveTaskRequestContent);

                        // request
                        responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                        content = await responce.Content.ReadAsStringAsync();
                        last_content = content;

                        //if (responce.StatusCode != HttpStatusCode.OK && responce.StatusCode != HttpStatusCode.Found && responce.Headers.Location == null && !responce.Headers.Location!.ToString().Contains("payin.html"))
                        //{
                        //    throw new Exception($"responce.StatusCode != HttpStatusCode.OK && responce.StatusCode != HttpStatusCode.Found && responce.Headers.Location == null && !responce.Headers.Location!.ToString().Contains(\"payin.html\") --- 2.1 перевод денег на рекламный баланс \n\n\n{content}");
                        //}

                        if (responce.StatusCode == HttpStatusCode.OK && content.Contains("Средства зачисляются на ваш баланс"))
                        {
                            htmlPageDocument = await base.htmlParseContext!.OpenAsync(req => req.Content(content));

                            var successful_replenishment = htmlPageDocument.QuerySelector("div.note.note-success");
                            if (successful_replenishment == null || !successful_replenishment.TextContent.Contains("Средства зачисляются на ваш баланс"))
                            {
                                throw new Exception($"successful_replenishment == null || !successful_replenishment.TextContent.Contains(\"Средства зачисляются на ваш баланс\") --- 2.1 перевод денег на рекламный баланс \n\n\n{content}");
                            }

                            need_to_check_transfer = false;

                            balances = htmlPageDocument.QuerySelectorAll("div.collapse.navbar-collapse > ul.page-sidebar-menu > li.balance-box > div.inner > div").ToList();

                            if (balances.Count != 2)
                            {
                                throw new Exception($"Ошибка при получении 'var balances = htmlPageDocument.QuerySelectorAll(\"div.collapse.navbar-collapse > ul.page-sidebar-menu > li.balance-box > div.inner > div\").ToList();' --- 2.1 перевод денег на рекламный баланс\n\n\n{content}");
                            }

                            main_balance = balances[0]!.QuerySelector("span.balance_rub")!.TextContent.ToDouble();
                            advert_balance = balances[1]!.QuerySelector("span.balance_rub_advert")!.TextContent.ToDouble();

                            //var div_with_parts = htmlPageDocument.QuerySelector("form[action=\"/account/payout.html?act=payout\"]")?.QuerySelectorAll("div.form-group")[2]?.QuerySelector("div > div"); ;

                            //if (div_with_parts == null)
                            //{
                            //    throw new Exception($"div_with_parts == null --- 2.1 перевод денег на рекламный баланс\n\n\n{content}");
                            //}

                            //// get exact main_balance value
                            //string? first_main_balance_value_part = div_with_parts.QuerySelector("span.font-size-16.color-grey")!.TextContent;
                            //string? second_main_balance_value_part = div_with_parts.QuerySelector("span.font-size-16.color-mid-grey")!.TextContent;

                            //main_balance = $"{first_main_balance_value_part}{second_main_balance_value_part}".ToDouble();

                            if (main_balance == null)
                            {
                                throw new Exception($"main_balance == null --- 2.1 перевод денег на рекламный баланс\n\n\n{content}");
                            }

                            if (advert_balance == null)
                            {
                                throw new Exception($"advert_balance == null --- 2.1 перевод денег на рекламный баланс\n\n\n{content}");
                            }

                            if (main_balance != 0.0000)
                            {
                                throw new Exception($"main_balance != 0.0000 --- main_balance={main_balance} rub. --- 2.1 перевод денег на рекламный баланс\n\n\n{content}");
                            }

                            base.TaskWithdrawMoney.Account!.MoneyMainBalance = main_balance;
                            base.TaskWithdrawMoney.Account.MoneyAdvertisingBalance = advert_balance;

                            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(randomDelay.Next(5, 31)));
                        }
                        else if (responce.StatusCode == HttpStatusCode.OK && !content.Contains("Средства зачисляются на ваш баланс"))
                        {
                            if (content.Contains("Учитывая некоторые обстоятельства вы не можете пополнить рекламный баланс используя средства своего основного баланса."))
                            {
                                throw new Exception($"responce.StatusCode == HttpStatusCode.OK && !content.Contains(\"Средства зачисляются на ваш баланс\") --- 2.1 перевод денег на рекламный баланс\ncontent.Contains(\"Учитывая некоторые обстоятельства вы не можете пополнить рекламный баланс используя средства своего основного баланса.\")\nРешение - возможно нужно сменить прокси\n\n\n{content}\n\n\nmain_balance={main_balance}\nadvert_balance={advert_balance}");
                            }

                            throw new Exception($"responce.StatusCode == HttpStatusCode.OK && !content.Contains(\"Средства зачисляются на ваш баланс\") --- 2.1 перевод денег на рекламный баланс\n\n\n{content}\n\n\nmain_balance={main_balance}\nadvert_balance={advert_balance}\nmain balance string amount = {startExecuteSlaveTaskRequestContent.Where(d => d.Key == "amount").FirstOrDefault()}");
                        }
                        else if (responce.StatusCode == HttpStatusCode.Found || responce.StatusCode == HttpStatusCode.Redirect)
                        {
                            need_to_check_transfer = true;
                        }
                    }

                    #endregion

                    if (need_to_check_transfer)
                    {
                        #region 2.2 прооверка успешности перевода

                        uri = $"{base.TaskWithdrawMoney!.Url}/account/payin.html";

                        using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                        {
                            // set headers to request
                            foreach (var header in base.contextHeaders!)
                            {
                                request.Headers.Add(header.Key, header.Value);
                            }

                            if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                            if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                            request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/payin.html");

                            // request
                            responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                            if (responce.StatusCode != HttpStatusCode.OK)
                            {
                                throw new Exception("responce.StatusCode != HttpStatusCode.OK --- 2.2 прооверка успешности перевода");
                            }

                            content = await responce.Content.ReadAsStringAsync();
                        }

                        if (!content.Contains("Средства зачисляются на ваш баланс."))
                        {
                            if (content.Contains("Учитывая некоторые обстоятельства вы не можете пополнить рекламный баланс используя средства своего основного баланса."))
                            {
                                throw new Exception($"!content.Contains(\"Средства зачисляются на ваш баланс.\") --- 2.2 прооверка успешности перевода\ncontent.Contains(\"Учитывая некоторые обстоятельства вы не можете пополнить рекламный баланс используя средства своего основного баланса.\")\nРешение - возможно нужно сменить прокси\n\n\n{content}\n\n\nmain_balance={main_balance}\nadvert_balance={advert_balance}");
                            }

                            throw new Exception($"!content.Contains(\"Средства зачисляются на ваш баланс.\") --- 2.2 прооверка успешности перевода \n\n\n{content}");
                        }

                        htmlPageDocument = await base.htmlParseContext!.OpenAsync(req => req.Content(content));

                        var successful_replenishment = htmlPageDocument.QuerySelector("div.note.note-success");
                        if (successful_replenishment == null || !successful_replenishment.TextContent.Contains("Средства зачисляются на ваш баланс"))
                        {
                            throw new Exception($"successful_replenishment == null || !successful_replenishment.TextContent.Contains(\"Средства зачисляются на ваш баланс\") --- 2.2 прооверка успешности перевода \n\n\n{content}");
                        }

                        balances = htmlPageDocument.QuerySelectorAll("div.collapse.navbar-collapse > ul.page-sidebar-menu > li.balance-box > div.inner > div").ToList();

                        if (balances.Count != 2)
                        {
                            throw new Exception("Ошибка при получении 'var balances = htmlPageDocument.QuerySelectorAll(\"div.collapse.navbar-collapse > ul.page-sidebar-menu > li.balance-box > div.inner > div\").ToList();' --- 2.2 прооверка успешности перевода");
                        }

                        main_balance = balances[0]!.QuerySelector("span.balance_rub")!.TextContent.ToDouble();
                        advert_balance = balances[1]!.QuerySelector("span.balance_rub_advert")!.TextContent.ToDouble();

                        //var div_with_parts = htmlPageDocument.QuerySelector("form[action=\"/account/payout.html?act=payout\"]")?.QuerySelectorAll("div.form-group")[2]?.QuerySelector("div > div"); ;

                        //if (div_with_parts == null)
                        //{
                        //    throw new Exception($"div_with_parts == null --- 2.2 прооверка успешности перевода\n\n\n{content}");
                        //}

                        //// get exact main_balance value
                        //string? first_main_balance_value_part = div_with_parts.QuerySelector("span.font-size-16.color-grey")!.TextContent;
                        //string? second_main_balance_value_part = div_with_parts.QuerySelector("span.font-size-16.color-mid-grey")!.TextContent;

                        //main_balance = $"{first_main_balance_value_part}{second_main_balance_value_part}".ToDouble();

                        if (main_balance == null)
                        {
                            throw new Exception("main_balance == null --- 2.2 прооверка успешности перевода");
                        }

                        if (advert_balance == null)
                        {
                            throw new Exception("advert_balance == null --- 2.2 прооверка успешности перевода");
                        }

                        if (main_balance != 0.0000)
                        {
                            throw new Exception($"main_balance != 0.0000 --- main_balance={main_balance} rub. --- 2.2 прооверка успешности перевода");
                        }

                        base.TaskWithdrawMoney.Account!.MoneyMainBalance = main_balance;
                        base.TaskWithdrawMoney.Account.MoneyAdvertisingBalance = advert_balance;

                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(randomDelay.Next(5, 31)));

                        #endregion
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message + "\n\n\nНеизвестная ошибка: 2-й шаг (по надобности): перевод денег с главного на рекламный баланс");
                }
            }

            #endregion

            #region 3-й шаг: получения списка заданий (переход на вкладку)

            if (!base.TaskWithdrawMoney!.NeedToRemovePlatformTaskForSlave.HasValue && !base.TaskWithdrawMoney.NeedToEditPlatformTaskForSlave.HasValue)
            {
                try
                {
                    uri = $"{base.TaskWithdrawMoney!.Url}/account/task_adv_list.html";

                    using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                    {
                        // set headers to request
                        foreach (var header in base.contextHeaders!)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }

                        if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                        if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                        request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/payin.html");

                        // request
                        responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                        if (responce.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception($"responce.StatusCode != HttpStatusCode.OK --- 3-й шаг: получения списка заданий (переход на вкладку)\n\n\n{content}");
                        }

                        content = await responce.Content.ReadAsStringAsync();
                    }

                    if (!content.Contains("У вас нет заданий"))
                    {
                        htmlPageDocument = await base.htmlParseContext!.OpenAsync(req => req.Content(content));

                        var platform_tasks = htmlPageDocument.QuerySelectorAll("tbody.last-tr-green > tr.border-top-green").ToList();

                        if (platform_tasks.Count == 0)
                        {
                            throw new Exception($"platform_tasks.Count == 0 --- 3-й шаг: получения списка заданий (переход на вкладку)\n\n\n{content}");
                        }
                        //else if (platform_tasks.Count > 1)
                        //{
                        //    throw new Exception($"platform_tasks.Count > 1 --- 3-й шаг: получения списка заданий (переход на вкладку)\n\n\n{content}");
                        //}
                        else
                        {
                            elementPlatformTask = platform_tasks[0];
                        }

                        if (elementPlatformTask == null)
                        {
                            throw new Exception($"elementPlatformTask == null --- 3-й шаг: получения списка заданий (переход на вкладку)\n\n\n{content}");
                        }

                        if (String.IsNullOrEmpty(InternalSlavePlatformTask.InternalNumberId))
                        {
                            //get id internal task if its 'null'
                            InternalSlavePlatformTask.InternalNumberId = elementPlatformTask.QuerySelector("td[data-label='Название'] > div > div.task-list-title > span.title-id")!.TextContent.Remove(0, 1);

                            if (String.IsNullOrEmpty(InternalSlavePlatformTask.InternalNumberId))
                            {
                                throw new Exception($"String.IsNullOrEmpty(InternalSlavePlatformTask.InternalNumberId) --- 3-й шаг: получения списка заданий (переход на вкладку)\n\n\n{content}");
                            }

                            #region 3.1 обновление в бд задания привязанного к этому слейву (добавление ид-номера)

                            //var update = await ServerHubConnection.InvokeAsync<bool>("update_slave_internal_account_task", base.TaskWithdrawMoney!.Account.Login, InternalSlavePlatformTask.InternalNumberId);
                            await ServerHubConnection.SendAsync("update_slave_internal_account_task", base.TaskWithdrawMoney!.Account.Login, InternalSlavePlatformTask.InternalNumberId);

                            //if (update == false)
                            //{
                            //    throw new Exception("update == false --- 3.1 обновление в бд задания привязанного к этому слейву (добавление ид-номера)");
                            //}

                            #endregion 
                        }

                        if (base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask == null)
                        {
                            base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask = new WithdrawalOfMoneyPlatformTaskModel();
                        }

                        base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskId = InternalSlavePlatformTask.InternalNumberId;
                        if (InternalSlavePlatformTask.approve_type == "auto")
                        {
                            base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.AutoPayment = true;
                            base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskCodePasswordForSolveTask = InternalSlavePlatformTask.approve_answer_1_1;
                        }
                        else if (InternalSlavePlatformTask.approve_type == "hand")
                        {
                            base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.AutoPayment = false;
                            base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskTextForSolveTask = InternalSlavePlatformTask.platformTaskTextForSolveTask;
                        }
                        base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskExecuteMaxTimeInSecs = InternalSlavePlatformTask.work_time?.ToInteger(300);
                        base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskMoney = InternalSlavePlatformTask.price_user?.ToDouble(0);
                        // remove this after
                        //if (InternalSlavePlatformTask.approve_answer_1_1 != null)
                        //{
                        //    base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskCodePasswordForSolveTask = InternalSlavePlatformTask.approve_answer_1_1;
                        //}
                        //else if (InternalSlavePlatformTask.approve_answer_1_1 == null)
                        //{
                        //    base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskTextForSolveTask = InternalSlavePlatformTask.platformTaskTextForSolveTask;
                        //}
                    }

                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(randomDelay.Next(5, 20)));
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message + "\n\n\nНеизвестная ошибка: 3-й шаг: получения списка заданий (переход на вкладку)");
                } 
            }

            #endregion

            #region 4-й шаг (по надобности): создание или редактирование или удаление задания

            #region 4.1 удаление задания

            if (base.TaskWithdrawMoney.NeedToRemovePlatformTaskForSlave.HasValue && base.TaskWithdrawMoney.NeedToRemovePlatformTaskForSlave.Value)
            {
                //#region 4.1.1 получение из бд задания привязанного к этому слейву

                //InternalSlavePlatformTask = await ServerHubConnection.InvokeAsync<PlatformInternalAccountTaskModel>("get_slave_internal_account_task", base.TaskWithdrawMoney!.Account!.Login);

                //if (InternalSlavePlatformTask == null)
                //{
                //    throw new Exception("InternalSlavePlatformTask == null --- 4.1.1 получение из бд задания привязанного к этому слейву");
                //}

                //#endregion

                #region 4.1.1 удаление задания

                queryParams = new Dictionary<string, string>()
                {
                    ["act"] = $"delete",
                    ["id"] = $"{InternalSlavePlatformTask.InternalNumberId}",
                    ["page"] = $"1",
                    ["folder_id"] = $"0",
                    ["session"] = $"",
                };

                uri = QueryHelpers.AddQueryString($"{base.TaskWithdrawMoney!.Url}/account/task_adv_list.html", queryParams);

                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    // set headers to request
                    foreach (var header in base.contextHeaders!)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }

                    if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                    if (request.Headers.Contains("origin")) request.Headers.Remove("origin");
                    if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                    request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/task_adv_list.html");

                    // request
                    responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                    content = await responce.Content.ReadAsStringAsync();

                    if (responce.StatusCode != HttpStatusCode.OK && responce.StatusCode != HttpStatusCode.Found && responce.Headers.Location == null && !responce.Headers.Location!.ToString().Contains("task_adv_list.html"))
                    {
                        throw new Exception($"responce.StatusCode != HttpStatusCode.OK && responce.StatusCode != HttpStatusCode.Found && responce.Headers.Location == null && !responce.Headers.Location!.ToString().Contains(\"task_adv_list.html\") --- 4.1.1 удаление задания \n\n\n{content}");
                    }
                }

                throw new Exception("task was removed successfully"); 

                #endregion
            }

            #endregion

            #region 4.2 редактирование задания

            if (base.TaskWithdrawMoney.NeedToEditPlatformTaskForSlave.HasValue && base.TaskWithdrawMoney.NeedToEditPlatformTaskForSlave.Value)
            {
                //#region 4.1.1 получение из бд задания привязанного к этому слейву

                //InternalSlavePlatformTask = await ServerHubConnection.InvokeAsync<PlatformInternalAccountTaskModel>("get_slave_internal_account_task", base.TaskWithdrawMoney!.Account!.Login);

                //if (InternalSlavePlatformTask == null)
                //{
                //    throw new Exception("InternalSlavePlatformTask == null --- 4.1.1 получение из бд задания привязанного к этому слейву");
                //}

                //#endregion

                #region 4.1.2 редактирвоание задания

                queryParams = new Dictionary<string, string>()
                {
                    ["act"] = $"save",
                    ["id"] = $"{InternalSlavePlatformTask.InternalNumberId}",
                    ["tpl_type"] = $"adv"
                };

                uri = QueryHelpers.AddQueryString($"{base.TaskWithdrawMoney!.Url}/account/task_adv_edit.html", queryParams);

                using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                {
                    // set headers to request
                    foreach (var header in base.contextHeaders!)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }

                    if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                    if (request.Headers.Contains("origin")) request.Headers.Remove("origin");
                    if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                    request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/task_adv_edit.html?id={InternalSlavePlatformTask.InternalNumberId}&tpl_type=adv");
                    request.Headers.Add("origin", $"{base.TaskWithdrawMoney.Url}");

                    // set content
                    Dictionary<string, string> createSlaveTaskRequestContent = new Dictionary<string, string>();

                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.session)}", $"{InternalSlavePlatformTask.session}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.name)}", $"{InternalSlavePlatformTask.name}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.url)}", $"{InternalSlavePlatformTask.url}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.url_count)}", $"{InternalSlavePlatformTask.url_count}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.type)}", $"{InternalSlavePlatformTask.type}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.description)}", $"{InternalSlavePlatformTask.description}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_type)}", $"{InternalSlavePlatformTask.approve_type}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_count)}", $"{InternalSlavePlatformTask.approve_count}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_text)}", $"{InternalSlavePlatformTask.approve_text}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_quest_0)}", $"{InternalSlavePlatformTask.approve_quest_0}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_answer_0_1)}", $"{InternalSlavePlatformTask.approve_answer_0_1}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_answer_0_count)}", $"{InternalSlavePlatformTask.approve_answer_0_count}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_quest_1)}", $"{InternalSlavePlatformTask.approve_quest_1}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_answer_1_1)}", $"{InternalSlavePlatformTask.approve_answer_1_1}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_answer_1_count)}", $"{InternalSlavePlatformTask.approve_answer_1_count}");
                    if (InternalSlavePlatformTask.special_adult != null)
                    {
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.special_adult)}", $"{InternalSlavePlatformTask.special_adult}");
                    }
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.day_1)}", $"{InternalSlavePlatformTask.day_1}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.day_2)}", $"{InternalSlavePlatformTask.day_2}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.day_3)}", $"{InternalSlavePlatformTask.day_3}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.day_4)}", $"{InternalSlavePlatformTask.day_4}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.day_5)}", $"{InternalSlavePlatformTask.day_5}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.day_6)}", $"{InternalSlavePlatformTask.day_6}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.day_7)}", $"{InternalSlavePlatformTask.day_7}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_6_9_flag)}", $"{InternalSlavePlatformTask.time_6_9_flag}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_6_9)}", $"{InternalSlavePlatformTask.time_6_9}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_9_12_flag)}", $"{InternalSlavePlatformTask.time_9_12_flag}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_9_12)}", $"{InternalSlavePlatformTask.time_9_12}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_12_15_flag)}", $"{InternalSlavePlatformTask.time_12_15_flag}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_12_15)}", $"{InternalSlavePlatformTask.time_12_15}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_15_18_flag)}", $"{InternalSlavePlatformTask.time_15_18_flag}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_15_18)}", $"{InternalSlavePlatformTask.time_15_18}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_18_21_flag)}", $"{InternalSlavePlatformTask.time_18_21_flag}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_18_21)}", $"{InternalSlavePlatformTask.time_18_21}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_21_24_flag)}", $"{InternalSlavePlatformTask.time_21_24_flag}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_21_24)}", $"{InternalSlavePlatformTask.time_21_24}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_0_3_flag)}", $"{InternalSlavePlatformTask.time_0_3_flag}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_0_3)}", $"{InternalSlavePlatformTask.time_0_3}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_3_6_flag)}", $"{InternalSlavePlatformTask.time_3_6_flag}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_3_6)}", $"{InternalSlavePlatformTask.time_3_6}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.timeout)}", $"{InternalSlavePlatformTask.timeout}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.work_filter)}", $"{InternalSlavePlatformTask.work_filter}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.family_filter)}", $"{InternalSlavePlatformTask.family_filter}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.gender_filter)}", $"{InternalSlavePlatformTask.gender_filter}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.age_from)}", $"{InternalSlavePlatformTask.age_from}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.age_to)}", $"{InternalSlavePlatformTask.age_to}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_filter)}", $"{InternalSlavePlatformTask.geo_filter}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru)}", $"{InternalSlavePlatformTask.geo_ru}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_48)}", $"{InternalSlavePlatformTask.geo_ru_48}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_47)}", $"{InternalSlavePlatformTask.geo_ru_47}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_66)}", $"{InternalSlavePlatformTask.geo_ru_66}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_71)}", $"{InternalSlavePlatformTask.geo_ru_71}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_38)}", $"{InternalSlavePlatformTask.geo_ru_38}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_111)}", $"{InternalSlavePlatformTask.geo_ru_111}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_13)}", $"{InternalSlavePlatformTask.geo_ru_13}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_53)}", $"{InternalSlavePlatformTask.geo_ru_53}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_65)}", $"{InternalSlavePlatformTask.geo_ru_65}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_86)}", $"{InternalSlavePlatformTask.geo_ru_86}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_51)}", $"{InternalSlavePlatformTask.geo_ru_51}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_8)}", $"{InternalSlavePlatformTask.geo_ru_8}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_29)}", $"{InternalSlavePlatformTask.geo_ru_29}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_73)}", $"{InternalSlavePlatformTask.geo_ru_73}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_61)}", $"{InternalSlavePlatformTask.geo_ru_61}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_5)}", $"{InternalSlavePlatformTask.geo_ua_5}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_14)}", $"{InternalSlavePlatformTask.geo_ua_14}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_other)}", $"{InternalSlavePlatformTask.geo_ru_other}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua)}", $"{InternalSlavePlatformTask.geo_ua}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_13)}", $"{InternalSlavePlatformTask.geo_ua_13}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_12)}", $"{InternalSlavePlatformTask.geo_ua_12}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_4)}", $"{InternalSlavePlatformTask.geo_ua_4}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_7)}", $"{InternalSlavePlatformTask.geo_ua_7}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_26)}", $"{InternalSlavePlatformTask.geo_ua_26}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_17)}", $"{InternalSlavePlatformTask.geo_ua_17}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_other)}", $"{InternalSlavePlatformTask.geo_ua_other}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.per_24)}", $"{InternalSlavePlatformTask.per_24}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.repeat_value)}", $"{InternalSlavePlatformTask.repeat_value}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.work_time)}", $"{InternalSlavePlatformTask.work_time}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.repeat_before_check)}", $"{InternalSlavePlatformTask.repeat_before_check}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.user_xp)}", $"{InternalSlavePlatformTask.user_xp}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.ip_filter)}", $"{InternalSlavePlatformTask.ip_filter}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.captcha_type)}", $"{InternalSlavePlatformTask.captcha_type}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.ref_filter)}", $"{InternalSlavePlatformTask.ref_filter}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.proove_email)}", $"{InternalSlavePlatformTask.proove_email}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.proove_phone)}", $"{InternalSlavePlatformTask.proove_phone}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.price_user)}", $"{InternalSlavePlatformTask.price_user}");
                    createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.auto_funds)}", $"{InternalSlavePlatformTask.auto_funds}");

                    request.Content = new FormUrlEncodedContent(createSlaveTaskRequestContent);

                    // request
                    responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                    content = await responce.Content.ReadAsStringAsync();

                    if (responce.StatusCode != HttpStatusCode.OK && responce.StatusCode != HttpStatusCode.Found && responce.Headers.Location == null && !responce.Headers.Location!.ToString().Contains("task_adv_edit.html"))
                    {
                        throw new Exception($"responce.StatusCode != HttpStatusCode.OK && responce.StatusCode != HttpStatusCode.Found && responce.Headers.Location == null && !responce.Headers.Location!.ToString().Contains(\"task_adv_edit.html\") --- 4.1.2 редактирвоание задания \n\n\n{content}");
                    }

                    throw new Exception("task was edited successfully");
                }

                #endregion
            }

            #endregion

            #region 4.3 создание задания

            if (elementPlatformTask == null)
            {
                try
                {
                    //#region 4.3.1 получение из бд задания привязанного к этому слейву

                    //InternalSlavePlatformTask = await ServerHubConnection.InvokeAsync<PlatformInternalAccountTaskModel?>("get_slave_internal_account_task", base.TaskWithdrawMoney!.Account!.Login);

                    //if (InternalSlavePlatformTask == null)
                    //{
                    //    throw new Exception("InternalSlavePlatformTask == null --- 4.3.1 получение из бд задания привязанного к этому слейву");
                    //}

                    //#endregion

                    bool need_to_check_task_creation = true;

                    #region 4.3.2 создание задания

                    uri = $"{base.TaskWithdrawMoney!.Url}/account/task_adv_add.html";

                    using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                    {
                        // set headers to request
                        foreach (var header in base.contextHeaders!)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }

                        if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                        if (request.Headers.Contains("origin")) request.Headers.Remove("origin");
                        if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                        request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/task_adv_add.html");
                        request.Headers.Add("origin", $"{base.TaskWithdrawMoney.Url}");

                        // set content
                        Dictionary<string, string> createSlaveTaskRequestContent = new Dictionary<string, string>();

                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.session)}", $"{InternalSlavePlatformTask.session}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.name)}", $"{InternalSlavePlatformTask.name}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.url)}", $"{InternalSlavePlatformTask.url}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.url_count)}", $"{InternalSlavePlatformTask.url_count ?? ""}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.type)}", $"{InternalSlavePlatformTask.type}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.description)}", $"{InternalSlavePlatformTask.description}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_type)}", $"{InternalSlavePlatformTask.approve_type}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_count)}", $"{InternalSlavePlatformTask.approve_count}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_text)}", $"{InternalSlavePlatformTask.approve_text}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_quest_0)}", $"{InternalSlavePlatformTask.approve_quest_0}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_answer_0_1)}", $"{InternalSlavePlatformTask.approve_answer_0_1}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_answer_0_count)}", $"{InternalSlavePlatformTask.approve_answer_0_count}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_quest_1)}", $"{InternalSlavePlatformTask.approve_quest_1}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_answer_1_1)}", $"{InternalSlavePlatformTask.approve_answer_1_1}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.approve_answer_1_count)}", $"{InternalSlavePlatformTask.approve_answer_1_count}");
                        if (InternalSlavePlatformTask.special_adult != null)
                        {
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.special_adult)}", $"{InternalSlavePlatformTask.special_adult}");
                        }
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.day_1)}", $"{InternalSlavePlatformTask.day_1}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.day_2)}", $"{InternalSlavePlatformTask.day_2}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.day_3)}", $"{InternalSlavePlatformTask.day_3}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.day_4)}", $"{InternalSlavePlatformTask.day_4}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.day_5)}", $"{InternalSlavePlatformTask.day_5}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.day_6)}", $"{InternalSlavePlatformTask.day_6}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.day_7)}", $"{InternalSlavePlatformTask.day_7}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_6_9_flag)}", $"{InternalSlavePlatformTask.time_6_9_flag}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_6_9)}", $"{InternalSlavePlatformTask.time_6_9}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_9_12_flag)}", $"{InternalSlavePlatformTask.time_9_12_flag}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_9_12)}", $"{InternalSlavePlatformTask.time_9_12}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_12_15_flag)}", $"{InternalSlavePlatformTask.time_12_15_flag}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_12_15)}", $"{InternalSlavePlatformTask.time_12_15}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_15_18_flag)}", $"{InternalSlavePlatformTask.time_15_18_flag}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_15_18)}", $"{InternalSlavePlatformTask.time_15_18}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_18_21_flag)}", $"{InternalSlavePlatformTask.time_18_21_flag}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_18_21)}", $"{InternalSlavePlatformTask.time_18_21}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_21_24_flag)}", $"{InternalSlavePlatformTask.time_21_24_flag}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_21_24)}", $"{InternalSlavePlatformTask.time_21_24}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_0_3_flag)}", $"{InternalSlavePlatformTask.time_0_3_flag}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_0_3)}", $"{InternalSlavePlatformTask.time_0_3}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_3_6_flag)}", $"{InternalSlavePlatformTask.time_3_6_flag}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.time_3_6)}", $"{InternalSlavePlatformTask.time_3_6}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.timeout)}", $"{InternalSlavePlatformTask.timeout}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.work_filter)}", $"{InternalSlavePlatformTask.work_filter}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.family_filter)}", $"{InternalSlavePlatformTask.family_filter}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.gender_filter)}", $"{InternalSlavePlatformTask.gender_filter}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.age_from)}", $"{InternalSlavePlatformTask.age_from}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.age_to)}", $"{InternalSlavePlatformTask.age_to}");
                        if (InternalSlavePlatformTask.geo_filter != null && InternalSlavePlatformTask.geo_filter == "1")
                        {
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_filter)}", $"{InternalSlavePlatformTask.geo_filter}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru)}", $"{InternalSlavePlatformTask.geo_ru}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_48)}", $"{InternalSlavePlatformTask.geo_ru_48}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_47)}", $"{InternalSlavePlatformTask.geo_ru_47}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_66)}", $"{InternalSlavePlatformTask.geo_ru_66}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_71)}", $"{InternalSlavePlatformTask.geo_ru_71}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_38)}", $"{InternalSlavePlatformTask.geo_ru_38}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_111)}", $"{InternalSlavePlatformTask.geo_ru_111}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_13)}", $"{InternalSlavePlatformTask.geo_ru_13}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_53)}", $"{InternalSlavePlatformTask.geo_ru_53}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_65)}", $"{InternalSlavePlatformTask.geo_ru_65}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_86)}", $"{InternalSlavePlatformTask.geo_ru_86}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_51)}", $"{InternalSlavePlatformTask.geo_ru_51}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_8)}", $"{InternalSlavePlatformTask.geo_ru_8}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_29)}", $"{InternalSlavePlatformTask.geo_ru_29}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_73)}", $"{InternalSlavePlatformTask.geo_ru_73}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_61)}", $"{InternalSlavePlatformTask.geo_ru_61}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_5)}", $"{InternalSlavePlatformTask.geo_ua_5}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_14)}", $"{InternalSlavePlatformTask.geo_ua_14}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ru_other)}", $"{InternalSlavePlatformTask.geo_ru_other}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua)}", $"{InternalSlavePlatformTask.geo_ua}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_13)}", $"{InternalSlavePlatformTask.geo_ua_13}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_12)}", $"{InternalSlavePlatformTask.geo_ua_12}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_4)}", $"{InternalSlavePlatformTask.geo_ua_4}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_7)}", $"{InternalSlavePlatformTask.geo_ua_7}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_26)}", $"{InternalSlavePlatformTask.geo_ua_26}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_17)}", $"{InternalSlavePlatformTask.geo_ua_17}");
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_ua_other)}", $"{InternalSlavePlatformTask.geo_ua_other}");
                        }
                        else if (InternalSlavePlatformTask.geo_filter != null && InternalSlavePlatformTask.geo_filter == "0")
                        {
                            createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.geo_filter)}", $"{InternalSlavePlatformTask.geo_filter}");
                        }
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.per_24)}", $"{InternalSlavePlatformTask.per_24}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.repeat_value)}", $"{InternalSlavePlatformTask.repeat_value}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.work_time)}", $"{InternalSlavePlatformTask.work_time}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.repeat_before_check)}", $"{InternalSlavePlatformTask.repeat_before_check}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.user_xp)}", $"{InternalSlavePlatformTask.user_xp}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.ip_filter)}", $"{InternalSlavePlatformTask.ip_filter}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.captcha_type)}", $"{InternalSlavePlatformTask.captcha_type}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.ref_filter)}", $"{InternalSlavePlatformTask.ref_filter}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.proove_email)}", $"{InternalSlavePlatformTask.proove_email}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.proove_phone)}", $"{InternalSlavePlatformTask.proove_phone}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.price_user)}", $"{InternalSlavePlatformTask.price_user}");
                        createSlaveTaskRequestContent.Add($"{nameof(InternalSlavePlatformTask.auto_funds)}", $"{InternalSlavePlatformTask.auto_funds}");

                        request.Content = new FormUrlEncodedContent(createSlaveTaskRequestContent);

                        // request
                        responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                        content = await responce.Content.ReadAsStringAsync();
                        last_content = content;

                        //if (responce.StatusCode != HttpStatusCode.OK && responce.StatusCode != HttpStatusCode.Found && responce.Headers.Location == null && !responce.Headers.Location!.ToString().Contains("task_adv_list.html"))
                        //{
                        //    throw new Exception($"responce.StatusCode != HttpStatusCode.OK && responce.StatusCode != HttpStatusCode.Found && responce.Headers.Location == null && !responce.Headers.Location!.ToString().Contains(\"task_adv_list.html\") --- 4.3.2 создание задания \n\n\n{content}");
                        //}

                        if (responce.StatusCode == HttpStatusCode.OK && (content.Contains("успешно создано") || !content.Contains("У вас нет заданий")))
                        {
                            need_to_check_task_creation = false;

                            htmlPageDocument = await base.htmlParseContext!.OpenAsync(req => req.Content(content));

                            var platform_tasks = htmlPageDocument.QuerySelectorAll("tbody.last-tr-green > tr").ToList();

                            if (platform_tasks.Count == 0)
                            {
                                throw new Exception($"platform_tasks.Count == 0 --- 4.3.2 создание задания\n\n\n{content}");
                            }
                            //else if (platform_tasks.Count > 1)
                            //{
                            //    throw new Exception($"platform_tasks.Count > 1 --- 4.3.2 создание задания\n\n\n{content}");
                            //}
                            else
                            {
                                elementPlatformTask = platform_tasks[0];
                            }

                            if (elementPlatformTask == null)
                            {
                                throw new Exception($"elementPlatformTask == null --- 4.3.2 создание задания\n\n\n{content}");
                            }

                            if (String.IsNullOrEmpty(InternalSlavePlatformTask.InternalNumberId))
                            {
                                //get id internal task if its 'null'
                                InternalSlavePlatformTask.InternalNumberId = elementPlatformTask.QuerySelector("td[data-label='Название'] > div > div.task-list-title > span.title-id")!.TextContent.Remove(0, 1);

                                if (String.IsNullOrEmpty(InternalSlavePlatformTask.InternalNumberId))
                                {
                                    throw new Exception($"String.IsNullOrEmpty(InternalSlavePlatformTask.InternalNumberId) --- 4.3.2 создание задания\n\n\n{content}");
                                }

                                #region 4.3.2.1 обновление в бд задания привязанного к этому слейву (добавление ид-номера)

                                //var update = await ServerHubConnection.InvokeAsync<bool>("update_slave_internal_account_task", base.TaskWithdrawMoney!.Account.Login, InternalSlavePlatformTask.InternalNumberId);
                                await ServerHubConnection.SendAsync("update_slave_internal_account_task", base.TaskWithdrawMoney!.Account.Login, InternalSlavePlatformTask.InternalNumberId);

                                //if (update == false)
                                //{
                                //    throw new Exception("update == false --- 4.3.2 создание задания");
                                //}

                                #endregion
                            }

                            if(base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask == null)
                            {
                                base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask = new WithdrawalOfMoneyPlatformTaskModel();
                            }

                            base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskId = InternalSlavePlatformTask.InternalNumberId;
                            if (InternalSlavePlatformTask.approve_type == "auto")
                            {
                                base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.AutoPayment = true;
                                base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskCodePasswordForSolveTask = InternalSlavePlatformTask.approve_answer_1_1;
                            }
                            else if (InternalSlavePlatformTask.approve_type == "hand")
                            {
                                base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.AutoPayment = false;
                                base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskTextForSolveTask = InternalSlavePlatformTask.platformTaskTextForSolveTask;
                            }
                            base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskExecuteMaxTimeInSecs = InternalSlavePlatformTask.work_time?.ToInteger(300);
                            base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskMoney = InternalSlavePlatformTask.price_user?.ToDouble(0);
                            //if (InternalSlavePlatformTask.approve_answer_1_1 != null)
                            //{
                            //    base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskCodePasswordForSolveTask = InternalSlavePlatformTask.approve_answer_1_1;
                            //}
                            //else if (InternalSlavePlatformTask.approve_answer_1_1 == null)
                            //{
                            //    base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskTextForSolveTask = InternalSlavePlatformTask.platformTaskTextForSolveTask;
                            //}

                            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(randomDelay.Next(5, 15)));
                        }
                        else if (responce.StatusCode == HttpStatusCode.OK && (content.Contains("У вас нет заданий") || !content.Contains("успешно создано")))
                        {
                            throw new Exception($"responce.StatusCode == HttpStatusCode.OK && (content.Contains(\"У вас нет заданий\") || !content.Contains(\"успешно создано\")) --- 4.3.2 создание задания \n\n\n{content}");
                        }
                        else if (responce.StatusCode == HttpStatusCode.Found || responce.StatusCode == HttpStatusCode.Redirect)
                        {
                            need_to_check_task_creation = true;
                        }
                    }

                    #endregion

                    if (need_to_check_task_creation)
                    {
                        #region 4.3.3 проверка успешности создания задания

                        uri = $"{base.TaskWithdrawMoney!.Url}/account/task_adv_list.html";

                        using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                        {
                            // set headers to request
                            foreach (var header in base.contextHeaders!)
                            {
                                request.Headers.Add(header.Key, header.Value);
                            }

                            if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                            if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                            request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/task_adv_add.html");

                            // request
                            responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                            if (responce.StatusCode != HttpStatusCode.OK)
                            {
                                throw new Exception($"responce.StatusCode != HttpStatusCode.OK --- 4.3.3 прооверка успешности создания задания\n\n\n{content}");
                            }

                            content = await responce.Content.ReadAsStringAsync();
                        }

                        if (!content.Contains("У вас нет заданий"))
                        {
                            htmlPageDocument = await base.htmlParseContext!.OpenAsync(req => req.Content(content));

                            var platform_tasks = htmlPageDocument.QuerySelectorAll("tbody.last-tr-green > tr").ToList();

                            if (platform_tasks.Count == 0)
                            {
                                throw new Exception($"platform_tasks.Count == 0 --- 4.3.3 прооверка успешности создания задания\n\n\n{content}");
                            }
                            //else if (platform_tasks.Count > 1)
                            //{
                            //    throw new Exception($"platform_tasks.Count > 1 --- 4.3.3 прооверка успешности создания задания\n\n\n{content}");
                            //}
                            else
                            {
                                elementPlatformTask = platform_tasks[0];
                            }

                            if (elementPlatformTask == null)
                            {
                                throw new Exception($"elementPlatformTask == null --- 4.3.3 прооверка успешности создания задания\n\n\n{content}");
                            }

                            if (String.IsNullOrEmpty(InternalSlavePlatformTask.InternalNumberId))
                            {
                                //get id internal task if its 'null'
                                InternalSlavePlatformTask.InternalNumberId = elementPlatformTask.QuerySelector("td[data-label='Название'] > div > div.task-list-title > span.title-id")!.TextContent.Remove(0, 1);

                                if (String.IsNullOrEmpty(InternalSlavePlatformTask.InternalNumberId))
                                {
                                    throw new Exception($"String.IsNullOrEmpty(InternalSlavePlatformTask.InternalNumberId) --- 4.3.2 создание задания\n\n\n{content}");
                                }

                                #region 4.3.2 создание задания

                                //var update = await ServerHubConnection.InvokeAsync<bool>("update_slave_internal_account_task", base.TaskWithdrawMoney!.Account.Login, InternalSlavePlatformTask.InternalNumberId);
                                await ServerHubConnection.SendAsync("update_slave_internal_account_task", base.TaskWithdrawMoney!.Account.Login, InternalSlavePlatformTask.InternalNumberId);

                                //if (update == false)
                                //{
                                //    throw new Exception("update == false --- 4.3.2 создание задания");
                                //}

                                #endregion
                            }

                            if (base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask == null)
                            {
                                base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask = new WithdrawalOfMoneyPlatformTaskModel();
                            }

                            base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskId = InternalSlavePlatformTask.InternalNumberId;
                            if (InternalSlavePlatformTask.approve_type == "auto")
                            {
                                base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.AutoPayment = true;
                                base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskCodePasswordForSolveTask = InternalSlavePlatformTask.approve_answer_1_1;
                            }
                            else if (InternalSlavePlatformTask.approve_type == "hand")
                            {
                                base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.AutoPayment = false;
                                base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskTextForSolveTask = InternalSlavePlatformTask.platformTaskTextForSolveTask;
                            }
                            base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskExecuteMaxTimeInSecs = InternalSlavePlatformTask.work_time?.ToInteger(300);
                            base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskMoney = InternalSlavePlatformTask.price_user?.ToDouble(0);
                            //if (InternalSlavePlatformTask.approve_answer_1_1 != null)
                            //{
                            //    base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskCodePasswordForSolveTask = InternalSlavePlatformTask.approve_answer_1_1;
                            //}
                            //else if (InternalSlavePlatformTask.approve_answer_1_1 == null)
                            //{
                            //    base.TaskWithdrawMoney.WithdrawalOfMoneyPlatformTask!.PlatformTaskTextForSolveTask = InternalSlavePlatformTask.platformTaskTextForSolveTask;
                            //}

                            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(randomDelay.Next(5, 15)));
                        }
                        else
                        {
                            throw new Exception($"!content.Contains(\"У вас нет заданий\"), задание не было создано! --- 4.3.3 прооверка успешности создания задания\n\n\n{last_content}");
                        }

                        #endregion
                    }

                    //#region 4.3.4 обновление в бд задания привязанного к этому слейву (добавление ид-номера)

                    ////var update = await ServerHubConnection.InvokeAsync<bool>("update_slave_internal_account_task", base.TaskWithdrawMoney!.Account.Login, InternalSlavePlatformTask.InternalNumberId);
                    //await ServerHubConnection.SendAsync("update_slave_internal_account_task", base.TaskWithdrawMoney!.Account.Login, InternalSlavePlatformTask.InternalNumberId);

                    ////if (update == false)
                    ////{
                    ////    throw new Exception("update == false --- 4.3.4 обновление в бд задания привязанного к этому слейву (добавление ид-номера)");
                    ////}

                    //#endregion
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message + "\n\n\nНеизвестная ошибка: 4.3 создание задания");
                }
            }

            #endregion

            #endregion

            #region 5-й шаг (по надобности): пополнение баланса задания

            try
            {
                //#region 5.1 получение из бд задания привязанного к этому слейву (по надобности)

                //if (InternalSlavePlatformTask == null)
                //{
                //    InternalSlavePlatformTask = await ServerHubConnection.InvokeAsync<PlatformInternalAccountTaskModel?>("get_slave_internal_account_task", base.TaskWithdrawMoney!.Account!.Login);

                //    if (InternalSlavePlatformTask == null)
                //    {
                //        throw new Exception("InternalSlavePlatformTask == null --- 5.1 получение из бд задания привязанного к этому слейву (по надобности)");
                //    }
                //}

                //#endregion

                #region 5.2 пополнение баланса задания

                // получение текущего баланса задания
                double task_current_balance = elementPlatformTask.QuerySelector("td[data-label='Баланс'] > span.font-size-14")!.TextContent.ToDouble();
                double task_current_price = InternalSlavePlatformTask.price_user.ToDouble();
                //double credit_sum = task_current_price * 0.3 + task_current_price - task_current_balance;
                double credit_sum = task_current_price * 0.30 + task_current_price;

                if (credit_sum < 0)
                {
                    throw new Exception($"credit_sum < 0 --- 5.2 пополнение баланса задания\n\n\n{content}");
                }

                if (advert_balance < credit_sum && task_current_balance < credit_sum)
                {
                    throw new Exception($"advert_balance < credit_sum && task_current_balance < credit_sum (Недостаточно средств для пополнения баланса задания и для запуска задания) --- 5.2 пополнение баланса задания\n\n\n{content}");
                }

                if (task_current_balance < credit_sum)
                {
                    //string final_credit_sum_in_str = credit_sum.ToString("0.00").Replace(',', '.');

                    // запрос на пополнение баланса задания
                    queryParams = new Dictionary<string, string>()
                    {
                        ["act"] = $"make_fund"
                    };

                    uri = QueryHelpers.AddQueryString($"{base.TaskWithdrawMoney!.Url}/task.ajax", queryParams);

                    using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                    {
                        // set headers to request
                        foreach (var header in base.contextHeaders!)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }

                        if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                        if (request.Headers.Contains("origin")) request.Headers.Remove("origin");
                        if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                        request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/task_adv_list.html");
                        request.Headers.Add("origin", $"{base.TaskWithdrawMoney.Url}");

                        // set content
                        Dictionary<string, string> creditBalanceSlaveTaskRequestContent = new Dictionary<string, string>();
                        creditBalanceSlaveTaskRequestContent.Add("task_id", $"{InternalSlavePlatformTask.InternalNumberId}");
                        creditBalanceSlaveTaskRequestContent.Add("fund_value", $"{credit_sum.ToString("0.00").Replace(',', '.')}");      /// баланс отсылать через ".", пример 1.5 рубля
                        creditBalanceSlaveTaskRequestContent.Add("fund_action", "credit");
                        creditBalanceSlaveTaskRequestContent.Add("session", $"");
                        request.Content = new FormUrlEncodedContent(creditBalanceSlaveTaskRequestContent);

                        // request
                        responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                        content = await responce.Content.ReadAsStringAsync();

                        if (responce.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception($"responce.StatusCode != HttpStatusCode.OK --- 5.2 пополнение баланса задания --- запрос на пополнение баланса задания \n\n\n{content}");
                        }

                        dynamic? creditBalanceResult = JsonConvert.DeserializeObject(content);

                        string? c_b_status_r_status = creditBalanceResult!.status;
                        string? c_b_status_r_text = creditBalanceResult!.text;

                        if (creditBalanceResult == null || c_b_status_r_status == null || c_b_status_r_text == null)
                        {
                            throw new Exception($"creditBalanceResult == null || c_b_status_r_status == null || c_b_status_r_text == null --- 5.2 пополнение баланса задания --- запрос на пополнение баланса задания \n\n\n{content}");
                        }

                        if (c_b_status_r_status != "success" || c_b_status_r_text != "Пополнение выполнено успешно.")
                        {
                            if(c_b_status_r_text!.Contains("недостаточно средств"))
                            {
                                throw new Exception($"c_b_status_r_status != \"success\" || c_b_status_r_text != \"Пополнение выполнено успешно.\"\nвозможно на рекламном балансе на самом деле меньше значение чем показано в аккаунте. --- 5.2 пополнение баланса задания --- запрос на пополнение баланса задания \n\n\n{content}");
                            }

                            throw new Exception($"c_b_status_r_status != \"success\" || c_b_status_r_text != \"Пополнение выполнено успешно.\" --- 5.2 пополнение баланса задания --- запрос на пополнение баланса задания \n\n\n{content}");
                        }

                        base.TaskWithdrawMoney.Account.MoneyAdvertisingBalance = advert_balance - credit_sum;
                    }

                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(randomDelay.Next(5, 10)));
                }

                #endregion

                #region 5.3 проверка успешности пополнения баланса задания

                uri = $"{base.TaskWithdrawMoney!.Url}/account/task_adv_list.html";

                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    // set headers to request
                    foreach (var header in base.contextHeaders!)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }

                    if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                    if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                    request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/task_adv_add.html");

                    // request
                    responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                    if (responce.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception("responce.StatusCode != HttpStatusCode.OK --- 5.3 проверка успешности пополнения баланса задания");
                    }

                    content = await responce.Content.ReadAsStringAsync();
                }

                if (!content.Contains("У вас нет заданий"))
                {
                    htmlPageDocument = await base.htmlParseContext!.OpenAsync(req => req.Content(content));

                    var platform_tasks = htmlPageDocument.QuerySelectorAll("tbody.last-tr-green > tr").ToList();

                    if (platform_tasks.Count == 0)
                    {
                        throw new Exception($"platform_tasks.Count == 0 --- 5.3 проверка успешности пополнения баланса задания\n\n\n{content}");
                    }
                    //else if (platform_tasks.Count > 1)
                    //{
                    //    throw new Exception($"platform_tasks.Count > 1 --- 5.3 проверка успешности пополнения баланса задания\n\n\n{content}");
                    //}
                    else
                    {
                        elementPlatformTask = platform_tasks[0];
                    }

                    if (elementPlatformTask == null)
                    {
                        throw new Exception($"elementPlatformTask == null --- 5.3 проверка успешности пополнения баланса задания\n\n\n{content}");
                    }

                    int internal_platform_task_count_number_to_execute_by_main = elementPlatformTask.QuerySelector("td[data-label='Баланс'] > span.tooltips.cursor-help")!.TextContent.Replace("Осталось:", "").Trim().ToInteger(-1);

                    if (internal_platform_task_count_number_to_execute_by_main == -1)
                    {
                        throw new Exception($"internal_platform_task_count_number_to_execute_by_main == -1 --- 5.3 проверка успешности пополнения баланса задания\n\n\n{content}");
                    }
                    else if (internal_platform_task_count_number_to_execute_by_main == 0)
                    {
                        throw new Exception($"internal_platform_task_count_number_to_execute_by_main == 0 --- 5.3 проверка успешности пополнения баланса задания\n\n\n{content}");
                    }
                }
                else
                {
                    throw new Exception($"!content.Contains(\"У вас нет заданий\"), задание не было создано! --- 5.3 проверка успешности пополнения баланса задания\n\n\n{content}");
                }

                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(randomDelay.Next(5, 12)));

                #endregion
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "\n\n\nНеизвестная ошибка: 5-й шаг (по надобности): пополнение баланса задания");
            }

            #endregion

            #region 6-й шаг (по надобности): запуск задания

            try
            {
                //проверка статуса
                string internal_slave_task_status = elementPlatformTask.QuerySelector("td[data-label='Статус'] > div.margin-bottom-10 > span")!.TextContent;

                bool need_to_check_task_launch = true;

                if (!internal_slave_task_status.Contains("включено"))
                {
                    #region 6.1 запуск задания

                    queryParams = new Dictionary<string, string>()
                    {
                        ["act"] = $"yes",
                        ["id"] = $"{InternalSlavePlatformTask.InternalNumberId}",
                        ["page"] = $"1",
                        ["folder_id"] = $"0",
                        ["session"] = $"",
                    };

                    uri = QueryHelpers.AddQueryString($"{base.TaskWithdrawMoney!.Url}/account/task_adv_list.html", queryParams);

                    using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                    {
                        // set headers to request
                        foreach (var header in base.contextHeaders!)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }

                        if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                        if (request.Headers.Contains("origin")) request.Headers.Remove("origin");
                        if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                        request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/task_adv_list.html");

                        // request
                        responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                        content = await responce.Content.ReadAsStringAsync();

                        //if (responce.StatusCode != HttpStatusCode.OK && responce.StatusCode != HttpStatusCode.Found && responce.Headers.Location == null && !responce.Headers.Location!.ToString().Contains("task_adv_list.html"))
                        //{
                        //    throw new Exception($"responce.StatusCode != HttpStatusCode.OK && responce.StatusCode != HttpStatusCode.Found && responce.Headers.Location == null && !responce.Headers.Location!.ToString().Contains(\"task_adv_list.html\") --- 6.1 запуск задания \n\n\n{content}");
                        //}

                        if (responce.StatusCode == HttpStatusCode.OK && !content.Contains("У вас нет заданий"))
                        {
                            need_to_check_task_launch = false;

                            htmlPageDocument = await base.htmlParseContext!.OpenAsync(req => req.Content(content));

                            var platform_tasks = htmlPageDocument.QuerySelectorAll("tbody.last-tr-green > tr").ToList();

                            if (platform_tasks.Count == 0)
                            {
                                throw new Exception($"platform_tasks.Count == 0 --- 6.1 запуск задания \n\n\n{content}");
                            }
                            //else if (platform_tasks.Count > 1)
                            //{
                            //    throw new Exception($"platform_tasks.Count > 1 --- 6.1 запуск задания \n\n\n{content}");
                            //}
                            else
                            {
                                elementPlatformTask = platform_tasks[0];
                            }

                            if (elementPlatformTask == null)
                            {
                                throw new Exception($"elementPlatformTask == null --- 6.1 запуск задания \n\n\n{content}");
                            }

                            //проверка статуса
                            internal_slave_task_status = elementPlatformTask.QuerySelector("td[data-label='Статус'] > div.margin-bottom-10 > span")!.TextContent;

                            if (!internal_slave_task_status.Contains("включено"))
                            {
                                throw new Exception($"!internal_slave_task_status.Contains(\"включено\"), internal_slave_task_status == {internal_slave_task_status} --- 6.1 запуск задания \n\n\n{content}");
                            }
                        }
                        else if (responce.StatusCode == HttpStatusCode.OK && content.Contains("У вас нет заданий"))
                        {
                            throw new Exception($"responce.StatusCode == HttpStatusCode.OK && content.Contains(\"У вас нет заданий\") --- 6.1 запуск задания \n\n\n{content}");
                        }
                        else if (responce.StatusCode == HttpStatusCode.Found || responce.StatusCode == HttpStatusCode.Redirect)
                        {
                            need_to_check_task_launch = true;
                        }
                    }

                    #endregion 
                }
                else
                {
                    need_to_check_task_launch = false;
                }

                if (need_to_check_task_launch)
                {
                    #region 6.2 проверка успешности запуска задания

                    queryParams = new Dictionary<string, string>()
                    {
                        ["folder_id"] = $"0",
                        ["page"] = $"1"
                    };

                    uri = QueryHelpers.AddQueryString($"{base.TaskWithdrawMoney!.Url}/account/task_adv_list.html", queryParams);

                    using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                    {
                        // set headers to request
                        foreach (var header in base.contextHeaders!)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }

                        if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                        if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                        request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/task_adv_list.html");

                        // request
                        responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                        if (responce.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception($"responce.StatusCode != HttpStatusCode.OK --- 6.2 проверка успешности запуска задания\n\n\n{content}");
                        }

                        content = await responce.Content.ReadAsStringAsync();
                    }

                    if (!content.Contains("У вас нет заданий"))
                    {
                        htmlPageDocument = await base.htmlParseContext!.OpenAsync(req => req.Content(content));

                        var platform_tasks = htmlPageDocument.QuerySelectorAll("tbody.last-tr-green > tr").ToList();

                        if (platform_tasks.Count == 0)
                        {
                            throw new Exception($"platform_tasks.Count == 0 --- 6.2 проверка успешности запуска задания\n\n\n{content}");
                        }
                        //else if (platform_tasks.Count > 1)
                        //{
                        //    throw new Exception($"platform_tasks.Count > 1 --- 6.2 проверка успешности запуска задания\n\n\n{content}");
                        //}
                        else
                        {
                            elementPlatformTask = platform_tasks[0];
                        }

                        if (elementPlatformTask == null)
                        {
                            throw new Exception($"elementPlatformTask == null --- 6.2 проверка успешности запуска задания\n\n\n{content}");
                        }

                        //проверка статуса
                        internal_slave_task_status = elementPlatformTask.QuerySelector("td[data-label='Статус'] > div.margin-bottom-10 > span")!.TextContent;

                        if (!internal_slave_task_status.Contains("включено"))
                        {
                            throw new Exception($"!internal_slave_task_status.Contains(\"включено\"), internal_slave_task_status == {internal_slave_task_status} --- 6.2 проверка успешности запуска задания\n\n\n{content}");
                        }
                    }
                    else
                    {
                        throw new Exception($"!content.Contains(\"У вас нет заданий\"), задание не было создано! --- 6.2 проверка успешности запуска задания\n\n\n{content}");
                    }

                    #endregion
                }

                base.TaskWithdrawMoney.IsTaskCreatedOnPlatformBySlave = true;
                base.TaskWithdrawMoney.InternalStatus = WithdrawMoneyGroupSelectiveTaskWithAuth.WithdrawalInternalStatus.CompletedSuccessfullBySlave;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "\n\n\nНеизвестная ошибка: 6-й шаг: запуск задания"); ;
            }

            #endregion
        }
        private async Task SelectiveTaskWithdrawalOfMoneyByMain()
        {
            Random randomDelayinSecs = new Random();
            bool allWithdrawalTasksCompleted = false;
            WithdrawMoneyGroupSelectiveTaskWithAuth? withdrawMoneyTaskFromSlave = null;

            do
            {
                // получение задания
                try
                {
                    withdrawMoneyTaskFromSlave = await ServerHubConnection.InvokeAsync<WithdrawMoneyGroupSelectiveTaskWithAuth>("get_new_withdraw_task_to_execute_by_main", base.TaskWithdrawMoney!.Account!.Id, base.TaskWithdrawMoney!.Account.Login);
                }
                catch(Exception e)
                {
                    throw new Exception($"withdrawMoneyTaskFromSlave = await ServerHubConnection.InvokeAsync<WithdrawMoneyGroupSelectiveTaskWithAuth>... error\n\n\n{e.Message}");
                }

                if(withdrawMoneyTaskFromSlave != null)
                {
                    bool task_need_to_relaunch_by_slave = false;

                    try
                    {
                        #region проверка на балбеса
                        if (base.TaskWithdrawMoney!.Account!.Login == "dashashendel" && (withdrawMoneyTaskFromSlave.Account!.Refer!.Login == "dashashendel" || withdrawMoneyTaskFromSlave.Account.Refer.Login == "ikuc365" || withdrawMoneyTaskFromSlave.Account.IsMain == true))
                        {
                            // потому что нельзя чтобы мейн дашашендел выполнила задания своих же рефералов или задания рефералов икуса, только хромосомы, и так сделать проверки с другими мейнами ниже (+2 элсе ифа)
                            throw new Exception("// проверка на балбеса\r\n 1 (first if block)");
                        }
                        else if (base.TaskWithdrawMoney!.Account!.Login == "xpomocoma777" && (withdrawMoneyTaskFromSlave.Account!.Refer!.Login == "xpomocoma777" || withdrawMoneyTaskFromSlave.Account.Refer.Login == "dashashendel" || withdrawMoneyTaskFromSlave.Account.IsMain == true))
                        {
                            throw new Exception("// проверка на балбеса\\r\\n 2 (second if block)\"");
                        }
                        else if (base.TaskWithdrawMoney!.Account!.Login == "ikuc365" && (withdrawMoneyTaskFromSlave.Account!.Refer!.Login == "ikuc365" || withdrawMoneyTaskFromSlave.Account.Refer.Login == "xpomocoma777" || withdrawMoneyTaskFromSlave.Account.IsMain == true))
                        {
                            throw new Exception("// проверка на балбеса\\r\\n 3 (thrid if block)\"");
                        }
                        #endregion

                        // важная проверка на нужное поле
                        if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask!.AutoPayment.HasValue == false)
                        {
                            throw new Exception("withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.AutoPayment.HasValue == false");
                        }
                        else if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask!.AutoPayment.Value == true && String.IsNullOrEmpty(withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTaskCodePasswordForSolveTask))
                        {
                            throw new Exception("withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask!.AutoPayment.Value == true && String.IsNullOrEmpty(withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTaskCodePasswordForSolveTask)");
                        }
                        else if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask!.AutoPayment == false && String.IsNullOrEmpty(withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTaskTextForSolveTask))
                        {
                            throw new Exception("withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask!.AutoPayment == false && String.IsNullOrEmpty(withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTaskTextForSolveTask)");
                        }

                        // изменение статуса взятого задания на Executing
                        try
                        {
                            await ServerHubConnection.SendAsync("set_task_status_to_executing", withdrawMoneyTaskFromSlave.Id);
                        }
                        catch(Exception e)
                        {
                            throw new Exception($"изменение статуса взятого задания на Executing --- error, task id = {withdrawMoneyTaskFromSlave.Id}");
                        }

                        Dictionary<string, string>? queryParams;
                        string? uri = "";
                        HttpResponseMessage? responce = null;
                        string content = "";
                        IDocument? htmlPageDocument = null;

                        bool need_to_start_task = true;
                        bool need_to_check_successfully_completed_platform_task = true;

                        #region 1-й запрос: переход на задание слейва

                        CancellationTokenSource cancellationTokenGetSlaveTask = new CancellationTokenSource(TimeSpan.FromSeconds(90));

                        try
                        {
                            queryParams = new Dictionary<string, string>()
                            {
                                ["id"] = withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTaskId!
                            };

                            uri = QueryHelpers.AddQueryString($"{base.TaskWithdrawMoney!.Url}/account/task_view.html", queryParams);

                            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                            {
                                // set headers to request
                                foreach (var header in base.contextHeaders!)
                                {
                                    request.Headers.Add(header.Key, header.Value);
                                }

                                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                                request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/task.html");

                                // request
                                responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationTokenGetSlaveTask.Token);

                                content = await responce.Content.ReadAsStringAsync();

                                if (responce.StatusCode != HttpStatusCode.OK)
                                {
                                    throw new Exception($"responce.StatusCode != HttpStatusCode.OK --- 1-й запрос: переход на задание слейва \n\n\n{content}");
                                }
                                else if (!content.Contains("Начать выполнение")) // практически бесполезная проверка
                                {
                                    throw new Exception($"!htmlPage.Contains(\"Начать выполнение\") --- 1-й запрос: переход на задание слейва \n\n\n{content}");
                                }

                                if(content.Contains("Следующее выполнение доступно через"))
                                {
                                    throw new Exception($"content.Contains(\"Следующее выполнение доступно через\") --- 1-й запрос: переход на задание слейва\nЗадание уже было выполнено, нужно удалить из списка \n\n\n{content}");
                                }

                                // проверка на уже запущенное задание
                                htmlPageDocument = await base.htmlParseContext!.OpenAsync(req => req.Content(content));

                                string? reservedPriceTask = htmlPageDocument.QuerySelector("span[id=\"started_task_bid_price_user\"]")?.TextContent;

                                if(reservedPriceTask == null)
                                {
                                    throw new Exception($"(reservedPriceTask == null --- 1-й запрос: переход на задание слейва \n\n\n{content}");
                                }

                                double reservedPriceTaskDouble = 0.00; 
                                if(Double.TryParse(reservedPriceTask, NumberStyles.Number, CultureInfo.InvariantCulture, out reservedPriceTaskDouble)) // CultureInfo.InvariantCulture - важная фигня
                                {
                                    if(reservedPriceTaskDouble == withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTaskMoney && reservedPriceTaskDouble != 0.00)
                                    {
                                        need_to_start_task = false;

                                        if(withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTask_Bid_Id == null)
                                        {
                                            throw new Exception($"withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTask_Bid_Id == null (проверка на уже запущенное задание) --- --- 1-й запрос: переход на задание слейва \n\n\n{content}");
                                        }
                                    }
                                    else if(reservedPriceTaskDouble != withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTaskMoney && reservedPriceTaskDouble != 0.00)
                                    {
                                        throw new Exception($"reservedPriceTaskDouble != withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTaskMoney && reservedPriceTaskDouble != 0.00 --- 1-й запрос: переход на задание слейва \n\n\n{content}\n\nreserverPrice={reservedPriceTaskDouble}");
                                    }
                                    else if(reservedPriceTaskDouble != withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTaskMoney && reservedPriceTaskDouble == 0.00)
                                    {
                                        need_to_start_task = true;
                                    }
                                }
                            }

                            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(randomDelayinSecs.Next(9, 60)));
                        }
                        catch (Exception e)
                        {
                            throw new Exception("1-й запрос: переход на задание слейва\n\n\n" + e.Message + "\n\n\n" + JsonConvert.SerializeObject(e));
                        }

                        #endregion

                        if (need_to_start_task)
                        {
                            #region 2-й запрос (по надобности): начало выполнения заданий (нажатие кнопки "начать выполнение")

                            CancellationTokenSource cancellationTokenStartExecuteSlaveTask = new CancellationTokenSource(TimeSpan.FromSeconds(90));
                            CancellationTokenSource cancellationTokenReadContentExecuteSlaveTask = new CancellationTokenSource(TimeSpan.FromSeconds(90));

                            try
                            {
                                queryParams = new Dictionary<string, string>()
                                {
                                    ["act"] = "task_start"
                                };

                                uri = QueryHelpers.AddQueryString($"{base.TaskWithdrawMoney!.Url}/task.ajax", queryParams);

                                using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                                {
                                    // set headers to request
                                    foreach (var header in base.contextHeaders!)
                                    {
                                        request.Headers.Add(header.Key, header.Value);
                                    }

                                    if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                                    if (request.Headers.Contains("origin")) request.Headers.Remove("origin");
                                    if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                                    request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/task_view.html?id={withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTaskId!}");
                                    request.Headers.Add("origin", $"{base.TaskWithdrawMoney.Url}");

                                    // set content
                                    Dictionary<string, string> startExecuteSlaveTaskRequestContent = new Dictionary<string, string>();
                                    startExecuteSlaveTaskRequestContent.Add("id", $"{withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTaskId!}");
                                    startExecuteSlaveTaskRequestContent.Add("next_url_number", $"{0}");
                                    startExecuteSlaveTaskRequestContent.Add("session", "");
                                    request.Content = new FormUrlEncodedContent(startExecuteSlaveTaskRequestContent);

                                    // request
                                    responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationTokenStartExecuteSlaveTask.Token);

                                    content = await responce.Content.ReadAsStringAsync(cancellationTokenReadContentExecuteSlaveTask.Token);

                                    if (responce.StatusCode != HttpStatusCode.OK)
                                    {
                                        throw new Exception($"responce.StatusCode != HttpStatusCode.OK --- 2-й запрос: начало выполнения заданий (нажатие кнопки \"начать выполнение\") \n\n\n{content}");
                                    }

                                    //check status
                                    dynamic? getStartExecSlaveTaskResponceObj = JsonConvert.DeserializeObject(content);

                                    if (getStartExecSlaveTaskResponceObj == null || getStartExecSlaveTaskResponceObj!.status != "success")
                                    {
                                        if (getStartExecSlaveTaskResponceObj!.text == "Задание выключено.")
                                        {
                                            task_need_to_relaunch_by_slave = true;
                                        }

                                        throw new Exception($"getStartExecSlaveTaskResponceObj == null || getStartExecSlaveTaskResponceObj!.status != \"success\" --- 2-й запрос: начало выполнения заданий (нажатие кнопки \"начать выполнение\") \n\n\n status={getStartExecSlaveTaskResponceObj!.status}\nstatus_text={getStartExecSlaveTaskResponceObj!.text}\n\n\n{content}");
                                    }

                                    string? text_status = getStartExecSlaveTaskResponceObj!.text;

                                    if (text_status == null)
                                    {
                                        throw new Exception($"text_status == null --- 2-й запрос: начало выполнения заданий (нажатие кнопки \"начать выполнение\") \n\n\n {content}");
                                    }

                                    if (!text_status.Contains("можете выполнять задание"))
                                    {
                                        throw new Exception($"!text_status.Contains(\"можете выполнять задание\") --- 2-й запрос: начало выполнения заданий (нажатие кнопки \"начать выполнение\") \n\n\n {content}");
                                    }

                                    if (getStartExecSlaveTaskResponceObj!.bid == null || getStartExecSlaveTaskResponceObj!.bid.id == null)
                                    {
                                        throw new Exception($"getStartExecSlaveTaskResponceObj!.bid == null || getStartExecSlaveTaskResponceObj!.bid.id == null --- 2-й запрос: начало выполнения заданий (нажатие кнопки \"начать выполнение\") \n\n\n {content}");
                                    }

                                    string? PlatformTask_Bid_Id = (string)getStartExecSlaveTaskResponceObj!.bid.id;

                                    if (String.IsNullOrEmpty(PlatformTask_Bid_Id))
                                    {
                                        throw new Exception($"String.IsNullOrEmpty(PlatformTask_Bid_Id) --- 2-й запрос: начало выполнения заданий (нажатие кнопки \"начать выполнение\") \n\n\n {content}");
                                    }

                                    withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTask_Bid_Id = PlatformTask_Bid_Id;
                                }

                                int _5_MinutsInSecs = (int)TimeSpan.FromMinutes(5).TotalSeconds;
                                int _15_MinutsInSecs = (int)TimeSpan.FromMinutes(15).TotalSeconds;
                                int _30_MinutsInSecs = (int)TimeSpan.FromMinutes(30).TotalSeconds;
                                int _60_MinutsInSecs = (int)TimeSpan.FromMinutes(60).TotalSeconds;
                                int _180_MinutsInSecs = (int)TimeSpan.FromMinutes(180).TotalSeconds;
                                int _360_MinutsInSecs = (int)TimeSpan.FromMinutes(360).TotalSeconds;
                                int _1day_InSecs = (int)TimeSpan.FromDays(1).TotalSeconds;
                                int _2day_InSecs = (int)TimeSpan.FromDays(2).TotalSeconds;
                                int _3day_InSecs = (int)TimeSpan.FromDays(3).TotalSeconds;
                                int _5day_InSecs = (int)TimeSpan.FromDays(5).TotalSeconds;

                                int waitTime = randomDelayinSecs.Next(60, 300);

                                if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTaskExecuteMaxTimeInSecs == _5_MinutsInSecs)
                                    waitTime = randomDelayinSecs.Next(60, 120);
                                else if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTaskExecuteMaxTimeInSecs == _15_MinutsInSecs)
                                    waitTime = randomDelayinSecs.Next(70, 140);
                                else if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTaskExecuteMaxTimeInSecs == _30_MinutsInSecs)
                                    waitTime = randomDelayinSecs.Next(80, 160);
                                else if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTaskExecuteMaxTimeInSecs == _60_MinutsInSecs)
                                    waitTime = randomDelayinSecs.Next(90, 180);
                                else if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTaskExecuteMaxTimeInSecs == _180_MinutsInSecs)
                                    waitTime = randomDelayinSecs.Next(100, 200);
                                else if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTaskExecuteMaxTimeInSecs == _360_MinutsInSecs)
                                    waitTime = randomDelayinSecs.Next(110, 220);
                                else if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTaskExecuteMaxTimeInSecs == _1day_InSecs)
                                    waitTime = randomDelayinSecs.Next(120, 240);
                                else if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTaskExecuteMaxTimeInSecs == _2day_InSecs)
                                    waitTime = randomDelayinSecs.Next(130, 260);
                                else if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTaskExecuteMaxTimeInSecs == _3day_InSecs)
                                    waitTime = randomDelayinSecs.Next(140, 280);
                                else if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTaskExecuteMaxTimeInSecs == _5day_InSecs)
                                    waitTime = randomDelayinSecs.Next(150, 300);

                                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(waitTime));
                            }
                            catch (Exception e)
                            {
                                throw new Exception("2-й запрос: начало выполнения заданий (нажатие кнопки \"начать выполнение\")\n" + e.Message + "\n\n\n" + JsonConvert.SerializeObject(e));
                            }

                            #endregion
                        }

                        #region 3-й запрос: отправка результата выполнения задания (тоже имитирование нажатия кнопки)

                        CancellationTokenSource cancellationTokenSendResultExecutionSlaveTask = new CancellationTokenSource(TimeSpan.FromSeconds(90));
                        CancellationTokenSource cancellationTokenReadContentResultExecutionSlaveTask = new CancellationTokenSource(TimeSpan.FromSeconds(90));

                        try
                        {
                            queryParams = new Dictionary<string, string>()
                            {
                                ["id"] = $"{withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTaskId!}",
                                ["bid_id"] = $"{withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTask_Bid_Id!}",
                                ["act"] = "report"
                            };

                            uri = QueryHelpers.AddQueryString($"{base.TaskWithdrawMoney!.Url}/account/task_view.html", queryParams);

                            using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                            {
                                // set headers to request
                                foreach (var header in base.contextHeaders!)
                                {
                                    request.Headers.Add(header.Key, header.Value);
                                }

                                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                                if (request.Headers.Contains("origin")) request.Headers.Remove("origin");
                                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                                request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/task_view.html?id={withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTaskId!}");
                                request.Headers.Add("origin", $"{base.TaskWithdrawMoney.Url}");

                                // set content
                                Dictionary<string, string> sendResultExecuteSlaveTaskRequestContent = new Dictionary<string, string>();
                                if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.AutoPayment.Value == true)
                                {
                                    sendResultExecuteSlaveTaskRequestContent.Add("text", $"{withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTaskCodePasswordForSolveTask}");
                                }
                                else if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.AutoPayment.Value == false)
                                {
                                    sendResultExecuteSlaveTaskRequestContent.Add("file", $"");
                                    sendResultExecuteSlaveTaskRequestContent.Add("text", $"{withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTaskTextForSolveTask}");
                                }
                                sendResultExecuteSlaveTaskRequestContent.Add("files", $"");
                                sendResultExecuteSlaveTaskRequestContent.Add("report", "1");
                                request.Content = new FormUrlEncodedContent(sendResultExecuteSlaveTaskRequestContent);

                                // request
                                responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSendResultExecutionSlaveTask.Token);

                                content = await responce.Content.ReadAsStringAsync(cancellationTokenReadContentResultExecutionSlaveTask.Token);

                                if (responce.StatusCode == HttpStatusCode.OK)
                                {
                                    if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.AutoPayment.Value == true)
                                    {
                                        if (!content.Contains("Задание оплачено (автооплата)"))
                                        {
                                            throw new Exception($"!content.Contains(\"Задание оплачено (автооплата)\") --- 3-й запрос: отправка результата выполнения задания (тоже имитирование нажатия кнопки)\n\n\n{content}");
                                        }

                                        need_to_check_successfully_completed_platform_task = false;
                                    }
                                    else if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.AutoPayment.Value == false)
                                    {
                                        if (!content.Contains("Ваш отчёт сформирован и отправлен"))
                                        {
                                            throw new Exception($"!content.Contains(\"Ваш отчёт сформирован и отправлен\") --- 3-й запрос: отправка результата выполнения задания (тоже имитирование нажатия кнопки)\n\n\n{content}");
                                        }

                                        need_to_check_successfully_completed_platform_task = false;
                                    }
                                }
                                else if (responce.StatusCode == HttpStatusCode.Found || responce.StatusCode == HttpStatusCode.Redirect)
                                {
                                    need_to_check_successfully_completed_platform_task = true;
                                }
                                else
                                {
                                    throw new Exception($"Неизвестный responce.StatusCode --- {responce.StatusCode} --- 3-й запрос: отправка результата выполнения задания (тоже имитирование нажатия кнопки) \n\n\n{content}");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception("3-й запрос: отправка результата выполнения задания (тоже имитирование нажатия кнопки)\n" + e.Message + "\n\n\n" + JsonConvert.SerializeObject(e));
                        }

                        #endregion

                        if (need_to_check_successfully_completed_platform_task)
                        {
                            #region 4-й запрос: проверка успешности выполнения задания

                            CancellationTokenSource cancellationTokenCheckResultExecutionSlaveTask = new CancellationTokenSource(TimeSpan.FromSeconds(90));

                            try
                            {
                                queryParams = new Dictionary<string, string>()
                                {
                                    ["id"] = $"{withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTaskId!}",
                                    ["bid_id"] = $"{withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTask_Bid_Id!}"
                                };

                                uri = QueryHelpers.AddQueryString($"{base.TaskWithdrawMoney!.Url}/account/task_view.html", queryParams);

                                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                                {
                                    // set headers to request
                                    foreach (var header in base.contextHeaders!)
                                    {
                                        request.Headers.Add(header.Key, header.Value);
                                    }

                                    if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                                    if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                                    request.Headers.Add("referer", $"{base.TaskWithdrawMoney.Url}/account/task_view.html?id={withdrawMoneyTaskFromSlave!.WithdrawalOfMoneyPlatformTask!.PlatformTaskId!}");

                                    // request
                                    responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationTokenCheckResultExecutionSlaveTask.Token);

                                    content = await responce.Content.ReadAsStringAsync();

                                    // проверка успешности тут ниже
                                    if (responce.StatusCode != HttpStatusCode.OK)
                                    {
                                        throw new Exception($"responce.StatusCode != HttpStatusCode.OK --- 4-й запрос: проверка успешности выполнения задания \n\n\n{content}");
                                    }

                                    if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.AutoPayment.Value == true)
                                    {
                                        if (!content.Contains("Задание оплачено (автооплата)"))
                                        {
                                            throw new Exception($"!content.Contains(\"Задание оплачено (автооплата)\") --- 4-й запрос: проверка успешности выполнения задания \n\n\n{content}");
                                        }
                                    }
                                    else if (withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.AutoPayment.Value == false)
                                    {
                                        if (!content.Contains("Ваш отчёт сформирован и отправлен"))
                                        {
                                            throw new Exception($"!content.Contains(\"Ваш отчёт сформирован и отправлен\") --- 4-й запрос: проверка успешности выполнения задания \n\n\n{content}");
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                throw new Exception("4-й запрос: проверка успешности выполнения задания\n" + e.Message + "\n\n\n" + JsonConvert.SerializeObject(e));
                            }

                            #endregion 
                        }

                        withdrawMoneyTaskFromSlave.IsTaskCompletedOnPlatformByMain = true;
                        withdrawMoneyTaskFromSlave.InternalStatus = WithdrawMoneyGroupSelectiveTaskWithAuth.WithdrawalInternalStatus.CompletedSuccessfullyByMain;
                        withdrawMoneyTaskFromSlave.ResultStatus = TaskResultStatus.Success;

                        MoneyEarnedByMain += withdrawMoneyTaskFromSlave.WithdrawalOfMoneyPlatformTask.PlatformTaskMoney ?? 0;

                        await ServerHubConnection.SendAsync("platform_socpubliccom_withdrawal_task_status_changed", withdrawMoneyTaskFromSlave);
                    }
                    catch (Exception e)
                    {
                        // send slave task to db with this error
                        try
                        {
                            withdrawMoneyTaskFromSlave.ResultStatus = TaskResultStatus.Error;
                            withdrawMoneyTaskFromSlave.ErrorStatus = TaskErrorStatus.AnotherError;
                            withdrawMoneyTaskFromSlave.ErrorMessage = e.Message;

                            withdrawMoneyTaskFromSlave.IsTaskCompletedOnPlatformByMain = false;
                            if (task_need_to_relaunch_by_slave)
                            {
                                withdrawMoneyTaskFromSlave.InternalStatus = WithdrawMoneyGroupSelectiveTaskWithAuth.WithdrawalInternalStatus.CompletedWithErrorBySlave;
                            }
                            else
                            {
                                withdrawMoneyTaskFromSlave.InternalStatus = WithdrawMoneyGroupSelectiveTaskWithAuth.WithdrawalInternalStatus.CompletedWithErrorByMain;
                            }

                            await ServerHubConnection.SendAsync("platform_socpubliccom_withdrawal_task_status_changed", withdrawMoneyTaskFromSlave);
                        }
                        catch { }
                    }
                    finally
                    {
                        withdrawMoneyTaskFromSlave = null;

                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(randomDelayinSecs.Next(15, 81)));
                    }
                }
                else
                {
                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(30));

                    // check if tasks no more exist
                    try
                    {
                        allWithdrawalTasksCompleted = await ServerHubConnection.InvokeAsync<bool>("check_all_withdrawal_tasks_are_completed", base.TaskWithdrawMoney.Account.Id, base.TaskWithdrawMoney.Account.Login);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($" allWithdrawalTasksCompleted = await ServerHubConnection.InvokeAsync<bool>(\"check_all_withdrawal_tasks_are_completed\");... error\n\n\n{e.Message}");
                    }
                }
            }
            while (!allWithdrawalTasksCompleted);

            base.TaskWithdrawMoney.Account.MoneyMainBalance += MoneyEarnedByMain;
        }

        #endregion

        internal async Task TaskChangeResultStatus_SendServerReport(TaskResultStatus resultStatus, Exception? exception = null)
        {
            try
            {
                if (exception != null)
                {
                    TaskErrorStatus errorStatus = exception switch
                    {
                        System.Net.Http.HttpRequestException => TaskErrorStatus.ConnectionError,
                        System.Threading.Tasks.TaskCanceledException => TaskErrorStatus.ConnectionError,
                        _ => TaskErrorStatus.AnotherError
                    };

                    // дорабатывать костыль
                    if (errorStatus == TaskErrorStatus.AnotherError)
                    {
                        if (exception.Message.Contains("HttpClient.Timeout") ||
                            exception.Message.Contains("Connection timed out") ||
                            exception.Message.Contains("error occurred while sending") ||
                            exception.Message.Contains("Bad Proxy") ||
                            exception.Message.Contains("unsupported compression") ||
                            exception.Message.Contains("Попытка установить соединение") ||
                            exception.Message.Contains("while copying content") ||
                            exception.Message.Contains("Connection refused") ||
                            exception.Message.Contains("Forbidden") ||
                            exception.Message.Contains("request was aborted") ||
                            exception.Message.Contains("GiftBoxErrorCount == 3") ||
                            exception.Message.Contains("globalAttemptsToExecuteTasks == 0") ||
                            exception.Message.Contains("Connection reset") ||
                            exception.Message.Contains("InternalServerError"))
                        {
                            errorStatus = TaskErrorStatus.ConnectionError;
                        }
                    }

                    base.TaskWithdrawMoney!.ErrorStatus = errorStatus;
                    base.TaskWithdrawMoney.ErrorMessage = exception.Message;

                    base.TaskWithdrawMoney.IsTaskCreatedOnPlatformBySlave = false;
                    base.TaskWithdrawMoney.InternalStatus = WithdrawMoneyGroupSelectiveTaskWithAuth.WithdrawalInternalStatus.CompletedWithErrorBySlave;
                }

                base.TaskWithdrawMoney!.ResultStatus = resultStatus;

                if (base.cookieContainer != null)
                {
                    base.TaskWithdrawMoney.Account!.LastCookies = base.cookieContainer.GetAllCookies().ToList();
                }
                if (base.contextHeaders != null)
                {
                    base.TaskWithdrawMoney.Account!.LastHeaders = base.contextHeaders;
                }

                await ServerHubConnection.SendAsync("platform_socpubliccom_withdrawal_task_status_changed", base.TaskWithdrawMoney);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        private void UnRegisterHubMethods()
        {
            try
            {
                base.ServerHubConnection.Remove("checked_platform_account_history_proxy");
                base.ServerHubConnection.Remove("new_platform_account_proxy");
                base.ServerHubConnection.Remove("changed_status_solve_captcha_recaptchav2");
                base.ServerHubConnection.Remove("changed_status_solve_captcha_cloudflare_turnstile");
            }
            catch { }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты)
                    try
                    {
                        UnRegisterHubMethods();
                        base.Dispose_PlatformSocpublicComTask_Data();
                    }
                    catch { }
                }

                oneLogAccount = null;
                platformCaptchaReCaptchaV2 = null;
                secretPageCode = null;

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                disposedValue = true;
            }
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        // ~AutoregAccountTask()
        // {
        //     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
