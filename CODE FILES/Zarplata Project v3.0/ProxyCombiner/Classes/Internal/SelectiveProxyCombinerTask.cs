using CommonModels.Client;
using CommonModels.ProjectTask.ProxyCombiner;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.DefaultCombineTask;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.SpecialCombineTask;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Net;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using ProxyCombinerTaskType = CommonModels.ProjectTask.ProxyCombiner.ProxyCombinerTaskEnums.ProxyCombinerTaskType;
using ProxyCombiner.Classes.Internal.SelectiveTask;
using CommonModels.ProjectTask.ProxyCombiner.Models;
using System;
using CommonModels.ProjectTask.Platform;
using CommonModels.ProjectTask.EarningSite;
using ProxyCombiner.Classes.Public;

namespace ProxyCombiner.Classes.Internal
{
    public class SelectiveProxyCombinerTask : IDisposable
    {
        internal ProxyCombinerTask Task { get; set; }
        internal HubConnection ServerHubConnection { get; set; }
        private HttpClient ProxyParseHttpClient { get; set; }
        private CookieContainer ProxyParseCookieContainer { get; set; }
        private Client client { get; set; }

        private bool disposedValue;

        public SelectiveProxyCombinerTask(ProxyCombinerTask task, HubConnection serverHubConnection, HttpClient proxyParseHttpClient, CookieContainer proxyParseCookieContainer, Client client)
        {
            Task = task;
            ServerHubConnection = serverHubConnection;
            ProxyParseHttpClient = proxyParseHttpClient;
            ProxyParseCookieContainer = proxyParseCookieContainer;
            this.client = client;
        }

        public async Task StartWork()
        {
            switch (this.Task.InternalType)
            {
                case ProxyCombinerTaskType.SpecialCombine:
                    await doProxyCombinerWork_SpecialCombine();
                    break;
                case ProxyCombinerTaskType.DefaultCombine:
                    await doProxyCombinerWork_DefaultCombine();
                    break;
                case ProxyCombinerTaskType.Parse:
                    await doProxyCombinerWork_Parse();
                    break;
                case ProxyCombinerTaskType.Check:
                    await doProxyCombinerWork_Check();
                    break;
                default:
                    break;
            }
        }

