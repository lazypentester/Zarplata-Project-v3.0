using CommonModels.Client;
using CommonModels.ProjectTask.Platform;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Linq;

namespace SocpublicCom.Interfaces
{
    internal interface IProxySetable
    {
        private protected static async Task<bool> CheckPlatformAccountHistoryProxy(Client client, Account account, HubConnection serverHubConnection)
        {
            bool NeedToGetNewProxy = true;

            // check account proxy
            if (account.History != null && account.History.Count() > 0)
            {
                List<CheckedProxy> history_proxies = new List<CheckedProxy>();
                List<CheckedProxy> without_dublicate_proxies = new List<CheckedProxy>();

                account.History!.ForEach(history =>
                {
                    if (history.Proxy != null)
                        history_proxies.Add(history.Proxy);
                });

                // add reged proxy
                if(account.RegedProxy != null)
                {
                    history_proxies.Add(account.RegedProxy);
                }

                // clear repeat proxies
                foreach(var proxy in history_proxies)
                {
                    if(proxy != null && without_dublicate_proxies.Where(p => p.FullProxyAddress == proxy.FullProxyAddress).Count() == 0)
                    {
                        without_dublicate_proxies.Add(proxy);
                    }
                }

                if (without_dublicate_proxies.Count() > 0)
                {
                    try
                    {
                        CheckRequirements checkRequirements = new CheckRequirements(
                            EarningSiteTaskUrls.SocpublicComUrl.http,
                            EarningSiteTaskUrls.SocpublicComUrl.https,
                            new CheckedProxy.CPProtocol[] { CheckedProxy.CPProtocol.http, CheckedProxy.CPProtocol.socks5, CheckedProxy.CPProtocol.socks4 },
                            new CheckedProxy.SupportsSecureConnectionState[] { CheckedProxy.SupportsSecureConnectionState.Support, CheckedProxy.SupportsSecureConnectionState.NotSupport, CheckedProxy.SupportsSecureConnectionState.Undefined },
                            new CheckedProxy.AnonymousState[] { CheckedProxy.AnonymousState.Anonymous },
                            5000,
                            new CheckedProxy.CPWorkState[] { CheckedProxy.CPWorkState.Work }
                            );

                        await serverHubConnection.SendAsync("check_platform_account_history_proxy", without_dublicate_proxies, checkRequirements);

                        NeedToGetNewProxy = false;
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"interface IProxySetable, method CheckPlatformAccountHistoryProxy(), error {e.Message}");
                    }
                }
            }

            return NeedToGetNewProxy;
        }

        private protected static async Task GetNewProxy(Client client, Account? account, HubConnection serverHubConnection)
        {
            CheckRequirements checkRequirements = new CheckRequirements(
                            EarningSiteTaskUrls.SocpublicComUrl.http,
                            EarningSiteTaskUrls.SocpublicComUrl.https,
                            new CheckedProxy.CPProtocol[] { CheckedProxy.CPProtocol.http, CheckedProxy.CPProtocol.socks5, CheckedProxy.CPProtocol.socks4 },
                            new CheckedProxy.SupportsSecureConnectionState[] { CheckedProxy.SupportsSecureConnectionState.Support, CheckedProxy.SupportsSecureConnectionState.NotSupport, CheckedProxy.SupportsSecureConnectionState.Undefined },
                            new CheckedProxy.AnonymousState[] { CheckedProxy.AnonymousState.Anonymous },
                            5000,
                            new CheckedProxy.CPWorkState[] { CheckedProxy.CPWorkState.Work }
                            );

            //await serverHubConnection.InvokeAsync("get_platform_account_new_proxy");
            await serverHubConnection.SendAsync("get_platform_account_new_proxy", client, account, checkRequirements);
        }

