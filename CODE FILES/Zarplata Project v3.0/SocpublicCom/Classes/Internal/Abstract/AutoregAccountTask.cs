using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using CommonModels.Captcha;
using CommonModels.Client;
using CommonModels.ProjectTask.EarningSite.SocpublicCom;
using CommonModels.ProjectTask.Platform;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MimeKit;
using Newtonsoft.Json;
using SocpublicCom.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using static System.Net.Mime.MediaTypeNames;
using HttpMethod = System.Net.Http.HttpMethod;

namespace SocpublicCom.Classes.Internal.Abstract
{
    internal class AutoregAccountTask : EarningSiteSocpublicComTask, IProxySetable, IDisposable
    {
        private bool disposedValue;

        public AutoregAccountTask(EarningSiteWorkBotClient client, SocpublicComAutoregTask task, HubConnection serverHubConnection, HttpClient serverHttpConnection) : base(client, serverHubConnection, serverHttpConnection, taskAutoreg: task) { }


        private protected string? registerConfirmationLink { get; private set; } = null;
        private protected CheckedProxy? checkedProxyForAccount { get; private set; } = null;
        private protected CheckedProxy? checkedProxyForEmail { get; private set; } = null;
        private protected ReCaptchaV2? platformCaptchaReCaptchaV2 { get; private set; } = null;
        private protected CloudflareTurnstile? platformCaptchaCloudflareTurnstile { get; private set; } = null;
        private protected string? secretPageCode { get; private set; } = null;
        private protected string? referId { get; private set; } = null;
        private HttpClient sitesClient { get; set; } = new HttpClient(new HttpClientHandler() { UseCookies = true, CookieContainer = new CookieContainer() });
        Random random = new Random();
        string login = "";

        internal override async Task StartWork()
        {
            try
            {
                RegisterHubMethods();
            }
            catch (Exception e)
            {
                throw new Exception($"class AutoregAccountTask(), method StartWork(), error {e.Message}");
            }

            try
            {
                FirstStep_InitWebContextData();
            }
            catch (Exception e)
            {
                throw new Exception($"class AutoregAccountTask(), method StartWork(), error {e.Message}");
            }

            try
            {
                await SecondStep_GetNewProxy();
            }
            catch (Exception e)
            {
                throw new Exception($"class AutoregAccountTask(), method StartWork(), error {e.Message}");
            }
        }

