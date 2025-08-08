using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Text;
using CommonModels.Captcha;
using CommonModels.Client;
using CommonModels.ProjectTask.EarningSite.SocpublicCom;
using CommonModels.ProjectTask.Platform.SocpublicCom;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth.SelectiveTask.TaskVisitWithoutTimer.Models;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.SelectiveTaskWithAuth;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MailKit;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.WebUtilities;
using SocpublicCom.Classes.Internal.Abstract;
using SocpublicCom.Interfaces;
using System.Net;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using CommonModels.ProjectTask.ProxyCombiner.Models;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Globalization;

namespace SocpublicCom.Classes.Internal
{
    internal class SelectiveGroupTaskWithAuth : WithAccountAuthTask, IProxySetable, IDisposable
    {
        string? registerConfirmationLink { get; set; } = null;
        string? avatar_url { get; set; } = null;
        byte[]? avatar { get; set; } = null;

        private bool disposedValue;

        public SelectiveGroupTaskWithAuth(EarningSiteWorkBotClient client, SocpublicComTask task, HubConnection serverHubConnection, HttpClient serverHttpConnection) : base(client, task, serverHubConnection, serverHttpConnection)
        {
        }

        internal override async Task StartWork()
        {
            if (!EmailConfirmed() && (base.Task as GroupSelectiveTaskWithAuth)!.InternalSubtype.Contains(SocpublicComTaskEnums.SocpublicComTaskSubtype.ConfirmRegisterAccountByLinkFromEmail))
            {
                // get confirm link

                try
                {
                    await GetConfirmationLinkFromEmail();
                }
                catch(Exception e)
                {
                    //await TaskChangeResultStatus_SendServerReport(TaskResultStatus.Error, new Exception($"class TaskVisitWithoutTimer(), method StartWork(), method EmailConfirmed(), error {e.Message}"));
                }
            }
            else if (!EmailConfirmed())
            {
                //await TaskChangeResultStatus_SendServerReport(TaskResultStatus.Error, new Exception($"class TaskVisitWithoutTimer(), method StartWork(), error EmailConfirmed==false"));
            }

            //if (IsFirstWorkOfAccountAfterRegistration())
            //{
            //    throw new Exception($"class TaskVisitWithoutTimer(), method StartWork(), error IsFirstWorkOfAccountAfterRegistration==true");
            //}

            // clear old history onelogaccount
            try
            {
                if (base.Task!.Account!.History!.Count() > 5)
                {
                    base.Task!.Account!.History!.RemoveRange(0, (base.Task!.Account!.History!.Count() - 5));
                }

                base.Task!.Account!.History!.RemoveAll(onelog_acc => onelog_acc == null || onelog_acc.Proxy == null);
            }
            catch (Exception e)
            {
                throw new Exception($"class TaskVisitWithoutTimer(), method StartWork(), clear old history onelogaccount, error {e.Message}");
            }

            // load avatar if needed
            try
            {
                if((base.Task as GroupSelectiveTaskWithAuth)!.InternalSubtype.Contains(SocpublicComTaskEnums.SocpublicComTaskSubtype.InstallAvatar) && (base.Task!.Account!.Anketa == null || base.Task!.Account!.Anketa.AvatarInstalled.HasValue == false || base.Task!.Account!.Anketa.AvatarInstalled.Value == false))
                {
                    avatar = await base.ServerHttpConnection.GetByteArrayAsync($"{base.Client.SERVER_HOST}/avatar/{base.Task.Account.Login}.jpg");

                    if (avatar == null || avatar.Length == 0)
                    {
                        throw new Exception("avatar == null || avatar.Length == 0");
                    }

                    avatar_url = $"{base.Client.SERVER_HOST}/avatar/{base.Task.Account.Login}.jpg";
                }
            }
            catch(Exception e)
            {
                //throw new Exception($"class TaskVisitWithoutTimer(), method StartWork(), load avatar if needed, error {e.Message}");
            }

            try
            {
                RegisterHubMethods();
            }
            catch(Exception e)
            {
                throw new Exception($"class TaskVisitWithoutTimer(), method StartWork(), error {e.Message}");
            }

            try
            {
                FirstStep_InitWebContextData();
            }
            catch (Exception e)
            {
                throw new Exception($"class TaskVisitWithoutTimer(), method StartWork(), error {e.Message}");
            }

            try
            {
                if (base.Task!.Account!.IsMain.HasValue && base.Task!.Account.IsMain.Value && base.Task!.Account!.PersonalPrivateProxy != null)
                {
                    try
                    {
                        ThirdStep_InitHttpClientDataWithPersonalProxy(base.Task.Account.PersonalPrivateProxy);

                        await ChooseBetweenSteps_FiveStep_or_SevenStep();
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            await TaskChangeResultStatus_SendServerReport(
                                TaskResultStatus.Error,
                                new Exception($"class TaskVisitWithoutTimer(), method StartWork(), error {e.Message}")
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
                throw new Exception($"class TaskVisitWithoutTimer(), method StartWork(), error {e.Message}");
            }
        }

        private void RegisterHubMethods()
        {
            base.ServerHubConnection.On<List<CheckedProxy>>("checked_platform_account_history_proxy", async (proxies) =>
            {
                CheckedProxy? bestWorkProxy = null;

                try
                {
                    bestWorkProxy = IProxySetable.CheckCheckedProxy(proxies, base.Task!.Account!.History!, base.Task!.Account);
                }
                catch(Exception e)
                {
                    try
                    {
                        await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error, 
                            new Exception($"class TaskVisitWithoutTimer(), method checked_platform_account_history_proxy(), error {e.Message}")
                            );
                    }
                    catch { }
                }

                if(bestWorkProxy == null)
                {
                    try
                    {
                        await ServerHubConnection.SendAsync("set_task_status_to_waiting_for_new_proxy", base.Task!.Id);

                        await IProxySetable.GetNewProxy(base.Client, base.Task.Account!, base.ServerHubConnection);
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class TaskVisitWithoutTimer(), method checked_platform_account_history_proxy(), error {e.Message}")
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
                    catch(Exception e)
                    {
                        try
                        {
                            await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class TaskVisitWithoutTimer(), method checked_platform_account_history_proxy(), error {e.Message}")
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
                    if(proxy != null)
                    {
                        ThirdStep_InitHttpClientDataWithProxy(proxy);

                        await ChooseBetweenSteps_FiveStep_or_SevenStep();
                    }
                    else
                    {
                        await IProxySetable.GetNewProxy(base.Client, base.Task.Account!, base.ServerHubConnection);
                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class TaskVisitWithoutTimer(), method new_platform_account_proxy(), error {e.Message}")
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

                        await base.CheckStatusSolveCaptchaReCaptchaV2();
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class TaskVisitWithoutTimer(), method changed_status_solve_captcha_recaptchav2(), block 'captcha.status == CaptchaStatus.Created', error {e.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}\n\nLocal captcha object info:\n{JsonConvert.SerializeObject(platformCaptchaReCaptchaV2)}")
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

                        await base.CheckStatusSolveCaptchaReCaptchaV2();

                        platformCaptchaReCaptchaV2.waitTime += 1;
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class TaskVisitWithoutTimer(), method changed_status_solve_captcha_recaptchav2(), block 'captcha.status == CaptchaStatus.inProgress', error {e.Message}\n\n\nCaptcha Info:\n{JsonConvert.SerializeObject(captcha)}")
                            );
                        }
                        catch { }
                    }
                }

                if (captcha.status == CaptchaStatus.Solved)
                {
                    try
                    {
                        await ServerHubConnection.SendAsync("set_task_status_to_executing", base.Task!.Id);
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
                            catch(Exception e)
                            {
                                try
                                {
                                    await TaskChangeResultStatus_SendServerReport(
                                    TaskResultStatus.Error,
                                    new Exception($"class TaskVisitWithoutTimer(), method changed_status_solve_captcha_recaptchav2(), block 'captcha.status == CaptchaStatus.Solved', error {e.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
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
                                new Exception($"class TaskVisitWithoutTimer(), method changed_status_solve_captcha_recaptchav2(), error {login.Exception?.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
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
                        new Exception($"class TaskVisitWithoutTimer(), method changed_status_solve_captcha_recaptchav2(), block 'captcha.status == CaptchaStatus.Error', error captcha.status == CaptchaStatus.Error\n\n\nCaptcha Info:\n{JsonConvert.SerializeObject(captcha)}")
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

                        await base.CheckStatusSolveCaptchaCloudflareTurnstile();
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class TaskVisitWithoutTimer(), method changed_status_solve_captcha_cloudflare_turnstile(), block 'captcha.status == CaptchaStatus.Created', error {e.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}\n\nLocal captcha object info:\n{JsonConvert.SerializeObject(platformCaptchaCloudflareTurnstile)}")
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

                        await base.CheckStatusSolveCaptchaCloudflareTurnstile();

                        platformCaptchaCloudflareTurnstile.waitTime += 1;
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            await TaskChangeResultStatus_SendServerReport(
                            TaskResultStatus.Error,
                            new Exception($"class TaskVisitWithoutTimer(), method changed_status_solve_captcha_cloudflare_turnstile(), block 'captcha.status == CaptchaStatus.inProgress', error {e.Message}\n\n\nCaptcha Info:\n{JsonConvert.SerializeObject(captcha)}")
                            );
                        }
                        catch { }
                    }
                }

                if (captcha.status == CaptchaStatus.Solved)
                {
                    try
                    {
                        await ServerHubConnection.SendAsync("set_task_status_to_executing", base.Task!.Id);
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
                                    new Exception($"class TaskVisitWithoutTimer(), method changed_status_solve_captcha_cloudflare_turnstile(), block 'captcha.status == CaptchaStatus.Solved', error {e.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
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
                                new Exception($"class TaskVisitWithoutTimer(), method changed_status_solve_captcha_cloudflare_turnstile(), error {login.Exception?.Message}\n\n\nCaptcha Info:\n{captcha.status}\n{captcha.statusMessage}")
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
                        new Exception($"class TaskVisitWithoutTimer(), method changed_status_solve_captcha_cloudflare_turnstile(), block 'captcha.status == CaptchaStatus.Error', error captcha.status == CaptchaStatus.Error\n\n\nCaptcha Info:\n{JsonConvert.SerializeObject(captcha)}")
                        );
                    }
                    catch { }
                }
            });
        }
        private bool EmailConfirmed()
        {
            if (base.Task!.Account!.EmailIsConfirmed.HasValue && base.Task.Account!.EmailIsConfirmed.Value)
            {
                return true;
            }

            return false;
        }
        private async Task GetConfirmationLinkFromEmail()
        {
            try
            {
                // get mail and parse link

                string HtmlTextFromEmail = "";

                using (var imapClient = new ImapClient())
                {
                    //imapClient.CheckCertificateRevocation = false;
                    //imapClient.ProxyClient = new Socks5Client("194.4.50.92", 12334);

                    using (CancellationTokenSource imapConnectCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                    {
                        await imapClient.ConnectAsync("imap.firstmail.ltd", 993, SecureSocketOptions.SslOnConnect, imapConnectCancellationToken.Token);

                        using (CancellationTokenSource imapAuthenticateCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                        {
                            await imapClient.AuthenticateAsync(base.Task!.Account!.Email!.Address, base.Task!.Account!.Email!.Password, imapAuthenticateCancellationToken.Token);

                            using (CancellationTokenSource imapInboxOpenCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                            {
                                await imapClient.Inbox.OpenAsync(FolderAccess.ReadOnly, imapInboxOpenCancellationToken.Token);

                                using (CancellationTokenSource imapInboxSearchCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                                {
                                    var uids = await imapClient.Inbox.SearchAsync(SearchOptions.All, SearchQuery.SubjectContains("socpublic"), imapInboxSearchCancellationToken.Token);

                                    if (uids.Count == 0)
                                        throw new Exception("uids.Count == 0");

                                    foreach (var uid in uids.UniqueIds)
                                    {
                                        using (CancellationTokenSource imapInboxGetMessageCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                                        {
                                            var mimeMessage = await imapClient.Inbox.GetMessageAsync(uid, imapInboxGetMessageCancellationToken.Token);

                                            if (mimeMessage != null)
                                            {
                                                var htmlBodyPart = mimeMessage.GetTextBody(MimeKit.Text.TextFormat.Html);

                                                if (htmlBodyPart != null && htmlBodyPart.Contains("Поздравляем! Вы зарегистрированы!"))
                                                {
                                                    HtmlTextFromEmail = htmlBodyPart;
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

                            if (href != null && href.Contains("auth_email"))
                            {
                                registerConfirmationLink = href;
                                break;
                            }
                        }
                    }

                    //parse pincode
                    var selector = htmlPageDocument.QuerySelector("div.textdark");

                    string text = selector!.TextContent;
                    string keyElementForIndexOne = "Вы можете сразу";
                    string keyElementForIndexTwo = "ПИН код:";

                    int index_to_remove_one = text.LastIndexOf(keyElementForIndexOne);
                    text = text.Remove(index_to_remove_one);

                    int index_to_remove_two = text.LastIndexOf(keyElementForIndexTwo);
                    text = text.Remove(0, index_to_remove_two + keyElementForIndexTwo.Length);

                    text = text.ReplaceLineEndings().Trim();

                    if (!String.IsNullOrEmpty(text) && String.IsNullOrEmpty(base.Task.Account.Pincode))
                    {
                        base.Task.Account.Pincode = text;
                    }
                }
                finally
                {
                    htmlPageDocument.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new Exception($"class SelectiveGroupTaskWithAuth(), method GetConfirmationLinkFromEmail(), error {e.Message}");
            }
        }
        private bool IsFirstWorkOfAccountAfterRegistration()
        {
            if (base.Task.Account!.IsFirstExecutionOfTasksAfterRegistration)
            {
                return true;
            }

            return false;
        }
        private void FirstStep_InitWebContextData()
        {
            // init this data
            var init = base.InitWebContextData();
            if(init == false)
            {
                throw new Exception($"class TaskVisitWithoutTimer(), method FirstStep_InitWebContextData(), error init == false");
            }
        }
        private async Task SecondStep_CheckPlatformAccountHistoryProxy_Or_GetNewProxy()
        {
            bool NeedToGetNewProxy = true;

            try
            {
                NeedToGetNewProxy = await IProxySetable.CheckPlatformAccountHistoryProxy(base.Client, base.Task.Account!, base.ServerHubConnection);
            }
            catch(Exception e)
            {
                throw new Exception($"class TaskVisitWithoutTimer(), method SecondStep_CheckPlatformAccountHistoryProxy_Or_GetNewProxy(), error {e.Message}");
            }

            // get new proxy
            if (NeedToGetNewProxy)
            {
                try
                {
                    await ServerHubConnection.SendAsync("set_task_status_to_waiting_for_new_proxy", base.Task!.Id);

                    await IProxySetable.GetNewProxy(base.Client, base.Task.Account!, base.ServerHubConnection);
                }
                catch (Exception e)
                {
                    throw new Exception($"class TaskVisitWithoutTimer(), method SecondStep_CheckPlatformAccountHistoryProxy_Or_GetNewProxy(), error {e.Message}");
                }
            }
            else
            {
                await ServerHubConnection.SendAsync("set_task_status_to_waiting_for_check_account_history_proxy", base.Task!.Id);
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
                throw new Exception($"class TaskVisitWithoutTimer(), method ThirdStep_InitHttpClientDataWithProxy(), error {e.Message}");
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
                throw new Exception($"class TaskVisitWithoutTimer(), method ThirdStep_InitHttpClientDataWithProxy(), error {e.Message}");
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
                throw new Exception($"class TaskVisitWithoutTimer(), method ThirdStep_InitHttpClientDataWithPersonalProxy(), error {e.Message}");
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
                throw new Exception($"class TaskVisitWithoutTimer(), method ThirdStep_InitHttpClientDataWithPersonalProxy(), error {e.Message}");
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
                throw new Exception($"class TaskVisitWithoutTimer(), method ChooseBetweenSteps_FiveStep_or_SevenStep(), error {e.Message}");
            }

            if (needAuth)
            {
                try
                {
                    await ServerHubConnection.SendAsync("set_task_status_to_waiting_for_captcha", base.Task!.Id);

                    await FiveStep_GetAndSolvePlatformCaptcha();
                }
                catch (Exception e)
                {
                    throw new Exception($"class TaskVisitWithoutTimer(), method ChooseBetweenSteps_FiveStep_or_SevenStep(), error {e.Message}");
                }
            }
            else
            {
                try
                {
                    await ServerHubConnection.SendAsync("set_task_status_to_executing", base.Task!.Id);

                    await SevenStep_SolvePlatformTasks_Boot();
                }
                catch (Exception e)
                {
                    throw new Exception($"class TaskVisitWithoutTimer(), method ChooseBetweenSteps_FiveStep_or_SevenStep(), error {e.Message}");
                }
            }
        }
        private async Task<bool> FourthStep_CheckNeedPlatformAccountAuth()
        {
            bool needAuth = false;

            try
            {
                needAuth = await CheckNeedPlatformAccountAuth($"{base.Task.Url}/account/", base.Task.Url);
            }
            catch(Exception e)
            {
                throw new Exception($"class TaskVisitWithoutTimer(), method FourthStep_CheckNeedPlatformAccountAuth(), error {e.Message}");
            }

            return needAuth;
        }
        private async Task FiveStep_GetAndSolvePlatformCaptcha()
        {
            // get captcha
            try
            {
                await GetCaptchaReCaptchaV2($"{base.Task.Url}/auth_login.html");
                //await GetCaptchaCloudflareTurnstile($"{base.Task.Url}/auth_login.html");
            }
            catch (Exception e)
            {
                throw new Exception($"class TaskVisitWithoutTimer(), method FiveStep_GetAndSolvePlatformCaptcha(), error {e.Message}");
            }

            // solve captcha
            try
            {
                await SolveCaptchaReCaptchaV2();
                //await SolveCaptchaCloudflareTurnstile();
            }
            catch (Exception e)
            {
                throw new Exception($"class TaskVisitWithoutTimer(), method FiveStep_GetAndSolvePlatformCaptcha(), error {e.Message}");
            }
        }
        private async Task SixStep_LoginIntoAccount()
        {
            try
            {
                await base.AuthorizeAccount($"{base.Task.Url}/auth_login.html");
            }
            catch(Exception e)
            {
                throw new Exception($"class TaskVisitWithoutTimer(), method SixStep_LoginIntoAccount(), error {e.Message}");
            }
        }
        private async Task SevenStep_SolvePlatformTasks_Boot()
        {
            //  НАЧАЛО ВЫПОЛНЕНИЯ ЗАДАНИЙ НА ПЛАТФОРМЕ

            bool success = true;
            Exception? ex = null;

            try
            {
                if (base.Task!.Account!.IsFirstExecutionOfTasksAfterRegistration)
                {
                    // Solve instruction
                    // work with Account - Operation Start
                    var operation = new Operation()
                    {
                        DateTimeStart = DateTime.UtcNow,
                        Type = OperationType.receivETrainingBeforePerformingVisitsWithoutTimer,
                        Action = "Прохождение инструктажа перед выполнением посещений без таймера"
                    };

                    try
                    {
                        await ReceiveTrainingBeforePerformingVisitsWithoutTimer();

                        base.Task.Account!.IsFirstExecutionOfTasksAfterRegistration = false;

                        operation.Status = OperationStatus.Success;
                    }
                    catch(Exception e)
                    {
                        operation.Status = OperationStatus.Failure;
                        operation.StatusMessage = e.Message;
                        throw;
                    }
                    finally
                    {
                        operation.DateTimeEnd = DateTime.UtcNow;
                        operation.SetTimeOfWorkInSeconds();
                        operation.MoneyMainBalancePlus = 0;

                        base.AddOperationToLogAccount(operation);
                    }
                }

                foreach (var task_subtype in (base.Task as GroupSelectiveTaskWithAuth)!.InternalSubtype)
                {
                    if(task_subtype == SocpublicComTaskEnums.SocpublicComTaskSubtype.CheckOnNeedToConfirmAccountEmail)
                    {
                        // work with Account - Operation Start
                        var operation = new Operation()
                        {
                            DateTimeStart = DateTime.UtcNow,
                            Type = OperationType.executeOneTask,
                            Action = "Проверка на потребность подтверждения аккаунта по почте"
                        };

                        try
                        {
                            await SelectiveTaskCheckOnNeedToConfirmAccountEmail();

                            operation.Status = OperationStatus.Success;
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
                            operation.MoneyMainBalancePlus = 0;

                            base.AddOperationToLogAccount(operation);
                        }
                    }
                    else if(task_subtype == SocpublicComTaskEnums.SocpublicComTaskSubtype.ConfirmRegisterAccountByLinkFromEmail)
                    {
                        // work with Account - Operation Start
                        var operation = new Operation()
                        {
                            DateTimeStart = DateTime.UtcNow,
                            Type = OperationType.executeOneTask,
                            Action = "Подтверждение аккаунта по почте"
                        };

                        try
                        {
                            if (registerConfirmationLink == null)
                            {
                                throw new Exception("registerConfirmationLink == null");
                            }

                            await SelectiveTaskConfirmRegisterAccountByLinkFromEmail();

                            operation.Status = OperationStatus.Success;
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
                            operation.MoneyMainBalancePlus = 0;

                            base.AddOperationToLogAccount(operation);
                        }
                    }
                    else if (task_subtype == SocpublicComTaskEnums.SocpublicComTaskSubtype.PassRulesKnowledgeTest && (base.Task!.Account!.Anketa == null || base.Task!.Account!.Anketa.TestPassed.HasValue == false || base.Task!.Account!.Anketa.TestPassed.Value == false))
                    {
                        // Pass Test
                        // work with Account - Operation Start
                        var operation = new Operation()
                        {
                            DateTimeStart = DateTime.UtcNow,
                            Type = OperationType.passRulesKnowledgeTest,
                            Action = "Прохождение теста на знание правил проекта."
                        };

                        try
                        {
                            bool? testPassedPreviousState = base.Task?.Account?.Anketa?.TestPassed ?? false;

                            await PassRulesKnowledgeTest();

                            operation.Status = OperationStatus.Success;

                            if ((!testPassedPreviousState.HasValue || testPassedPreviousState.Value == false) && (base.Task!.Account.Anketa != null && base.Task.Account.Anketa!.TestPassed.HasValue && base.Task.Account.Anketa.TestPassed.Value == true))
                            {
                                operation.StatusMessage += $"Тест на знание правил проекта был успешно пройден.";
                            }
                            else if ((testPassedPreviousState.HasValue && testPassedPreviousState.Value == true) && (base.Task!.Account.Anketa != null && base.Task.Account.Anketa!.TestPassed.HasValue && base.Task.Account.Anketa.TestPassed.Value == true))
                            {
                                operation.StatusMessage += $"Тест на знание правил проекта уже был пройден ранее.";
                            }
                            else
                            {
                                operation.StatusMessage += $"Произошла ошибка при прохождении теста на знание правил проекта.";
                            }
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

                            base.AddOperationToLogAccount(operation);
                        }
                    }
                    else if (task_subtype == SocpublicComTaskEnums.SocpublicComTaskSubtype.FillAnketaMainInfo && (base.Task!.Account!.Anketa == null || base.Task!.Account!.Anketa.AnketaMainInfoIsFilled.HasValue == false || base.Task!.Account!.Anketa.AnketaMainInfoIsFilled.Value == false))
                    {
                        // Pass Test
                        // work with Account - Operation Start
                        var operation = new Operation()
                        {
                            DateTimeStart = DateTime.UtcNow,
                            Type = OperationType.fillAnketaMainInfo,
                            Action = "Заполнение анкеты профиля."
                        };

                        try
                        {
                            bool? anketaInfoFilledPreviousState = base.Task?.Account?.Anketa?.AnketaMainInfoIsFilled ?? false;
                            Anketa? previousAnketa = base.Task?.Account?.Anketa;

                            await FillAnketaMainInfo();

                            operation.Status = OperationStatus.Success;

                            if ((!anketaInfoFilledPreviousState.HasValue || anketaInfoFilledPreviousState.Value == false) && (base.Task!.Account.Anketa != null && base.Task.Account.Anketa!.AnketaMainInfoIsFilled.HasValue && base.Task.Account.Anketa.AnketaMainInfoIsFilled.Value == true))
                            {
                                operation.StatusMessage += $"Параметр 'Вид деятельности' успешно установлен: {base.Task.Account.Anketa.KindOfActivity?.ToString()}";
                                operation.StatusMessage += $"\nПараметр 'Семейное положение' успешно установлен: {base.Task.Account.Anketa.FamilyStatus?.ToString()}";
                                operation.StatusMessage += $"\nПараметр 'Пол' успешно установлен: {base.Task.Account.Anketa.Gender?.ToString()}";
                                operation.StatusMessage += $"\nПараметр 'Дата рождения' успешно установлен: {base.Task.Account.Anketa.DateOfBirth?.ToString()}";
                            }
                            else if ((anketaInfoFilledPreviousState.HasValue && anketaInfoFilledPreviousState.Value == true) && (base.Task!.Account.Anketa != null && base.Task.Account.Anketa!.AnketaMainInfoIsFilled.HasValue && base.Task.Account.Anketa.AnketaMainInfoIsFilled.Value == true))
                            {
                                operation.StatusMessage += $"Параметр 'Вид деятельности' успешно обновлён: {previousAnketa?.KindOfActivity?.ToString()} ==> {base.Task.Account.Anketa.KindOfActivity?.ToString()}";
                                operation.StatusMessage += $"\nПараметр 'Семейное положение' успешно обновлён: {previousAnketa?.FamilyStatus?.ToString()} ==> {base.Task.Account.Anketa.FamilyStatus?.ToString()}";
                                operation.StatusMessage += $"\nПараметр 'Пол' успешно обновлён: {previousAnketa?.Gender?.ToString()} ==> {base.Task.Account.Anketa.Gender?.ToString()}";
                                operation.StatusMessage += $"\nПараметр 'Дата рождения' успешно обновлён: {previousAnketa?.DateOfBirth?.ToString()} ==> {base.Task.Account.Anketa.DateOfBirth?.ToString()}";
                            }
                            else
                            {
                                operation.StatusMessage += $"Произошла ошибка при заполнение анкеты профиля.";
                            }
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

                            base.AddOperationToLogAccount(operation);
                        }
                    }
                    else if (task_subtype == SocpublicComTaskEnums.SocpublicComTaskSubtype.InstallAvatar && (base.Task!.Account!.Anketa == null || base.Task!.Account!.Anketa.AvatarInstalled.HasValue == false || base.Task!.Account!.Anketa.AvatarInstalled.Value == false))
                    {
                        // Pass Test
                        // work with Account - Operation Start
                        var operation = new Operation()
                        {
                            DateTimeStart = DateTime.UtcNow,
                            Type = OperationType.installAvatar,
                            Action = "Установка аватара профиля."
                        };

                        try
                        {
                            bool? avatarInstalledPreviousState = base.Task?.Account?.Anketa?.AvatarInstalled ?? false;
                            Anketa? previousAnketa = base.Task?.Account?.Anketa;

                            await InstallAvatar();

                            operation.Status = OperationStatus.Success;

                            if ((!avatarInstalledPreviousState.HasValue || avatarInstalledPreviousState.Value == false) && (base.Task!.Account.Anketa != null && base.Task.Account.Anketa!.AvatarInstalled.HasValue && base.Task.Account.Anketa.AvatarInstalled.Value == true))
                            {
                                operation.StatusMessage += $"Аватар успешно установлен";
                                operation.StatusMessage += $"\nАватар URL: {base.Task.Account.Anketa.AvatarUrl}";
                                operation.StatusMessage += $"\nАватар инфо: {base.Task.Account.Anketa.AvatarInfo}";
                            }
                            else if ((avatarInstalledPreviousState.HasValue && avatarInstalledPreviousState.Value == true) && (base.Task!.Account.Anketa != null && base.Task.Account.Anketa!.AvatarInstalled.HasValue && base.Task.Account.Anketa.AvatarInstalled.Value == true))
                            {
                                operation.StatusMessage += $"Новый аватар успешно установлен";
                                operation.StatusMessage += $"\nАватар URL: {previousAnketa?.AvatarUrl} ==> {base.Task.Account.Anketa.AvatarUrl}";
                                operation.StatusMessage += $"\nАватар инфо: {previousAnketa?.AvatarInfo} ==> {base.Task.Account.Anketa.AvatarInfo}";
                            }
                            else
                            {
                                operation.StatusMessage += $"Произошла ошибка при установке аватара профиля.";
                            }
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

                            base.AddOperationToLogAccount(operation);
                        }
                    }
                    else if (task_subtype == SocpublicComTaskEnums.SocpublicComTaskSubtype.CheckAndGetFreeGiftBoxes)
                    {
                        // Check on free gift boxes and get it
                        // work with Account - Operation Start
                        var operation = new Operation()
                        {
                            DateTimeStart = DateTime.UtcNow,
                            Type = OperationType.checkAndGetFreeGiftBoxes,
                            Action = "Получение бесплатных подарочных сундуков"
                        };

                        try
                        {  
                            await SelectiveTaskCheckAndGetFreeGiftBoxes();

                            operation.Status = OperationStatus.Success;
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

                            base.AddOperationToLogAccount(operation);
                        }
                    }
                    else if (task_subtype == SocpublicComTaskEnums.SocpublicComTaskSubtype.TaskVisitWithoutTimer)
                    {
                        // Solve visit tasks without timer
                        // work with Account - Operation Start
                        var operation = new Operation()
                        {
                            DateTimeStart = DateTime.UtcNow,
                            Type = OperationType.executeTasks,
                            Action = "Выполнение посещений без таймера"
                        };

                        try
                        {
                            if((base.Task as GroupSelectiveTaskWithAuth)!.taskVisitWithoutTimer == null)
                            {
                                throw new Exception("(base.Task as GroupSelectiveTaskWithAuth)!.taskVisitWithoutTimer == null");
                            }

                            await SelectiveTaskVisitWithoutTimer();

                            operation.Status = OperationStatus.Success;
                        }
                        catch(Exception e)
                        {
                            operation.Status = OperationStatus.Failure;
                            operation.StatusMessage = e.Message;
                            throw;
                        }
                        finally
                        {
                            operation.DateTimeEnd = DateTime.UtcNow;
                            operation.SetTimeOfWorkInSeconds();
                            operation.MoneyMainBalancePlus = (base.Task as GroupSelectiveTaskWithAuth)!.taskVisitWithoutTimer!.moneyEarned;

                            base.AddOperationToLogAccount(operation);
                        }
                    }
                    else if(task_subtype == SocpublicComTaskEnums.SocpublicComTaskSubtype.TaskVisitWithTimer)
                    {

                    }
                    else if (task_subtype == SocpublicComTaskEnums.SocpublicComTaskSubtype.UpdatingTheCurrentBalance)
                    {
                        // Update account current balance
                        // work with Account - Operation Start
                        var operation = new Operation()
                        {
                            DateTimeStart = DateTime.UtcNow,
                            Type = OperationType.updatingTheCurrentBalance,
                            Action = "Обновление текущего баланса"
                        };

                        try
                        {
                            double? previousMainBalance = base.Task.Account.MoneyMainBalance;
                            double? previousAdBalance = base.Task.Account.MoneyAdvertisingBalance;

                            await UpdatingAccountCurrentBalances();

                            operation.Status = OperationStatus.Success;

                            if (previousMainBalance == null || previousMainBalance != base.Task.Account.MoneyMainBalance)
                            {
                                operation.StatusMessage += $"Основной баланс обновлён успешно: {previousMainBalance ?? 0.00}руб. ==> {base.Task.Account.MoneyMainBalance}руб.";
                            }
                            if (previousAdBalance == null || previousAdBalance != base.Task.Account.MoneyAdvertisingBalance)
                            {
                                operation.StatusMessage += $"\nРекламный баланс обновлён успешно: {previousAdBalance ?? 0.00}руб. ==> {base.Task.Account.MoneyAdvertisingBalance}руб.";
                            }
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

                            base.AddOperationToLogAccount(operation);
                        }
                    }
                    else if (task_subtype == SocpublicComTaskEnums.SocpublicComTaskSubtype.CheckCareerLadder)
                    {
                        // Check Career Ladder
                        // work with Account - Operation Start
                        var operation = new Operation()
                        {
                            DateTimeStart = DateTime.UtcNow,
                            Type = OperationType.checkCareerLadder,
                            Action = "Проверка изменений в карьерной лестнице"
                        };

                        try
                        {
                            Status? PreviousStatus = base.Task.Account.CareerLadder?.Status;
                            double? PreviousStatusPoints = base.Task.Account.CareerLadder?.StatusPoints;
                            int? PreviousStatusProgress = base.Task.Account.CareerLadder?.StatusProgress;
                            int? PreviousCareerLevel = base.Task.Account.CareerLadder?.CareerLevel;

                            //if ((base.Task as GroupSelectiveTaskWithAuth)!.checkCareerLadder == null)
                            //{
                            //    throw new Exception("(base.Task as GroupSelectiveTaskWithAuth)!.checkCareerLadder == null");
                            //}

                            await SelectiveTaskcheckCareerLadder();

                            operation.Status = OperationStatus.Success;

                            if(PreviousStatus != base.Task.Account.CareerLadder?.Status)
                            {
                                operation.StatusMessage += $"Статус успешно обновлён: {PreviousStatus?.ToString()} ==> {base.Task.Account.CareerLadder?.Status.ToString()}";
                            }
                            if (PreviousStatusPoints != base.Task.Account.CareerLadder?.StatusPoints)
                            {
                                operation.StatusMessage += $"\nОчки статуса успешно обновлены: {PreviousStatusPoints} очков ==> {base.Task.Account.CareerLadder?.StatusPoints} очков";
                            }
                            if (PreviousStatusProgress != base.Task.Account.CareerLadder?.StatusProgress)
                            {
                                operation.StatusMessage += $"\nПрогресс успешно обновлён: {PreviousStatusProgress}% ==> {base.Task.Account.CareerLadder?.StatusProgress}%";
                            }
                            if (PreviousCareerLevel != base.Task.Account.CareerLadder?.CareerLevel)
                            {
                                operation.StatusMessage += $"\nУровень успешно обновлён: {PreviousCareerLevel} ==> {base.Task.Account.CareerLadder?.CareerLevel}";
                            }
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

                            base.AddOperationToLogAccount(operation);
                        }
                    }
                }
            }
            catch(Exception e)
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

        #region Selective Tasks
        private async Task ReceiveTrainingBeforePerformingVisitsWithoutTimer()
        {
            Dictionary<string, string>? queryParams;
            string? uri = "";
            HttpResponseMessage? responce = null;
            string htmlPage = "";
            IDocument? htmlPageDocument = null;

            //get instruction
            uri = $"{base.Task.Url}/account/visit_instruct.html";

            using(var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                // set headers to request
                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                // request
                responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                if (responce.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("responce.StatusCode != HttpStatusCode.OK");
                }

                htmlPage = await responce.Content.ReadAsStringAsync();
            }

            //confirm instruction
            queryParams = new Dictionary<string, string>()
            {
                ["instruct"] = "1"
            };

            uri = QueryHelpers.AddQueryString($"{base.Task.Url}/account/visit.html", queryParams);

            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                // set headers to request
                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");
                //if (request.Headers.Contains("upgrade-insecure-requests")) request.Headers.Remove("upgrade-insecure-requests");

                request.Headers.Add("referer", $"{base.Task.Url}/account/visit_instruct.html");
                //request.Headers.Add("upgrade-insecure-requests", "1");

                // request
                responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                if (responce.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("responce.StatusCode != HttpStatusCode.OK");
                }

                htmlPage = await responce.Content.ReadAsStringAsync();
            }
        }
        private async Task PassRulesKnowledgeTest()
        {
            Dictionary<string, string>? queryParams;
            string? uri = "";
            HttpResponseMessage? responce = null;
            string htmlPage = "";
            IDocument? htmlPageDocument = null;
            bool need_to_pass_test = false;
            bool need_to_check_pass_test = false;

            #region Check test info

            CancellationTokenSource getPassRulestTestCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            uri = $"{base.Task!.Url}/account/quiz.html";

            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                // set headers to request
                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                request.Headers.Add("referer", $"{base.Task.Url}/account/profile.html");

                // request
                responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, getPassRulestTestCancellationTokenSource.Token);

                htmlPage = await responce.Content.ReadAsStringAsync();

                if (responce.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"responce.StatusCode != HttpStatusCode.OK --- Check test info\n\n\n{htmlPage}");
                }

                if (!htmlPage.Contains("Проверить результат") && !htmlPage.Contains("Поздравляем!"))
                {
                    throw new Exception($"!htmlPage.Contains(\"Проверить результат\") && !htmlPage.Contains(\"Поздравляем!\") --- region Check test info\n\n\n{htmlPage}");
                }
                else if (!htmlPage.Contains("Поздравляем!") && htmlPage.Contains("Проверить результат"))
                {
                    need_to_pass_test = true;
                }
                else if (htmlPage.Contains("Поздравляем!"))
                {
                    if(base.Task!.Account!.Anketa == null)
                    {
                        base.Task!.Account!.Anketa = new Anketa()
                        {
                            TestPassed = true
                        };
                    }
                    else
                    {
                        base.Task!.Account!.Anketa.TestPassed = true;
                    }

                    need_to_pass_test = false;
                    need_to_check_pass_test = false;
                }
            } 

            #endregion

            if (need_to_pass_test)
            {
                #region Pass test

                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(15));

                // set query parameters
                var passRulestTestInfoQuery = new Dictionary<string, string>()
                {
                    ["act"] = "done"
                };

                CancellationTokenSource passRulestTestCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                // set query parameters
                uri = QueryHelpers.AddQueryString($"{base.Task!.Url}/account/quiz.html", passRulestTestInfoQuery);

                using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                {
                    // set headers to request
                    foreach (var header in base.contextHeaders!)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }

                    if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                    if (request.Headers.Contains("referer")) request.Headers.Remove("referer");
                    if (request.Headers.Contains("origin")) request.Headers.Remove("origin");

                    //request.Headers.Add("referer", $"{base.Task.Url}/account/quiz.html?act=done"); //// ???? может реферер без акта?
                    request.Headers.Add("referer", $"{base.Task.Url}/account/quiz.html");
                    request.Headers.Add("origin", $"{base.Task.Url}");

                    // set content 
                    Dictionary<string, string> passTestRequestContent = new Dictionary<string, string>();
                    passTestRequestContent.Add("session", "");
                    passTestRequestContent.Add("quest_1", "1");
                    passTestRequestContent.Add("quest_2", "3");
                    passTestRequestContent.Add("quest_3", "3");
                    passTestRequestContent.Add("quest_4", "2");
                    passTestRequestContent.Add("quest_5", "3");
                    passTestRequestContent.Add("quest_6", "1");
                    passTestRequestContent.Add("quest_7", "3");
                    passTestRequestContent.Add("quest_8", "2");
                    passTestRequestContent.Add("quest_9", "1");
                    passTestRequestContent.Add("quest_10", "1");
                    passTestRequestContent.Add("quest_11", "3");
                    passTestRequestContent.Add("quest_12", "1");
                    passTestRequestContent.Add("quest_13", "3");
                    passTestRequestContent.Add("quest_14", "3");
                    passTestRequestContent.Add("quest_15", "3");
                    passTestRequestContent.Add("quest_16", "2");
                    passTestRequestContent.Add("quest_17", "2");
                    passTestRequestContent.Add("quest_18", "3");
                    passTestRequestContent.Add("quest_19", "2");
                    passTestRequestContent.Add("quest_20", "1");
                    passTestRequestContent.Add("quest_21", "3");
                    passTestRequestContent.Add("quest_22", "2");
                    request.Content = new FormUrlEncodedContent(passTestRequestContent);

                    // request
                    responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, passRulestTestCancellationTokenSource.Token);

                    string content = await responce.Content.ReadAsStringAsync();

                    if (responce.StatusCode == HttpStatusCode.OK && content.Contains("Поздравляем!"))
                    {
                        if (base.Task!.Account!.Anketa == null)
                        {
                            base.Task!.Account!.Anketa = new Anketa()
                            {
                                TestPassed = true
                            };
                        }
                        else
                        {
                            base.Task!.Account!.Anketa.TestPassed = true;
                        }

                        need_to_check_pass_test = false;
                    }
                    else if (responce.StatusCode == HttpStatusCode.OK && !content.Contains("Поздравляем!"))
                    {
                        throw new Exception($"responce.StatusCode == HttpStatusCode.OK && !content.Contains(\"Поздравляем!\") --- Pass test\n\n\n{content}");
                    }
                    else if (responce.StatusCode == HttpStatusCode.Found || responce.StatusCode == HttpStatusCode.Redirect)
                    {
                        need_to_check_pass_test = true;
                    }
                    else
                    {
                        throw new Exception($"Error in PassRulesKnowledgeTest\n\n\nresponce = {responce.StatusCode}\n\n\n{content}");
                    }
                }  

                #endregion
            }

            if (need_to_check_pass_test)
            {
                #region Check Pass Test

                CancellationTokenSource checkPassRulestTestCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                uri = $"{base.Task!.Url}/account/quiz.html";

                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    // set headers to request
                    foreach (var header in base.contextHeaders!)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }

                    if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                    if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                    request.Headers.Add("referer", $"{base.Task.Url}/account/quiz.html?act=done");

                    // request
                    responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, checkPassRulestTestCancellationTokenSource.Token);

                    htmlPage = await responce.Content.ReadAsStringAsync();

                    if (responce.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"responce.StatusCode != HttpStatusCode.OK --- Check Pass Test\n\n\n{htmlPage}");
                    }

                    if (!htmlPage.Contains("Проверить результат") && !htmlPage.Contains("Поздравляем!"))
                    {
                        throw new Exception($"!htmlPage.Contains(\"Проверить результат\") && !htmlPage.Contains(\"Поздравляем!\") --- Check Pass Test\n\n\n{htmlPage}");
                    }
                    else if (!htmlPage.Contains("Поздравляем!"))
                    {
                        throw new Exception($"!htmlPage.Contains(\"Поздравляем!\") --- тест не пройден --- Check Pass Test\n\n\n{htmlPage}");
                    }
                    else if (htmlPage.Contains("Поздравляем!"))
                    {
                        if (base.Task!.Account!.Anketa == null)
                        {
                            base.Task!.Account!.Anketa = new Anketa()
                            {
                                TestPassed = true
                            };
                        }
                        else
                        {
                            base.Task!.Account!.Anketa.TestPassed = true;
                        }
                    }
                } 

                #endregion
            }
        }
        private async Task FillAnketaMainInfo()
        {
            Dictionary<string, string>? queryParams;
            string? uri = "";
            HttpResponseMessage? responce = null;
            string htmlPage = "";
            IDocument? htmlPageDocument = null;
            bool need_to_check_saved_info = false;
            Random random = new Random();


            KindOfActivity kindOfActivity = ((KindOfActivity)(random.Next(0, 7)));
            FamilyStatus familyStatus = ((FamilyStatus)(random.Next(0, 4)));
            Gender gender = ((Gender)(random.Next(0, 2)));
            int born_day = random.Next(1, 29);
            int born_month = random.Next(1, 13);
            int born_year = random.Next(1975, 2006);

            #region Fill Anketa Main Info

            // set query parameters
            var FillAnketaMainInfoQuery = new Dictionary<string, string>()
            {
                ["act"] = "save"
            };

            CancellationTokenSource fillAnketaMainInfoCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            // set query parameters
            uri = QueryHelpers.AddQueryString($"{base.Task!.Url}/account/profile.html", FillAnketaMainInfoQuery);

            using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                // set headers to request
                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");
                if (request.Headers.Contains("origin")) request.Headers.Remove("origin");

                request.Headers.Add("referer", $"{base.Task.Url}/account/profile.html");
                request.Headers.Add("origin", $"{base.Task.Url}");

                // set content 
                Dictionary<string, string> fillAnketaMainInfoRequestContent = new Dictionary<string, string>();
                fillAnketaMainInfoRequestContent.Add("session", "");
                fillAnketaMainInfoRequestContent.Add("work", $"{kindOfActivity.ToString()}");
                fillAnketaMainInfoRequestContent.Add("family", $"{familyStatus.ToString()}");
                fillAnketaMainInfoRequestContent.Add("gender", $"{gender.ToString()}");
                fillAnketaMainInfoRequestContent.Add("born_day", $"{born_day}");
                fillAnketaMainInfoRequestContent.Add("born_month", $"{born_month}");
                fillAnketaMainInfoRequestContent.Add("born_year", $"{born_year}");
                request.Content = new FormUrlEncodedContent(fillAnketaMainInfoRequestContent);

                // request
                responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, fillAnketaMainInfoCancellationTokenSource.Token);

                string content = await responce.Content.ReadAsStringAsync();

                if (responce.StatusCode == HttpStatusCode.OK && content.Contains("Данные успешно сохранены"))
                {
                    if (base.Task!.Account!.Anketa == null)
                    {
                        base.Task!.Account!.Anketa = new Anketa()
                        {
                            AnketaMainInfoIsFilled = true,
                            KindOfActivity = kindOfActivity,
                            FamilyStatus = familyStatus,
                            Gender = gender,
                            DateOfBirth = new DateTime(born_year, born_month, born_day)
                        };
                    }
                    else
                    {
                        base.Task!.Account!.Anketa.AnketaMainInfoIsFilled = true;
                        base.Task!.Account!.Anketa.KindOfActivity = kindOfActivity;
                        base.Task!.Account!.Anketa.FamilyStatus = familyStatus;
                        base.Task!.Account!.Anketa.Gender = gender;
                        base.Task!.Account!.Anketa.DateOfBirth = new DateTime(born_year, born_month, born_day);
                    }

                    need_to_check_saved_info = false;
                }
                else if (responce.StatusCode == HttpStatusCode.OK && !content.Contains("Данные успешно сохранены"))
                {
                    throw new Exception($"responce.StatusCode == HttpStatusCode.OK && !content.Contains(\"Данные успешно сохранены\") --- Fill Anketa Main Info\n\n\n{content}");
                }
                else if (responce.StatusCode == HttpStatusCode.Found || responce.StatusCode == HttpStatusCode.Redirect)
                {
                    need_to_check_saved_info = true;
                }
                else
                {
                    throw new Exception($"Error in FillAnketaMainInfo\n\n\nresponce = {responce.StatusCode}\n\n\n{content}");
                }
            }

            #endregion

            if (need_to_check_saved_info)
            {
                #region Check Save Fill Info

                CancellationTokenSource checkSaveFillCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                uri = $"{base.Task!.Url}/account/profile.html";

                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    // set headers to request
                    foreach (var header in base.contextHeaders!)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }

                    if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                    if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                    request.Headers.Add("referer", $"{base.Task.Url}/account/profile.html");

                    // request
                    responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, checkSaveFillCancellationTokenSource.Token);

                    htmlPage = await responce.Content.ReadAsStringAsync();

                    if (responce.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"responce.StatusCode != HttpStatusCode.OK --- Check Save Fill Info\n\n\n{htmlPage}");
                    }

                    if (!htmlPage.Contains("Данные успешно сохранены"))
                    {
                        throw new Exception($"!htmlPage.Contains(\"Данные успешно сохранены\") --- Check Save Fill Info\n\n\n{htmlPage}");
                    }
                    else if (htmlPage.Contains("Данные успешно сохранены"))
                    {
                        if (base.Task!.Account!.Anketa == null)
                        {
                            base.Task!.Account!.Anketa = new Anketa()
                            {
                                AnketaMainInfoIsFilled = true,
                                KindOfActivity = kindOfActivity,
                                FamilyStatus = familyStatus,
                                Gender = gender,
                                DateOfBirth = new DateTime(born_year, born_month, born_day)
                            };
                        }
                        else
                        {
                            base.Task!.Account!.Anketa.AnketaMainInfoIsFilled = true;
                            base.Task!.Account!.Anketa.KindOfActivity = kindOfActivity;
                            base.Task!.Account!.Anketa.FamilyStatus = familyStatus;
                            base.Task!.Account!.Anketa.Gender = gender;
                            base.Task!.Account!.Anketa.DateOfBirth = new DateTime(born_year, born_month, born_day);
                        }
                    }
                }

                #endregion
            }
        }
        private async Task InstallAvatar()
        {
            Dictionary<string, string>? queryParams;
            string? uri = "";
            HttpResponseMessage? responce = null;
            string htmlPage = "";
            IDocument? htmlPageDocument = null;
            bool need_to_check_saved_avatar = false;

            #region Send Avatar

            if (avatar == null || avatar.Length == 0)
            {
                throw new Exception("avatar == null || avatar.Length == 0");
            }

            // set query parameters
            var SendAvatarInfoQuery = new Dictionary<string, string>()
            {
                ["act"] = "avatar_upload"
            };

            CancellationTokenSource sendAvatarCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            // set query parameters
            uri = QueryHelpers.AddQueryString($"{base.Task!.Url}/account/profile.html", SendAvatarInfoQuery);

            using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                // set headers to request
                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");
                if (request.Headers.Contains("origin")) request.Headers.Remove("origin");

                request.Headers.Add("referer", $"{base.Task.Url}/account/profile.html");
                request.Headers.Add("origin", $"{base.Task.Url}");

                // set content 
                MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent($"----WebKitFormBoundary{DateTime.Now.Ticks.ToString("x")}"); // or ToString(CultureInfo.InvariantCulture)
                multipartFormDataContent.Add(new StringContent(""), "session");
                multipartFormDataContent.Add(new ByteArrayContent(avatar!, 0, avatar!.Length), "avatar_file", $"{base.Task.Account!.Login}.jpg");
                request.Content = multipartFormDataContent;

                // request
                responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, sendAvatarCancellationTokenSource.Token);

                string content = await responce.Content.ReadAsStringAsync();

                if (responce.StatusCode == HttpStatusCode.OK && content.Contains("Аватарка успешно установлена"))
                {
                    if (base.Task!.Account!.Anketa == null)
                    {
                        base.Task!.Account!.Anketa = new Anketa()
                        {
                            AvatarInstalled = true,
                            AvatarUrl = avatar_url,
                            AvatarInfo = $"{base.Task.Account!.Login}.jpg"
                        };
                    }
                    else
                    {
                        base.Task!.Account!.Anketa.AvatarInstalled = true;
                        base.Task!.Account!.Anketa.AvatarUrl = avatar_url;
                        base.Task!.Account!.Anketa.AvatarInfo = $"{base.Task.Account!.Login}.jpg";
                    }

                    need_to_check_saved_avatar = false;
                }
                else if (responce.StatusCode == HttpStatusCode.OK && !content.Contains("Аватарка успешно установлена"))
                {
                    if(content.Contains("Не удалось загрузить картинку"))
                    {
                        if (content.Contains("Файловая ошибка"))
                        {
                            throw new Exception($"responce.StatusCode == HttpStatusCode.OK && !content.Contains(\"Аватарка успешно установлена\") && content.Contains(\"Не удалось загрузить картинку\") && content.Contains(\"Файловая ошибка\") --- Send Avatar\n\n\n{content}");
                        }
                        else
                        {
                            throw new Exception($"responce.StatusCode == HttpStatusCode.OK && !content.Contains(\"Аватарка успешно установлена\") && content.Contains(\"Не удалось загрузить картинку\") --- Send Avatar\n\n\n{content}");
                        }
                    }
                    else
                    {
                        throw new Exception($"responce.StatusCode == HttpStatusCode.OK && !content.Contains(\"Аватарка успешно установлена\") --- Send Avatar\n\n\n{content}");
                    }
                }
                else if (responce.StatusCode == HttpStatusCode.Found || responce.StatusCode == HttpStatusCode.Redirect)
                {
                    need_to_check_saved_avatar = true;
                }
                else
                {
                    throw new Exception($"Error in PassRulesKnowledgeTest\n\n\nresponce = {responce.StatusCode}\n\n\n{content}");
                }
            }

            #endregion

            if (need_to_check_saved_avatar)
            {
                #region Check Save Avatar

                CancellationTokenSource checkSaveAvatarCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                uri = $"{base.Task!.Url}/account/profile.html";

                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    // set headers to request
                    foreach (var header in base.contextHeaders!)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }

                    if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                    if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                    request.Headers.Add("referer", $"{base.Task.Url}/account/profile.html");

                    // request
                    responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, checkSaveAvatarCancellationTokenSource.Token);

                    htmlPage = await responce.Content.ReadAsStringAsync();

                    if (responce.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"responce.StatusCode != HttpStatusCode.OK --- Check Save Avatar\n\n\n{htmlPage}");
                    }