        private async Task doProxyCombinerWork_SpecialCombine()
        {
            //removeafterthis!!!--------
            Console.WriteLine("doProxyCombinerWork_SpecialCombine");

            #region Parse Process

            Parse parse = new Parse(ProxyParseHttpClient, ProxyParseCookieContainer, client);
            List<ParsedProxy> parsedProxies = new List<ParsedProxy>();

            // init data
            try
            {
                var init = await parse.InitWebContextData();
                if (init == false)
                {
                    throw new Exception($"init == false");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_SpecialCombine(), error {e.Message}");
            }

            // parse proxy

            //removeafterthis!!!--------
            Console.WriteLine("loading parse proxy");
            try
            {
                SiteParseBalancer site;

                int countPP = (this.Task as SpecialCombineTask)!.parseRequirements.countOfParsedProxy;

                while (parsedProxies.Count < countPP)
                {
                    #region get and filter site from balancer

                    List<SiteParseBalancer>? siteParseBalancers = await ServerHubConnection.InvokeAsync<List<SiteParseBalancer>?>("proxy_combiner_get_site_parse_balancer_collection", Task, client);

                    if(siteParseBalancers == null)
                    {
                        //removeafterthis!!!--------
                        await SaveMessage.Save(client!.ID!, "siteParseBalancers == null");
                        throw new Exception("siteParseBalancers == null");
                    }

                    #region remove after

                    List<SiteParseBalancer> sitesToRemoveTEMP = siteParseBalancers.Where(site => site.Site == MethodName.ParseSite_Proxy_dailyDotCom ||
                    site.Site == MethodName.ParseSite_ProxyserversDotPro ||
                    site.Site == MethodName.ParseSite_ProxyscrapeDotCom ||
                    site.Site == MethodName.ParseSite_IproyalDotCom ||
                    site.Site == MethodName.ParseSite_Free_proxy_listDotCom ||
                    site.Site == MethodName.ParseSite_IpaddressDotCom ||
                    site.Site == MethodName.ParseSite_Best_proxiesDotRu
                    ).ToList();

                    foreach (var site_ in sitesToRemoveTEMP)
                    {
                        siteParseBalancers.Remove(site_);
                    }

                    #endregion

                    if (siteParseBalancers.Count == 0)
                    {
                        //removeafterthis!!!--------
                        await SaveMessage.Save(client!.ID!, "siteParseBalancers.Count == 0");
                        throw new Exception("siteParseBalancers.Count == 0");
                    }

                    // filter by minimal count parse
                    int minimalCountParse = siteParseBalancers.Min(s => s.CountParseSite);
                    List<SiteParseBalancer> sitesFilteredByMinimalParseCount = siteParseBalancers.Where(s => s.CountParseSite == minimalCountParse).ToList();

                    if(sitesFilteredByMinimalParseCount.Count == 0)
                    {
                        //removeafterthis!!!--------
                        await SaveMessage.Save(client!.ID!, "sitesFilteredByMinimalParseCount.Count == 0");
                        Console.WriteLine("sitesFilteredByMinimalParseCount.Count == 0");
                        throw new Exception("sitesFilteredByMinimalParseCount.Count == 0");
                    }

                    // filter by minimal parse time (latest parse time)
                    DateTime minimalTimeParse = sitesFilteredByMinimalParseCount.Min(s => s.LastTimeParseSite);
                    List<SiteParseBalancer> sitesFilteredByMinimalTimeParse = sitesFilteredByMinimalParseCount.Where(
                        s => (s.LastTimeParseSite.Month == minimalTimeParse.Month && s.LastTimeParseSite.Day == minimalTimeParse.Day && s.LastTimeParseSite.Hour == minimalTimeParse.Hour && s.LastTimeParseSite.Minute == minimalTimeParse.Minute)
                        ).ToList();

                    if (sitesFilteredByMinimalTimeParse.Count == 0)
                    {
                        //removeafterthis!!!--------
                        await SaveMessage.Save(client!.ID!, "sitesFilteredByMinimalTimeParse.Count == 0");
                        Console.WriteLine("sitesFilteredByMinimalTimeParse.Count == 0");
                        throw new Exception("sitesFilteredByMinimalTimeParse.Count == 0");
                    }
                    else if(sitesFilteredByMinimalTimeParse.Count > 1)
                    {
                        if(sitesFilteredByMinimalTimeParse.Where(x => x.LastClientIdParseSite == this.client.ID).Count() < sitesFilteredByMinimalTimeParse.Count)
                        {
                            sitesFilteredByMinimalTimeParse.RemoveAll(s => s.LastClientIdParseSite == this.client.ID);
                        }
                    }

                    if(sitesFilteredByMinimalTimeParse.Count == 0)
                    {
                        //removeafterthis!!!--------
                        await SaveMessage.Save(client!.ID!, "sitesFilteredByMinimalTimeParse.Count == 0 after else if(sitesFilteredByMinimalTimeParse.Count > 1) block");
                        Console.WriteLine("sitesFilteredByMinimalTimeParse.Count == 0 after else if(sitesFilteredByMinimalTimeParse.Count > 1) block");
                        throw new Exception("sitesFilteredByMinimalTimeParse.Count == 0 after else if(sitesFilteredByMinimalTimeParse.Count > 1) block");
                    }

                    site = sitesFilteredByMinimalTimeParse.First();

                    #endregion

                    // parse proxy from one site

                    //removeafterthis!!!--------
                    Console.WriteLine("parse proxy from one site");

                    try
                    {
                        parsedProxies.AddRange(await parse.ParseProxy(site.Site));

                        //removeafterthis!!!--------
                        Console.WriteLine($"Спарсено {parsedProxies.Count} проксей с одного сайта..");
                    }
                    catch(Exception e)
                    {
                        await ServerHubConnection.SendAsync("proxy_combiner_errors_log", site);

                        await SaveMessage.Save(client!.ID!, $"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_SpecialCombine().arsedProxies.AddRange(await parse.ParseProxy(site.Site)), error {e.Message}");
                        throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_SpecialCombine().arsedProxies.AddRange(await parse.ParseProxy(site.Site)), error {e.Message}");
                    }

                    // update parsed site to siteParseBalancer
                    try
                    {
                        site.CountParseSite += 1;
                        site.LastTimeParseSite = DateTime.UtcNow;
                        site.LastClientIdParseSite = this.client.ID!;

                        await ServerHubConnection.SendAsync("proxy_combiner_update_site_parse_balancer_collection", site);
                        //bool updated = await ServerHubConnection.InvokeAsync<bool>("proxy_combiner_update_site_parse_balancer_collection", site);
                        //if (!updated)
                        //{
                        //    throw new Exception("!updated");
                        //}
                    }
                    catch(Exception e)
                    {
                        //await ServerHubConnection.SendAsync("proxy_combiner_errors_log", site);

                        await SaveMessage.Save(client!.ID!, $"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_SpecialCombine().updateParsedSiteToSiteParseBalancer, error {e.Message}");
                        throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_SpecialCombine().updateParsedSiteToSiteParseBalancer, error {e.Message}");
                    }
                }

                //removeafterthis!!!--------
                Console.WriteLine();
                Console.WriteLine($"ВСЕГО Спарсено {parsedProxies.Count} проксей");

                if (parsedProxies.Count == 0)
                {
                    await SaveMessage.Save(client!.ID!, "sitesFilteredByMinimalTimeParse.Count == 0");
                    throw new Exception("proxies.Count == 0");
                }
                else
                {
                    parsedProxies.RemoveRange(countPP, parsedProxies.Count - countPP);
                }
            }
            catch (Exception e)
            {
                //removeafterthis!!!--------
                Console.WriteLine();
                Console.WriteLine($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_Parse().parse.ParseProxy(), error {e.Message}");

                parse.Dispose();

                await SaveMessage.Save(client!.ID!, $"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_Parse().parse.ParseProxy(), error {e.Message}");
                //throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_Parse().parse.ParseProxy(), error {e.Message}");
            }
            finally
            {
                parse.Dispose();
            }

            #endregion

            #region Check Process

            if (parsedProxies.Count > 0)
            {
                try
                {
                    //removeafterthis!!!--------
                    Console.WriteLine("check proxy");

                    Check check = new Check(client);
                    CheckedProxy? checkedProxy = null;
                    List<ParsedProxy> clearParsedProxy = new List<ParsedProxy>();

                    // init data
                    try
                    {
                        var init = check.InitWebContextData();
                        if (init == false)
                        {
                            throw new Exception($"init == false");
                        }
                    }
                    catch (Exception e)
                    {
                        await SaveMessage.Save(client!.ID!, $"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_SpecialCombine().initData, error {e.Message}");
                        throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_SpecialCombine().initData, error {e.Message}");
                    }

                    // get and clear reserved proxies
                    try
                    {
                        EarningSiteTaskEnums.EarningSiteEnum reservedPlatform = (this.Task as SpecialCombineTask)!.specialRequirements.ReservedPlatform;

                        List<AccountReservedProxy>? reservedProxies = await ServerHubConnection.InvokeAsync<List<AccountReservedProxy>>("proxy_combiner_get_account_reserved_proxy_collection", client, reservedPlatform);

                        if (reservedProxies == null)
                        {
                            await SaveMessage.Save(client!.ID!, $"reservedProxies == null");
                            throw new Exception("reservedProxies == null");
                        }

                        foreach (var proxy in parsedProxies)
                        {
                            if (reservedProxies.Where(p => p.Proxy.FullProxyAddress == proxy.FullProxyAddress).Count() == 0)
                            {
                                clearParsedProxy.Add(proxy);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        await SaveMessage.Save(client!.ID!, $"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_SpecialCombine().getAndClearReservedProxies, error {e.Message}");
                        throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_SpecialCombine().getAndClearReservedProxies, error {e.Message}");
                    }

                    // check proxy
                    try
                    {
                        foreach (var proxy in clearParsedProxy)
                        {
                            checkedProxy = await check.CheckOneProxy(proxy, (this.Task as SpecialCombineTask)!.checkRequirements);

                            if (checkedProxy != null)
                            {
                                break;
                            }

                            //removeafterthis!!!--------
                            Console.WriteLine($"{proxy.FullProxyAddress}");
                        }
                    }
                    catch (Exception e)
                    {
                        check.Dispose();

                        await SaveMessage.Save(client!.ID!, $"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_SpecialCombine().check.CheckOneProxy(), error {e.Message}");
                        throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_SpecialCombine().check.CheckOneProxy(), error {e.Message}");
                    }
                    finally
                    {
                        check.Dispose();
                    }

                    // push checked proxy to AccountReservedProxyCollection
                    if ((this.Task as SpecialCombineTask)!.specialRequirements.PlatformAccountId != null && checkedProxy != null)
                    {
                        try
                        {
                            AccountReservedProxy accountReservedProxy = new AccountReservedProxy(
                                (this.Task as SpecialCombineTask)!.specialRequirements.PlatformAccountId!,
                                (this.Task as SpecialCombineTask)!.specialRequirements.ReservedPlatform,
                                DateTime.UtcNow,
                                checkedProxy!.proxy
                                );

                            await ServerHubConnection.SendAsync("proxy_combiner_push_checked_proxy_to_account_reserved_proxy_collection", client, accountReservedProxy);
                            //bool pushed = await ServerHubConnection.InvokeAsync<bool>("proxy_combiner_push_checked_proxy_to_account_reserved_proxy_collection", Task, client, accountReservedProxy);
                            //if (!pushed)
                            //{
                            //    throw new Exception("!pushed");
                            //}
                        }
                        catch (Exception e)
                        {
                            await SaveMessage.Save(client!.ID!, $"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_SpecialCombine().pushCheckedPproxyToAccountReservedProxyCollection, error {e.Message}");
                            throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_SpecialCombine().pushCheckedPproxyToAccountReservedProxyCollection, error {e.Message}");
                        }
                    }
                    else if (checkedProxy != null)
                    {
                        try
                        {
                            AccountReservedProxy accountReservedProxy = new AccountReservedProxy(
                                "",
                                (this.Task as SpecialCombineTask)!.specialRequirements.ReservedPlatform,
                                DateTime.UtcNow,
                                checkedProxy!.proxy
                                );

                            await ServerHubConnection.SendAsync("proxy_combiner_push_checked_proxy_autoreg_to_account_reserved_proxy_collection", client, accountReservedProxy);
                            //bool pushed = await ServerHubConnection.InvokeAsync<bool>("proxy_combiner_push_checked_proxy_to_account_reserved_proxy_collection", Task, client, accountReservedProxy);
                            //if (!pushed)
                            //{
                            //    throw new Exception("!pushed");
                            //}
                        }
                        catch (Exception e)
                        {
                            //removeafterthis!!!--------
                            await SaveMessage.Save(client!.ID!, $"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_SpecialCombine().pushCheckedPproxyToAccountReservedProxyCollection, error {e.Message}");
                            throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_SpecialCombine().pushCheckedPproxyToAccountReservedProxyCollection, error {e.Message}");
                        }
                    }

                    // save result proxy
                    (this.Task as SpecialCombineTask)!.result = checkedProxy;
                }
                catch (Exception e)
                {
                    //removeafterthis!!!--------
                    Console.WriteLine();
                    Console.WriteLine($"Ошибка при чекинге {e.Message}");

                    throw new Exception(e.Message);
                }
            }
            else
            {
                (this.Task as SpecialCombineTask)!.result = null;
            }

            #endregion
        }

        private async Task doProxyCombinerWork_DefaultCombine()
        {
            #region Parse Process

            Parse parse = new Parse(ProxyParseHttpClient, ProxyParseCookieContainer, client);
            List<ParsedProxy> parsedProxies = new List<ParsedProxy>();

            Random random = new Random();
            int currentMethodIndex = 0;
            List<int> previousMethodIndex = new List<int>();

            // init data
            try
            {
                var init = await parse.InitWebContextData();
                if (init == false)
                {
                    throw new Exception($"init == false");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_DefaultCombine(), error {e.Message}");
            }

            // parse proxy
            try
            {
                int countPP = (this.Task as DefaultCombineTask)!.parseRequirements.countOfParsedProxy;

                while (parsedProxies.Count < countPP)
                {
                    if (previousMethodIndex.Count == 56)
                    {
                        break;
                    }

                    do
                    {
                        currentMethodIndex = random.Next(1, 56);
                    }
                    while (previousMethodIndex.Contains(currentMethodIndex));

                    // parse proxy from one site
                    parsedProxies.AddRange(await parse.ParseProxy((MethodName)currentMethodIndex));

                    previousMethodIndex.Add(currentMethodIndex);
                }

                if (parsedProxies.Count == 0)
                {
                    throw new Exception("proxies.Count == 0");
                }

                parsedProxies.RemoveRange(countPP, parsedProxies.Count - countPP);
            }
            catch (Exception e)
            {
                parse.Dispose();

                throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_DefaultCombine().parse.ParseProxy(), error {e.Message}");
            }
            finally
            {
                parse.Dispose();
            }

            #endregion

            #region Check Process

            Check check = new Check(client);
            List<CheckedProxy>? checkedProxies = null;

            // init data
            try
            {
                var init = check.InitWebContextData();
                if (init == false)
                {
                    throw new Exception($"init == false");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_DefaultCombine().initData, error {e.Message}");
            }

            // check proxy
            try
            {
                checkedProxies = await check.CheckListProxy(parsedProxies, (this.Task as DefaultCombineTask)!.checkRequirements);
            }
            catch (Exception e)
            {
                check.Dispose();

                throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_DefaultCombine().check.CheckListProxy(), error {e.Message}");
            }
            finally
            {
                check.Dispose();
            }

            (this.Task as DefaultCombineTask)!.result = checkedProxies;

            #endregion
        }

        private async Task doProxyCombinerWork_Parse()
        {
            Parse parse = new Parse(ProxyParseHttpClient, ProxyParseCookieContainer, client);
            List<ParsedProxy> parsedProxies = new List<ParsedProxy>();

            Random random = new Random();
            int currentMethodIndex = 0;
            List<int> previousMethodIndex = new List<int>();

            // init data
            try
            {
                var init = await parse.InitWebContextData();
                if (init == false)
                {
                    throw new Exception($"init == false");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_Parse().initData, error {e.Message}");
            }

            // parse proxy
            try
            {
                int countPP = (this.Task as ParseTask)!.parseRequirements.countOfParsedProxy;

                // parse proxy from one site
                //parsedProxies.AddRange(await parse.ParseProxy(MethodName.ParseSite_FreeproxyDotlunaproxyDotCom));

                while (parsedProxies.Count < countPP)
                {
                    if (previousMethodIndex.Count == 56)
                    {
                        break;
                    }

                    do
                    {
                        currentMethodIndex = random.Next(1, 56);
                    }
                    while (previousMethodIndex.Contains(currentMethodIndex));
                    //while (previousMethodIndex.Contains(currentMethodIndex) || temp_id.Contains(currentMethodIndex));

                    // parse proxy from one site
                    parsedProxies.AddRange(await parse.ParseProxy((MethodName)currentMethodIndex));

                    previousMethodIndex.Add(currentMethodIndex);
                }

                if (parsedProxies.Count == 0)
                {
                    throw new Exception("proxies.Count == 0");
                }

                parsedProxies.RemoveRange(countPP, parsedProxies.Count - countPP);
            }
            catch (Exception e)
            {
                parse.Dispose();

                throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_Parse().parse.ParseProxy(), error {e.Message}");
            }
            finally
            {
                parse.Dispose();
            }

            (this.Task as ParseTask)!.result = parsedProxies;
        }

        private async Task doProxyCombinerWork_Check()
        {
            //removeafterthis!!!--------
            Console.WriteLine("doProxyCombinerWork_Check");

            Check check = new Check(client);
            List<CheckedProxy>? checkedProxies = null;

            // init data
            try
            {
                var init = check.InitWebContextData();
                if (init == false)
                {
                    throw new Exception($"init == false");
                }
            }
            catch(Exception e)
            {
                //removeafterthis!!!--------
                await SaveMessage.Save(client!.ID!, $"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_Check().initData, error {e.Message}");
                throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_Check().initData, error {e.Message}");
            }

            //removeafterthis!!!--------
            Console.WriteLine("check proxy");

            // check proxy
            try
            {
                checkedProxies = await check.CheckListProxy((this.Task as CheckTask)!.parsedProxyList, (this.Task as CheckTask)!.checkRequirements);

                //removeafterthis!!!--------
                checkedProxies.ForEach(p => Console.WriteLine(p.FullProxyAddress));
            }
            catch (Exception e)
            {
                check.Dispose();

                //removeafterthis!!!--------
                await SaveMessage.Save(client!.ID!, $"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_Check().check.CheckListProxy(), error {e.Message}");
                throw new Exception($"class SelectiveProxyCombinerTask(), method doProxyCombinerWork_Check().check.CheckListProxy(), error {e.Message}");
            }
            finally
            {
                check.Dispose();
            }

            (this.Task as CheckTask)!.result = checkedProxies;
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

                    this.Task.ErrorStatus = errorStatus;
                    this.Task.ErrorMessage = exception.Message;
                }

                this.Task.ResultStatus = resultStatus;

                await SendTaskChangedResultStatusReport();
            }
            catch (Exception e)
            {
                //removeafterthis!!!--------
                await SaveMessage.Save(client!.ID!, $"TaskChangeResultStatus_SendServerReport {e.Message}");
                throw new Exception(e.Message);
            }
        }

        private protected async Task SendTaskChangedResultStatusReport()
        {
            try
            {
                await ServerHubConnection.SendAsync($"proxy_combiner_{Task.InternalType.ToString().ToLower()}_task_status_changed", Task);
            }
            catch (Exception e)
            {
                //removeafterthis!!!--------
                await SaveMessage.Save(client!.ID!, $"SendTaskChangedResultStatusReport {e.Message}");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты)
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                disposedValue = true;
            }
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        // ~SelectiveTask()
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