        private void RegisterHubMethods()
        {
            base.ServerHubConnection.On<CheckedProxy?>("new_platform_account_proxy", async (proxy) =>
            {
                try
                {
                    if(proxy != null)
                    {
                        checkedProxyForAccount = proxy;

                        ThirdStep_InitHttpClientDataWithProxy(proxy);

                        await GoToMainPageAndSolveCaptcha();
                    }
                    else
                    {
                        await IProxySetable.GetNewProxy(base.Client, null, base.ServerHubConnection);
                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class AutoregAccountTask(), method new_platform_account_proxy(), error {e.Message}")
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
                            new Exception($"class AutoregAccountTask(), method changed_status_solve_captcha_recaptchav2(), error {e.Message}")
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
                            new Exception($"class AutoregAccountTask(), method changed_status_solve_captcha_recaptchav2(), error {e.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
                            );
                        }
                        catch { }
                    }
                }

                if (captcha.status == CaptchaStatus.Solved)
                {
                    platformCaptchaReCaptchaV2!.status = CaptchaStatus.Solved;
                    platformCaptchaReCaptchaV2!.resolvedCaptchaHash = captcha.resolvedCaptchaHash;

                    await SixStep_RegisterAccount($"{base.TaskAutoreg!.Url}/auth_signup.html", $"{base.TaskAutoreg!.Url}/auth_signup.html").ContinueWith(async register =>
                    {
                        if (register.IsCompletedSuccessfully)
                        {
                            try
                            {
                                await ServerHubConnection.SendAsync("set_task_status_to_executing", base.TaskAutoreg!.Id);

                                //await SevenStep_GetNewEmailProxy();

                                await SevenStep_GetConfirmationLinkFromEmail();

                                await EightStep_ConfirmRegisterAccountByLinkFromEmail();

                                try
                                {
                                    await TaskChangeResultStatus_SendServerReport(TaskResultStatus.Success);
                                }
                                catch { }
                            }
                            catch (Exception e)
                            {
                                try
                                {
                                    await TaskChangeResultStatus_SendServerReport(
                                    TaskResultStatus.Error,
                                    new Exception($"class AutoregAccountTask(), method changed_status_solve_captcha_recaptchav2(), error {e.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
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
                                new Exception($"class AutoregAccountTask(), method changed_status_solve_captcha_recaptchav2(), error {register.Exception?.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
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
                        new Exception($"class AutoregAccountTask(), method changed_status_solve_captcha_recaptchav2(), error captcha.status == CaptchaStatus.Error\n\n\nCaptcha Info:\n{JsonConvert.SerializeObject(captcha)}")
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
                            new Exception($"class AutoregAccountTask(), method changed_status_solve_captcha_cloudflare_turnstile(), error {e.Message}")
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
                            new Exception($"class AutoregAccountTask(), method changed_status_solve_captcha_cloudflare_turnstile(), error {e.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
                            );
                        }
                        catch { }
                    }
                }

                if (captcha.status == CaptchaStatus.Solved)
                {
                    platformCaptchaCloudflareTurnstile!.status = CaptchaStatus.Solved;
                    platformCaptchaCloudflareTurnstile!.resolvedCaptchaHash = captcha.resolvedCaptchaHash;

                    await SixStep_RegisterAccount($"{base.TaskAutoreg!.Url}/auth_signup.html", $"{base.TaskAutoreg!.Url}/auth_signup.html").ContinueWith(async register =>
                    {
                        if (register.IsCompletedSuccessfully)
                        {
                            try
                            {
                                await ServerHubConnection.SendAsync("set_task_status_to_executing", base.TaskAutoreg!.Id);

                                //await SevenStep_GetNewEmailProxy();

                                await SevenStep_GetConfirmationLinkFromEmail();

                                await EightStep_ConfirmRegisterAccountByLinkFromEmail();

                                try
                                {
                                    await TaskChangeResultStatus_SendServerReport(TaskResultStatus.Success);
                                }
                                catch { }
                            }
                            catch (Exception e)
                            {
                                try
                                {
                                    await TaskChangeResultStatus_SendServerReport(
                                    TaskResultStatus.Error,
                                    new Exception($"class AutoregAccountTask(), method changed_status_solve_captcha_cloudflare_turnstile(), error {e.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
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
                                new Exception($"class AutoregAccountTask(), method changed_status_solve_captcha_cloudflare_turnstile(), error {register.Exception?.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
                                );
                            }
                            catch { }
                        }
                    });
                }

                if (captcha.status == CaptchaStatus.Error)
                {
                    platformCaptchaCloudflareTurnstile!.status = CaptchaStatus.Error;

                    try
                    {
                        await TaskChangeResultStatus_SendServerReport(
                        TaskResultStatus.Error,
                        new Exception($"class AutoregAccountTask(), method changed_status_solve_captcha_cloudflare_turnstile(), error captcha.status == CaptchaStatus.Error\n\n\nCaptcha Info:\n{JsonConvert.SerializeObject(captcha)}")
                        );
                    }
                    catch { }
                }
            });

            //base.ServerHubConnection.On<CheckedProxy>("new_email_proxy", async (proxy) =>
            //{
            //    try
            //    {
            //        checkedProxyForEmail = proxy;

            //        await EightStep_ConfirmRegisterAccountInEmail();
            //    }
            //    catch (Exception e)
            //    {
            //        try
            //        {
            //            await TaskChangeResultStatus_SendServerReport(
            //                TaskResultStatus.Error,
            //                new Exception($"class AutoregAccountTask(), method new_email_proxy(), error {e.Message}")
            //                );
            //        }
            //        catch { }
            //    }
            //});
        }
        private void FirstStep_InitWebContextData()
        {
            // init this data
            var init = base.InitWebContextData();
            if (init == false)
            {
                throw new Exception($"class AutoregAccountTask(), method FirstStep_InitWebContextData(), error init == false");
            }
        }
        private async Task SecondStep_GetNewProxy()
        {
            try
            {
                await ServerHubConnection.SendAsync("set_task_status_to_waiting_for_new_proxy", base.TaskAutoreg!.Id);

                await IProxySetable.GetNewProxy(base.Client, null, base.ServerHubConnection);
            }
            catch (Exception e)
            {
                throw new Exception($"class AutoregAccountTask(), method SecondStep_GetNewProxy(), error {e.Message}");
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
                throw new Exception($"class AutoregAccountTask(), method ThirdStep_InitHttpClientDataWithProxy(), error {e.Message}");
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
                throw new Exception($"class AutoregAccountTask(), method ThirdStep_InitHttpClientDataWithProxy(), error {e.Message}");
            }
        }
        private async Task GoToMainPageAndSolveCaptcha()
        {
            // сначала просто перейдем на главную страницу для того чтобы собрать куки и заголовки чтобы не показаться ботом
            bool mainPageIsOk = false;
            try
            {
                // get referers dump
                //string[] referers_dump = File.ReadAllLines(@"C:\Users\glebg\Desktop\Zarplata Project v3.0\referers_platforms.txt");
                string[] referers_dump = File.ReadAllLines(@"referers_platforms.txt");

                //mainPageIsOk = await GoToMainPage($"{base.TaskAutoreg!.Url}/", "https://www.google.com/");
                mainPageIsOk = await GoToMainPage($"{base.TaskAutoreg!.Url}/", $"{referers_dump[random.Next(0, referers_dump.Length)]}");
            }
            catch (Exception e)
            {
                throw new Exception($"class AutoregAccountTask(), method GoToMainPageAndSolveCaptcha(), error {e.Message}");
            }

            if (!mainPageIsOk)
            {
                throw new Exception($"class AutoregAccountTask(), method GoToMainPageAndSolveCaptcha(), error regPageIsOk={mainPageIsOk}");
            }

            try
            {
                await FiveStep_GetAndSolvePlatformCaptchaAndCheckLoginOnFree();
            }
            catch(Exception e)
            {
                throw new Exception($"class AutoregAccountTask(), method GoToMainPageAndSolveCaptcha(), error {e.Message}");
            }
        }
        private protected async Task<bool> GoToMainPage(string target_url, string referer)
        {
            CancellationTokenSource goToMainPageCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            // get referals dump
            //string[] ref_dump = File.ReadAllLines(@"C:\Users\glebg\Desktop\Zarplata Project v3.0\refers.txt");
            string[] ref_dump = File.ReadAllLines(@"refers.txt");

            // set query parameters
            //var query = new Dictionary<string, string>()
            //{
            //    ["i"] = "8458675",
            //    ["slide"] = "1"
            //};

            referId = ref_dump[random.Next(0, ref_dump.Length)];
            var query = new Dictionary<string, string>()
            {
                ["i"] = $"{referId}",
                ["slide"] = "1"
            };

            var uri = QueryHelpers.AddQueryString(target_url, query);

            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                // set headers to request
                foreach (var header in contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                //request.Headers.Add("referer", $"{referer}");

                var responce = await workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, goToMainPageCancellationTokenSource.Token);

                if (responce.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"{responce.StatusCode}\n\n{responce.Content}");
                }

                var htmlPage = await responce.Content.ReadAsStringAsync();
                var htmlPageDocument = await htmlParseContext!.OpenAsync(req => req.Content(htmlPage));

                bool mainPageIsOk = false;
                try
                {
                    var htmlMainPageButton = htmlPageDocument.QuerySelector("li.start-btn > a");

                    if (htmlMainPageButton != null && htmlMainPageButton.TextContent.Contains("Регистрация"))
                    {
                        mainPageIsOk = true;
                    }
                }
                finally
                {
                    responce.Dispose();
                    htmlPageDocument.Dispose();
                }

                return mainPageIsOk;
            }
        }
        private async Task FiveStep_GetAndSolvePlatformCaptchaAndCheckLoginOnFree()
        {
            // get captcha
            try
            {
                await GetCaptchaReCaptchaV2($"{base.TaskAutoreg.Url}/auth_signup.html", "https://socpublic.com/");
                //await GetCaptchaCloudflareTurnstile($"{base.TaskAutoreg.Url}/auth_signup.html", "https://socpublic.com/");
            }
            catch (Exception e)
            {
                throw new Exception($"class AutoregAccountTask(), method FiveStep_GetAndSolvePlatformCaptchaAndCheckLoginOnFree(), error {e.Message}");
            }

            bool loginIsValid = false;
            int attempts = 10;
            string data = "";
            try
            {
                // generate login
                while (attempts > 0 && !loginIsValid)
                {
                    //string login = base.TaskAutoreg!.Email.Address.Split('@')[0];

                    int random_num = random.Next(0, 1001);

                    if (random_num <= 200)
                    {
                        // get request random login
                        try
                        {
                            switch (random.Next(1, 4))
                            {
                                case 1:
                                    data = await sitesClient.GetStringAsync("https://randomuser.me/api/");
                                    login = JsonConvert.DeserializeObject<dynamic>(data)?.results[0].login.username!;
                                    break;
                                case 2:
                                    data = await sitesClient.GetStringAsync($"https://nordpass.com/api/v1/tools/username-generator/items/all-b2c/{random.Next(3, 12)}/?qty=1");
                                    login = JsonConvert.DeserializeObject<dynamic>(data)?.words[0]!;
                                    break;
                                case 3:
                                    string payload = "{\r\n    \"snr\": {\r\n        \"category\": 0,\r\n        \"UserName\": \"\",\r\n        \"Hobbies\": \"\",\r\n        \"ThingsILike\": \"\",\r\n        \"Numbers\": \"\",\r\n        \"WhatAreYouLike\": \"\",\r\n        \"Words\": \"\",\r\n        \"Stub\": \"username\",\r\n        \"LanguageCode\": \"en\",\r\n        \"NamesLanguageID\": \"45\",\r\n        \"Rhyming\": false,\r\n        \"OneWord\": true,\r\n        \"UseExactWords\": false,\r\n        \"ScreenNameStyleString\": \"Any\",\r\n        \"GenderAny\": false,\r\n        \"GenderMale\": false,\r\n        \"GenderFemale\": false\r\n    }\r\n}";
                                    data = await sitesClient.PostAsync($"https://www.spinxo.com/services/NameService.asmx/GetNames", new StringContent(payload, Encoding.UTF8, "application/json")).Result.Content.ReadAsStringAsync();
                                    login = JsonConvert.DeserializeObject<dynamic>(data)?.d.Names[random.Next(0, 30)]!;
                                    break;
                            }
                        }
                        catch (Exception)
                        {
                            attempts -= 1;
                            continue;
                        } 
                    }
                    else
                    {
                        // get existing random login
                        try
                        {
                            List<string> usernames_dump = File.ReadAllLines(@"usernames.txt").ToList();
                            login = usernames_dump[random.Next(0, usernames_dump.Count)];
                            usernames_dump.Remove(login);
                            await File.WriteAllLinesAsync(@"usernames.txt", usernames_dump);
                        }
                        catch (Exception)
                        {
                            attempts -= 1;
                            continue;
                        }
                    }

                    loginIsValid = await CheckLoginOnFree($"{base.TaskAutoreg!.Url}/misc.ajax", $"{base.TaskAutoreg!.Url}/auth_signup.html", login);

                    if (!loginIsValid)
                    {
                        attempts -= 1;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"class AutoregAccountTask(), method FiveStep_GetAndSolvePlatformCaptchaAndCheckLoginOnFree(), error {e.Message}");
            }

            if (!loginIsValid)
            {
                throw new Exception($"class AutoregAccountTask(), method FiveStep_GetAndSolvePlatformCaptchaAndCheckLoginOnFree(), error loginIsValid={loginIsValid}");
            }

            // solve captcha
            try
            {
                await ServerHubConnection.SendAsync("set_task_status_to_waiting_for_captcha", base.TaskAutoreg!.Id);

                await SolveCaptchaReCaptchaV2();
                //await SolveCaptchaCloudflareTurnstile();
            }
            catch (Exception e)
            {
                throw new Exception($"class AutoregAccountTask(), method FiveStep_GetAndSolvePlatformCaptchaAndCheckLoginOnFree(), error {e.Message}");
            }
        }
        private protected async Task GetCaptchaReCaptchaV2(string url, string referer)
        {
            CancellationTokenSource getCaptchaCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            string? htmlPage = null;
            IDocument? htmlPageDocument = null;

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                // set headers to request
                foreach (var header in contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                request.Headers.Add("referer", $"{referer}");

                var responce = await workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, getCaptchaCancellationTokenSource.Token);
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

            platformCaptchaReCaptchaV2 = new ReCaptchaV2(url, siteKey!, base.cookieContainer!.GetCookieHeader(new Uri(base.TaskAutoreg.Url)), base.contextUserAgent!.Useragent!, 8);
        }
        private protected async Task GetCaptchaCloudflareTurnstile(string url, string referer)
        {
            CancellationTokenSource getCaptchaCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            string? htmlPage = null;
            IDocument? htmlPageDocument = null;

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                // set headers to request
                foreach (var header in contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                request.Headers.Add("referer", $"{referer}");

                var responce = await workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, getCaptchaCancellationTokenSource.Token);
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
        private protected async Task<bool> CheckLoginOnFree(string target_url, string referer, string login)
        {
            CancellationTokenSource checkAccountLoginCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            // set query parameters
            var query = new Dictionary<string, string>()
            {
                ["act"] = "check_name"
            };

            var uri = QueryHelpers.AddQueryString(target_url, query);

            using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                // set headers to request
                foreach (var header in contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");
                if (request.Headers.Contains("origin")) request.Headers.Remove("origin");

                request.Headers.Add("referer", $"{referer}");
                request.Headers.Add("origin", $"https://socpublic.com");

                // set content
                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("term", login)
                });

                var responce = await workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, checkAccountLoginCancellationTokenSource.Token);

                if (responce.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"{responce.StatusCode}\n\n{responce.Content}");
                }

                string content = await responce.Content.ReadAsStringAsync();

                bool valid_login = false;

                //check status
                try
                {
                    dynamic? responceObj = JsonConvert.DeserializeObject(content);
                    if (responceObj != null && responceObj!.status != "fail")
                    {
                        valid_login = true;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    throw new Exception($"{responce.StatusCode}\n\n{responce.Content}");
                }
                finally
                {
                    responce.Dispose();
                }

                return valid_login;
            }
        }
        private protected async Task SolveCaptchaReCaptchaV2()
        {
            await ServerHubConnection.SendAsync("solve_captcha_recaptchav2", platformCaptchaReCaptchaV2);
        }
        private protected async Task SolveCaptchaCloudflareTurnstile()
        {
            await ServerHubConnection.SendAsync("solve_captcha_cloudflare_turnstile", platformCaptchaCloudflareTurnstile);
        }
        private protected async Task CheckStatusSolveCaptchaReCaptchaV2()
        {
            await ServerHubConnection.SendAsync("check_status_solve_captcha_recaptchav2", platformCaptchaReCaptchaV2);
        }
        private protected async Task CheckStatusSolveCaptchaCloudflareTurnstile()
        {
            await ServerHubConnection.SendAsync("check_status_solve_captcha_cloudflare_turnstile", platformCaptchaCloudflareTurnstile);
        }
        private async Task SixStep_RegisterAccount(string url, string referer)
        {
            CancellationTokenSource registerAccountCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            string? htmlPage = null;
            IDocument? htmlPageDocument = null;

            try
            {
                TaskAutoreg!.Account = new Account()
                {
                    //Login = base.TaskAutoreg!.Email.Address.Split('@')[0],
                    Login = login,
                    EmailIsConfirmed = false,
                    Email = base.TaskAutoreg!.Email,
                    //Password = base.TaskAutoreg!.Email.Address.Split('@')[0] + "Botik24",
                    Password = base.TaskAutoreg!.Email.Address.Split('@')[0] + "btk24",
                    IsAlive = true,
                    IsMain = false,
                    MoneyMainBalance = 0,
                    MoneyAdvertisingBalance = 0,
                    //Refer = new Referal()
                    //{
                    //    Id = "8458675",
                    //    Email = "golubov.gleb@mail.ru",
                    //    Login = "xpomocoma777"
                    //},
                    Refer = new Referal()
                    {
                        Id = $"{referId}",
                        Email = null,
                        Login = null,
                    },
                    RegedDateTime = DateTime.UtcNow,
                    RegedProxy = checkedProxyForAccount!,
                    RegedUseragent = base.contextUserAgent,
                    RegedHeaders = base.contextHeaders,
                    LastHeaders = base.contextHeaders,
                    LastCookies = base.cookieContainer!.GetAllCookies().ToList(),
                    History = new List<OneLogAccounResult>()
                };

                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    // set headers to request
                    foreach (var header in contextHeaders!)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }

                    if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                    if (request.Headers.Contains("referer")) request.Headers.Remove("referer");
                    if (request.Headers.Contains("origin")) request.Headers.Remove("origin");

                    request.Headers.Add("referer", $"{referer}");
                    request.Headers.Add("origin", "https://socpublic.com");

                    Dictionary<string, string> requestContent = new Dictionary<string, string>();
                    requestContent.Add("name", TaskAutoreg.Account!.Login!);
                    requestContent.Add("email", TaskAutoreg.Account!.Email!.Address);
                    requestContent.Add("password", TaskAutoreg.Account!.Password!);
                    requestContent.Add("password_repeat", TaskAutoreg.Account!.Password!);
                    //requestContent.Add("cf-turnstile-response", platformCaptchaCloudflareTurnstile!.resolvedCaptchaHash!);
                    requestContent.Add("g-recaptcha-response", platformCaptchaReCaptchaV2!.resolvedCaptchaHash!);
                    //requestContent.Add("secret", secretPageCode!);

                    request.Content = new FormUrlEncodedContent(requestContent);

                    var responce = await workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, registerAccountCancellationTokenSource.Token);

                    htmlPage = await responce.Content.ReadAsStringAsync();

                    if (responce.StatusCode != HttpStatusCode.Redirect && responce.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"{responce.StatusCode}\n\n{htmlPage}");
                    }

                    // удостовериться что регистрация прошла успешно

                    htmlPageDocument = await htmlParseContext!.OpenAsync(req => req.Content(htmlPage));

                    var success_reg = htmlPageDocument.QuerySelector("div.col-md-12 > h2");
                    if(success_reg == null || !success_reg.TextContent.Contains("успешно"))
                    {
                        throw new Exception($"{responce.StatusCode}\n\n{htmlPage}");
                    }

                    var creditionals_trs = htmlPageDocument.QuerySelectorAll("div.col-md-12 > table > tbody > tr");
                    if(creditionals_trs == null || creditionals_trs.Count() != 3)
                    {
                        throw new Exception($"{responce.StatusCode}\n\n{htmlPage}");
                    }

                    var pin_code_tds = creditionals_trs[2]!.QuerySelectorAll("td");
                    if (pin_code_tds == null || pin_code_tds.Count() != 2)
                    {
                        throw new Exception($"{responce.StatusCode}\n\n{htmlPage}");
                    }

                    var pin_code = pin_code_tds[1]!.TextContent;
                    if(pin_code == null || String.IsNullOrEmpty(pin_code))
                    {
                        throw new Exception($"{responce.StatusCode}\n\n{htmlPage}");
                    }

                    TaskAutoreg!.Account.Pincode = pin_code;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"class AutoregAccountTask(), method SixStep_RegisterAccount(), error {e.Message}");
            }
        }
        //private async Task SevenStep_GetNewEmailProxy()
        //{
        //    try
        //    {
        //        CheckRequirements checkRequirements = new CheckRequirements(
        //                    EarningSiteTaskUrls.SocpublicComUrl.http,
        //                    new CheckedProxy.CPProtocol[] { CheckedProxy.CPProtocol.imap },
        //                    new CheckedProxy.SupportsSecureConnectionState[] { CheckedProxy.SupportsSecureConnectionState.Support, CheckedProxy.SupportsSecureConnectionState.NotSupport, CheckedProxy.SupportsSecureConnectionState.Undefined },
        //                    new CheckedProxy.AnonymousState[] { CheckedProxy.AnonymousState.Anonymous },
        //                    5000,
        //                    new CheckedProxy.CPWorkState[] { CheckedProxy.CPWorkState.Work }
        //                    );

        //        //await serverHubConnection.InvokeAsync("get_email_new_proxy");
        //        await base.ServerHubConnection.SendAsync("get_email_new_proxy", base.Client, null, checkRequirements);
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception($"class AutoregAccountTask(), method SevenStep_GetNewEmailProxy(), error {e.Message}");
        //    }
        //}
        private async Task SevenStep_GetConfirmationLinkFromEmail()
        {
            try
            {
                // get mail and parse link

                string HtmlTextFromEmail = "";

                using (var imapClient = new ImapClient())
                {
                    //imapClient.CheckCertificateRevocation = false;
                    //imapClient.ProxyClient = new Socks5Client("194.4.50.92", 12334);

                    using(CancellationTokenSource imapConnectCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                    {
                        await imapClient.ConnectAsync("imap.firstmail.ltd", 993, SecureSocketOptions.SslOnConnect, imapConnectCancellationToken.Token);

                        using(CancellationTokenSource imapAuthenticateCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                        {
                            await imapClient.AuthenticateAsync(base.TaskAutoreg!.Email.Address, base.TaskAutoreg!.Email.Password, imapAuthenticateCancellationToken.Token);

                            using(CancellationTokenSource imapInboxOpenCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                            {
                                await imapClient.Inbox.OpenAsync(FolderAccess.ReadOnly, imapInboxOpenCancellationToken.Token);

                                using(CancellationTokenSource imapInboxSearchCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                                {
                                    var uids = await imapClient.Inbox.SearchAsync(SearchOptions.All, SearchQuery.SubjectContains("socpublic"), imapInboxSearchCancellationToken.Token);

                                    if (uids.Count == 0)
                                        throw new Exception("uids.Count == 0");

                                    foreach (var uid in uids.UniqueIds)
                                    {
                                        using(CancellationTokenSource imapInboxGetMessageCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                                        {
                                            var mimeMessage = await imapClient.Inbox.GetMessageAsync(uid, imapInboxGetMessageCancellationToken.Token);

                                            if (mimeMessage != null)
                                            {
                                                var htmlBodyPart = mimeMessage.GetTextBody(MimeKit.Text.TextFormat.Html);

                                                if (htmlBodyPart != null && htmlBodyPart.Contains("Поздравляем! Вы зарегистрированы!"))
                                                {
                                                    HtmlTextFromEmail = htmlBodyPart;

                                                    //remove after
                                                    //try
                                                    //{
                                                    //    await File.WriteAllTextAsync($@"C:\Users\glebg\Desktop\{base.TaskAutoreg!.Account!.Login}.txt", HtmlTextFromEmail);
                                                    //}
                                                    //catch { }

                                                    break;
                                                } 
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    await imapClient.Inbox.CloseAsync(false);
                    await imapClient.DisconnectAsync(true);
                }

                // parse link

                var htmlPageDocument = await htmlParseContext!.OpenAsync(req => req.Content(HtmlTextFromEmail));

                try
                {
                    var emailLinks = htmlPageDocument.Links;

                    if (emailLinks != null && emailLinks.Count() > 0)
                    {
                        foreach (var link in emailLinks)
                        {
                            var href = link.GetAttribute("href");

                            if(href != null && href.Contains("auth_email"))
                            {
                                registerConfirmationLink = href;
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    htmlPageDocument.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new Exception($"class AutoregAccountTask(), method SevenStep_GetConfirmationLinkFromEmail(), error {e.Message}");
            }
        }
        private async Task EightStep_ConfirmRegisterAccountByLinkFromEmail()
        {
            CancellationTokenSource confirmRegisterAccountCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            try
            {
                // confirm

                if(registerConfirmationLink == null)
                {
                    throw new Exception("registerConfirmationLink == null");
                }

                // correct confirmation link from https to http
                registerConfirmationLink = registerConfirmationLink.Replace("https", "http");

                // request to confirm

                using (var request = new HttpRequestMessage(HttpMethod.Get, registerConfirmationLink))
                {
                    // set headers to request
                    foreach (var header in contextHeaders!)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }

                    if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                    if (request.Headers.Contains("referer")) request.Headers.Remove("referer");
                    if (request.Headers.Contains("origin")) request.Headers.Remove("origin");

                    var responce = await workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, confirmRegisterAccountCancellationTokenSource.Token);

                    if (responce.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"{responce.StatusCode}\n\n{responce.Content}");
                    }

                    string content = await responce.Content.ReadAsStringAsync();

                    //check status
                    var htmlPageDocument = await htmlParseContext!.OpenAsync(req => req.Content(content));

                    try
                    {
                        var statusPanel = htmlPageDocument.QuerySelector("div.note.note-success");
                        
                        if(statusPanel != null && statusPanel.TextContent.Contains("Ваш email адрес успешно подтверждён."))
                        {
                            TaskAutoreg!.Account!.EmailIsConfirmed = true;
                        }
                        else
                        {
                            TaskAutoreg!.Account!.EmailIsConfirmed = false;
                            throw new Exception($"TaskAutoreg!.Account!.EmailIsConfirmed = false;\n{statusPanel?.TextContent}");
                        }
                    }
                    catch(Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        htmlPageDocument.Dispose();
                        responce.Dispose();
                    }
                }

            }
            catch (Exception e)
            {
                throw new Exception($"class AutoregAccountTask(), method EightStep_ConfirmRegisterAccountByLinkFromEmail(), error {e.Message}");
            }
        }


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

                    base.TaskAutoreg!.ErrorStatus = errorStatus;
                    base.TaskAutoreg.ErrorMessage = exception.Message;
                }

                base.TaskAutoreg!.ResultStatus = resultStatus;

                await ServerHubConnection.SendAsync("platform_socpubliccom_autoreg_task_status_changed", base.TaskAutoreg);
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
                base.ServerHubConnection.Remove("new_platform_account_proxy");
                base.ServerHubConnection.Remove("changed_status_solve_captcha_recaptchav2");
                base.ServerHubConnection.Remove("changed_status_solve_captcha_cloudflare_turnstile");
                //base.ServerHubConnection.Remove("new_email_proxy");
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

                registerConfirmationLink = null;
                checkedProxyForAccount = null;
                checkedProxyForEmail = null;
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
