
using CommonModels.ProjectTask.Platform.SocpublicCom.Models;
using Microsoft.AspNetCore.SignalR;
using Server.Database.Services;
using Server.Hubs;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels;
using CommonModels.EmailModels;
using CommonModels.ProjectTask.ProxyCombiner.Models;
using static CommonModels.ProjectTask.ProxyCombiner.Models.EnvironmentProxy;
using CommonModels.Client.Models;
using CommonModels.Client;
using CommonModels.ProjectTask.EarningSite;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.SpecialCombineTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.SpecialCombineTask;
using MongoDB.Bson;
using static CommonModels.Client.Client;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using TaskStatus = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus;
using CommonModels.ProjectTask.Platform;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask;
using CommonModels.ProjectTask.ProxyCombiner;
using CommonModels.ProjectTask.EarningSite.SocpublicCom.TaskType.GSelectiveTaskWithAuth.SelectiveTask.WithdrawMoney;
using static CommonModels.ProjectTask.Platform.EarningSiteTaskUrls;
using CommonModels.ProjectTask.EarningSite.SocpublicCom.Models.WithdrawalOfMoneyModels;
using System.Collections.Generic;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IHubContext<ClientHub> clientHubContext;
        private readonly ClientsService clientsService;
        private readonly UsersService usersService;
        private readonly UserSessionService userSessionsService;
        private readonly EarnSiteTasksService earnSiteTasksService;
        private readonly ProxyTasksService proxyTasksService;
        private readonly SocpublicAccountsService socpublicAccountsService;
        private readonly SiteParseBalancerService siteParseBalancerService;
        private readonly RunTasksSettingsService runTasksSettingsService;
        private readonly PlatformInternalAccountTaskService platformInternalAccountTasksCollection;
        private readonly ProxyTasksErorrsLog proxyTasksErorrsLog;
        private readonly AccountReservedProxyService accountReservedProxyService;
        private readonly BlockedMachinesService blockedMachinesService;
        private readonly EnvironmentProxiesService environmentProxiesService;

        //remove after
        private Random random;
        private readonly string[] ips =
        {
            "168.235.81.167",
            "176.56.236.175",
            "176.103.130.131",
            "176.103.130.130",
            "176.103.130.132",
            "176.103.130.134",
            "37.252.185.229",
            "206.189.215.75",
            "104.24.120.142",
            "104.24.121.142",
            "108.61.201.119",
            "139.59.48.222",
            "104.16.249.249",
            "104.16.248.249",
            "199.58.81.218",
            "23.92.29.236",
            "104.28.0.106",
            "104.28.1.106",
            "8.8.4.4",
            "8.8.8.8",
            "185.95.218.42",
            "185.95.218.43",
            "185.222.222.222",
            "185.184.222.222",
            "46.101.66.244",
            "172.64.108.27",
            "172.64.109.27",
            "45.77.124.64",
            "45.32.253.116",
            "104.236.178.232",
            "89.234.186.112",
            "45.90.28.0",
            "45.90.30.0",
            "193.17.47.1",
            "185.43.135.1",
            "136.144.215.158",
            "118.126.68.223",
            "118.89.110.78",
            "47.96.179.163",
            "145.100.185.15",
            "145.100.185.16",
            "174.138.29.175",
            "45.77.180.10",
            "185.216.27.142",
            "217.169.20.23",
            "217.169.20.22",
            "172.65.3.223",
            "188.60.252.16",
        };
        private readonly string[] pc_names =
        {
            "Atlas Architectural Designs",
            "Atlas Realty",
            "Avant Garde Appraisal",
            "Avant Garde Appraisal Group",
            "Avant Garde Interior Designs",
            "Awthentikz",
            "Back To Basics Chiropractic Clinic",
            "Balanced Fortune",
            "Beasts of Beauty",
            "Belle Ladi",
            "Belle Lady",
            "Benesome",
            "Best Biz Survis",
            "Better Business Ideas and Services",
            "Buena Vista Garden Maintenance",
            "Buena Vista Realty Service",
            "Body Fate",
            "Body Toning",
            "Bold Ideas",
            "Bonanza Produce Stores",
            "Bountiful Harvest Health Food Store",
            "Brilliant Home Designs",
            "Capitalcorp",
            "Castle Realty",
            "Chargepal",
            "Choices",
            "Circuit Design",
            "Complete Tech",
            "Corinthian Designs",
            "Corpbay",
            "Cougar Investment",
            "Crazy Tiger",
            "Creative Wealth",
            "Creative Wealth Management",
            "Custom Lawn Care",
            "Custom Lawn Service",
            "Cut Above",
            "Cut Rite",
            "Cut Rite Lawn Care",
            "Datacorp",
            "Desert Garden Help",
            "Destiny Planners",
            "Destiny Realty",
            "Destiny Realty Solutions",
            "Dream Home Improvements",
            "Dream Home Real Estate Service",
            "Dreamscape Garden Care",
            "Dun Rite Lawn Care",
            "Dun Rite Lawn Maintenance",
            "Dynatronics Accessories",
            "E-zhe Source",
            "Earthworks Garden Kare",
            "Earthworks Yard Maintenance",
            "Eden Lawn Service",
            "Edge Garden Services",
            "Edge Yard Service",
            "Ejecta",
            "Electronic Geek",
            "Electronics Source",
            "Enrich Garden Services",
            "Enviro Architectural Designs",
            "Environ Architectural Design",
            "EnviroSource Design",
            "Envirotecture Design",
            "Envirotecture Design Service",
            "Exact Realty",
            "Exact Solutions",
            "Excella",
            "Express Merchant Service",
            "Fellowship Investments",
            "Fireball",
            "First Choice Garden Maintenance",
            "First Choice Yard Help",
            "First Rate Choice",
            "Fit Tonic",
            "Flexus",
            "Formula Gray",
            "Formula Grey",
            "Four Leaf Clover",
            "Fragrant Flower Lawn Services",
            "Freedom Map",
            "Fresh Start",
            "Friendly Advice",
            "Friendly Interior Design",
            "Full Color",
            "Future Bright",
            "Future Plan",
            "Galaxy Man",
            "Gamma Gas",
            "Gamma Grays",
            "Gamma Realty",
            "Garden Guru",
            "Garden Master",
            "Gas Depot",
            "Gas Legion",
            "Gas Zone",
            "Gold Leaf Garden Management",
            "Gold Touch",
            "Golden Joy"
        };
        private readonly string[] os_platforms =
        {
            "Windows",
            "Linux",
            "MacOS",
            "Parrot Security Os",
            "Hidemyname OS",
            "Windows",
            "Linux",
            "Honor OS"
        };
        private readonly string[] usernames =
        {
            "cmartinez5",
            "martinezc5",
            "gcwilliams",
            "bindu",
            "shakilahmed",
            "williamsgc",
            "kevincr",
            "stran",
            "trans",
            "pnj",
            "sameer.sharma",
            "dianef",
            "cperry2",
            "perryc2",
            "mmmurphy",
            "sameersharma",
            "samad",
            "murphymm",
            "jimmh",
            "ssims",
            "simss",
            "avm",
            "ryan.stewart",
            "annt",
            "mwood3",
            "woodm3",
            "mrreyes",
            "majed",
            "ryanstewart",
            "fogarty",
            "reyesmr",
            "rachelcm",
            "pwatson",
            "watsonp",
            "nna",
            "rodney.smith",
            "tinaj",
            "aadams4",
            "adamsa4",
            "jlsanchez",
            "keeley",
            "rodneysmith",
            "dozier",
            "sanchezjl",
            "pauldg",
            "cliu",
            "liuc",
            "vsa",
            "mike.obrien",
            "charliep",
            "rsmith18",
            "smithr18",
            "aawong",
            "asaf",
            "mikeobrien",
            "wongaa",
            "mattjj",
            "aweaver",
            "weavera",
            "lrn",
            "michelle.baker",
            "kennyh",
            "lyoung3",
            "youngl3"
        };
        private readonly string[] identity_keys =
        {
            "HDSFJSDFO4K834JKB348F5KNDSG334LSD",
            "145AJHKDJJGAO7DMSWN6SCT8FR22G92O1",
            "JK734JKDC4K824J4K3SDFND34JKD33LC2",
            "ZKY0GEJ97ZROOTZRSM4FG3KPYCDTNHSD7",
            "REOGD79O2OLBGHJ2T4WD67XOW2S00BE76",
            "12W4NAPRMW0475C2L07NV4A3O2OOCXU24",
            "SA6H8MUUBUA7SBJR8TX1N7NV55JZM09AT",
            "ZRV0Y6WBVF69HGP0CXOOQJRG4S4JPPEHD",
            "QDFCXHR8A27QT1DYDFAKM8NS6145H933D",
            "7N1TMLRJDYW6K6AEAUMKWGYZ6YMFGI9FH",
            "WEE36V3Z3O4USKB7UFHON5TOWKFSIOH9N",
            "UM7SIT5K5RTP6FVTZKQ0XFSF8VKI9IGX6",
            "VWNHC0HHSIILKJ3ZQ3RWTY4F1304LW5E4",
            "BNUVA7WJU8ZK2ETXOROIJ30E6ZZQSGQ58",
            "S2UGX2HI8BK583MJFT5B9F28OYZJANC55",
            "N893ZT491TUYUDMAMAILI6IM9417S6STX",
            "5JE8G0GD7YL9YKVI1F65S5I7YMOLHAPR1",
            "KV4SZN8PVUN7GUPDDT9M52UQ2LJ1HY1GJ",
            "BWMGG5TFJL6GUG6USPYFF8WS7CMG1AHXT",
        };
        //remove after


        //remove after

        //remove after

        public TestController(IHubContext<ClientHub> _clientHubContext, ClientsService clientsService, UsersService usersService, UserSessionService userSessionsService, EarnSiteTasksService earnSiteTasksService, SocpublicAccountsService socpublicAccountsService, SiteParseBalancerService siteParseBalancerService, ProxyTasksService proxyTasksService, RunTasksSettingsService runTasksSettingsService, PlatformInternalAccountTaskService platformInternalAccountTasksCollection, ProxyTasksErorrsLog proxyTasksErorrsLog, AccountReservedProxyService accountReservedProxyService, BlockedMachinesService blockedMachinesService, EnvironmentProxiesService environmentProxiesService)
        {
            clientHubContext = _clientHubContext;
            this.clientsService = clientsService;
            random = new Random();
            this.usersService = usersService;
            this.userSessionsService = userSessionsService;
            this.earnSiteTasksService = earnSiteTasksService;
            this.socpublicAccountsService = socpublicAccountsService;
            this.siteParseBalancerService = siteParseBalancerService;
            this.proxyTasksService = proxyTasksService;
            this.runTasksSettingsService = runTasksSettingsService;
            this.platformInternalAccountTasksCollection = platformInternalAccountTasksCollection;
            this.proxyTasksErorrsLog = proxyTasksErorrsLog;
            this.accountReservedProxyService = accountReservedProxyService;
            this.blockedMachinesService = blockedMachinesService;
            this.environmentProxiesService = environmentProxiesService;
        }

        [HttpGet]
        [Route("connect")]
        public async Task<IActionResult> Connect()
        {
            #region remove after
            //List<ModelClient> bots = new List<ModelClient>();
            //for (int i = 0; i < 50; i++)
            //{
            //    BotRole currentRole = random.Next(0, 6) switch
            //    {
            //        0 => BotRole.PlatformWorkBot,
            //        1 => BotRole.ProxyCombineBot,
            //        2 => BotRole.BotManager,
            //        3 => BotRole.PlatformWorkBot,
            //        4 => BotRole.ProxyCombineBot,
            //        _ => BotRole.PlatformWorkBot
            //    };
            //    ClientStatus currentClientBusyStatus = random.Next(0, 4) switch
            //    {
            //        0 => ClientStatus.Free,
            //        1 => ClientStatus.AtWork,
            //        2 => ClientStatus.Stopped,
            //        _ => ClientStatus.AtWork
            //    };
            //    string current_os_platform = os_platforms[random.Next(0, 8)];
            //    Platform platform = Platform.Other;
            //    if (current_os_platform == Platform.Windows.ToString()) platform = Platform.Windows;
            //    else if (current_os_platform == Platform.Linux.ToString()) platform = Platform.Linux;
            //    else if (current_os_platform == Platform.MacOS.ToString()) platform = Platform.MacOS;

            //    bots.Add(new ModelClient()
            //    {
            //        SERVER_HOST = "http://localhost:5074",
            //        RegistrationDateTime = DateTime.UtcNow,
            //        Role = currentRole,
            //        IP = ips[random.Next(0, 47)],
            //        MACHINE = new Machine()
            //        {
            //            MACHINE_NAME = pc_names[random.Next(0, 99)],
            //            OS_PLATFORM = platform,
            //            OS_PLATFORM_TEXT = current_os_platform,
            //            USER_NAME = usernames[random.Next(0, 64)],
            //            IDENTITY_KEY = identity_keys[random.Next(0, 19)]
            //        },
            //        Status = currentClientBusyStatus
            //    });
            //}
            //await clientsService.CreateAsync(bots);

            // add field OSPlatform
            //await clientsService.TEMPMETHOD();
            //await userSessionsService.TEMPMETHOD();
            #endregion remove after

            #region remove after 2

            //List<GroupSelectiveTaskWithAuth> tasks = new List<GroupSelectiveTaskWithAuth>();

            //Account account = new Account()
            //{
            //    Login = "ferreririvett75",
            //    Email = "lovelandleonetti@mail.com",
            //    Password = "ferreririvett75BOT0",
            //    RegedUseragent = new CommonModels.UserAgentClasses.UserAgent("Mozilla/5.0 (Linux; U; Linux i654 x86_64; en-US) AppleWebKit/601.44 (KHTML, like Gecko) Chrome/49.0.2133.247 Safari/601.9 Edge/15.14164"),
            //    LastCookies = new List<System.Net.Cookie>()
            //    {
            //        /*new System.Net.Cookie()
            //        {
            //            Name = "session_id",
            //            Value = "C9C636F3-F78A-F4D1-015E-BBC82766448F",
            //            Domain = "socpublic.com",
            //            Path = "/",
            //            Expired = false,
            //            Expires = DateTime.Today,
            //            Secure = false,
            //            HttpOnly = true
            //        },

            //        new System.Net.Cookie()
            //        {
            //            Name = "secret",
            //            Value = "1C349EF4-E729-9999-5BB7-53EC8D19B821",
            //            Domain = "socpublic.com",
            //            Path = "/",
            //            Expired = false,
            //            Expires = DateTime.Today,
            //            Secure = false,
            //            HttpOnly = false
            //        },

            //        new System.Net.Cookie()
            //        {
            //            Name = "user_data",
            //            Value = "a%3A0%3A%7B%7D",
            //            Domain = "socpublic.com",
            //            Path = "/",
            //            Expired = false,
            //            Expires = DateTime.Today,
            //            Secure = false,
            //            HttpOnly = false
            //        }*/

            //        new System.Net.Cookie("session_id", "C9C636F3-F78A-F4D1-015E-BBC82766448F", "/", "socpublic.com"),
            //        new System.Net.Cookie("secret", "1C349EF4-E729-9999-5BB7-53EC8D19B821", "/", "socpublic.com"),
            //        new System.Net.Cookie("user_data", "a%3A0%3A%7B%7D", "/", "socpublic.com")
            //    },
            //    History = new List<OneLogAccounResult>(),
            //    IsFirstExecutionOfTasksAfterRegistration = false
            //};

            //SocpublicComTaskSubtype[] socpublicComTaskSubtypes = new SocpublicComTaskSubtype[]
            //{
            //    SocpublicComTaskSubtype.TaskVisitWithoutTimer,
            //    SocpublicComTaskSubtype.CheckCareerLadder
            //};

            //for (int i = 0; i < 1; i++)
            //{
            //    tasks.Add(new GroupSelectiveTaskWithAuth(random.Next(1, 10000).ToString(), TaskFrom.Management, SocpublicComUrl.http, random.Next(1, 10000).ToString(), account, socpublicComTaskSubtypes)
            //    {
            //        Id = ObjectId.GenerateNewId().ToString(),
            //        ResultStatus = TaskResultStatus.Success,
            //        taskVisitWithoutTimer = new TaskVisitWithoutTimer(),
            //        checkCareerLadder = new CheckCareerLadder()
            //    });

            //    tasks.Add(new GroupSelectiveTaskWithAuth(random.Next(1, 10000).ToString(), TaskFrom.Bot, SocpublicComUrl.http, random.Next(1, 10000).ToString(), account, socpublicComTaskSubtypes)
            //    {
            //        Id = ObjectId.GenerateNewId().ToString(),
            //        ResultStatus = TaskResultStatus.Error,
            //        taskVisitWithoutTimer = new TaskVisitWithoutTimer(),
            //        checkCareerLadder = new CheckCareerLadder()
            //    });

            //    tasks.Add(new GroupSelectiveTaskWithAuth(random.Next(1, 10000).ToString(), TaskFrom.Bot, SocpublicComUrl.http, random.Next(1, 10000).ToString(), account, socpublicComTaskSubtypes)
            //    {
            //        Id = ObjectId.GenerateNewId().ToString(),
            //        taskVisitWithoutTimer = new TaskVisitWithoutTimer(),
            //        checkCareerLadder = new CheckCareerLadder()
            //    });
            //}

            //await earnSiteTasksService.CreateAsync(tasks);

            #endregion

            #region remove after 3

            //CheckedProxy personalProxy = new CheckedProxy(new ParsedProxy("", ""))
            //{

            //};

            //Account account = new Account()
            //{
            //    Login = "mowwkfly",
            //    Email = new CommonModels.EmailModels.Email("mowwkfly@fmaillerbox.com", "uipzgrexX7814!", new CommonModels.Email.Settings.ConnectSettings("imap.firstmail.ltd", 993, true)),
            //    Password = "mowwkflyBotik24",
            //    RegedUseragent = new CommonModels.UserAgentClasses.UserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:125.0) Gecko/20100101 Firefox/125.0"),
            //    LastCookies = new List<System.Net.Cookie>()
            //    {
            //        /*new System.Net.Cookie()
            //        {
            //            Name = "session_id",
            //            Value = "C9C636F3-F78A-F4D1-015E-BBC82766448F",
            //            Domain = "socpublic.com",
            //            Path = "/",
            //            Expired = false,
            //            Expires = DateTime.Today,
            //            Secure = false,
            //            HttpOnly = true
            //        },

            //        new System.Net.Cookie()
            //        {
            //            Name = "secret",
            //            Value = "1C349EF4-E729-9999-5BB7-53EC8D19B821",
            //            Domain = "socpublic.com",
            //            Path = "/",
            //            Expired = false,
            //            Expires = DateTime.Today,
            //            Secure = false,
            //            HttpOnly = false
            //        },

            //        new System.Net.Cookie()
            //        {
            //            Name = "user_data",
            //            Value = "a%3A0%3A%7B%7D",
            //            Domain = "socpublic.com",
            //            Path = "/",
            //            Expired = false,
            //            Expires = DateTime.Today,
            //            Secure = false,
            //            HttpOnly = false
            //        }*/

            //        new System.Net.Cookie("session_id", "C9C636F3-F78A-F4D1-015E-BBC82766448F", "/", "socpublic.com"),
            //        new System.Net.Cookie("secret", "1C349EF4-E729-9999-5BB7-53EC8D19B821", "/", "socpublic.com"),
            //        new System.Net.Cookie("user_data", "a%3A0%3A%7B%7D", "/", "socpublic.com")
            //    },
            //    History = new List<OneLogAccounResult>(),
            //    IsFirstExecutionOfTasksAfterRegistration = false,
            //    IsAlive = true,
            //    IsMain = false,
            //    PersonalProxy = null
            //};

            //await socpublicAccountsService.CreateAsync(account);

            #endregion

            #region remove after 4

            //List<SiteParseBalancer> balancers = new List<SiteParseBalancer>();
            //for (int i = 1; i < 56; i++)
            //{
            //    balancers.Add(new SiteParseBalancer((MethodName)i, DateTime.UtcNow, "192.168.0.1", 1));
            //}

            //await siteParseBalancerService.CreateAsync(balancers);

            #endregion

            #region remove after 5

            //ParseRequirements prequirements = new ParseRequirements(500);
            //SpecialRequirements specialRequirements = new SpecialRequirements("", EarningSiteTaskEnums.EarningSiteEnum.SocpublicDotCom);

            //CheckRequirements requirements = new CheckRequirements(
            //    "https://firstmail.ltd",
            //    new CheckedProxy.CPProtocol[] { CheckedProxy.CPProtocol.http, CheckedProxy.CPProtocol.socks4, CheckedProxy.CPProtocol.socks5 },
            //    new CheckedProxy.SupportsSecureConnectionState[] { CheckedProxy.SupportsSecureConnectionState.Support },
            //    new CheckedProxy.AnonymousState[] { CheckedProxy.AnonymousState.Anonymous },
            //TimeSpan.FromSeconds(10).Milliseconds,
            //new CheckedProxy.CPWorkState[] { CheckedProxy.CPWorkState.Work }
            //);

            //SpecialCombineTask task = new SpecialCombineTask("", CommonModels.ProjectTask.ProjectTaskEnums.TaskFrom.Bot, CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created, prequirements, requirements, specialRequirements);

            //// send Task to bot
            //await clientHubContext.Clients.All.SendAsync($"do_new_proxycombiner_{task.InternalType.ToString().ToLower()}_task", task);

            #endregion

            #region remove after 6
            //try
            //{
            //    List<dynamic>? tasks = await proxyTasksService.GetAsync();
            //    dynamic? task_ = tasks.Where(t => t.Type == CommonModels.ProjectTask.ProjectTaskEnums.TaskType.ProxyCombineWork && t.Status == CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created).FirstOrDefault();
            //}
            //catch (Exception ex)
            //{

            //    throw;
            //}
            #endregion

            #region remove after 7

            //Account account = new Account()
            //{
            //    Login = "liiwlkxy",
            //    EmailIsConfirmed = true,
            //    Email = new Email("liiwlkxy@fmaillerbox.net", "ruqnlhalY1679!", new CommonModels.Email.Settings.ConnectSettings("imap.firstmail.ltd", 993, true)),
            //    Password = "liiwlkxyBotik24",
            //    Pincode = "4D1D",
            //    IsAlive = true,
            //    IsMain = false,
            //    MoneyMainBalance = 0,
            //    MoneyAdvertisingBalance = 0,
            //    Refer = new Referal()
            //    {
            //        Id = "8458675",
            //        Email = "golubov.gleb@mail.ru",
            //        Login = "xpomocoma777"
            //    },
            //    RegedDateTime = DateTime.UtcNow,
            //    RegedUseragent = new CommonModels.UserAgentClasses.UserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36"),
            //    LastCookies = new List<System.Net.Cookie>()
            //    {

            //    },
            //    History = new List<OneLogAccounResult>(),
            //    IsFirstExecutionOfTasksAfterRegistration = true
            //};

            //await socpublicAccountsService.CreateAsync(account);

            #endregion

            #region remove after 8

            //Account account = new Account()
            //{
            //    Login = "test",
            //    EmailIsConfirmed = true,
            //    Email = new Email("test", "test", new CommonModels.Email.Settings.ConnectSettings("imap.firstmail.ltd", 993, true)),
            //    Password = "test",
            //    Pincode = "test",
            //    IsAlive = true,
            //    IsMain = false,
            //    MoneyMainBalance = 0,
            //    MoneyAdvertisingBalance = 0,
            //    Refer = new Referal()
            //    {
            //        Id = "8458675",
            //        Email = "golubov.gleb@mail.ru",
            //        Login = "xpomocoma777"
            //    },
            //    RegedDateTime = DateTime.UtcNow,
            //    RegedUseragent = new CommonModels.UserAgentClasses.UserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.3"),
            //    LastCookies = new List<System.Net.Cookie>()
            //    {

            //    },
            //    History = new List<OneLogAccounResult>(),
            //    IsFirstExecutionOfTasksAfterRegistration = true,
            //    PersonalProxy = null,
            //    PersonalPrivateProxy = new CommonModels.ProjectTask.ProxyCombiner.Models.EnvironmentProxy("Test personal proxy", "Test desc pers proxy", CommonModels.ProjectTask.ProxyCombiner.Models.EnvironmentProxy.EPMarker.MainAccount, "address test", "pass test", CommonModels.ProjectTask.ProxyCombiner.Models.EnvironmentProxy.EPProtocol.http)
            //};

            //await socpublicAccountsService.CreateAsync(account);

            #endregion

            #region remove after 9

            //Account account = new Account()
            //{
            //    Login = "pomjolpl",
            //    EmailIsConfirmed = true,
            //    Email = new Email("pomjolpl@fmailler.net", "ufpmerrfA6149!", new CommonModels.Email.Settings.ConnectSettings("imap.firstmail.ltd", 993, true)),
            //    Password = "pomjolplBotik24",
            //    Pincode = "3E04",
            //    IsAlive = true,
            //    IsMain = false,
            //    MoneyMainBalance = 0,
            //    MoneyAdvertisingBalance = 0,
            //    Refer = new Referal()
            //    {
            //        Id = "8458675",
            //        Email = "golubov.gleb@mail.ru",
            //        Login = "xpomocoma777"
            //    },
            //    RegedDateTime = DateTime.UtcNow,
            //    RegedUseragent = new CommonModels.UserAgentClasses.UserAgent("Mozilla/5.0 (Windows NT 6.1; rv:38.0) Gecko/20100101 Firefox/38.0"),
            //    LastCookies = new List<System.Net.Cookie>()
            //    {

            //    },
            //    History = new List<OneLogAccounResult>(),
            //    IsFirstExecutionOfTasksAfterRegistration = true
            //};

            //await socpublicAccountsService.CreateAsync(account);

            #endregion

            #region remove after 10

            //Account account = new Account()
            //{
            //    Login = "taextjnu",
            //    EmailIsConfirmed = true,
            //    Email = new Email("taextjnu@lamesamail.com", "tpovxmfoS6996", new CommonModels.Email.Settings.ConnectSettings("imap.firstmail.ltd", 993, true)),
            //    Password = "taextjnuBotik24",
            //    Pincode = "FC57",
            //    IsAlive = true,
            //    IsMain = false,
            //    MoneyMainBalance = 0,
            //    MoneyAdvertisingBalance = 0,
            //    Refer = new Referal()
            //    {
            //        Id = "8458675",
            //        Email = "golubov.gleb@mail.ru",
            //        Login = "xpomocoma777"
            //    },
            //    RegedDateTime = DateTime.UtcNow,
            //    RegedUseragent = new CommonModels.UserAgentClasses.UserAgent("Mozilla/5.0 (Windows NT 6.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.85 Safari/537.36"),
            //    LastCookies = new List<System.Net.Cookie>()
            //    {

            //    },
            //    History = new List<OneLogAccounResult>(),
            //    IsFirstExecutionOfTasksAfterRegistration = true
            //};

            //await socpublicAccountsService.CreateAsync(account);

            #endregion

            #region remove after 11

            //var accs = await socpublicAccountsService.GetAsync();

            //List<Account> dubles = new List<Account>();

            //foreach (var acc in accs)
            //{
            //    if (!dubles.Contains(acc) && accs.Where(a => a.Login == acc.Login).Count() > 1)
            //    {
            //        dubles.Add(acc);
            //    }
            //}

            //var json = JsonConvert.SerializeObject(dubles);

            //System.IO.File.WriteAllText(@"C:\Users\glebg\Desktop\acc.json", json);

            #endregion

            #region remove after 12

            //var accs = await socpublicAccountsService.GetAsync();

            //double money = 0;

            //foreach (var ac in accs)
            //{
            //    money += ac.MoneyMainBalance ?? 0;
            //}

            //Console.WriteLine(money.ToString());

            #endregion

            #region remove after 13

            //List<Account> accounts = new List<Account>();

            //Account accountXpomocoma = new Account()
            //{
            //    Login = "xpomocoma777",
            //    EmailIsConfirmed = true,
            //    Email = null,
            //    Password = "qwerty777",
            //    Pincode = "9C41",
            //    IsAlive = true,
            //    IsMain = true,
            //    MoneyMainBalance = 0,
            //    MoneyAdvertisingBalance = 0,
            //    Refer = null,
            //    RegedDateTime = DateTime.UtcNow,
            //    RegedUseragent = new CommonModels.UserAgentClasses.UserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0"),
            //    LastCookies = new List<System.Net.Cookie>()
            //    {

            //    },
            //    History = new List<OneLogAccounResult>(),
            //    IsFirstExecutionOfTasksAfterRegistration = false,
            //    PersonalProxy = null,
            //    PersonalPrivateProxy = new EnvironmentProxy("xpomocoma777 proxy", "Personal private proxy of xpomocoma777", EPMarker.MainAccount, "31.42.120.121", "51523", EPProtocol.http)
            //    {
            //        Username = "golubovgleb",
            //        Password = "aS7mBqX7mL"
            //    }
            //};
            //accounts.Add(accountXpomocoma);

            //Account accountDashshendel = new Account()
            //{
            //    Login = "dashshendel",
            //    EmailIsConfirmed = true,
            //    Email = null,
            //    Password = "dashshendel777",
            //    Pincode = "BC48",
            //    IsAlive = true,
            //    IsMain = true,
            //    MoneyMainBalance = 0,
            //    MoneyAdvertisingBalance = 0,
            //    Refer = null,
            //    RegedDateTime = DateTime.UtcNow,
            //    RegedUseragent = new CommonModels.UserAgentClasses.UserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0"),
            //    LastCookies = new List<System.Net.Cookie>()
            //    {

            //    },
            //    History = new List<OneLogAccounResult>(),
            //    IsFirstExecutionOfTasksAfterRegistration = false,
            //    PersonalProxy = null,
            //    PersonalPrivateProxy = new EnvironmentProxy("dashshendel proxy", "Personal private proxy of dashshendel", EPMarker.MainAccount, "31.42.120.187", "51523", EPProtocol.http)
            //    {
            //        Username = "golubovgleb",
            //        Password = "aS7mBqX7mL"
            //    }
            //};
            //accounts.Add(accountDashshendel);

            //Account accountIkuc365 = new Account()
            //{
            //    Login = "ikuc365",
            //    EmailIsConfirmed = true,
            //    Email = null,
            //    Password = "ikuc365.",
            //    Pincode = "70EA",
            //    IsAlive = true,
            //    IsMain = true,
            //    MoneyMainBalance = 0,
            //    MoneyAdvertisingBalance = 0,
            //    Refer = null,
            //    RegedDateTime = DateTime.UtcNow,
            //    RegedUseragent = new CommonModels.UserAgentClasses.UserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.6312.123 Safari/537.36"),
            //    LastCookies = new List<System.Net.Cookie>()
            //    {

            //    },
            //    History = new List<OneLogAccounResult>(),
            //    IsFirstExecutionOfTasksAfterRegistration = false,
            //    PersonalProxy = null,
            //    PersonalPrivateProxy = new EnvironmentProxy("ikuc365 proxy", "Personal private proxy of ikuc365", EPMarker.MainAccount, "94.137.79.95", "51523", EPProtocol.http)
            //    {
            //        Username = "golubovgleb",
            //        Password = "aS7mBqX7mL"
            //    }
            //};
            //accounts.Add(accountIkuc365);

            //await socpublicAccountsService.CreateAsync(accounts);

            //await socpublicAccountsService.TempMethodUpdateAsync();

            #endregion

            #region remove after 14

            // set run tasks settings
            /*await runTasksSettingsService.CreateAsync(new CommonModels.ProjectTask.RunTasksSettings()
            {
                TasksIsRunning = false,
                CurrentLaunchedTaskType = CommonModels.ProjectTask.LaunchTaskType.EarningMoney
            });*/

            #endregion

            //Referal refer = new Referal()
            //{
            //    Id = "8458675",
            //    Email = "golubov.gleb@mail.ru",
            //    Login = "xpomocoma777"
            //};

            //var accounts = await socpublicAccountsService.GetAsync();

            //foreach (var acc in accounts)
            //{
            //    if(acc.IsMain.HasValue && !acc.IsMain.Value && acc.Refer == null)
            //    {
            //        acc.Refer = refer;
            //        await socpublicAccountsService.ReplaceAsync(acc.Id!, acc);
            //    }
            //}

            //List<WithdrawMoneyGroupSelectiveTaskWithAuth> tasks = new List<WithdrawMoneyGroupSelectiveTaskWithAuth>()
            //{
            //    new WithdrawMoneyGroupSelectiveTaskWithAuth("1", TaskFrom.Management, SocpublicComUrl.http, "", new Account(), new CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums.SocpublicComTaskSubtype[]{ CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums.SocpublicComTaskSubtype.WithdrawalOfMoney})
            //    {
            //        Id = ObjectId.GenerateNewId().ToString(),
            //        QueuePosition = 1,
            //        Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created,
            //        ResultStatus = TaskResultStatus.Unknown,
            //        ActionAfterFinish = TaskActionAfterFinish.Renew,
            //        IdOfTheBotAttachedToThisMainAccount = "",
            //        DatabaseSlaveAccountId = "",
            //        DatabaseMainAccountId = "",
            //        DatabaseSlaveAccountLogin = "",
            //        DatabaseMainAccountLogin = "",
            //        IsTaskCreatedOnPlatformBySlave = false,
            //        IsTaskCompletedOnPlatformByMain = false
            //    },

            //    new WithdrawMoneyGroupSelectiveTaskWithAuth("1", TaskFrom.Management, SocpublicComUrl.http, "", new Account(), new CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums.SocpublicComTaskSubtype[]{ CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums.SocpublicComTaskSubtype.WithdrawalOfMoney})
            //    {
            //        Id = ObjectId.GenerateNewId().ToString(),
            //        QueuePosition = 2,
            //        Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created,
            //        ResultStatus = TaskResultStatus.Unknown,
            //        ActionAfterFinish = TaskActionAfterFinish.Renew,
            //        IdOfTheBotAttachedToThisMainAccount = "",
            //        DatabaseSlaveAccountId = "",
            //        DatabaseMainAccountId = "",
            //        DatabaseSlaveAccountLogin = "",
            //        DatabaseMainAccountLogin = "",
            //        IsTaskCreatedOnPlatformBySlave = false,
            //        IsTaskCompletedOnPlatformByMain = false
            //    }
            //};
            //await earnSiteTasksService.CreateAsync(tasks);

            #region  add internall tasks to database
            ////string data = System.IO.File.ReadAllText(@"C:\Users\glebg\Desktop\Zarplata Project v3.0\platform socpublic internal wirthdraw tasks files\internal_acc_tasks_new_add.json");
            //string data = System.IO.File.ReadAllText(@"C:\Users\glebg\Desktop\Zarplata Project v3.0\platform socpublic internal wirthdraw tasks files\internal_acc_tasks_new_add_with_logins.json");
            //List<PlatformInternalAccountTaskModel>? platformInternal = JsonConvert.DeserializeObject<List<PlatformInternalAccountTaskModel>>(data);
            ////List<PlatformInternalAccountTaskModel>? platformInternalWithLogins = new List<PlatformInternalAccountTaskModel>();
            //if (platformInternal == null)
            //    return Ok(new
            //    {
            //        status = "ERROR platformInternal == null"
            //    });

            //// install logins slaves
            ////var accounts = await socpublicAccountsService.GetAsync();
            ////accounts.RemoveAll(acc => acc.IsAlive == false || acc.IsMain == true);
            ////for (int i = 0; i < accounts.Count; i++)
            ////{
            ////    //if (accounts[i].IsAlive.HasValue && accounts[i].IsAlive!.Value == false)
            ////    //    continue;
            ////    //if (accounts[i].IsMain.HasValue && accounts[i].IsMain!.Value == true)
            ////    //    continue;

            ////    platformInternal[i].SlaveAccountWhoCreateTaskLogin = accounts[i].Login;

            ////    platformInternalWithLogins.Add(platformInternal[i]);
            ////}

            ////await System.IO.File.WriteAllTextAsync(@"C:\Users\glebg\Desktop\Zarplata Project v3.0\platform socpublic internal wirthdraw tasks files\internal_acc_tasks_new_add_with_logins.json", JsonConvert.SerializeObject(platformInternalWithLogins));
            //await platformInternalAccountTasksCollection.CreateAsync(platformInternal); 
            #endregion

            #region Rename files to logins
            //Console.Clear();

            //string avatar_folder_path = @"C:\Users\glebg\Desktop\pics for profile avatar\socpublic.com\avatar";
            //string[] files_names = Directory.GetFiles(avatar_folder_path).Select(file => Path.GetFileName(file)).ToArray();

            //List<Account> socpublicAccounts = new List<Account>();
            //try
            //{
            //    socpublicAccounts = await socpublicAccountsService.GetAsync();
            //    socpublicAccounts.RemoveAll(acc => acc.IsAlive != null && acc.IsAlive == false);
            //}
            //catch (Exception e) { }

            //for (int i = 0; i < files_names.Length; i++)
            //{
            //    System.IO.File.Move(Path.Combine(avatar_folder_path, files_names[i]), Path.Combine(avatar_folder_path, $"{socpublicAccounts[i].Login!}.jpg"));

            //    Console.ForegroundColor = ConsoleColor.Green;
            //    Console.Write("[+]");
            //    Console.ForegroundColor = ConsoleColor.Gray;
            //    Console.Write($" {socpublicAccounts[i].Login!}.jpg");
            //    Console.WriteLine();
            //} 
            #endregion

            //List<string> logins = System.IO.File.ReadAllLinesAsync(@"C:\Users\glebg\Desktop\LoginsWithEditedTasks.txt").Result.ToList();
            //await earnSiteTasksService.DeleteManyByAccLoginAsync(logins);

            //List<string> logins = new List<string>();

            //var tasks = await earnSiteTasksService.GetAsync();

            //foreach (var task in tasks)
            //{
            //    string? error = task.ErrorMessage;

            //    if (error != null && error.Contains("edited successfully"))
            //    {
            //        logins.Add(task.Account.Login);
            //    }
            //}

            //await System.IO.File.WriteAllLinesAsync(@"C:\Users\glebg\Desktop\LoginsWithEditedTasks.txt", logins);

            //await socpublicAccountsService.TempMethodUpdateAsync();

            //await platformInternalAccountTasksCollection.TESTUPDATEASYNC();

            //await proxyTasksService.DeleteAsync();
            //await proxyTasksErorrsLog.DeleteAsync();
            //await earnSiteTasksService.DeleteAsync();

            //dynamic? _task = await earnSiteTasksService.TESTGetOneCreatedWirthdrawForSlaveAsync();
            //dynamic? _task2 = await earnSiteTasksService.GetOneCreatedWirthdrawForSlaveAsync();

            //var dump1 = await accountReservedProxyService.GetAsync();
            //await System.IO.File.WriteAllTextAsync(@$"C:\Users\glebg\Desktop\БЕКАП БД КОЛЛЕКЦИЙ\accountReservedProxiesCollection.json", JsonConvert.SerializeObject(dump1));
            //var dump2 = await blockedMachinesService.GetAsync();
            //await System.IO.File.WriteAllTextAsync(@$"C:\Users\glebg\Desktop\БЕКАП БД КОЛЛЕКЦИЙ\blockedMachinesCollection.json", JsonConvert.SerializeObject(dump2));
            //var dump3 = await clientsService.GetAsync();
            //await System.IO.File.WriteAllTextAsync(@$"C:\Users\glebg\Desktop\БЕКАП БД КОЛЛЕКЦИЙ\clientsCollection.json", JsonConvert.SerializeObject(dump3));
            //var dump4 = await earnSiteTasksService.GetAsync();
            //await System.IO.File.WriteAllTextAsync(@$"C:\Users\glebg\Desktop\БЕКАП БД КОЛЛЕКЦИЙ\earnSiteTasksCollection.json", JsonConvert.SerializeObject(dump4));
            //var dump5 = await environmentProxiesService.GetAsync();
            //await System.IO.File.WriteAllTextAsync(@$"C:\Users\glebg\Desktop\БЕКАП БД КОЛЛЕКЦИЙ\environmentProxiesCollection(09.12.2024).json", JsonConvert.SerializeObject(dump5));
            //var dump6 = await platformInternalAccountTasksCollection.GetAsync();
            //await System.IO.File.WriteAllTextAsync(@$"C:\Users\glebg\Desktop\БЕКАП БД КОЛЛЕКЦИЙ\platformInternalAccountTasksCollection(09.12.2024).json", JsonConvert.SerializeObject(dump6));
            //var dump7 = await proxyTasksErorrsLog.GetAsync();
            //await System.IO.File.WriteAllTextAsync(@$"C:\Users\glebg\Desktop\БЕКАП БД КОЛЛЕКЦИЙ\proxyTasksErorrsLog.json", JsonConvert.SerializeObject(dump7));
            //var dump8 = await proxyTasksService.GetAsync();
            //await System.IO.File.WriteAllTextAsync(@$"C:\Users\glebg\Desktop\БЕКАП БД КОЛЛЕКЦИЙ\proxyTasksCollection.json", JsonConvert.SerializeObject(dump8));
            //var dump9 = await runTasksSettingsService.GetAsync();
            //await System.IO.File.WriteAllTextAsync(@$"C:\Users\glebg\Desktop\БЕКАП БД КОЛЛЕКЦИЙ\runTasksSettings.json", JsonConvert.SerializeObject(dump9));
            //var dump10 = await siteParseBalancerService.GetAsync();
            //await System.IO.File.WriteAllTextAsync(@$"C:\Users\glebg\Desktop\БЕКАП БД КОЛЛЕКЦИЙ\siteParseBalancerCollection.json", JsonConvert.SerializeObject(dump10));
            //var dump11 = await socpublicAccountsService.GetAsync();
            //await System.IO.File.WriteAllTextAsync(@$"C:\Users\glebg\Desktop\БЕКАП БД КОЛЛЕКЦИЙ\socpublicAccountsCollection(09.12.2024).json", JsonConvert.SerializeObject(dump11));
            //var dump12 = await userSessionsService.GetAsync();
            //await System.IO.File.WriteAllTextAsync(@$"C:\Users\glebg\Desktop\БЕКАП БД КОЛЛЕКЦИЙ\userSessionsCollection.json", JsonConvert.SerializeObject(dump12));
            //var dump13 = await usersService.GetAsync();
            //await System.IO.File.WriteAllTextAsync(@$"C:\Users\glebg\Desktop\БЕКАП БД КОЛЛЕКЦИЙ\usersCollection.json", JsonConvert.SerializeObject(dump13));










            /////
            //List<KeyValuePair<string, string>> accs = new List<KeyValuePair<string, string>>();
            //var tasks = await earnSiteTasksService.GetAsync();

            //foreach (var task in tasks)
            //{
            //    if(task.ResultStatus == TaskResultStatus.Error && (task.ErrorMessage as string)?.Contains("Неверный логин и/или пароль"))
            //    {
            //        if(accs.Where(acc => acc.Key == task.Account.Login).Count() == 0)
            //        {
            //            accs.Add(new KeyValuePair<string, string>(task.Account.Login, task.Account.Email.Address));
            //        }
            //    }
            //}

            //await System.IO.File.WriteAllTextAsync(@"C:\Users\glebg\Desktop\BadAccsLogPass.json", JsonConvert.SerializeObject(accs));

            //Console.WriteLine($"All tasks count: {tasks.Count}");
            //Console.WriteLine($"Bad Log|pass accounts count: {accs.Count}");


            ///////
            //List<KeyValuePair<string, string>> proxies = new List<KeyValuePair<string, string>>();
            //List<KeyValuePair<string, string>> badaccs = new List<KeyValuePair<string, string>>();

            //foreach (var task in tasks)
            //{
            //    List<OneLogAccounResult>? history = (List<OneLogAccounResult>?)task.Account.History;

            //    if (history != null && history.Count() > 0)
            //    {
            //        OneLogAccounResult? result = history[history.Count() - 1];

            //        if (result != null && result.Proxy != null && proxies.Where(p => p.Key == result.Proxy.proxy.Ip && p.Value == result.Proxy.proxy.Port).Count() > 0)
            //        {
            //            badaccs.Add(new KeyValuePair<string, string>(task.Account.Login, task.Account.Email.Address));
            //        }

            //        if (result!.Proxy != null)
            //        {
            //            proxies.Add(new KeyValuePair<string, string>(result.Proxy.proxy.Ip, result.Proxy.proxy.Port)); 
            //        }
            //    }
            //}

            //await System.IO.File.WriteAllTextAsync(@"C:\Users\glebg\Desktop\BadAccsProxy.json", JsonConvert.SerializeObject(badaccs));
            //await System.IO.File.WriteAllTextAsync(@"C:\Users\glebg\Desktop\BadProxy.json", JsonConvert.SerializeObject(proxies));

            //Console.WriteLine($"Bad proxy accounts count: {badaccs.Count}");


            /// remove bad accs
            //List<dynamic>? accounts = JsonConvert.DeserializeObject<List<dynamic>>(System.IO.File.ReadAllText(@"C:\Users\glebg\Desktop\tasks bad statistics\BadAccsLogPass.json"));
            //foreach(dynamic account in accounts!)
            //{
            //    string login = account.Key;
            //    string email = account.Value;
            //    await socpublicAccountsService.TempMethodDeleteAsync(login, email);
            //}


            ///check count blocked accs
            //var accs = await socpublicAccountsService.GetAsync();
            //int count = 0;
            //foreach (var item in accs)
            //{
            //    if(item.IsAlive.HasValue && item.IsAlive.Value == false)
            //    {
            //        count++;
            //    }
            //}
            //Console.WriteLine($"Count blocked accounts: {count}");


            ///
            //await earnSiteTasksService.DeleteTempmethodAsync();

            //var accs = await socpublicAccountsService.GetAsync();
            //List<string> logins = new List<string>();
            //foreach (var acc in accs)
            //{
            //    if(acc.EmailIsConfirmed.HasValue && acc.EmailIsConfirmed.Value == false)
            //    {

            //    }
            //}



            ///check count alive accs
            //var accs = await socpublicAccountsService.GetAsync();
            //int count = 0;
            //foreach (var item in accs)
            //{
            //    if (item.IsAlive.HasValue && item.IsAlive.Value == true)
            //    {
            //        count++;
            //    }
            //}
            //Console.WriteLine($"Count Alive accounts: {count}");


            // update internal tasks price
            //var internalTasks = await platformInternalAccountTasksCollection.GetAsync();
            //var accs = await socpublicAccountsService.GetAsync();
            //int count = 0;
            //double task_pay_fee = 0.30;
            //foreach (var internalTask in internalTasks)
            //{
            //    Account? account = accs.Where(acc => acc.Login == internalTask.SlaveAccountWhoCreateTaskLogin).FirstOrDefault();
            //    if (account != null && account.IsAlive.HasValue && account.IsAlive.Value)
            //    {
            //        string final_new_price = "";

            //        double current_fee = account.MoneyMainBalance!.Value * task_pay_fee;

            //        double current_price_with_fee = account.MoneyMainBalance!.Value - current_fee;

            //        if(current_price_with_fee < 1)
            //        {
            //            current_price_with_fee = 1.00;
            //        }

            //        //double price_new = Math.Round(account.MoneyMainBalance!.Value, 2, MidpointRounding.ToZero);
            //        double price_new = Math.Round(current_price_with_fee, 2, MidpointRounding.ToZero);

            //        final_new_price = ((int)price_new).ToString();

            //        //double priceCompare = (double)(int)price_new;

            //        //if(price_new - priceCompare == 0)
            //        //{
            //        //    final_new_price = ((int)price_new).ToString();
            //        //}
            //        //else
            //        //{
            //        //    final_new_price = (price_new).ToString();
            //        //}

            //        await platformInternalAccountTasksCollection.UpdateBySlaveAccLoginAsync(internalTask.SlaveAccountWhoCreateTaskLogin!, "price_user", final_new_price);

            //        count++;

            //        Console.WriteLine($"Task price changed from {internalTask.price_user} to {final_new_price}");
            //    }
            //}
            //Console.WriteLine();
            //Console.WriteLine($"Count updated tasks: {count}");



            return Ok(new
            {
                status = "Connected successfully"
            });
        }

        //[HttpGet]
        //[Route("createproxytask")]
        //public async Task<IActionResult> createproxytask()
        //{
        //    //clear proxy task queue
        //    await proxyTasksService.DeleteAsync();

        //    // request to proxy-combiner service with data checkRequirements
        //    try
        //    {
        //        CheckRequirements checkRequirements = new CheckRequirements(
        //                    EarningSiteTaskUrls.SocpublicComUrl.http,
        //                    EarningSiteTaskUrls.SocpublicComUrl.https,
        //                    new CheckedProxy.CPProtocol[] { CheckedProxy.CPProtocol.http, CheckedProxy.CPProtocol.socks5, CheckedProxy.CPProtocol.socks4 },
        //                    new CheckedProxy.SupportsSecureConnectionState[] { CheckedProxy.SupportsSecureConnectionState.Support, CheckedProxy.SupportsSecureConnectionState.NotSupport, CheckedProxy.SupportsSecureConnectionState.Undefined },
        //                    new CheckedProxy.AnonymousState[] { CheckedProxy.AnonymousState.Anonymous },
        //                    5000,
        //                    new CheckedProxy.CPWorkState[] { CheckedProxy.CPWorkState.Work }
        //                    );

        //        ParseRequirements prequirements = new ParseRequirements(100);
        //        string? id = null;
        //        SpecialRequirements specialRequirements = new SpecialRequirements(id, EarningSiteTaskEnums.EarningSiteEnum.SocpublicDotCom);


        //        SpecialCombineTask task = new SpecialCombineTask("authorid", CommonModels.ProjectTask.ProjectTaskEnums.TaskFrom.Bot, CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created, prequirements, checkRequirements, specialRequirements)
        //        {
        //            Id = ObjectId.GenerateNewId().ToString(),
        //            QueuePosition = 0,
        //            Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created,
        //            ResultStatus = TaskResultStatus.Unknown,
        //            ActionAfterFinish = TaskActionAfterFinish.Renew,
        //        };

        //        await proxyTasksService.CreateAsync(task);

        //        // give task to bot
        //        List<ModelClient> bots = await clientsService.GetAsync();

        //        List<ModelClient> freeBots = bots.Where(bot => bot.Status == CommonModels.Client.Client.ClientStatus.Free && bot.Role == BotRole.ProxyCombineBot).ToList();

        //        if (freeBots.Count > 0)
        //        {
        //            // send Task to bot
        //            await clientHubContext.Clients.User(freeBots[0].ID!).SendAsync($"do_new_proxycombiner_{task.InternalType.ToString().ToLower()}_task", task);

        //            // update Bot status
        //            await clientsService.UpdateAsync(freeBots[0].ID!, "Status", (int)ClientStatus.AtWork);

        //            // update task status
        //            await proxyTasksService.UpdateAsync(task.Id!, nameof(task.Status), (int)TaskStatus.Executing);

        //            // update Task executorId
        //            await proxyTasksService.UpdateAsync(task.Id!, "ExecutorId", freeBots[0].ID!);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new
        //        {
        //            status = $"Proxy task create Error\n\n{ex.Message}"
        //        });
        //    }

        //    return Ok(new
        //    {
        //        status = "Proxy task created successfully"
        //    });
        //}

        /*[HttpGet]
        [Route("errorTestServerHubConnection")]
        public async Task<IActionResult> errorTestServerHubConnection()
        {
            //await taskHubContext.Clients.Group("socpublic.com").SendAsync("startWork");

            //save error to DB

            return Ok(new
            {
                status = "Error saved successfully"
            });
        }*/
    }
}
