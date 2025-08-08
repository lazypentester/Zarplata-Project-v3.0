using CommonModels.Client;
using CommonModels.ClientLibraries.ProjectTask;
using Microsoft.AspNetCore.SignalR.Client;
using CommonModels.ProjectTask.ProxyCombiner;
using System.Net;
using AngleSharp;
using CommonModels.UserAgentClasses;
using Jint;
using Tesseract;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models;
using static CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models.CheckedProxy;
using System.Diagnostics;
using System.Net.Sockets;
using Newtonsoft.Json;
using ProxyCombiner.Classes.Public;

namespace ProxyCombiner.Classes.Internal.SelectiveTask
{
    internal class Check : WebClientTask, IDisposable
    {
        private int defaultProxyCheckRequestTimeoutMs { get; set; } = 5000;
        private Client client { get; set; }

        private string[] httpProxyJudgeLinks = new string[4]
{
            "http://azenv.net/",
            "http://www.meow.org.uk/cgi-bin/env.pl",
            "http://wfuchs.de/azenv.php",
            "http://mojeip.net.pl/asdfa/azenv.php",
};
        private string[] httpsProxyJudgeLinks = new string[5]
        {
            "https://wfuchs.de/azenv.php",
            "https://mojeip.net.pl/asdfa/azenv.php",
            "https://www.proxy-listen.de/azenv.php",
            "https://pool.proxyspace.pro/judge.php",
            "https://zcbs.nl/cgi-bin/printenv.pl"
        };

        private bool disposedValue;

        internal Check(
            Client client)
        {
            this.client = client;
        }

        #region WebClientTask Overrided Methods

        internal bool InitWebContextData()
        {
            // init this data
            var init = base.SetWebContextData(
                contextUserA: null,
                contextH: null,
                htmlParseC: null,
                jsExecuteC: null,
                tesseractExecuteC: null);
            if (init == false)
            {
                throw new Exception($"class ProxyCombinerTask(), method InitWebContextData(), error init == false");
            }

            return true;
        }
        protected override UserAgent? CreateNewRandomUserAgent()
        {
            return null;
        }
        protected override CookieCollection? CreateNewRandomCookieCollection()
        {
            return null;
        }
        protected override Dictionary<string, List<string>>? CreateNewRandomHeaderCollection(UserAgent userAgent)
        {
            return null;
        }

        #endregion

        #region Check Proxy Methods