        private protected static CheckedProxy? CheckCheckedProxy(List<CheckedProxy> proxies, List<OneLogAccounResult>? account_history, Account account)
        {
            CheckedProxy? bestWorkProxy = null;

            // clear bad proxies
            try
            {
                if (proxies.Any() && account_history != null && account_history.Count() > 0)
                {
                    OneLogAccounResult? lastAccLog = account_history.LastOrDefault();

                    if(lastAccLog != null && lastAccLog.Proxy != null && lastAccLog.Operations != null && lastAccLog.Operations.Count() > 0)
                    {
                        // remove unstable(bad connection) proxy
                        try
                        {
                            Operation? executeTasksWithoutTimer = lastAccLog.Operations.Where(o => o.Type == OperationType.executeTasks).FirstOrDefault();

                            if (executeTasksWithoutTimer != null && executeTasksWithoutTimer.Status == OperationStatus.Failure)
                            {
                                if (proxies.Where(p => p.FullProxyAddress == lastAccLog.Proxy!.FullProxyAddress).Any())
                                {
                                    proxies.RemoveAll(p => p.FullProxyAddress == lastAccLog.Proxy!.FullProxyAddress);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"in block 'remove unstable(bad connection) proxy', {e.Message}");
                        }
                    }
                    else if(lastAccLog!.Operations!.Count() == 0)
                    {
                        proxies.RemoveAll(p => p.FullProxyAddress == lastAccLog!.Proxy?.FullProxyAddress);
                    }

                    // remove proxy with bad region or bad proxy(headers in responce)
                    List<CheckedProxy> bad_region_proxy = new List<CheckedProxy>();
                    try
                    {
                        foreach (var checked_proxy in proxies)
                        {
                            bool existOneGoodEarnMoneyLogWithCurrentProxy = false;
                            int countLogsWithCurrentProxy = 0;

                            foreach (var log in account_history)
                            {
                                if (log != null && log.Proxy != null && log.Proxy.FullProxyAddress == checked_proxy.FullProxyAddress)
                                {
                                    countLogsWithCurrentProxy++;

                                    if (log.MoneyMainBalancePlus != null && log.MoneyMainBalancePlus > 0)
                                    {
                                        existOneGoodEarnMoneyLogWithCurrentProxy = true;
                                        break;
                                    }
                                }
                            }

                            if (!existOneGoodEarnMoneyLogWithCurrentProxy && countLogsWithCurrentProxy >= 2)
                            {
                                bad_region_proxy.Add(checked_proxy);

                                // remove history logs with current proxy
                                try
                                {
                                    account.History?.RemoveAll(log => log.Proxy == null || log.Proxy.FullProxyAddress == checked_proxy.FullProxyAddress);
                                }
                                catch (Exception e)
                                {
                                    throw new Exception($"in block 'remove history logs with current proxy 1', {e.Message}");
                                }

                            }
                            else if (existOneGoodEarnMoneyLogWithCurrentProxy && countLogsWithCurrentProxy >= 3)
                            {
                                bad_region_proxy.Add(checked_proxy);

                                // remove history logs with current proxy
                                try
                                {
                                    account.History?.RemoveAll(log => log.Proxy == null || log.Proxy.FullProxyAddress == checked_proxy.FullProxyAddress);
                                }
                                catch (Exception e)
                                {
                                    throw new Exception($"in block 'remove history logs with current proxy 2', {e.Message}");
                                }
                            }
                        }
                        foreach (var bad_r_p in bad_region_proxy)
                        {
                            proxies.RemoveAll(p => p.FullProxyAddress == bad_r_p.FullProxyAddress);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"in block 'remove proxy with bad region or bad proxy(headers in responce)', {e.Message}");
                    }
                }
            }
            catch(Exception e)
            {
                throw new Exception($"interface IProxySetable, method CheckCheckedProxy(), block 'clear bad proxies', error {e.Message}");
            }

            // get the the best and fastest proxy of all checked proxy, if exist
            try
            {
                if (proxies.Count() > 1)
                {
                    var minimalSpeedOfProxies = proxies.Min(p => p.speed);
                    if (minimalSpeedOfProxies != null)
                    {
                        bestWorkProxy = proxies.Where(p => p.speed == minimalSpeedOfProxies).FirstOrDefault();
                    }
                }
                else
                {
                    bestWorkProxy = proxies.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                throw new Exception($"interface IProxySetable, method CheckCheckedProxy(), block 'get the the best and fastest proxy of all checked proxy, if exist', error {e.Message}");
            }

            return bestWorkProxy;
        }
    }
}