                    if (!htmlPage.Contains("Аватарка успешно установлена"))
                    {
                        throw new Exception($"!htmlPage.Contains(\"Аватарка успешно установлена\") --- Check Save Avatar\n\n\n{htmlPage}");
                    }
                    else if (htmlPage.Contains("Аватарка успешно установлена"))
                    {
                        if (base.Task!.Account!.Anketa == null)
                        {
                            base.Task!.Account!.Anketa = new Anketa()
                            {
                                AvatarInstalled = true,
                                AvatarUrl = avatar_url,
                                AvatarInfo = $"{base.Task.Account!.Login}.jpg"
                            };
                        }
                        else
                        {
                            base.Task!.Account!.Anketa.AvatarInstalled = true;
                            base.Task!.Account!.Anketa.AvatarUrl = avatar_url;
                            base.Task!.Account!.Anketa.AvatarInfo = $"{base.Task.Account!.Login}.jpg";
                        }
                    }
                }

                #endregion
            }
        }
        private async Task UpdatingAccountCurrentBalances()
        {
            Dictionary<string, string>? queryParams;
            string? uri = "";
            HttpResponseMessage? responce = null;
            string content = "";
            IDocument? htmlPageDocument = null;
            List<IElement>? balances = null;
            double? main_balance = null;
            double? advert_balance = null;

            uri = $"{base.Task!.Url}/account/payout.html";

            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                // set headers to request
                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                request.Headers.Add("referer", $"{base.Task.Url}/account/index.html");

                // request
                responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                if (responce.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"responce.StatusCode != HttpStatusCode.OK --- UpdatingAccountCurrentBalances\n\n\n{content}");
                }

                content = await responce.Content.ReadAsStringAsync();
            }

            htmlPageDocument = await base.htmlParseContext!.OpenAsync(req => req.Content(content));

            balances = htmlPageDocument.QuerySelectorAll("div.collapse.navbar-collapse > ul.page-sidebar-menu > li.balance-box > div.inner > div").ToList();

            if (balances.Count != 2)
            {
                throw new Exception($"Ошибка при получении 'var balances = htmlPageDocument.QuerySelectorAll(\"div.collapse.navbar-collapse > ul.page-sidebar-menu > li.balance-box > div.inner > div\").ToList();' --- UpdatingAccountCurrentBalances\n\n\n{content}");
            }

            // main_balance будм получать ниже, более точное значение
            //main_balance = balances[0]!.QuerySelector("span.balance_rub")!.TextContent.ToDouble();
            advert_balance = balances[1]!.QuerySelector("span.balance_rub_advert")!.TextContent.ToDouble();

            var div_with_parts = htmlPageDocument.QuerySelector("form[action=\"/account/payout.html?act=payout\"]")?.QuerySelectorAll("div.form-group")[2]?.QuerySelector("div > div"); ;

            if (div_with_parts == null)
            {
                throw new Exception($"div_with_parts == null --- UpdatingAccountCurrentBalances\n\n\n{content}");
            }

            // get exact main_balance value
            string? first_main_balance_value_part = div_with_parts.QuerySelector("span.font-size-16.color-grey")!.TextContent;
            string? second_main_balance_value_part = div_with_parts.QuerySelector("span.font-size-16.color-mid-grey")!.TextContent;

            main_balance = $"{first_main_balance_value_part}{second_main_balance_value_part}".ToDouble();

            if (main_balance == null)
            {
                throw new Exception($"main_balance == null --- UpdatingAccountCurrentBalances\n\n\n{content}");
            }

            if (advert_balance == null)
            {
                throw new Exception($"advert_balance == null --- UpdatingAccountCurrentBalances\n\n\n{content}");
            }

            base.Task!.Account!.MoneyMainBalance = main_balance;
            base.Task.Account.MoneyAdvertisingBalance = advert_balance;
        }
        private async Task SelectiveTaskCheckAndGetFreeGiftBoxes()
        {
            string? uri = "";
            HttpResponseMessage? responce = null;
            dynamic? gift_box_time = null;
            GiftBoxType? currentGiftBoxType = null;

            #region Переход на вкладку сундуков с бонусами

            uri = $"{base.Task!.Url}/account/treasury.html";

            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                // set headers to request
                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                request.Headers.Add("referer", $"{base.Task.Url}/account/index.html");

                // request
                responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                if (responce.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("responce.StatusCode != HttpStatusCode.OK (Переход на вкладку сундуков с бонусами)");
                }
            }

            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5));

            #endregion

            #region Получение и открытие сундуков

            int GiftBoxErrorCount = 0;

            // set query parameters
            var getGiftBoxInfoQuery = new Dictionary<string, string>()
            {
                ["act"] = "treasury_prize_pay"
            };
            var openGiftBoxQuery = new Dictionary<string, string>()
            {
                ["act"] = "treasury_open"
            };

            for (int i = 1; i <= 3; i++)
            {
                gift_box_time = null;
                currentGiftBoxType = i switch
                {
                    1 => GiftBoxType.BronzeBox,
                    2 => GiftBoxType.SilverBox,
                    3 => GiftBoxType.GoldenBox,
                    _ => null
                };

                try
                {
                    #region Get info about GiftBox

                    CancellationTokenSource getGiftBoxInfoCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                    // set query parameters
                    uri = QueryHelpers.AddQueryString($"{base.Task!.Url}/treasury.ajax", getGiftBoxInfoQuery);

                    using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                    {
                        // set headers to request
                        foreach (var header in base.contextHeaders!)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }

                        if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                        if (request.Headers.Contains("referer")) request.Headers.Remove("referer");
                        if (request.Headers.Contains("origin")) request.Headers.Remove("origin");

                        request.Headers.Add("referer", $"{base.Task.Url}/account/treasury.html");
                        request.Headers.Add("origin", $"{base.Task.Url}");

                        // set content
                        Dictionary<string, string> getGiftBoxInfoRequestContent = new Dictionary<string, string>();
                        getGiftBoxInfoRequestContent.Add("boxId", $"{(int)currentGiftBoxType!}");
                        getGiftBoxInfoRequestContent.Add("session", "");
                        request.Content = new FormUrlEncodedContent(getGiftBoxInfoRequestContent);

                        // request
                        responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, getGiftBoxInfoCancellationTokenSource.Token);
                        if (responce.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception("responce.StatusCode != HttpStatusCode.OK (Get info about GiftBox)");
                        }

                        string content = await responce.Content.ReadAsStringAsync();

                        //check status
                        dynamic? getGiftBoxInfoResponceObj = JsonConvert.DeserializeObject(content);

                        if (getGiftBoxInfoResponceObj != null && getGiftBoxInfoResponceObj!.status == "success")
                        {
                            gift_box_time = (int)getGiftBoxInfoResponceObj!.text;
                        }
                        else
                        {
                            throw new Exception("responceObj != null && responceObj!.status == \"success\" (Get info about GiftBox)");
                        }
                    }

                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5));

                    #endregion

                    #region Open GiftBox

                    if (gift_box_time != null && ((gift_box_time is int && gift_box_time == 0) || (gift_box_time is bool && gift_box_time == false)))
                    {
                        CancellationTokenSource openGiftBoxCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                        // set query parameters
                        uri = QueryHelpers.AddQueryString($"{base.Task!.Url}/treasury.ajax", openGiftBoxQuery);

                        using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                        {
                            // set headers to request
                            foreach (var header in base.contextHeaders!)
                            {
                                request.Headers.Add(header.Key, header.Value);
                            }

                            if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                            if (request.Headers.Contains("referer")) request.Headers.Remove("referer");
                            if (request.Headers.Contains("origin")) request.Headers.Remove("origin");

                            request.Headers.Add("referer", $"{base.Task.Url}/account/treasury.html");
                            request.Headers.Add("origin", $"{base.Task.Url}");

                            // set content
                            Dictionary<string, string> openGiftBoxRequestContent = new Dictionary<string, string>();
                            openGiftBoxRequestContent.Add("boxId", $"{(int)currentGiftBoxType!}");
                            openGiftBoxRequestContent.Add("session", "");
                            request.Content = new FormUrlEncodedContent(openGiftBoxRequestContent);

                            // request
                            responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead, openGiftBoxCancellationTokenSource.Token);
                            if (responce.StatusCode != HttpStatusCode.OK)
                            {
                                throw new Exception("responce.StatusCode != HttpStatusCode.OK (Open gift box)");
                            }

                            string content = await responce.Content.ReadAsStringAsync();

                            //check status
                            dynamic? openGiftBoxResponceObj = JsonConvert.DeserializeObject(content);

                            if (base.oneLogAccount!.ReceivedGifts == null)
                            {
                                base.oneLogAccount!.ReceivedGifts = new List<GiftBox>();
                            }

                            if (openGiftBoxResponceObj != null && openGiftBoxResponceObj!.status == "success")
                            {
                                // create and add received gift box

                                GiftBox giftBox = new GiftBox()
                                {
                                    Received = true,
                                    Error = false,
                                    GiftBoxType = currentGiftBoxType!,
                                    GiftBoxDescription = openGiftBoxResponceObj!.result.description,
                                    GiftBoxValue = openGiftBoxResponceObj.result.value,
                                };

                                base.oneLogAccount!.ReceivedGifts.Add(giftBox);
                            }
                            else
                            {
                                // create and add not received gift box

                                GiftBox giftBox = new GiftBox()
                                {
                                    Received = false,
                                    Error = true,
                                    GiftBoxType = currentGiftBoxType!
                                };

                                base.oneLogAccount!.ReceivedGifts.Add(giftBox);

                                throw new Exception("responceObj != null && responceObj!.status == \"success\" (Open gift box)");
                            }
                        }
                    }

                    #endregion
                }
                catch
                {
                    GiftBoxErrorCount += 1;
                }
                finally
                {
                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5));
                }
            }

            if(GiftBoxErrorCount == 3)
            {
                throw new Exception("GiftBoxErrorCount == 3");
            }

            #endregion
        }
        private async Task SelectiveTaskVisitWithoutTimer()
        {
            Dictionary<string, string>? queryParams;
            string? uri = "";
            HttpResponseMessage? responce = null;
            string htmlPage = "";
            IDocument? htmlPageDocument = null;
            List<IElement>? htmlElementsVisitWithoutTimer = null;
            List<TaskVisitWithoutTimerModel> tasksVisitWithoutTimer = new List<TaskVisitWithoutTimerModel>();

            for (int visit_tasks_page = 1; visit_tasks_page <= 3; visit_tasks_page++)
            {
                #region get html page of all tasks(max 100 tasks per page)

                queryParams = new Dictionary<string, string>()
                {
                    ["type"] = "redirect",
                    ["page_limit"] = "100",
                    ["page"] = visit_tasks_page.ToString()
                };

                uri = QueryHelpers.AddQueryString($"{base.Task!.Url}/account/visit.html", queryParams);

                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    // set headers to request
                    if (visit_tasks_page == 1)
                    {
                        foreach (var header in base.contextHeaders!)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        } 
                    }

                    if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                    if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                    if (visit_tasks_page == 1)
                    {
                        request.Headers.Add("referer", $"{base.Task.Url}/account/visit.html?type=redirect");
                    }
                    else
                    {
                        request.Headers.Add("referer", $"{base.Task.Url}/account/visit.html?type=redirect&page_limit=100&page={visit_tasks_page - 1}");
                    }

                    // request
                    responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                    if (responce.StatusCode != HttpStatusCode.Redirect && responce.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"responce.StatusCode != HttpStatusCode.Redirect && responce.StatusCode != HttpStatusCode.OK (region get html page of all tasks(max 100 tasks per page), visit_tasks_page={visit_tasks_page})");
                    }

                    htmlPage = await responce.Content.ReadAsStringAsync();
                }

                #endregion

                #region parse all tasks to list && get task link

                htmlPageDocument = await base.htmlParseContext!.OpenAsync(req => req.Content(htmlPage));
                htmlElementsVisitWithoutTimer = htmlPageDocument.QuerySelectorAll("div.letter-list > div.letter.visit-redirect").ToList();

                foreach (var el in htmlElementsVisitWithoutTimer)
                {
                    tasksVisitWithoutTimer.Add(new TaskVisitWithoutTimerModel()
                    {
                        Number = el.QuerySelector("div.top > div.left > span.additional-title.color-grey")!.TextContent.Trim('№', ' ').Split("от")[0] ?? throw new Exception("Ошибка при получении значения атрибута 'span.additional-title.color-grey' элемента 'Number'."),
                        Provider = el.QuerySelector("div.top > div.left > span.additional-title.color-grey > a")!.TextContent.Trim(),
                        Name = el.QuerySelector("div.top > div.left > a")!.TextContent.Trim('«', '»'),
                        Description = el.QuerySelector("div.text")!.TextContent.Trim(),
                        Pay = el.QuerySelector("div.bottom > div.left")!.TextContent.Split(new char[] { '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList().Where(w => w.Contains('.')).FirstOrDefault("null").ToDouble(),
                        CountViewsLeft = el.QuerySelector("div.bottom > div.right > span.color-grey")!.TextContent.Split(new char[] { '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries)[1].ToInteger(0),
                        taskLink = el.QuerySelector("div.top > div.left > a")!.GetAttribute("href") ?? throw new Exception("Ошибка при получении значения атрибута 'href' элемента 'taskLink'.")
                    });
                }

                if(tasksVisitWithoutTimer.Where(task => task.CountViewsLeft == 0).Count() > 0)
                {
                    throw new Exception("Ошибка при получении значения атрибута 'div.bottom > div.right > span.color-grey' элемента 'CountViewsLeft'.tasksVisitWithoutTimer.Where(task => task.CountViewsLeft == 0).Count() > 0");
                }

                (base.Task as GroupSelectiveTaskWithAuth)!.taskVisitWithoutTimer!.countAllTasks += htmlElementsVisitWithoutTimer.Count;

                #endregion 
            }

            #region execution tasks

            // sort by CountViewsLeft
            tasksVisitWithoutTimer.Sort((x, y) => x.CountViewsLeft.CompareTo(y.CountViewsLeft));

            bool checkInstructionDone = false;
            bool checkEmailConfirmDone = false;
            int globalAttemptsToExecuteTasks = 20;
            int attemptsToExecuteOneTask = 2;
            string error_messages = "";

            foreach (var task in tasksVisitWithoutTimer)
            {
                while (attemptsToExecuteOneTask > 0)
                {
                    // get confirmation task link 
                    using (var request = new HttpRequestMessage(HttpMethod.Get, task.taskLink))
                    {
                        // set headers to request
                        foreach (var header in base.contextHeaders)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }

                        if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                        if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                        // request
                        responce = await base.workHttpConnection.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                        var content = await responce.Content.ReadAsStringAsync();

                        if (responce.StatusCode != HttpStatusCode.Found && responce.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception($"get confirmation task link --- responce.StatusCode != HttpStatusCode.Found && responce.StatusCode != HttpStatusCode.OK\n\n\n{error_messages}");
                        }

                        htmlPage = content;
                    }

                    // Проверка контента страницы на наличие не пройденной инструкции, бывает что поле "IsFirstWorkOfAccountAfterRegistration" стоит false, а инструкция все равно не пройдена, поэтому
                    // выскакивает ошибка типа "Index was outside the bounds of the array."
                    if (!checkInstructionDone && htmlPage.Contains("Я прочитал инструкцию"))
                    {
                        base.Task!.Account!.IsFirstExecutionOfTasksAfterRegistration = true;

                        throw new Exception($"execution tasks: htmlPage.Contains('Я прочитал инструкцию') - не пройдена инструкция, (base.Task!.Account!.IsFirstExecutionOfTasksAfterRegistration = true)");
                    }
                    checkInstructionDone = true;

                    // Проверка контента на наличие не подтвержденной почты , бывает что поле "EmailIsConfirmed" стоит true, а почта все равно не подтверждена, поэтому
                    // выскакивает ошибка типа "execution tasks: globalAttemptsToExecuteTasks == 0."
                    if (!checkEmailConfirmDone && htmlPage.Contains("Подтвердите ваш электронный адрес") && htmlPage.Contains("Доступ к разделу заблокирован"))
                    {
                        base.Task!.Account!.EmailIsConfirmed = false;

                        throw new Exception($"execution tasks: htmlPage.Contains(\"Подтвердите ваш электронный адрес\") && htmlPage.Contains(\"Доступ к разделу заблокирован\") - почта не подтверждена, (base.Task!.Account!.EmailIsConfirmed = false)");
                    }
                    checkEmailConfirmDone = true;

                    // проверка на "нельзя выполнять 2 посещения одновременно"
                    if (htmlPage.Contains("Нельзя выполнять 2 посещения одновременно"))
                    {
                        error_messages += "Нельзя выполнять 2 посещения одновременно\n";
                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(3));
                        continue;
                    }

                    // execution confirmation task link 
                    try
                    {
                        htmlPageDocument = await htmlParseContext!.OpenAsync(req => req.Content(htmlPage));
                        task.taskConfirmationLink = $"{base.Task.Url}" + htmlPageDocument.Scripts[htmlPageDocument.Scripts.Length - 1].Text.Split("'")[3];

                        using (var request = new HttpRequestMessage(HttpMethod.Get, task.taskConfirmationLink))
                        {
                            // set headers to request
                            foreach (var header in base.contextHeaders)
                            {
                                request.Headers.Add(header.Key, header.Value);
                            }

                            if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                            if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                            request.Headers.Add("referer", task.taskLink);

                            // request
                            responce = await base.workHttpConnection.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                            var content = await responce.Content.ReadAsStringAsync();

                            if (responce.StatusCode != HttpStatusCode.OK)
                            {
                                throw new Exception($"execution tasks: responce.StatusCode != HttpStatusCode.OK. (responce.StatusCode: {responce.StatusCode})");
                            }
                        }

                        // ONE TASK DONED
                        (base.Task as GroupSelectiveTaskWithAuth)!.taskVisitWithoutTimer!.moneyEarned += task.Pay;
                        (base.Task as GroupSelectiveTaskWithAuth)!.taskVisitWithoutTimer!.countCompletedTasks += 1;

                        attemptsToExecuteOneTask = 0;
                    }
                    catch(Exception e)
                    {
                        error_messages += $"{e.Message}\n";
                        globalAttemptsToExecuteTasks -= 1;
                        attemptsToExecuteOneTask -= 1;
                    }
                    finally
                    {
                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }

                attemptsToExecuteOneTask = 2;

                if(globalAttemptsToExecuteTasks == 0)
                {
                    throw new Exception($"execution tasks: globalAttemptsToExecuteTasks == 0\n\n\n{error_messages}");
                }
            }

            #endregion

            #region clear local memory

            clearLocalMemory();
            void clearLocalMemory()
            {
                queryParams = null;
                uri = null;
                if (responce != null) responce.Dispose();
                htmlPage = "";
                if (htmlPageDocument != null) htmlPageDocument.Dispose();
                if (htmlElementsVisitWithoutTimer != null) htmlElementsVisitWithoutTimer.Clear();
                if (tasksVisitWithoutTimer != null) tasksVisitWithoutTimer.Clear();
            }

            #endregion
        }
        private async Task SelectiveTaskcheckCareerLadder()
        {
            string? uri = "";
            HttpResponseMessage? responce = null;
            string htmlPage = "";
            IDocument? htmlPageDocument = null;

            #region checkCareerLadder

            uri = $"{base.Task!.Url}/account/status.html";

            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                // set headers to request
                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                request.Headers.Add("referer", $"{base.Task.Url}/account/visit.html?type=redirect");

                // request
                responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                if (responce.StatusCode != HttpStatusCode.OK && responce.StatusCode != HttpStatusCode.Redirect)
                {
                    throw new Exception("responce.StatusCode != HttpStatusCode.OK && responce.StatusCode != HttpStatusCode.Redirect");
                }

                htmlPage = await responce.Content.ReadAsStringAsync();
            }

            htmlPageDocument = await base.htmlParseContext!.OpenAsync(req => req.Content(htmlPage));

            string status = htmlPageDocument.QuerySelector("div.status > div.ribbon-wrapper > div.ribbon-front")!.TextContent.Split(',')[0].Trim('\n', ' ') ?? throw new Exception("Ошибка при получении значения атрибута 'div.status > div.ribbon-wrapper > div.ribbon-front > i' элемента 'status'.");
            double statuspoints = htmlPageDocument.QuerySelector("div.status > div.ribbon-wrapper > div.ribbon-front")!.TextContent.Split(',')[1].Trim('\n', ' ').Split(' ')[0].ToDouble();
            int statusprogress = htmlPageDocument.QuerySelector("div.alert.alert-success > div.result-bar.margin-top-10.tooltips > div.count")!.TextContent.Trim('%').ToInteger(defaultValue: 0);

            var valueStatus = status switch
            {
                "новичок" => Status.beginner,
                "студент" => Status.student,
                "опытный" => Status.beginner,
                "продвинутый" => Status.advanced,
                "активист" => Status.activist,
                "специалист" => Status.specialist,
                "эксперт" => Status.expert,
                "мастер" => Status.master,
                "гранд-мастер" => Status.grandmaster,
                _ => throw new Exception($"oneLogAccounResult!.CareerLadder.Status = status switch --> not found, status was: {status}")
            };

            base.AddCareerLadderData(valueStatus, statuspoints, statusprogress, (int)valueStatus);

            #endregion

            #region clear local memory

            clearLocalMemory();
            void clearLocalMemory()
            {
                uri = null;
                if (responce != null) responce.Dispose();
                htmlPage = "";
                if (htmlPageDocument != null) htmlPageDocument.Dispose();
            }

            #endregion
        }
        private async Task SelectiveTaskCheckOnNeedToConfirmAccountEmail()
        {
            string? uri = "";
            HttpResponseMessage? responce = null;
            string htmlPage = "";
            IDocument? htmlPageDocument = null;

            //get instruction
            uri = $"{base.Task.Url}/account/settings.html";

            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                // set headers to request
                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (request.Headers.Contains("cookie")) request.Headers.Remove("cookie");
                if (request.Headers.Contains("referer")) request.Headers.Remove("referer");

                request.Headers.Add("referer", "http://socpublic.com/account/index.html");

                // request
                responce = await base.workHttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                if (responce.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("responce.StatusCode != HttpStatusCode.OK");
                }

                htmlPage = await responce.Content.ReadAsStringAsync();

                htmlPageDocument = await base.htmlParseContext!.OpenAsync(req => req.Content(htmlPage));

                var selector = htmlPageDocument.QuerySelector("div.tab-content.no-bottom-space");

                if (selector != null && selector.TextContent.Contains("Email подтверждён, это хорошо!"))
                {
                    base.Task!.Account!.EmailIsConfirmed = true;
                }
                else
                {
                    base.Task!.Account!.EmailIsConfirmed = false;
                }
            }
        }
        private async Task SelectiveTaskConfirmRegisterAccountByLinkFromEmail()
        {
            CancellationTokenSource confirmRegisterAccountCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            try
            {
                // confirm

                if (registerConfirmationLink == null)
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

                        if (statusPanel != null && (statusPanel.TextContent.Contains("Ваш email адрес успешно подтверждён.") || statusPanel.TextContent.Contains("подтверждён")))
                        {
                            Task!.Account!.EmailIsConfirmed = true;
                        }
                        else
                        {
                            Task!.Account!.EmailIsConfirmed = false;
                            throw new Exception($"Task!.Account!.EmailIsConfirmed = false;\n{statusPanel?.TextContent}");
                        }
                    }
                    catch (Exception e)
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
                throw new Exception($"class SelectiveGroupTaskWithAuth(), method SelectiveTaskConfirmRegisterAccountByLinkFromEmail(), error {e.Message}");
            }
        }
        #endregion

        internal async Task TaskChangeResultStatus_SendServerReport(TaskResultStatus resultStatus, Exception? exception = null)
        {
            try
            {
                if(exception != null)
                {
                    TaskErrorStatus errorStatus = exception switch
                    {
                        System.Net.Http.HttpRequestException => TaskErrorStatus.ConnectionError,
                        System.Threading.Tasks.TaskCanceledException => TaskErrorStatus.ConnectionError,
                        _ => TaskErrorStatus.AnotherError
                    };

                    // дорабатывать костыль
                    if(errorStatus == TaskErrorStatus.AnotherError)
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
                            exception.Message.Contains("InternalServerError") ||
                            exception.Message.Contains("NotImplemented") ||
                            exception.Message.Contains("BadGateway") ||
                            exception.Message.Contains("GatewayTimeout") ||
                            exception.Message.Contains("NotFound") ||
                            exception.Message.Contains("not indicate success") ||
                            exception.Message.Contains("task was canceled") ||
                            exception.Message.Contains("Bad Request") ||
                            exception.Message.Contains("ServiceUnavailable"))
                        {
                            errorStatus = TaskErrorStatus.ConnectionError;
                        }
                    }

                    base.Task.ErrorStatus = errorStatus;
                    base.Task.ErrorMessage = exception.Message;
                }

                base.SetEndTimeWorkOneLogAccount(DateTime.UtcNow);

                base.Task.ResultStatus = resultStatus;

                if(base.cookieContainer != null)
                {
                    base.Task.Account!.LastCookies = base.cookieContainer.GetAllCookies().ToList();
                }
                if (base.contextHeaders != null)
                {
                    base.Task.Account!.LastHeaders = base.contextHeaders;
                }

                await base.SendTaskChangedResultStatusReport();
            }
            catch(Exception e)
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
                    try
                    {
                        UnRegisterHubMethods();
                        base.Dispose_PlatformSocpublicComTask_Data();
                    }
                    catch { }
                    // TODO: освободить управляемое состояние (управляемые объекты)
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей

                //base.DisposeData();

                disposedValue = true;
            }
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        // ~TaskVisitWithoutTimer()
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