        internal async Task<CheckedProxy?> CheckOneProxy(ParsedProxy parsedProxy, CheckRequirements checkRequirements)
        {
            if (checkRequirements.requestTimeoutMs != 0) defaultProxyCheckRequestTimeoutMs = checkRequirements.requestTimeoutMs;

            CheckedProxy? checkedProxy = new CheckedProxy(parsedProxy)
            {
                requestURL = checkRequirements.requestURL
            };

            // логика

            // 1.try connect to proxy server socket
            bool canConnect = await TryConnectToSocketInternalMethod(checkedProxy);

            if (canConnect)
            {
                // 1.check proxy on http
                if (checkRequirements.requiredProxyProtocol.Contains(CPProtocol.http))
                {
                    checkedProxy = await CheckProxyInternalMethod(checkedProxy, CPProtocol.http, checkRequirements.requestURL, checkRequirements.requestURL_SSL);

                    if (checkedProxy.work == CPWorkState.Work)
                    {
                        checkedProxy.protocol = CPProtocol.http;
                    }
                }

                // 2.check proxy on socks5
                if (checkRequirements.requiredProxyProtocol.Contains(CPProtocol.socks5) && (checkedProxy.work == null || checkedProxy.work != CPWorkState.Work))
                {
                    checkedProxy = await CheckProxyInternalMethod(checkedProxy, CPProtocol.socks5, checkRequirements.requestURL, checkRequirements.requestURL_SSL);

                    if (checkedProxy.work == CPWorkState.Work)
                    {
                        checkedProxy.protocol = CPProtocol.socks5;
                    }
                }

                // 3.check proxy on socks4
                if (checkRequirements.requiredProxyProtocol.Contains(CPProtocol.socks4) && (checkedProxy.work == null || checkedProxy.work != CPWorkState.Work))
                {
                    checkedProxy = await CheckProxyInternalMethod(checkedProxy, CPProtocol.socks4, checkRequirements.requestURL, checkRequirements.requestURL_SSL);

                    if (checkedProxy.work == CPWorkState.Work)
                    {
                        checkedProxy.protocol = CPProtocol.socks4;
                    }
                }

                // 4.check proxy on imap
                if (checkRequirements.requiredProxyProtocol.Contains(CPProtocol.imap))
                {
                    checkedProxy = await CheckProxyOnImapInternalMethod(checkedProxy, CPProtocol.imap, checkRequirements.requestImapHost, checkRequirements.requestImapPort);

                    if (checkedProxy.work == CPWorkState.Work)
                    {
                        checkedProxy.protocol = CPProtocol.imap;
                    }
                }
            }

            // присвоение null обьекту если прокси не подходит в соответствии с CheckRequirements
            if (!checkRequirements.requiredWorkState.Contains((CPWorkState)checkedProxy.work!))
            {
                checkedProxy = null;
            }
            else if (checkedProxy.supportsSecureConnection != null && !checkRequirements.requiredSupportsSecureConnectionState.Contains((SupportsSecureConnectionState)checkedProxy.supportsSecureConnection))
            {
                checkedProxy = null;
            }
            else if (checkedProxy.anonymity! != null && !checkRequirements.requiredAnonymityState.Contains((AnonymousState)checkedProxy.anonymity))
            {
                checkedProxy = null;
            }

            return checkedProxy;
        }
        internal async Task<List<CheckedProxy>> CheckListProxy(List<ParsedProxy> parsedProxyList, CheckRequirements checkRequirements)
        {
            List<CheckedProxy> checkedProxies = new List<CheckedProxy>();
            List<Task> checkTasks = new List<Task>();

            if (checkRequirements.requestTimeoutMs != 0) defaultProxyCheckRequestTimeoutMs = checkRequirements.requestTimeoutMs;

            parsedProxyList.ForEach(parsedProxy =>
            {
                checkTasks.Add(System.Threading.Tasks.Task.Run(async () =>
                {
                    CheckedProxy checkedProxy = new CheckedProxy(parsedProxy)
                    {
                        requestURL = checkRequirements.requestURL
                    };

                    // логика

                    // 1.try connect to proxy server socket
                    bool canConnect = await TryConnectToSocketInternalMethod(checkedProxy);
                    //bool canConnect = true;

                    if (canConnect)
                    {
                        // 1.check proxy on http
                        if (checkRequirements.requiredProxyProtocol.Contains(CPProtocol.http))
                        {
                            checkedProxy = await CheckProxyInternalMethod(checkedProxy, CPProtocol.http, checkRequirements.requestURL, checkRequirements.requestURL_SSL);

                            if (checkedProxy.work == CPWorkState.Work)
                            {
                                checkedProxy.protocol = CPProtocol.http;
                            }
                        }

                        // 2.check proxy on socks5
                        if (checkRequirements.requiredProxyProtocol.Contains(CPProtocol.socks5) && (checkedProxy.work == null || checkedProxy.work != CPWorkState.Work))
                        {
                            checkedProxy = await CheckProxyInternalMethod(checkedProxy, CPProtocol.socks5, checkRequirements.requestURL, checkRequirements.requestURL_SSL);

                            if (checkedProxy.work == CPWorkState.Work)
                            {
                                checkedProxy.protocol = CPProtocol.socks5;
                            }
                        }

                        // 3.check proxy on socks4
                        if (checkRequirements.requiredProxyProtocol.Contains(CPProtocol.socks4) && (checkedProxy.work == null || checkedProxy.work != CPWorkState.Work))
                        {
                            checkedProxy = await CheckProxyInternalMethod(checkedProxy, CPProtocol.socks4, checkRequirements.requestURL, checkRequirements.requestURL_SSL);

                            if (checkedProxy.work == CPWorkState.Work)
                            {
                                checkedProxy.protocol = CPProtocol.socks4;
                            }
                        }


                        //if (checkRequirements.requiredProxyProtocol.Contains(CPProtocol.imap))
                        //{
                        //    checkedProxy = await CheckProxyOnImapInternalMethod(checkedProxy, CPProtocol.imap, checkRequirements.requestImapHost, checkRequirements.requestImapPort);

                        //    if (checkedProxy.work == CPWorkState.Work)
                        //    {
                        //        checkedProxy.protocol = CPProtocol.imap;
                        //    }
                        //}
                        //else
                        //{
                        //    // 1.check proxy on http
                        //    if (checkRequirements.requiredProxyProtocol.Contains(CPProtocol.http))
                        //    {
                        //        checkedProxy = await CheckProxyInternalMethod(checkedProxy, CPProtocol.http, checkRequirements.requestURL);

                        //        if (checkedProxy.work == CPWorkState.Work)
                        //        {
                        //            checkedProxy.protocol = CPProtocol.http;
                        //        }
                        //    }

                        //    // 2.check proxy on socks5
                        //    if (checkRequirements.requiredProxyProtocol.Contains(CPProtocol.socks5) && (checkedProxy.work == null || checkedProxy.work != CPWorkState.Work))
                        //    {
                        //        checkedProxy = await CheckProxyInternalMethod(checkedProxy, CPProtocol.socks5, checkRequirements.requestURL);

                        //        if (checkedProxy.work == CPWorkState.Work)
                        //        {
                        //            checkedProxy.protocol = CPProtocol.socks5;
                        //        }
                        //    }

                        //    // 3.check proxy on socks4
                        //    if (checkRequirements.requiredProxyProtocol.Contains(CPProtocol.socks4) && (checkedProxy.work == null || checkedProxy.work != CPWorkState.Work))
                        //    {
                        //        checkedProxy = await CheckProxyInternalMethod(checkedProxy, CPProtocol.socks4, checkRequirements.requestURL);

                        //        if (checkedProxy.work == CPWorkState.Work)
                        //        {
                        //            checkedProxy.protocol = CPProtocol.socks4;
                        //        }
                        //    }
                        //}
                    }

                    checkedProxies.Add(checkedProxy);
                }));
            });

            await System.Threading.Tasks.Task.WhenAll(checkTasks);

            // удаление из списка ненужных прокси, в соответствии с CheckRequirements
            List<CheckedProxy> checkedProxyListForRemove = new List<CheckedProxy>();
            foreach (var checkedProxy in checkedProxies)
            {
                if (!checkRequirements.requiredWorkState.Contains((CPWorkState)checkedProxy.work!))
                {
                    checkedProxyListForRemove.Add(checkedProxy);
                }
                else if (checkedProxy.supportsSecureConnection != null && !checkRequirements.requiredSupportsSecureConnectionState.Contains((SupportsSecureConnectionState)checkedProxy.supportsSecureConnection))
                {
                    checkedProxyListForRemove.Add(checkedProxy);
                }
                else if (checkedProxy.anonymity! != null && !checkRequirements.requiredAnonymityState.Contains((AnonymousState)checkedProxy.anonymity))
                {
                    checkedProxyListForRemove.Add(checkedProxy);
                }
            }
            foreach (var element in checkedProxyListForRemove)
            {
                checkedProxies.Remove(element);
            }

            return checkedProxies;
        }
        private async Task<bool> TryConnectToSocketInternalMethod(CheckedProxy checkedProxy)
        {
            CancellationTokenSource cancellationTokenSourceConnectToSocket = new CancellationTokenSource(TimeSpan.FromSeconds(5)); //default 5 sec
            bool connected = false;

            Socket? socketProxyServer = null;

            try
            {
                IPAddress socketIpAddress = IPAddress.Parse(checkedProxy.proxy.Ip);
                int socketPortAddress = Int32.Parse(checkedProxy.proxy.Port);
                IPEndPoint socketEndPoint = new IPEndPoint(socketIpAddress, socketPortAddress);

                socketProxyServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                await socketProxyServer.ConnectAsync(socketEndPoint, cancellationTokenSourceConnectToSocket.Token);

                if (socketProxyServer.Connected)
                {
                    connected = true;
                }
            }
            catch (Exception e)
            {
                connected = false;

                checkedProxy.work = CPWorkState.Error;

                var ex = e switch
                {
                    System.Net.Http.HttpRequestException => CheckedProxy.ExeptionState.System_Net_Http_HttpRequestException,
                    System.Threading.Tasks.TaskCanceledException => CheckedProxy.ExeptionState.System_Threading_Tasks_TaskCanceledException,
                    _ => CheckedProxy.ExeptionState.Other
                };

                if (checkedProxy.errorType == null)
                    checkedProxy.errorType = new CheckedProxy.ProxyExeption(ex, e.Message);
                else
                {
                    checkedProxy.errorType.exeption = ex;
                    checkedProxy.errorType.message = e.Message;
                }
            }
            finally
            {
                try
                {
                    socketProxyServer?.Shutdown(SocketShutdown.Both);
                }
                catch { }

                try
                {
                    socketProxyServer?.Close();
                }
                catch { }

                socketProxyServer?.Dispose();
                cancellationTokenSourceConnectToSocket.Dispose();
            }

            return connected;
        }
        private async Task<CheckedProxy> CheckProxyInternalMethod(CheckedProxy checkedProxy, CPProtocol proxyProtocol, string requestURL, string requestURL_SSL)
        {
            WebProxy webProxy = new WebProxy
            {
                Address = new Uri($"{proxyProtocol}://{checkedProxy.proxy.FullProxyAddress}"),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = true
            };

            HttpClientHandler clientHandler = new HttpClientHandler
            {
                UseProxy = true,
                Proxy = webProxy,
                //ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using (var client = new HttpClient(clientHandler, true))
            {
                Stopwatch stopwatch = new Stopwatch();
                try
                {
                    client.Timeout = TimeSpan.FromMilliseconds(defaultProxyCheckRequestTimeoutMs);

                    // start record request time
                    stopwatch.Start();

                    var result = await client.GetAsync(requestURL, HttpCompletionOption.ResponseHeadersRead);

                    // stop record request time
                    stopwatch.Stop();
                    checkedProxy.speed = (int)stopwatch.ElapsedMilliseconds;

                    if (result.IsSuccessStatusCode)
                    {
                        checkedProxy.work = CPWorkState.Work;
                        checkedProxy.errorType = null;
                        checkedProxy.requestStatusCode = (int)result.StatusCode;
                        checkedProxy.requestStatusCodeName = result.StatusCode.ToString();

                        // set supportsSecureConnectionState and set/check anonymity
                        if (proxyProtocol == CPProtocol.socks5 || proxyProtocol == CPProtocol.socks4)
                        {
                            checkedProxy.supportsSecureConnection = SupportsSecureConnectionState.Support;
                            checkedProxy.anonymity = AnonymousState.Anonymous;
                        }
                        else if (proxyProtocol == CPProtocol.http)
                        {
                            // check http proxy on ssl support
                            try
                            {
                                // start record request time
                                stopwatch.Reset();
                                stopwatch.Start();

                                var resultSsl = await client.GetAsync(requestURL_SSL, HttpCompletionOption.ResponseHeadersRead);

                                // stop record request time
                                stopwatch.Stop();

                                if (result.IsSuccessStatusCode)
                                {
                                    checkedProxy.supportsSecureConnection = SupportsSecureConnectionState.Support;
                                    checkedProxy.speed = (int)stopwatch.ElapsedMilliseconds;
                                }
                                else
                                {
                                    checkedProxy.supportsSecureConnection = SupportsSecureConnectionState.NotSupport;
                                }
                            }
                            catch
                            {
                                stopwatch.Stop();
                                checkedProxy.supportsSecureConnection = SupportsSecureConnectionState.Undefined;
                            }

                            //if (requestURL.Contains("https"))
                            //{
                            //    checkedProxy.supportsSecureConnection = SupportsSecureConnectionState.Support;
                            //}
                            //else
                            //{
                            //    checkedProxy.supportsSecureConnection = SupportsSecureConnectionState.Undefined;
                            //}

                            // request to check anonymity
                            checkedProxy = await CheckProxyAnonymityInternalMethod(checkedProxy, client);
                        }

                        // get country and city
                        if (checkedProxy.anonymity == AnonymousState.Anonymous)
                        {
                            checkedProxy = await GetProxyGeolocationInternalMethod(checkedProxy, client);
                        }
                    }
                    else
                    {
                        checkedProxy.work = CPWorkState.NotWork;
                        checkedProxy.errorType = null;
                        checkedProxy.requestStatusCode = (int)result.StatusCode;
                        checkedProxy.requestStatusCodeName = result.StatusCode.ToString();
                    }
                }
                catch (Exception exeption)
                {
                    // stop record request time
                    stopwatch.Stop();
                    checkedProxy.speed = null;

                    checkedProxy.work = CPWorkState.Error;

                    checkedProxy.requestStatusCode = null;
                    checkedProxy.requestStatusCodeName = null;

                    var ex = exeption switch
                    {
                        System.Net.Http.HttpRequestException => CheckedProxy.ExeptionState.System_Net_Http_HttpRequestException,
                        System.Threading.Tasks.TaskCanceledException => CheckedProxy.ExeptionState.System_Threading_Tasks_TaskCanceledException,
                        _ => CheckedProxy.ExeptionState.Other
                    };

                    if (checkedProxy.errorType == null)
                        checkedProxy.errorType = new CheckedProxy.ProxyExeption(ex, exeption.Message);
                    else
                    {
                        checkedProxy.errorType.exeption = ex;
                        checkedProxy.errorType.message = exeption.Message;
                    }
                }

                return checkedProxy;
            }
        }
        private async Task<CheckedProxy> CheckProxyOnImapInternalMethod(CheckedProxy checkedProxy, CPProtocol proxyProtocol, string requestImapHost, int requestImapPort)
        {
            WebProxy webProxy = new WebProxy
            {
                Address = new Uri($"{proxyProtocol}://{checkedProxy.proxy.FullProxyAddress}"),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = true
            };

            HttpClientHandler clientHandler = new HttpClientHandler
            {
                UseProxy = true,
                Proxy = webProxy
            };

            using (var client = new HttpClient(clientHandler, true))
            {
                Stopwatch stopwatch = new Stopwatch();
                try
                {
                    client.Timeout = TimeSpan.FromMilliseconds(defaultProxyCheckRequestTimeoutMs);

                    // start record request time
                    stopwatch.Start();

                    var result = await client.GetAsync("", HttpCompletionOption.ResponseHeadersRead);

                    // stop record request time
                    stopwatch.Stop();
                    checkedProxy.speed = (int)stopwatch.ElapsedMilliseconds;

                    if (result.IsSuccessStatusCode)
                    {
                        checkedProxy.work = CPWorkState.Work;
                        checkedProxy.errorType = null;
                        checkedProxy.requestStatusCode = (int)result.StatusCode;
                        checkedProxy.requestStatusCodeName = result.StatusCode.ToString();

                        // set supportsSecureConnectionState and set/check anonymity
                        if (proxyProtocol == CPProtocol.socks5 || proxyProtocol == CPProtocol.socks4)
                        {
                            checkedProxy.supportsSecureConnection = SupportsSecureConnectionState.Support;
                            checkedProxy.anonymity = AnonymousState.Anonymous;
                        }
                        else if (proxyProtocol == CPProtocol.http)
                        {
                            if ("".Contains("https"))
                            {
                                checkedProxy.supportsSecureConnection = SupportsSecureConnectionState.Support;
                            }
                            else
                            {
                                checkedProxy.supportsSecureConnection = SupportsSecureConnectionState.Undefined;
                            }

                            // request to check anonymity
                            checkedProxy = await CheckProxyAnonymityInternalMethod(checkedProxy, client);
                        }

                        // get country and city
                        if (checkedProxy.anonymity == AnonymousState.Anonymous)
                        {
                            checkedProxy = await GetProxyGeolocationInternalMethod(checkedProxy, client);
                        }
                    }
                    else
                    {
                        checkedProxy.work = CPWorkState.NotWork;
                        checkedProxy.errorType = null;
                        checkedProxy.requestStatusCode = (int)result.StatusCode;
                        checkedProxy.requestStatusCodeName = result.StatusCode.ToString();
                    }
                }
                catch (Exception exeption)
                {
                    // stop record request time
                    stopwatch.Stop();
                    checkedProxy.speed = null;

                    checkedProxy.work = CPWorkState.Error;

                    checkedProxy.requestStatusCode = null;
                    checkedProxy.requestStatusCodeName = null;

                    var ex = exeption switch
                    {
                        System.Net.Http.HttpRequestException => CheckedProxy.ExeptionState.System_Net_Http_HttpRequestException,
                        System.Threading.Tasks.TaskCanceledException => CheckedProxy.ExeptionState.System_Threading_Tasks_TaskCanceledException,
                        _ => CheckedProxy.ExeptionState.Other
                    };

                    if (checkedProxy.errorType == null)
                        checkedProxy.errorType = new CheckedProxy.ProxyExeption(ex, exeption.Message);
                    else
                    {
                        checkedProxy.errorType.exeption = ex;
                        checkedProxy.errorType.message = exeption.Message;
                    }
                }

                return checkedProxy;
            }
        }
        private async Task<CheckedProxy> CheckProxyAnonymityInternalMethod(CheckedProxy checkedProxy, HttpClient httpClient)
        {
            List<AnonymousState> proxyAnonymousStates = new List<AnonymousState>();

            if (checkedProxy.supportsSecureConnection == null)
            {
                proxyAnonymousStates.Add(AnonymousState.Undefined);
            }
            else if (checkedProxy.supportsSecureConnection == SupportsSecureConnectionState.Support)
            {
                foreach (var judge in httpsProxyJudgeLinks)
                {
                    try
                    {
                        var responce = await httpClient.GetAsync(judge, HttpCompletionOption.ResponseContentRead);

                        if (responce.IsSuccessStatusCode)
                        {
                            string content = await responce.Content.ReadAsStringAsync();

                            //content.Contains(client.IP!) || content.Contains("46.211.3.203") || content.Contains("46.211.30.126") || content.Contains("2a02:2378:102f:154:207f:c75b:b161:8427")
                            if (content.Contains(client.IP!) || content.Contains("185.42.130.41") || content.Contains("5.199.233.180"))
                            {
                                proxyAnonymousStates.Add(AnonymousState.Transparent);
                            }
                            else proxyAnonymousStates.Add(AnonymousState.Anonymous);
                        }
                    }
                    catch
                    {
                        proxyAnonymousStates.Add(AnonymousState.Error);
                    }         
                }

                // так делать не нужно, но я сделаю для подстраховки, проверяю ссл прокси на анонимность через http без ssl
                foreach (var judge in httpProxyJudgeLinks)
                {
                    try
                    {
                        var responce = await httpClient.GetAsync(judge, HttpCompletionOption.ResponseContentRead);

                        if (responce.IsSuccessStatusCode)
                        {
                            string content = await responce.Content.ReadAsStringAsync();

                            if (content.Contains(client.IP!) || content.Contains("185.42.130.41") || content.Contains("5.199.233.180"))
                            {
                                proxyAnonymousStates.Add(AnonymousState.Transparent);
                            }
                            else proxyAnonymousStates.Add(AnonymousState.Anonymous);
                        }
                    }
                    catch
                    {
                        proxyAnonymousStates.Add(AnonymousState.Error);
                    }
                }
            }
            else
            {
                foreach (var judge in httpProxyJudgeLinks)
                {
                    try
                    {
                        var responce = await httpClient.GetAsync(judge, HttpCompletionOption.ResponseContentRead);

                        if (responce.IsSuccessStatusCode)
                        {
                            string content = await responce.Content.ReadAsStringAsync();

                            if (content.Contains(client.IP!) || content.Contains("185.42.130.41") || content.Contains("5.199.233.180"))
                            {
                                proxyAnonymousStates.Add(AnonymousState.Transparent);
                            }
                            else proxyAnonymousStates.Add(AnonymousState.Anonymous);
                        }
                    }
                    catch
                    {
                        proxyAnonymousStates.Add(AnonymousState.Error);
                    }
                }
            }

            if (proxyAnonymousStates.Where(state => state == AnonymousState.Transparent).Count() > 0)
            {
                checkedProxy.anonymity = AnonymousState.Transparent;
            }
            else if (proxyAnonymousStates.Where(state => state == AnonymousState.Anonymous).Count() > 0)
            {
                checkedProxy.anonymity = AnonymousState.Anonymous;
            }
            else if (proxyAnonymousStates.Where(state => state == AnonymousState.Error).Count() == proxyAnonymousStates.Count())
            {
                checkedProxy.anonymity = AnonymousState.Error;
            }
            else if (proxyAnonymousStates.Where(state => state == AnonymousState.Undefined).Count() > 0)
            {
                checkedProxy.anonymity = AnonymousState.Undefined;
            }
            else
            {
                checkedProxy.anonymity = AnonymousState.Undefined;
            }

            return checkedProxy;
        }
        private async Task<CheckedProxy> GetProxyGeolocationInternalMethod(CheckedProxy checkedProxy, HttpClient httpClient)
        {
            try
            {
                var responce = await httpClient.GetAsync(@"http://ip-api.com/json", HttpCompletionOption.ResponseContentRead);

                if (responce.IsSuccessStatusCode)
                {
                    string content = await responce.Content.ReadAsStringAsync();

                    Dictionary<string, string>? keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

                    if (keyValuePairs != null)
                    {
                        checkedProxy.country = keyValuePairs.Where(p => p.Key == "country").FirstOrDefault().Value;
                        checkedProxy.city = keyValuePairs.Where(p => p.Key == "city").FirstOrDefault().Value;
                    }
                }
            }
            catch { }

            return checkedProxy;
        }

        #endregion

        #region Dispose Data

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты)
                    base.DisposeData();
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                disposedValue = true;
            }
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        // ~Check()
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

        #endregion
    }
}
