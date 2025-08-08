using AngleSharp.Dom;
using AngleSharp;
using CommonModels.Client;
using CommonModels.ClientLibraries.ProjectTask;
using CommonModels.ProjectTask.ProxyCombiner;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models;
using Jint;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tesseract;
using CommonModels.UserAgentClasses;
using Newtonsoft.Json;
using CommonModels.ProjectTask.ProxyCombiner.Models;
using System.Reflection;

namespace ProxyCombiner.Classes.Internal.SelectiveTask
{
    internal class Parse : WebClientTask, IDisposable
    {
        private HttpClient ProxyParseHttpClient { get; set; }
        private CookieContainer ProxyParseCookieContainer { get; set; }
        private Client client { get; set; }

        private static readonly string REIpPattern = @"(((25[0-5])|(2[0-4][0-9])|(1[0-9][0-9])|([1-9][0-9]|[0-9]))(\.)){3}((25[0-5])|(2[0-4][0-9])|(1[0-9][0-9])|([1-9][0-9]|[0-9]))";
        private static readonly string REPortPattern = @"((6553[0-5])|(655[0-2][0-9])|(65[0-4][0-9][0-9])|(6[0-4][0-9][0-9][0-9])|([1-5][0-9][0-9][0-9][0-9])|([1-9][0-9][0-9][0-9])|([1-9][0-9][0-9])|([1-9][0-9])|([0-9]))";
        private static readonly string REProxyAddressPattern = @$"({REIpPattern}" + @"(\:)" + @$"{REPortPattern})";

        private bool disposedValue;

        internal protected Parse(
            HttpClient proxyParseHttpClient,
            CookieContainer proxyParseCookieContainer,
            Client client)
        {
            ProxyParseHttpClient = proxyParseHttpClient;
            ProxyParseCookieContainer = proxyParseCookieContainer;
            this.client = client;
        }

        #region Parse site enums

        #region spys.one
        //По
        private enum SpysDotOne_xpp
        {
            _25 = 0,
            _50 = 1,
            _100 = 2,
            _200 = 3,
            _300 = 4,
            _500 = 5
        }
        //Страна
        private enum SpysDotOne_tldc
        {
            ALL_179_стран = 0,
            US_United_States = 1,
            ID_Indonesia = 2,
            RU_Russia = 3,
            FR_France = 4,
            DE_Germany = 5,
            BR_Brazil = 6,
            TR_Turkey = 7,
            BD_Bangladesh = 8,
            EC_Ecuador = 9,
            IN_India = 10,
            CO_Colombia = 11,
            SG_Singapore = 12,
            GB_United_Kingdom = 13,
            MX_Mexico = 14,
            HK_Hong_Kong = 15,
            IR_Iran = 16,
            ZA_South_Africa = 17,
            VE_Venezuela = 18,
            JP_Japan = 19,
            FI_Finland = 20,
            VN_VietNam = 21,
            AR_Argentina = 22,
            ES_Spain = 23,
            TH_Thailand = 24,
            CL_Chile = 25,
            CN_China = 26,
            PL_Poland = 27,
            PE_Peru = 28,
            EG_Egypt = 29,
            NL_Netherlands = 30,
            CA_Canada = 31,
            KR_South_Korea = 32,
            PK_Pakistan = 33,
            DO_Dominican_Republic = 34,
            UA_Ukraine = 35,
            PH_Philippines = 36,
            AU_Australia = 37,
            IT_Italy = 38,
            KH_Cambodia = 39,
            TW_Taiwan = 40,
            KE_Kenya = 41,
            MY_Malaysia = 42,
            IQ_Iraq = 43,
            SE_Sweden = 44,
            LY_Libyan_Arab_Jamahiriya = 45,
            HU_Hungary = 46,
            CZ_Czech_Republic = 47,
            GT_Guatemala = 48,
            NP_Nepal = 49,
            HN_Honduras = 50,
            YE_Yemen = 51,
            NG_Nigeria = 52,
            PS_Palestinian_Territory = 53,
            RS_Serbia = 54,
            BG_Bulgaria = 55,
            PY_Paraguay = 56,
            SA_Saudi_Arabia = 57,
            RO_Romania = 58,
            IE_Ireland = 59,
            KZ_Kazakhstan = 60,
            AE_United_Arab_Emirates = 61,
            CH_Switzerland = 62,
            AL_Albania = 63,
            LB_Lebanon = 64,
            GR_Greece = 65,
            UZ_Uzbekistan = 66,
            BY_Belarus = 67,
            SK_Slovakia = 68,
            BO_Bolivia = 69,
            TZ_Tanzania = 70,
            PR_Puerto_Rico = 71,
            AF_Afghanistan = 72,
            MD_Moldova = 73,
            AT_Austria = 74,
            LV_Latvia = 75,
            NO_Norway = 76,
            LT_Lithuania = 77,
            AM_Armenia = 78,
            BE_Belgium = 79,
            GE_Georgia = 80,
            QA_Qatar = 81,
            IL_Israel = 82,
            HR_Croatia = 83,
            CR_Costa_Rica = 84,
            BA_Bosnia_and_Herzegovina = 85,
            MN_Mongolia = 86,
            KG_Kyrgyzstan = 87,
            MM_Myanmar = 88,
            UY_Uruguay = 89,
            MK_Macedonia = 90,
            XK_Kosovo = 91,
            GH_Ghana = 92,
            PT_Portugal = 93,
            NZ_New_Zealand = 94,
            ME_Montenegro = 95,
            PA_Panama = 96,
            UG_Uganda = 97,
            DK_Denmark = 98,
            MG_Madagascar = 99,
            MW_Malawi = 100,
            CD_Congo = 101,
            NI_Nicaragua = 102,
            EE_Estonia = 103,
            CM_Cameroon = 104,
            ZM_Zambia = 105,
            ZW_Zimbabwe = 106,
            SI_Slovenia = 107,
            SV_El_Salvador = 108,
            MZ_Mozambique = 109,
            CY_Cyprus = 110,
            SY_Syrian_Arab_Republic = 111,
            SC_Seychelles = 112,
            LK_Sri_Lanka = 113,
            OM_Oman = 114,
            AZ_Azerbaijan = 115,
            BW_Botswana = 116,
            MV_Maldives = 117,
            LA_Lao_Peoples_Democratic_Republic = 118,
            TN_Tunisia = 119,
            NA_Namibia = 120,
            TJ_Tajikistan = 121,
            JO_Jordan = 122,
            TL_Timor = 123,
            RW_Rwanda = 124,
            GN_Guinea = 125,
            LU_Luxembourg = 126,
            BJ_Benin = 127,
            CU_Cuba = 128,
            BI_Burundi = 129,
            AO_Angola = 130,
            MU_Mauritius = 131,
            IS_Iceland = 132,
            GQ_Equatorial_Guinea = 133,
            SO_Somalia = 134,
            CI_Cote_dIvoire = 135,
            MA_Morocco = 136,
            DZ_Algeria = 137,
            KW_Kuwait = 138,
            MO_Macao = 139,
            HT_Haiti = 140,
            AD_Andorra = 141,
            BH_Bahrain = 142,
            ML_Mali = 143,
            TD_Chad = 144,
            SS_South_Sudan = 145,
            SN_Senegal = 146,
            MT_Malta = 147,
            YT_Mayotte = 148,
            SZ_Swaziland = 149,
            GA_Gabon = 150,
            BT_Bhutan = 151,
            ET_Ethiopia = 152,
            KY_Cayman_Islands = 153,
            LS_Lesotho = 154,
            WS_Samoa = 155,
            BZ_Belize = 156,
            FJ_Fiji = 157,
            CG_Congo = 158,
            GY_Guyana = 159,
            TT_Trinidad_and_Tobago = 160,
            SH_Saint_Helena = 161,
            SL_Sierra_Leone = 162,
            PG_Papua_New_Guinea = 163,
            JM_Jamaica = 164,
            TG_Togo = 165,
            SD_Sudan = 166,
            BB_Barbados = 167,
            VG_Virgin_Islands, _British = 168,
            SR_Suriname = 169,
            LI_Liechtenstein = 170,
            DJ_Djibouti = 171,
            SX_Sint_Maarten = 172,
            LR_Liberia = 173,
            CV_Cape_Verde = 174,
            BM_Bermuda = 175,
            BF_Burkina_Faso = 176,
            VU_Vanuatu = 177,
            GU_Guam = 178,
            TM_Turkmenistan = 179
        }
        //ANM
        private enum SpysDotOne_xf1
        {
            ALL = 0,
            A_plus_H = 1,
            NOA = 2,
            ANM = 3,
            HIA = 4
        }
        //SSL
        private enum SpysDotOne_xf2
        {
            ALL = 0,
            SSL_plus = 1,
            SSL_minus = 2
        }
        //Порт
        private enum SpysDotOne_xf4
        {
            ALL = 0,
            _3128 = 1,
            _8080 = 2,
            _80 = 3,
            _1080 = 4
        }
        //Тип
        private enum SpysDotOne_xf5
        {
            ALL = 0,
            HTTP = 1,
            SOCKS = 2
        }
        #endregion

        #region geonode.com
        // limit
        private enum GeonodeDotCom_limit
        {
            _100 = 100,
            _500 = 500
        }
        // page
        private enum GeonodeDotCom_page
        {
            _1 = 1
        }
        // sort_by
        private enum GeonodeDotCom_sort_by
        {
            lastChecked
        }
        // sort_type
        private enum GeonodeDotCom_sort_type
        {
            desc
        }
        #endregion

        #region proxyscrape.com
        // protocol
        private enum ProxyscrapeDotCom_protocol
        {
            http,
            socks4,
            socks5
        }
        #endregion

        #region www.freeproxy.world
        // type
        private enum FreeproxyDotWorld_type
        {
            http,
            https,
            socks4,
            socks5
        }
        // anonymity
        private enum FreeproxyDotWorld_anonymity
        {
            high = 4,
            no = 1
        }
        // page
        private enum FreeproxyDotWorld_page
        {
            _1 = 1
        }
        #endregion

        #region openproxy.space
        // protocol
        private enum OpenproxyDotSpace_protocol
        {
            http,
            socks4,
            socks5
        }
        #endregion

        #region advanced.name
        // type
        private enum AdvancedDotName_type
        {
            all,
            http,
            https,
            socks4,
            socks5,
            anon,
            elite,
            transparent,
        }
        #endregion

        #region github.com/TheSpeedX/PROXY-List
        // protocol
        private enum GithubDotComTheSpeedXPROXY_List_protocol
        {
            http,
            socks4,
            socks5
        }
        #endregion

        #endregion

        #region WebClientTask Overrided Methods
        internal async virtual Task<bool> InitWebContextData()
        {
            // load data from resources
            var assembly = Assembly.GetExecutingAssembly();
            // load JavaScriptFiles
            string jsProjectFunctions = await client.ReadResourceFileToString("ProxyCombinerJavaScriptFile.js", assembly);
            // load tesseract ocr model(.traineddata) files
            string tessdataDir = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
            if (!Directory.Exists(tessdataDir))
            {
                Directory.CreateDirectory(tessdataDir);
            }
            if (!File.Exists(Path.Combine(tessdataDir, "eng.traineddata")))
            {
                await client.ExtractResourceFile("eng.traineddata", "eng.traineddata", assembly, tessdataDir);
            }

            // set or create UserAgent
            UserAgent? userAgent = CreateNewRandomUserAgent();
            if (userAgent == null)
            {
                throw new Exception($"class ProxyCombinerTask(), method InitWebContextData(), error userAgent == null");
            }

            // set or create Cookies
            // not necessary

            // set or create Headers
            var headers = CreateNewRandomHeaderCollection(userAgent!);
            if (headers == null)
            {
                throw new Exception($"class ProxyCombinerTask(), method InitWebContextData(), error h == null");
            }

            // create BrowsingContext for parse
            BrowsingContext ParseContext = new BrowsingContext();

            // creage JSContext for execute js code
            Engine jsEngine = new Engine();

            // create TesseractContext for recognize text from img
            TesseractEngine? tesseractEngine = null;

            try
            {
                tesseractEngine = new TesseractEngine(tessdataDir, "eng", EngineMode.TesseractAndLstm);
            }
            catch(Exception e)
            {
                Console.WriteLine($"create TesseractContext for recognize text from img\n\n{e.InnerException.Message}");
            }

            // init this data
            var init = base.SetWebContextData(
                contextUserA: userAgent,
                contextH: headers,
                htmlParseC: ParseContext,
                jsExecuteC: jsEngine,
                tesseractExecuteC: tesseractEngine);
            if (init == false)
            {
                throw new Exception($"class ProxyCombinerTask(), method InitWebContextData(), error init == false");
            }


            try
            {
                base.jsExecuteContextEngine!.Execute(jsProjectFunctions);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"base.jsExecuteContextEngine!.Execute(jsProjectFunctions);\n\n{ex.Message}");
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

            string supportOnlineTalkID = Guid.NewGuid().ToString("N");
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
                        Path = "/",
                        Expired = false,
                        Expires = DateTime.UtcNow,
                        Secure = true,
                        HttpOnly = false
                    },
                    new Cookie()
                    {
                        Name = "supportOnlineTalkID",
                        Value = supportOnlineTalkID,
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
                headerCollection.Add("accept", new List<string>() { "text/html", "text/plain", "application/json", "application/xhtml+xml", "application/xml;q=0.9", "image/webp", "image/apng", "image/jpeg", "*/*;q=0.8", "application/signed-exchange;v=b3;q=0.7" });
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
        #endregion

        #region Parse
        internal async Task<List<ParsedProxy>> ParseProxy(MethodName method)
        {
            List<ParsedProxy> currentParsedProxyList = new List<ParsedProxy>();

            switch (method)
            {
                case MethodName.ParseSite_SpysDotOne:
                    currentParsedProxyList.AddRange(await ParseSite_SpysDotOne());
                    break;
                case MethodName.ParseSite_GeonodeDotCom:
                    currentParsedProxyList.AddRange(await ParseSite_GeonodeDotCom());
                    break;
                case MethodName.ParseSite_ProxyscrapeDotCom:
                    currentParsedProxyList.AddRange(await ParseSite_ProxyscrapeDotCom());
                    break;
                case MethodName.ParseSite_FreeproxyDotWorld:
                    currentParsedProxyList.AddRange(await ParseSite_FreeproxyDotWorld());
                    break;
                case MethodName.ParseSite_OpenproxyDotSpace:
                    currentParsedProxyList.AddRange(await ParseSite_OpenproxyDotSpace());
                    break;
                case MethodName.ParseSite_AdvancedDotName:
                    currentParsedProxyList.AddRange(await ParseSite_AdvancedDotName());
                    break;
                case MethodName.ParseSite_ProxypremiumDotTop:
                    currentParsedProxyList.AddRange(await ParseSite_ProxypremiumDotTop());
                    break;
                case MethodName.ParseSite_FineproxyDotOrg:
                    currentParsedProxyList.AddRange(await ParseSite_FineproxyDotOrg());
                    break;
                case MethodName.ParseSite_Free_proxy_listDotNet:
                    currentParsedProxyList.AddRange(await ParseSite_Free_proxy_listDotNet());
                    break;
                case MethodName.ParseSite_SslproxiesDotOrg:
                    currentParsedProxyList.AddRange(await ParseSite_SslproxiesDotOrg());
                    break;
                case MethodName.ParseSite_Hide_my_ipDotcom:
                    currentParsedProxyList.AddRange(await ParseSite_Hide_my_ipDotcom());
                    break;
                case MethodName.ParseSite_GhostealthDotCom:
                    currentParsedProxyList.AddRange(await ParseSite_GhostealthDotCom());
                    break;
                case MethodName.ParseSite_Us_proxyDotOrg:
                    currentParsedProxyList.AddRange(await ParseSite_Us_proxyDotOrg());
                    break;
                case MethodName.ParseSite_My_proxyDotCom:
                    currentParsedProxyList.AddRange(await ParseSite_My_proxyDotCom());
                    break;
                case MethodName.ParseSite_Socks_proxyDotNet:
                    currentParsedProxyList.AddRange(await ParseSite_Socks_proxyDotNet());
                    break;
                case MethodName.ParseSite_Google_proxyDotNet:
                    currentParsedProxyList.AddRange(await ParseSite_Google_proxyDotNet());
                    break;
                case MethodName.ParseSite_SpysDotMe:
                    currentParsedProxyList.AddRange(await ParseSite_SpysDotMe());
                    break;
                case MethodName.ParseSite_FreeproxyDotlunaproxyDotCom:
                    currentParsedProxyList.AddRange(await ParseSite_FreeproxyDotlunaproxyDotCom());
                    break;
                case MethodName.ParseSite_Proxy_dailyDotCom:
                    currentParsedProxyList.AddRange(await ParseSite_Proxy_dailyDotCom());
                    break;
                case MethodName.ParseSite_FreeproxylistDotRu:
                    currentParsedProxyList.AddRange(await ParseSite_FreeproxylistDotRu());
                    break;
                case MethodName.ParseSite_ProxyserversDotPro:
                    currentParsedProxyList.AddRange(await ParseSite_ProxyserversDotPro());
                    break;
                case MethodName.ParseSite_FineproxyDotDe:
                    currentParsedProxyList.AddRange(await ParseSite_FineproxyDotDe());
                    break;
                case MethodName.ParseSite_VpnsideDotCom:
                    currentParsedProxyList.AddRange(await ParseSite_VpnsideDotCom());
                    break;
                case MethodName.ParseSite_GithubDotComTheSpeedXPROXY_List:
                    currentParsedProxyList.AddRange(await ParseSite_GithubDotComTheSpeedXPROXY_List());
                    break;
                case MethodName.ParseSite_Proxy11DotCom:
                    currentParsedProxyList.AddRange(await ParseSite_Proxy11DotCom());
                    break;
                case MethodName.ParseSite_IproyalDotCom:
                    currentParsedProxyList.AddRange(await ParseSite_IproyalDotCom());
                    break;
                case MethodName.ParseSite_Free_proxy_listDotCom:
                    currentParsedProxyList.AddRange(await ParseSite_Free_proxy_listDotCom());
                    break;
                case MethodName.ParseSite_Proxy_listDotOrg:
                    currentParsedProxyList.AddRange(await ParseSite_Proxy_listDotOrg());
                    break;
                case MethodName.ParseSite_Best_proxyDotCom:
                    currentParsedProxyList.AddRange(await ParseSite_Best_proxyDotCom());
                    break;
                case MethodName.ParseSite_Best_proxiesDotRu:
                    currentParsedProxyList.AddRange(await ParseSite_Best_proxiesDotRu());
                    break;
                case MethodName.ParseSite_ProxynovaDotCom:
                    currentParsedProxyList.AddRange(await ParseSite_ProxynovaDotCom());
                    break;
                case MethodName.ParseSite_ProxydockerDotCom:
                    currentParsedProxyList.AddRange(await ParseSite_ProxydockerDotCom());
                    break;
                case MethodName.ParseSite_GoodproxiesDotRu:
                    currentParsedProxyList.AddRange(await ParseSite_GoodproxiesDotRu());
                    break;
                case MethodName.ParseSite_CheckerproxyDotNet:
                    currentParsedProxyList.AddRange(await ParseSite_CheckerproxyDotNet());
                    break;
                case MethodName.ParseSite_XseoDotIn:
                    currentParsedProxyList.AddRange(await ParseSite_XseoDotIn());
                    break;
                case MethodName.ParseSite_MarcosblDotCom:
                    currentParsedProxyList.AddRange(await ParseSite_MarcosblDotCom());
                    break;
                case MethodName.ParseSite_IpaddressDotCom:
                    currentParsedProxyList.AddRange(await ParseSite_IpaddressDotCom());
                    break;
                case MethodName.ParseSite_HttptunnelDotGe:
                    currentParsedProxyList.AddRange(await ParseSite_HttptunnelDotGe());
                    break;
                case MethodName.ParseSite_KuaidailiDotCom:
                    currentParsedProxyList.AddRange(await ParseSite_KuaidailiDotCom());
                    break;
                case MethodName.ParseSite_Ip3366DotNet:
                    currentParsedProxyList.AddRange(await ParseSite_Ip3366DotNet());
                    break;
                case MethodName.ParseSite_CybersyndromeDotNet:
                    currentParsedProxyList.AddRange(await ParseSite_CybersyndromeDotNet());
                    break;
                case MethodName.ParseSite_GithubDotComHookzofSocks5_list:
                    currentParsedProxyList.AddRange(await ParseSite_GithubDotComHookzofSocks5_list());
                    break;
                case MethodName.ParseSite_GithubDotComSunny9577Proxy_scraper:
                    currentParsedProxyList.AddRange(await ParseSite_GithubDotComSunny9577Proxy_scraper());
                    break;
                case MethodName.ParseSite_GithubDotComRoosterkidOpenproxylist:
                    currentParsedProxyList.AddRange(await ParseSite_GithubDotComRoosterkidOpenproxylist());
                    break;
                case MethodName.ParseSite_GithubDotComMuRongPIGProxy_Master:
                    currentParsedProxyList.AddRange(await ParseSite_GithubDotComMuRongPIGProxy_Master());
                    break;
                case MethodName.ParseSite_GithubDotComPrxchkProxy_list:
                    currentParsedProxyList.AddRange(await ParseSite_GithubDotComPrxchkProxy_list());
                    break;
                case MethodName.ParseSite_GithubDotComErcinDedeogluProxies:
                    currentParsedProxyList.AddRange(await ParseSite_GithubDotComErcinDedeogluProxies());
                    break;
                case MethodName.ParseSite_GithubDotComOfficialputuidKangProxy:
                    currentParsedProxyList.AddRange(await ParseSite_GithubDotComOfficialputuidKangProxy());
                    break;
                case MethodName.ParseSite_GithubDotComZloi_userHideipDotMe:
                    currentParsedProxyList.AddRange(await ParseSite_GithubDotComZloi_userHideipDotMe());
                    break;
                case MethodName.ParseSite_GithubDotComProxiflyFree_proxy_list:
                    currentParsedProxyList.AddRange(await ParseSite_GithubDotComProxiflyFree_proxy_list());
                    break;
                case MethodName.ParseSite_GithubDotComObcbOGetproxy:
                    currentParsedProxyList.AddRange(await ParseSite_GithubDotComObcbOGetproxy());
                    break;
                case MethodName.ParseSite_GithubDotComAnonym0usWork1221Free_Proxies:
                    currentParsedProxyList.AddRange(await ParseSite_GithubDotComAnonym0usWork1221Free_Proxies());
                    break;
                case MethodName.ParseSite_GithubDotComCasals_arProxy_list:
                    currentParsedProxyList.AddRange(await ParseSite_GithubDotComCasals_arProxy_list());
                    break;
                case MethodName.ParseSite_GithubDotComProxy4parsingProxy_list:
                    currentParsedProxyList.AddRange(await ParseSite_GithubDotComProxy4parsingProxy_list());
                    break;
                case MethodName.ParseSite_GithubDotComMmpx12Proxy_list:
                    currentParsedProxyList.AddRange(await ParseSite_GithubDotComMmpx12Proxy_list());
                    break;
                default:
                    throw new Exception("MethodName not found");
            }

            return currentParsedProxyList;
        }
        private async Task<List<ParsedProxy>> ParseSite_SpysDotOne(SpysDotOne_xpp? xpp = null, SpysDotOne_tldc? tldc = null, SpysDotOne_xf1? xf1 = null, SpysDotOne_xf2? xf2 = null, SpysDotOne_xf4? xf4 = null, SpysDotOne_xf5? xf5 = null)
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                FormUrlEncodedContent? formContent = null;
                IDocument? htmlPageDocument = null;

                string? xx0_key_value = null;

                #region request to get html page with xx0_key_value
                endpoint = new Uri($"https://spys.one/proxies/");

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://spys.one/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var el = htmlPageDocument.QuerySelector("input[name=\"xx0\"]");

                if (el == null)
                {
                    throw new Exception($"el == null\n{content}");
                }

                xx0_key_value = el!.GetAttribute("value");

                if (string.IsNullOrEmpty(xx0_key_value))
                {
                    throw new Exception($"string.IsNullOrEmpty(xx0_key_value)\n{content}");
                }

                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5));
                #endregion

                #region request to get html page with proxy
                Dictionary<string, string> keyValuePairsForContent = new Dictionary<string, string>()
                {
                    { "xx0", xx0_key_value },
                    { "xpp", (xpp is null ? (int)SpysDotOne_xpp._500 : (int)xpp.Value).ToString() },
                    { "tldc", (tldc is null ? (int)SpysDotOne_tldc.ALL_179_стран : (int)tldc.Value).ToString() },
                    { "xf1", (xf1 is null ? (int)SpysDotOne_xf1.ALL : (int)xf1.Value).ToString() },
                    { "xf2", (xf2 is null ? (int)SpysDotOne_xf2.ALL : (int)xf2.Value).ToString() },
                    { "xf4", (xf4 is null ? (int)SpysDotOne_xf4.ALL : (int)xf4.Value).ToString() },
                    { "xf5", (xf5 is null ? (int)SpysDotOne_xf5.ALL : (int)xf5.Value).ToString() },
                };

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = endpoint,
                    Content = (formContent = new FormUrlEncodedContent(keyValuePairsForContent))
                };

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://spys.one/proxies/");
                request.Headers.Add("origin", @"https://spys.one");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));
                #endregion

                #region parse proxy from html page
                string? obfuscatedProxyPortFunction = null;
                foreach (var script in htmlPageDocument.Scripts)
                {
                    if (script.Text.Contains("function(p,r,o,x,y,s)"))
                    {
                        obfuscatedProxyPortFunction = script.Text;
                        break;
                    }
                }
                if (obfuscatedProxyPortFunction == null)
                {
                    throw new Exception("");
                }
                base.jsExecuteContextEngine!.Execute(obfuscatedProxyPortFunction);

                var elements_in_body = htmlPageDocument.QuerySelectorAll("body > *");
                var trs = elements_in_body[2].QuerySelectorAll("tbody > tr");
                var proxies = trs[2].QuerySelectorAll("td > table > tbody > tr");

                Regex regexAddress = new Regex(REIpPattern);
                Regex regexPort = new Regex(REPortPattern);

                foreach (var proxy in proxies)
                {
                    try
                    {
                        if (proxy.Attributes.Where(attribute => attribute.Name == "onmouseover").ToArray().Length == 0)
                        {
                            continue;
                        }

                        var cellOne = proxy.QuerySelectorAll("td")[0];
                        string cellOneTextContent = cellOne.QuerySelector("font[class='spy14']")!.TextContent;

                        // get address
                        MatchCollection matchesAddress = regexAddress.Matches(cellOneTextContent);
                        if (matchesAddress.Count != 1)
                        {
                            throw new Exception("");
                        }
                        string address = matchesAddress[0].Value;

                        // get port
                        //MatchCollection matchesPort = regexPort.Matches(cellOneTextContent);
                        //string portValuesArrayAsString = "";
                        //if (matchesPort.Count <= 1)
                        //{
                        //    throw new Exception("");
                        //}
                        //portValuesArrayAsString = "[" + string.Join(',', matchesPort.Select(match => match.Value).ToArray()) + "]";
                        //string port = jsExecuteContextEngine!.Evaluate($"union({portValuesArrayAsString})").AsString();

                        List<string> port_parts = cellOneTextContent.Split('+').ToList();
                        port_parts.RemoveAt(0);
                        string port_parts_together = "(";
                        for (int i = 0; i < port_parts.Count; i++)
                        {
                            port_parts_together += port_parts[i];

                            if (i != port_parts.Count - 1)
                            {
                                port_parts_together += "+''+";
                            }
                        }
                        string port = jsExecuteContextEngine!.Evaluate($"{port_parts_together}").ToString();
                        MatchCollection matchesPort = regexPort.Matches(port);
                        if (matchesPort.Count < 1)
                        {
                            throw new Exception("matchesPort.Count <= 1");
                        }

                        parsedProxy.Add(new ParsedProxy(address, port));
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"foreach (var proxy in proxies)\n{proxy?.InnerHtml}");
                    }
                }

                if (parsedProxy.Count == 0)
                {
                    throw new Exception($"parsedProxy.Count == 0\n{content}");
                }
                #endregion

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                formContent = null;
                htmlPageDocument = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_SpysDotOne(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GeonodeDotCom(GeonodeDotCom_limit? limit = null, GeonodeDotCom_page? page = null, GeonodeDotCom_sort_by? sort_By = null, GeonodeDotCom_sort_type? sort_Type = null)
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                Dictionary<string, string> queryParams = new Dictionary<string, string>()
                {
                    ["limit"] = (limit is null ? (int)GeonodeDotCom_limit._500 : (int)limit.Value).ToString(),
                    ["page"] = (page is null ? (int)GeonodeDotCom_page._1 : (int)page.Value).ToString(),
                    ["sort_by"] = (sort_By is null ? GeonodeDotCom_sort_by.lastChecked : sort_By.Value).ToString(),
                    ["sort_type"] = (sort_Type is null ? GeonodeDotCom_sort_type.desc : sort_Type.Value).ToString()
                };

                string url = QueryHelpers.AddQueryString($"https://proxylist.geonode.com/api/proxy-list", queryParams);
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://geonode.com/");
                request.Headers.Add("origin", @"https://geonode.com");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                dynamic? proxies = JsonConvert.DeserializeObject(content);

                if (proxies == null)
                {
                    throw new Exception($"proxies == null\n{content}");
                }

                foreach (var proxy in proxies.data)
                {
                    //string ip = proxy.i;
                    //string port = proxy.p;

                    string ip = proxy.ip;
                    string port = proxy.port;

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                if (parsedProxy.Count == 0)
                {
                    throw new Exception($"parsedProxy.Count == 0\n{content}");
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                proxies = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_GeonodeDotCom(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_ProxyscrapeDotCom(ProxyscrapeDotCom_protocol? protocol = null)
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                Dictionary<string, string> queryParams = new Dictionary<string, string>()
                {
                    ["request"] = "getproxies",
                    ["protocol"] = (protocol is null ? ((ProxyscrapeDotCom_protocol)new Random().Next(0, 3)) : protocol.Value).ToString()
                };

                string url = QueryHelpers.AddQueryString($"https://api.proxyscrape.com/v2/", queryParams);
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://proxyscrape.com/free-proxy-list/");
                request.Headers.Add("origin", @"https://proxyscrape.com");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                var proxies = content.Split(Environment.NewLine);

                Regex regexAddress = new Regex(REIpPattern);
                Regex regexPort = new Regex(REPortPattern);

                foreach (var proxy in proxies)
                {
                    if (!proxy.Contains(":"))
                    {
                        continue;
                    }

                    var data = proxy.Split(':');

                    MatchCollection matchesAddress = regexAddress.Matches(data[0]);
                    if (matchesAddress.Count != 1)
                    {
                        throw new Exception($"matchesAddress.Count != 1\n{content}");
                    }
                    string address = matchesAddress[0].Value;

                    MatchCollection matchesPort = regexPort.Matches(data[1]);
                    if (matchesPort.Count < 1)
                    {
                        throw new Exception($"matchesPort.Count < 1\n{content}");
                    }
                    string port = matchesPort[0].Value;

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                if (parsedProxy.Count == 0 && queryParams.Where(p => p.Key == "protocol").FirstOrDefault().Value != "2")
                {
                    throw new Exception($"parsedProxy.Count == 0\n{content}");
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                proxies = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_ProxyscrapeDotCom(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_FreeproxyDotWorld(FreeproxyDotWorld_type? type = null, FreeproxyDotWorld_anonymity? anonymity = null, FreeproxyDotWorld_page? page = null)
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                Dictionary<string, string> queryParams = new Dictionary<string, string>()
                {
                    ["type"] = type is null ? String.Empty : type.Value.ToString()!,
                    ["anonymity"] = anonymity is null ? "0" : ((int)anonymity.Value).ToString(),
                    ["country"] = "",
                    ["speed"] = "",
                    ["port"] = "",
                    ["page"] = (page is null ? (int)FreeproxyDotWorld_page._1 : ((int)page.Value)).ToString(),
                };

                string url = QueryHelpers.AddQueryString(@"https://www.freeproxy.world/", queryParams);
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://www.freeproxy.world/");
                request.Headers.Add("origin", @"https://www.freeproxy.world");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var proxy_row_trs = htmlPageDocument.QuerySelectorAll("table.layui-table > tbody > tr");

                Regex regexAddress = new Regex(REIpPattern);
                Regex regexPort = new Regex(REPortPattern);

                foreach (var proxy in proxy_row_trs)
                {
                    var countAdressCellsByTagValue = proxy.QuerySelectorAll("td[class='show-ip-div']").Count();
                    if (countAdressCellsByTagValue == 0)
                    {
                        continue;
                    }

                    var cellAddressTextContent = proxy.QuerySelectorAll("td")[0].TextContent;
                    var cellPortTextContent = proxy.QuerySelectorAll("td")[1].GetElementsByTagName("a")[0].TextContent;

                    MatchCollection matchesAddress = regexAddress.Matches(cellAddressTextContent);
                    if (matchesAddress.Count != 1)
                    {
                        throw new Exception($"matchesAddress.Count != 1\n{content}");
                    }
                    string address = matchesAddress[0].Value;

                    MatchCollection matchesPort = regexPort.Matches(cellPortTextContent);
                    if (matchesPort.Count != 1)
                    {
                        throw new Exception($"matchesPort.Count != 1\n{content}");
                    }
                    string port = matchesPort[0].Value;

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                if (parsedProxy.Count == 0)
                {
                    throw new Exception($"parsedProxy.Count == 0\n{content}");
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                proxy_row_trs = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_FreeproxyDotWorld(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_OpenproxyDotSpace(OpenproxyDotSpace_protocol? protocol = null)
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                string url = @$"https://openproxy.space/list/{(protocol is null ? ((OpenproxyDotSpace_protocol)new Random().Next(0, 3)).ToString() : protocol.ToString())}/";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://openproxy.space/list/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                string? scriptDataText = null;
                foreach (var script in htmlPageDocument.Scripts)
                {
                    if (script.Text.Contains("window.__NUXT__"))
                    {
                        scriptDataText = script.Text;
                        break;
                    }
                }
                if (scriptDataText == null)
                {
                    throw new Exception($"scriptDataText == null\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);
                MatchCollection? matchesProxy = regexProxy.Matches(scriptDataText);
                if (matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy.Count == 0\n{content}");
                }

                foreach (Match match in matchesProxy)
                {
                    if (!match.Value.Contains(':'))
                    {
                        continue;
                    }

                    string[] data = match.Value.Split(':');
                    string address = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_OpenproxyDotSpace(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_AdvancedDotName(AdvancedDotName_type? type = null)
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                #region request to get html page with proxy
                // set query parameter
                string url = "https://advanced.name/freeproxy/";
                url += type is null || type is AdvancedDotName_type.all ? "" : $"?type={type}";

                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://advanced.name/freeproxy/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5));
                #endregion

                #region request to get encode js function
                endpoint = new Uri("https://advanced.name/build/js/guest-scripts-cfb0961e.js");

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://advanced.name/freeproxy/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                string jsEncodeAndDecodeFunc = content;
                string keyElementForIndexOne = "t(function()";
                string keyElementForIndexTwo = "jQuery(function(t){";
                int index_to_remove_one = jsEncodeAndDecodeFunc.LastIndexOf(keyElementForIndexOne);
                jsEncodeAndDecodeFunc = jsEncodeAndDecodeFunc.Remove(index_to_remove_one);
                int index_to_remove_two = jsEncodeAndDecodeFunc.LastIndexOf(keyElementForIndexTwo);
                jsEncodeAndDecodeFunc = jsEncodeAndDecodeFunc.Remove(index_to_remove_two, keyElementForIndexTwo.Length);

                base.jsExecuteContextEngine!.Execute(jsEncodeAndDecodeFunc);
                #endregion

                #region parse and decode proxy from html page
                var proxy_row_trs = htmlPageDocument.QuerySelectorAll("table[id='table_proxies'] > tbody > tr");

                foreach (var proxy in proxy_row_trs)
                {
                    var countAdressCellsByTagName = proxy.QuerySelectorAll("td[data-ip]").Count();
                    if (countAdressCellsByTagName == 0)
                    {
                        continue;
                    }

                    string cellTextContentAddress = proxy.QuerySelector("td[data-ip]")!.GetAttribute("data-ip")!;
                    string cellTextContentPort = proxy.QuerySelector("td[data-port]")!.GetAttribute("data-port")!;

                    string address = base.jsExecuteContextEngine!.Evaluate($"e.decode(\"{cellTextContentAddress}\")").AsString();
                    string port = base.jsExecuteContextEngine!.Evaluate($"e.decode(\"{cellTextContentPort}\")").AsString();

                    // или можно использовать представление на с# этой же функции ниже
                    /*string address = AdvancedDotNameDecodeBase64(cellTextContentAddress);
                    string port = AdvancedDotNameDecodeBase64(cellTextContentPort);
                    string AdvancedDotNameDecodeBase64(string t)
                    {
                        string keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
                        string d = "";
                        int s = 0;
                        t = t.Replace("[^A-Za-z0-9\\+\\/\\=]", "");
                        while (s < t.Length)
                        {
                            int n = keyStr.IndexOf(t[s++]);
                            int i = keyStr.IndexOf(t[s++]);
                            int c = keyStr.IndexOf(t[s++]);
                            int h = keyStr.IndexOf(t[s++]);
                            int r = n << 2 | i >> 4;
                            int o = (15 & i) << 4 | c >> 2;
                            int a = (3 & c) << 6 | h;
                            d += Convert.ToChar(r);
                            if (c != 64)
                            {
                                d += Convert.ToChar(o);
                            }
                            if (h != 64)
                            {
                                d += Convert.ToChar(a);
                            }
                        }
                        return Encoding.UTF8.GetString(Array.ConvertAll(Regex.Unescape(d).ToCharArray(), c => (byte)c));
                    }*/

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                if (parsedProxy.Count == 0)
                {
                    throw new Exception($"parsedProxy.Count == 0\n{content}");
                }
                #endregion

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                proxy_row_trs = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_AdvancedDotName(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_ProxypremiumDotTop()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            // remove after !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // нельзя использовать пока не пофикшу ошибку Failed to find library "libleptonica-1.82.0.so" for platform x64.
            if (base.tesseractExecuteContextEngine == null)
            {
                return parsedProxy;
            }

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                FormUrlEncodedContent? formContent = null;
                IDocument? htmlPageDocument = null;

                string captcha_recognized_code_value = "";

                #region request to get captcha img
                endpoint = new Uri(@"https://proxypremium.top/captcha.php");

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://proxypremium.top/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\nrequest to get captcha img");
                }

                byte[] captcha_img = await responce.Content.ReadAsByteArrayAsync();

                // resolve captcha
                base.tesseractExecuteContextEngine!.SetVariable("tessedit_char_whitelist", "0123456789");
                using (var img = Pix.LoadFromMemory(captcha_img))
                {
                    using (var page = base.tesseractExecuteContextEngine!.Process(img))
                    {
                        var text = page.GetText();
                        captcha_recognized_code_value = text.TrimEnd();
                    }
                }
                if (string.IsNullOrEmpty(captcha_recognized_code_value))
                {
                    throw new Exception("string.IsNullOrEmpty(captcha_recognized_code_value) == true");
                }
                #endregion

                #region request to get html page with proxy
                endpoint = new Uri(@"https://proxypremium.top/search-proxy");

                Dictionary<string, string> keyValuePairsForContent = new Dictionary<string, string>()
                {
                    { "tldc", "ALL - All Countries" },
                    { "anon", "all" },
                    { "prox", "all" },
                    { "eport", "" },
                    { "code", captcha_recognized_code_value },
                    { "submit", "Search" }
                };

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = endpoint,
                    Content = (formContent = new FormUrlEncodedContent(keyValuePairsForContent))
                };

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://proxypremium.top/");
                request.Headers.Add("origin", @"https://proxypremium.top");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));
                #endregion

                #region parse proxy from html page
                var trs = htmlPageDocument.QuerySelectorAll("body > center > div > table > tbody > tr");

                Regex regexProxy = new Regex(REProxyAddressPattern);

                foreach (var proxy in trs)
                {
                    if (proxy.Attributes.Where(attribute => attribute.Name == "onmouseover").ToArray().Length == 0)
                    {
                        continue;
                    }
                    if (proxy.QuerySelectorAll("td").Count() == 0)
                    {
                        continue;
                    }

                    var cellOne = proxy.QuerySelectorAll("td")[0];

                    string cellOneProxyTextContent = cellOne.QuerySelector("font[class='pp14']")!.TextContent;

                    MatchCollection matchesProxy = regexProxy.Matches(cellOneProxyTextContent);
                    if (matchesProxy.Count != 1)
                    {
                        throw new Exception($"matchesProxy.Count != 1, Count == {matchesProxy.Count}, cellOneProxyTextContent == {cellOneProxyTextContent}");
                    }
                    string _proxy = matchesProxy[0].Value;

                    if (!_proxy.Contains(':'))
                    {
                        throw new Exception($"!_proxy.Contains(':'), cellOneProxyTextContent == {cellOneProxyTextContent}");
                    }

                    string[] data = _proxy.Split(':');
                    string address = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(address, port));
                }
                #endregion

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                formContent = null;
                htmlPageDocument = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_ProxypremiumDotTop(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_FineproxyDotOrg()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                string first_html_page = "";

                #region request to get 'antibot' cookie
                string url = $"https://fineproxy.org/wp-content/themes/fineproxyorg/proxy-list.php";

                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://fineproxy.org/free-proxy/");
                request.Headers.Add("origin", @"https://fineproxy.org");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();
                first_html_page = content;

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }
                #endregion

                if (content.Contains("Loading..."))
                {
                    #region manipulations to bypass site security

                    string param_h1 = "";
                    string param_date = "";
                    string param_hdc = "";
                    string param_a = "";
                    string param_country = "";
                    string param_ip = "";
                    string param_v = "";
                    string param_cid = "";
                    string param_ptr = "";
                    string param_w = "";
                    string param_h = "";
                    string param_cw = "";
                    string param_ch = "";
                    string param_co = "";
                    string param_pi = "";
                    string param_ref = "";
                    string param_accept = "";
                    string param_tz = "";
                    string param_ipdbc = "";
                    string param_ipv4 = "";
                    string param_rct = "";
                    string param_cookieoff = "";
                    string param_xdf85f705340b16065bde9c5f01ebacbf = "";
                    string param_xxx = "";
                    string param_rowid = "";
                    string param_gray = "";

                    #region collect all form content parts parameters

                    #region get param_h1
                    try
                    {
                        param_h1 = ParseElementFromText(content, "h1=", "&date=");
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_h1': {e.Message}");
                    }
                    #endregion

                    #region get param_date
                    try
                    {
                        param_date = ParseElementFromText(content, "date=", "&hdc=");
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_date': {e.Message}");
                    }
                    #endregion

                    #region get param_hdc
                    try
                    {
                        param_hdc = ParseElementFromText(content, "hdc=", "&a=");
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_hdc': {e.Message}");
                    }
                    #endregion

                    #region get param_a
                    try
                    {
                        // AdBlock
                        param_a = "0";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_a': {e.Message}");
                    }
                    #endregion

                    #region get param_country
                    try
                    {
                        param_country = ParseElementFromText(content, "country=", "&ip=");
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_country': {e.Message}");
                    }
                    #endregion

                    #region get param_ip
                    try
                    {
                        param_ip = ParseElementFromText(content, "ip=", "&v=");
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_ip': {e.Message}");
                    }
                    #endregion

                    #region get param_v
                    try
                    {
                        param_v = ParseElementFromText(content, "v=", "&cid=");
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_v': {e.Message}");
                    }
                    #endregion

                    #region get param_cid
                    try
                    {
                        param_cid = ParseElementFromText(content, "cid=", "&ptr=");
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_cid': {e.Message}");
                    }
                    #endregion

                    #region get param_ptr
                    try
                    {
                        param_ptr = ParseElementFromText(content, "ptr=", "&w=");
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_ptr': {e.Message}");
                    }
                    #endregion

                    #region get param_w
                    try
                    {
                        param_w = "1920";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_w': {e.Message}");
                    }
                    #endregion

                    #region get param_h
                    try
                    {
                        param_h = "1080";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_h': {e.Message}");
                    }
                    #endregion

                    #region get param_cw
                    try
                    {
                        param_cw = "1920";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_cw': {e.Message}");
                    }
                    #endregion

                    #region get param_ch
                    try
                    {
                        param_ch = "1080";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_ch': {e.Message}");
                    }
                    #endregion

                    #region get param_co
                    try
                    {
                        param_co = "24";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_co': {e.Message}");
                    }
                    #endregion

                    #region get param_pi
                    try
                    {
                        param_pi = "24";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_pi': {e.Message}");
                    }
                    #endregion

                    #region get param_ref
                    try
                    {
                        param_ref = "";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_ref': {e.Message}");
                    }
                    #endregion

                    #region get param_accept
                    try
                    {
                        param_accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_accept': {e.Message}");
                    }
                    #endregion

                    #region get param_tz
                    try
                    {
                        param_tz = "Europe/Moscow";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_tz': {e.Message}");
                    }
                    #endregion

                    #region get param_ipdbc
                    try
                    {
                        param_ipdbc = "";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_ipdbc': {e.Message}");
                    }
                    #endregion

                    #region get param_ipv4
                    try
                    {
                        param_ipv4 = "";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_ipv4': {e.Message}");
                    }
                    #endregion

                    #region get param_rct
                    try
                    {
                        param_rct = "";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_rct': {e.Message}");
                    }
                    #endregion

                    #region get param_cookieoff
                    try
                    {
                        param_cookieoff = "0";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_cookieoff': {e.Message}");
                    }
                    #endregion

                    #region get param_xdf85f705340b16065bde9c5f01ebacbf
                    try
                    {
                        param_xdf85f705340b16065bde9c5f01ebacbf = "ab";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_xdf85f705340b16065bde9c5f01ebacbf': {e.Message}");
                    }
                    #endregion

                    #region get param_xxx
                    try
                    {
                        param_xxx = "";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_xxx': {e.Message}");
                    }
                    #endregion

                    #region get param_rowid
                    try
                    {
                        param_rowid = "0";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_rowid': {e.Message}");
                    }
                    #endregion

                    #region get param_gray
                    try
                    {
                        param_gray = "1";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get param_gray': {e.Message}");
                    }
                    #endregion

                    #endregion

                    #region first post request
                    try
                    {
                        Dictionary<string, string> paramContent = new Dictionary<string, string>()
                        {
                            { "h1", param_h1 },
                            { "date", param_date },
                            { "hdc", param_hdc },
                            { "a", param_a },
                            { "country", param_country },
                            { "ip", param_ip },
                            { "v", param_v },
                            { "cid", param_cid },
                            { "ptr", param_ptr },
                            { "w", param_w },
                            { "h", param_h },
                            { "cw", param_cw },
                            { "ch", param_ch },
                            { "co", param_co },
                            { "pi", param_pi },
                            { "ref", param_ref },
                            { "accept", param_accept },
                            { "tz", param_tz },
                            { "ipdbc", param_ipdbc },
                            { "ipv4", param_ipv4 },
                            { "rct", param_rct },
                            { "cookieoff", param_cookieoff },
                            { "xdf85f705340b16065bde9c5f01ebacbf", param_xdf85f705340b16065bde9c5f01ebacbf },
                            { "xxx", param_xxx },
                            { "rowid", param_rowid },
                            { "gray", param_gray }
                        };
                        FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(paramContent);

                        request = new HttpRequestMessage()
                        {
                            Method = HttpMethod.Post,
                            RequestUri = endpoint,
                            Content = formUrlEncodedContent
                        };

                        foreach (var header in base.contextHeaders!)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }
                        request.Headers.Add("referer", @"https://fineproxy.org/wp-content/themes/fineproxyorg/proxy-list.php");
                        request.Headers.Add("origin", @"https://fineproxy.org");
                        //request.Headers.Add("host", @"best-proxies.ru");
                        //request.Headers.Add("connection", "keep-alive");

                        responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                        content = await responce.Content.ReadAsStringAsync();
                    }
                    catch(Exception e)
                    {
                        throw new Exception($"Error in '#region first post request': {e.Message}");
                    }
                    #endregion

                    if (content.Contains("error"))
                    {
                        #region update param_xdf85f705340b16065bde9c5f01ebacbf
                        try
                        {
                            param_xdf85f705340b16065bde9c5f01ebacbf = "post";
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Error in '#region update param_xdf85f705340b16065bde9c5f01ebacbf': {e.Message}");
                        }
                        #endregion

                        #region update param_xxx
                        try
                        {
                            string encoded_param_xxx_content = ParseElementFromText(first_html_page, "b64_to_utf8(\"", "\");\n}");
                            string decoded_param_xxx_content = Base64Decode(encoded_param_xxx_content);

                            // get button styles and button class id
                            string styleContent = ParseElementFromText(decoded_param_xxx_content, "<style>", "</style>");
                            string display_button_class_id = styleContent.Split('.').Where(s => !String.IsNullOrEmpty(s) && !s.Contains("display") && !s.Contains("none")).First().Split(' ')[0];

                            // get current display button id
                            string display_button = decoded_param_xxx_content.Split("div").Where(b => b.Contains("onclick") && b.Contains(display_button_class_id)).First();

                            param_xxx = ParseElementFromText(display_button, "data, '", "')\">");
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Error in '#region update param_xxx': {e.Message}");
                        }
                        #endregion

                        #region second post request
                        try
                        {
                            Dictionary<string, string> paramContent = new Dictionary<string, string>()
                            {
                                { "h1", param_h1 },
                                { "date", param_date },
                                { "hdc", param_hdc },
                                { "a", param_a },
                                { "country", param_country },
                                { "ip", param_ip },
                                { "v", param_v },
                                { "cid", param_cid },
                                { "ptr", param_ptr },
                                { "w", param_w },
                                { "h", param_h },
                                { "cw", param_cw },
                                { "ch", param_ch },
                                { "co", param_co },
                                { "pi", param_pi },
                                { "ref", param_ref },
                                { "accept", param_accept },
                                { "tz", param_tz },
                                { "ipdbc", param_ipdbc },
                                { "ipv4", param_ipv4 },
                                { "rct", param_rct },
                                { "cookieoff", param_cookieoff },
                                { "xdf85f705340b16065bde9c5f01ebacbf", param_xdf85f705340b16065bde9c5f01ebacbf },
                                { "xxx", param_xxx },
                                { "rowid", param_rowid },
                                { "gray", param_gray }
                            };
                            FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(paramContent);

                            request = new HttpRequestMessage()
                            {
                                Method = HttpMethod.Post,
                                RequestUri = endpoint,
                                Content = formUrlEncodedContent
                            };

                            foreach (var header in base.contextHeaders!)
                            {
                                request.Headers.Add(header.Key, header.Value);
                            }
                            request.Headers.Add("referer", @"https://fineproxy.org/wp-content/themes/fineproxyorg/proxy-list.php");
                            request.Headers.Add("origin", @"https://fineproxy.org");
                            //request.Headers.Add("host", @"best-proxies.ru");
                            //request.Headers.Add("connection", "keep-alive");

                            responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                            content = await responce.Content.ReadAsStringAsync();
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Error in '#region first post request': {e.Message}");
                        }
                        #endregion

                        #region create and add main cookie
                        Cookie mainCookie = new Cookie();
                        try
                        {
                            string data_key = JsonConvert.DeserializeObject<Dictionary<string, string>>(content)!.Where(el => el.Key == "cookie").First().Value;
                            string mainCookieName = ProxyParseCookieContainer.GetAllCookies().Where(k => k.Name == "antibot").First().Value;
                            TimeSpan add_time = TimeSpan.FromMilliseconds(7 * 24 * 60 * 60 * 1000);
                            DateTime expires = DateTime.UtcNow + add_time;

                            mainCookie.Name = mainCookieName;
                            mainCookie.Value = $"{data_key}-{param_date}";
                            mainCookie.Expires = Convert.ToDateTime(expires);
                            mainCookie.Path = "/";
                            mainCookie.Domain = ".fineproxy.org";

                            ProxyParseCookieContainer.Add(mainCookie);
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Error in '#region create and add main cookie': {e.Message}");
                        }
                        #endregion

                        #region last get request to get proxy
                        try
                        {
                            request = new HttpRequestMessage()
                            {
                                Method = HttpMethod.Get,
                                RequestUri = endpoint,
                            };
                            request.Headers.Host = endpoint.Host;

                            foreach (var header in base.contextHeaders!)
                            {
                                request.Headers.Add(header.Key, header.Value);
                            }
                            request.Headers.Add("referer", @"https://fineproxy.org/wp-content/themes/fineproxyorg/proxy-list.php");

                            responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                            content = await responce.Content.ReadAsStringAsync();
                        }
                        catch(Exception e)
                        {
                            throw new Exception($"Error in '#region last get request to get proxy': {e.Message}");
                        }
                        #endregion
                    }

                    #endregion
                }

                #region parse proxy from json
                dynamic? proxies = JsonConvert.DeserializeObject(content);

                if (proxies == null)
                {
                    throw new Exception($"proxies == null\n{content}");
                }

                foreach (var proxy in proxies)
                {
                    string ip = proxy.ip;
                    string port = proxy.port;

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                if (parsedProxy.Count == 0)
                {
                    throw new Exception($"parsedProxy.Count == 0\n{content}");
                }
                #endregion

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                proxies = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_FineproxyDotOrg(), error {e.Message}");
            }

            return parsedProxy;

            string ParseElementFromText(string textContent, string startIndexElement, string endIndexElement)
            {
                string result = "";

                int index_to_remove_one = textContent.LastIndexOf(startIndexElement);
                textContent = textContent.Remove(0, index_to_remove_one + startIndexElement.Length);

                int index_to_remove_two = textContent.IndexOf(endIndexElement);
                textContent = textContent.Remove(index_to_remove_two);

                return result = textContent;
            }
            string Base64Decode(string base64EncodedData)
            {
                var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
                return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
        }
        private async Task<List<ParsedProxy>> ParseSite_Free_proxy_listDotNet()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                string url = @"https://free-proxy-list.net/";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://google.com/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var proxyСontainer = htmlPageDocument.QuerySelector("div.modal-body > textarea.form-control");

                if (proxyСontainer == null)
                {
                    throw new Exception($"proxyСontainer == null\n{content}");
                }

                string proxyСontainerTextContent = proxyСontainer.TextContent;

                Regex regexProxy = new Regex(REProxyAddressPattern);
                MatchCollection? matchesProxy = regexProxy.Matches(proxyСontainerTextContent);
                if (matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy.Count == 0\n{content}");
                }

                foreach (Match match in matchesProxy)
                {
                    if (!match.Value.Contains(':'))
                    {
                        continue;
                    }

                    string[] data = match.Value.Split(':');
                    string address = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_Free_proxy_listDotNet(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_SslproxiesDotOrg()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                string url = @"https://www.sslproxies.org/";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://google.com/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var proxyСontainer = htmlPageDocument.QuerySelector("div.modal-body > textarea.form-control");

                if (proxyСontainer == null)
                {
                    throw new Exception($"proxyСontainer == null\n{content}");
                }

                string proxyСontainerTextContent = proxyСontainer.TextContent;

                Regex regexProxy = new Regex(REProxyAddressPattern);
                MatchCollection? matchesProxy = regexProxy.Matches(proxyСontainerTextContent);
                if (matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy.Count == 0\n{content}");
                }

                foreach (Match match in matchesProxy)
                {
                    if (!match.Value.Contains(':'))
                    {
                        continue;
                    }

                    string[] data = match.Value.Split(':');
                    string address = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_SslproxiesDotOrg(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_Hide_my_ipDotcom()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                //string url = @"https://www.hide-my-ip.com/proxylist.shtml";
                //string url = @"https://www.hidemyip.com/proxylist";
                string url = @"https://www.hidemyip.com/proxylist-data/";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                //request.Headers.Add("referer", @"https://www.hide-my-ip.com/");
                request.Headers.Add("referer", @"https://www.hidemyip.com/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }


                #region Старая реализация парсинга, сайт изменился
                //htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                //string? scriptDataText = null;
                //foreach (var script in htmlPageDocument.Scripts)
                //{
                //    if (script.Text.Contains("var json ="))
                //    {
                //        scriptDataText = script.Text;
                //        break;
                //    }
                //}
                //if (scriptDataText == null)
                //{
                //    throw new Exception($"scriptDataText == null\n{content}");
                //}

                //string keyElementForIndexOne = "var json =";
                //string keyElementForIndexTwo = "}];";
                //int index_to_remove_one = scriptDataText.IndexOf(keyElementForIndexOne, 0);
                //scriptDataText = scriptDataText.Remove(index_to_remove_one, keyElementForIndexOne.Length).TrimStart();
                //int index_to_remove_two = scriptDataText.LastIndexOf(keyElementForIndexTwo);
                //scriptDataText = scriptDataText.Remove(index_to_remove_two + 2).TrimEnd();

                //dynamic? proxies = JsonConvert.DeserializeObject(scriptDataText); 
                #endregion


                dynamic? proxies = JsonConvert.DeserializeObject(content);

                if (proxies == null)
                {
                    throw new Exception($"proxies == null\n{content}");
                }

                foreach (var proxy in proxies)
                {
                    string ip = proxy.i;
                    string port = proxy.p;

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                if (parsedProxy.Count == 0)
                {
                    throw new Exception($"parsedProxy.Count == 0\n{content}");
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                proxies = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_Hide_my_ipDotcom(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GhostealthDotCom()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                string url = @"https://ghostealth.com/api/v1.0/dev/tools/proxy-scraper/proxies/limited";

                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://ghostealth.com/proxy-scraper");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                dynamic? proxies = JsonConvert.DeserializeObject(content);

                if (proxies == null)
                {
                    throw new Exception($"proxies == null\n{content}");
                }

                foreach (var proxy in proxies.result)
                {
                    string _proxy = proxy.ip;

                    if (!_proxy.Contains(':'))
                    {
                        continue;
                    }

                    string[] data = _proxy.Split(':');
                    string address = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                if (parsedProxy.Count == 0 && proxies.status != "succeeded")
                {
                    throw new Exception($"parsedProxy.Count == 0\n{content}");
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                proxies = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_GhostealthDotCom(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_Us_proxyDotOrg()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                string url = @"https://www.us-proxy.org/";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://google.com/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var proxyСontainer = htmlPageDocument.QuerySelector("div.modal-body > textarea.form-control");

                if (proxyСontainer == null)
                {
                    throw new Exception($"proxyСontainer == null\n{content}");
                }

                string proxyСontainerTextContent = proxyСontainer.TextContent;

                Regex regexProxy = new Regex(REProxyAddressPattern);
                MatchCollection? matchesProxy = regexProxy.Matches(proxyСontainerTextContent);
                if (matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy.Count == 0\n{content}");
                }

                foreach (Match match in matchesProxy)
                {
                    if (!match.Value.Contains(':'))
                    {
                        continue;
                    }

                    string[] data = match.Value.Split(':');
                    string address = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_Us_proxyDotOrg(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_My_proxyDotCom()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                Random random = new Random();

                string url = url = random.Next(1, 13) switch
                {
                    1 => @"https://www.my-proxy.com/free-proxy-list.html",
                    2 => @"https://www.my-proxy.com/free-proxy-list-2.html",
                    3 => @"https://www.my-proxy.com/free-proxy-list-3.html",
                    4 => @"https://www.my-proxy.com/free-proxy-list-4.html",
                    5 => @"https://www.my-proxy.com/free-proxy-list-5.html",
                    6 => @"https://www.my-proxy.com/free-proxy-list-6.html",
                    7 => @"https://www.my-proxy.com/free-proxy-list-7.html",
                    8 => @"https://www.my-proxy.com/free-proxy-list-8.html",
                    9 => @"https://www.my-proxy.com/free-proxy-list-9.html",
                    10 => @"https://www.my-proxy.com/free-proxy-list-10.html",
                    11 => @"https://www.my-proxy.com/free-socks-4-proxy.html",
                    12 => @"https://www.my-proxy.com/free-socks-5-proxy.html",
                    _ => @$"https://www.my-proxy.com/free-proxy-list-{random.Next(1, 11)}.html"
                };
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://google.com/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var proxyСontainer = htmlPageDocument.QuerySelector("div.col-sm-9.text-center > div.list");

                if (proxyСontainer == null)
                {
                    throw new Exception($"proxyСontainer == null\n{content}");
                }

                string proxyСontainerTextContent = proxyСontainer.TextContent;

                Regex regexProxy = new Regex(REProxyAddressPattern);
                MatchCollection? matchesProxy = regexProxy.Matches(proxyСontainerTextContent);
                if (matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy.Count == 0\n{content}");
                }

                foreach (Match match in matchesProxy)
                {
                    if (!match.Value.Contains(':'))
                    {
                        continue;
                    }

                    string[] data = match.Value.Split(':');
                    string address = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_My_proxyDotCom(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_Socks_proxyDotNet()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                string url = @"https://www.socks-proxy.net/";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://google.com/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var proxyСontainer = htmlPageDocument.QuerySelector("div.modal-body > textarea.form-control");

                if (proxyСontainer == null)
                {
                    throw new Exception($"proxyСontainer == null\n{content}");
                }

                string proxyСontainerTextContent = proxyСontainer.TextContent;

                Regex regexProxy = new Regex(REProxyAddressPattern);
                MatchCollection? matchesProxy = regexProxy.Matches(proxyСontainerTextContent);
                if (matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy.Count == 0\n{content}");
                }

                foreach (Match match in matchesProxy)
                {
                    if (!match.Value.Contains(':'))
                    {
                        continue;
                    }

                    string[] data = match.Value.Split(':');
                    string address = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_Socks_proxyDotNet(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_Google_proxyDotNet()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                string url = @"https://www.google-proxy.net/";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://google.com/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var proxyСontainer = htmlPageDocument.QuerySelector("div.modal-body > textarea.form-control");

                if (proxyСontainer == null)
                {
                    throw new Exception($"proxyСontainer == null\n{content}");
                }

                string proxyСontainerTextContent = proxyСontainer.TextContent;

                Regex regexProxy = new Regex(REProxyAddressPattern);
                MatchCollection? matchesProxy = regexProxy.Matches(proxyСontainerTextContent);
                if (matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy.Count == 0\n{content}");
                }

                foreach (Match match in matchesProxy)
                {
                    if (!match.Value.Contains(':'))
                    {
                        continue;
                    }

                    string[] data = match.Value.Split(':');
                    string address = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_Google_proxyDotNet(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_SpysDotMe()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            // т.к. сайт почему-то лежит, возвращаеем пока что пустой лист, мб в будущем он проснется..Проснулся вроде
            //if (true)
            //{
            //    return parsedProxy;
            //}

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                Random random = new Random();

                string url = random.Next(0, 2) switch
                {
                    0 => @"https://spys.me/proxy.txt",
                    1 => @"https://spys.me/socks.txt",
                    _ => @"https://spys.me/proxy.txt"
                };
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://google.com/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);
                Regex regexAddress = new Regex(REIpPattern);
                Regex regexPort = new Regex(REPortPattern);

                MatchCollection? matchesProxy = regexProxy.Matches(content);
                if (matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy.Count == 0\n{content}");
                }

                foreach (Match proxy in matchesProxy)
                {
                    if (!proxy.Value.Contains(":"))
                    {
                        continue;
                    }

                    var data = proxy.Value.Split(':');

                    MatchCollection matchesAddress = regexAddress.Matches(data[0]);
                    if (matchesAddress.Count != 1)
                    {
                        throw new Exception($"matchesAddress.Count != 1\n{content}");
                    }
                    string address = matchesAddress[0].Value;

                    MatchCollection matchesPort = regexPort.Matches(data[1]);
                    if (matchesPort.Count != 1)
                    {
                        throw new Exception($"matchesPort.Count != 1\n{content}");
                    }
                    string port = matchesPort[0].Value;

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_SpysDotMe(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_FreeproxyDotlunaproxyDotCom()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;
                Random random = new Random();

                //string url = @"https://gapi.lunaproxy.com/get_free_ip_list";

                string url = url = random.Next(1, 15) switch
                {
                    1 => @"https://freeproxy.lunaproxy.com/page/1.htmll",
                    2 => @"https://freeproxy.lunaproxy.com/page/2.html",
                    3 => @"https://freeproxy.lunaproxy.com/page/3.html",
                    4 => @"https://freeproxy.lunaproxy.com/page/4.html",
                    5 => @"https://freeproxy.lunaproxy.com/page/5.html",
                    6 => @"https://freeproxy.lunaproxy.com/page/6.html",
                    7 => @"https://freeproxy.lunaproxy.com/page/7.html",
                    8 => @"https://freeproxy.lunaproxy.com/page/8.html",
                    9 => @"https://freeproxy.lunaproxy.com/page/9.html",
                    10 => @"https://freeproxy.lunaproxy.com/page/10.html",
                    _ => @$"https://freeproxy.lunaproxy.com/page/{random.Next(1, 11)}.html"
                };
                endpoint = new Uri(url);

                #region New solution

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://freeproxy.lunaproxy.com/");
                request.Headers.Add("origin", @"https://freeproxy.lunaproxy.com");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var divs = htmlPageDocument.QuerySelectorAll("div.table > div.list > div");

                Regex regexAddress = new Regex(REIpPattern);
                Regex regexPort = new Regex(REPortPattern);

                string address = "";
                string port = "";

                foreach (var div in divs)
                {
                    address = div.QuerySelectorAll("div.td")[0].TextContent;
                    port = div.QuerySelectorAll("div.td")[1].TextContent;

                    MatchCollection matchesAddress = regexAddress.Matches(address);
                    if (matchesAddress.Count != 1)
                    {
                        continue;
                        //throw new Exception($"matchesAddress.Count != 1\n{content}");
                    }
                    address = matchesAddress[0].Value;

                    MatchCollection matchesPort = regexPort.Matches(port);
                    if (matchesPort.Count != 1)
                    {
                        continue;
                        //throw new Exception($"matchesPort.Count != 1\n{content}");
                    }
                    port = matchesPort[0].Value;

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                #endregion

                #region Old solution

                //request = new HttpRequestMessage()
                //{
                //    Method = HttpMethod.Post,
                //    RequestUri = endpoint,
                //};
                //request.Headers.Host = endpoint.Host;

                //foreach (var header in base.contextHeaders!)
                //{
                //    request.Headers.Add(header.Key, header.Value);
                //}
                //request.Headers.Add("referer", @"https://freeproxy.lunaproxy.com/");
                //request.Headers.Add("origin", @"https://freeproxy.lunaproxy.com");
                //request.Headers.Add("authorization", "Bearer");

                //responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                //content = await responce.Content.ReadAsStringAsync();

                //if (!responce.IsSuccessStatusCode)
                //{
                //    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                //}

                //dynamic? proxies = JsonConvert.DeserializeObject(content);

                //if (proxies == null)
                //{
                //    throw new Exception($"proxies == null\n{content}");
                //}

                //foreach (var proxy in proxies.ret_data)
                //{
                //    string ip = proxy.anonymity;
                //    string port = proxy.google;

                //    parsedProxy.Add(new ParsedProxy(ip, port));
                //} 

                #endregion

                if (parsedProxy.Count == 0)
                {
                    throw new Exception($"parsedProxy.Count == 0\n{content}");
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                //proxies = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_FreeproxyDotlunaproxyDotCom(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_Proxy_dailyDotCom()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            // сайт умер(
            if (true)
            {
                return parsedProxy;
            }

            //try
            //{
            //    Uri? endpoint = null;
            //    HttpRequestMessage? request = null;
            //    HttpResponseMessage? responce = null;
            //    string? content = null;
            //    IDocument? htmlPageDocument = null;

            //    string url = @"https://proxy-daily.com/";
            //    endpoint = new Uri(url);

            //    request = new HttpRequestMessage()
            //    {
            //        Method = HttpMethod.Get,
            //        RequestUri = endpoint
            //    };
            //    request.Headers.Host = endpoint.Host;

            //    foreach (var header in base.contextHeaders!)
            //    {
            //        request.Headers.Add(header.Key, header.Value);
            //    }
            //    request.Headers.Add("referer", @"https://google.com/");

            //    responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            //    content = await responce.Content.ReadAsStringAsync();

            //    if (!responce.IsSuccessStatusCode)
            //    {
            //        throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
            //    }

            //    htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

            //    var proxyСontainer = htmlPageDocument.QuerySelector("div[id='free-proxy-list']");

            //    if (proxyСontainer == null)
            //    {
            //        throw new Exception($"proxyСontainer == null\n{content}");
            //    }

            //    string proxyСontainerTextContent = proxyСontainer.TextContent;

            //    Regex regexProxy = new Regex(REProxyAddressPattern);
            //    MatchCollection? matchesProxy = regexProxy.Matches(proxyСontainerTextContent);
            //    if (matchesProxy.Count == 0)
            //    {
            //        throw new Exception($"matchesProxy.Count == 0\n{content}");
            //    }

            //    foreach (Match match in matchesProxy)
            //    {
            //        if (!match.Value.Contains(':'))
            //        {
            //            continue;
            //        }

            //        string[] data = match.Value.Split(':');
            //        string address = data[0];
            //        string port = data[1];

            //        parsedProxy.Add(new ParsedProxy(address, port));
            //    }

            //    endpoint = null;
            //    request = null;
            //    responce = null;
            //    content = null;
            //    htmlPageDocument = null;

            //    matchesProxy = null;
            //}
            //catch (Exception e)
            //{
            //    throw new Exception($"class ProxyCombinerTask(), method ParseSite_Proxy_dailyDotCom(), error {e.Message}");
            //}

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_FreeproxylistDotRu()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                Random random = new Random();

                string url = random.Next(0, 28) switch
                {
                    0 => @"https://freeproxylist.ru/en/target/clash-free-proxy/",
                    1 => @"https://freeproxylist.ru/en/target/free-proxy-with-javascript-support/",
                    2 => @"https://freeproxylist.ru/en/target/free-proxy-address-lookup-site/",
                    3 => @"https://freeproxylist.ru/en/target/free-boston-proxy/",
                    4 => @"https://freeproxylist.ru/en/target/free-proxy-y/",
                    5 => @"https://freeproxylist.ru/en/target/free-cloud-proxy/",
                    6 => @"https://freeproxylist.ru/en/target/hotstar-free-proxy/",
                    7 => @"https://freeproxylist.ru/en/target/celcom-free-internet-proxy-android/",
                    8 => @"https://freeproxylist.ru/en/target/free-proxy-hide.me/",
                    9 => @"https://freeproxylist.ru/en/target/high-anonymity-free-proxy/",
                    10 => @"https://freeproxylist.ru/en/target/pia-s5-proxy-free/",
                    11 => @"https://freeproxylist.ru/en/target/free-proxy-for-selenium/",
                    12 => @"https://freeproxylist.ru/en/target/free-proxy-extratorrent/",
                    13 => @"https://freeproxylist.ru/en/target/free-geo-proxy/",
                    14 => @"https://freeproxylist.ru/en/target/freedom-free-download-proxy/",
                    15 => @"https://freeproxylist.ru/en/target/free-undetectable-proxy/",
                    16 => @"https://freeproxylist.ru/en/target/install-free-proxy/",
                    17 => @"https://freeproxylist.ru/en/target/free-proxy-for-frigate-3/",
                    18 => @"https://freeproxylist.ru/en/target/free-ghost-proxy/",
                    19 => @"https://freeproxylist.ru/en/target/free-web-proxy-proxium/",
                    20 => @"https://freeproxylist.ru/en/target/plainproxies-free-web-proxy/",
                    21 => @"https://freeproxylist.ru/en/target/free-residental-proxy/",
                    22 => @"https://freeproxylist.ru/en/target/auto-proxy-apk-free-download/",
                    23 => @"https://freeproxylist.ru/en/target/free-proxy-sute/",
                    24 => @"https://freeproxylist.ru/en/target/free-proxy-orange/",
                    25 => @"https://freeproxylist.ru/en/target/free-proxy-server-pokemon-go/",
                    26 => @"https://freeproxylist.ru/en/target/incloak.com-free-proxy-list/",
                    27 => @"https://freeproxylist.ru/en/target/free-proxy-to-unblock-everything/",
                    _ => @"https://freeproxylist.ru/"
                };
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://google.com/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var proxy_table = htmlPageDocument.QuerySelector("[class='table-proxy-list']");

                if (proxy_table == null)
                {
                    throw new Exception($"proxy_table == null\n{content}");
                }

                var proxy_trs = proxy_table.QuerySelectorAll("tr");

                if (proxy_trs == null || proxy_trs.Count() == 0)
                {
                    throw new Exception($"proxy_trs == null\n{content}");
                }

                Regex regexAddress = new Regex(REIpPattern);
                Regex regexPort = new Regex(REPortPattern);

                foreach (var proxy in proxy_trs)
                {
                    if (proxy.QuerySelectorAll("th[class='w-30 tblport']").Count() == 0)
                    {
                        continue;
                    }

                    var cellAddressTextContent = proxy.QuerySelectorAll("th")[0].TextContent;
                    var cellPortTextContent = proxy.QuerySelectorAll("td")[0].TextContent;

                    MatchCollection matchesAddress = regexAddress.Matches(cellAddressTextContent);
                    if (matchesAddress.Count != 1)
                    {
                        throw new Exception($"matchesAddress.Count != 1\n{content}");
                    }
                    string address = matchesAddress[0].Value;

                    MatchCollection matchesPort = regexPort.Matches(cellPortTextContent);
                    if (matchesPort.Count != 1)
                    {
                        throw new Exception($"matchesPort.Count != 1\n{content}");
                    }
                    string port = matchesPort[0].Value;

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                proxy_trs = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_FreeproxylistDotRu(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_ProxyserversDotPro()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            // сайт умер
            //if (true)
            //{
            //    return parsedProxy;
            //}

            // сайт снова ожил
            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                #region request to get html page with proxy
                string url = @"https://proxyservers.pro/";

                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://www.google.com/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                // get and execute "chash" code
                string? chash = null;
                foreach (var script in htmlPageDocument.Scripts)
                {
                    if (script.Text.Contains("chash"))
                    {
                        chash = script.Text;
                        break;
                    }
                }
                if (chash == null)
                {
                    throw new Exception($"chash == null\n{htmlPageDocument.TextContent}");
                }
                base.jsExecuteContextEngine!.Execute(chash);

                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5));
                #endregion

                #region request to get encode js function
                endpoint = new Uri(@"https://proxyservers.pro/build/site/app.88db5fcb.js");

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://proxyservers.pro/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                string jsEncodeAndDecodeFunc = content;
                string keyElementForIndexOne = ",window.decode_all";
                string keyElementForIndexTwo = "window.decode=";
                int index_to_remove_one = jsEncodeAndDecodeFunc.IndexOf(keyElementForIndexOne);
                jsEncodeAndDecodeFunc = jsEncodeAndDecodeFunc.Remove(index_to_remove_one);
                int index_to_remove_two = jsEncodeAndDecodeFunc.IndexOf(keyElementForIndexTwo);
                jsEncodeAndDecodeFunc = jsEncodeAndDecodeFunc.Remove(0, index_to_remove_two + keyElementForIndexTwo.Length);
                jsEncodeAndDecodeFunc = jsEncodeAndDecodeFunc.Replace("function(t,e)", "function DecodeProxyserversDotPro(t,e)");

                base.jsExecuteContextEngine!.Execute(jsEncodeAndDecodeFunc);
                #endregion

                #region parse and decode proxy from html page
                var proxy_row_trs = htmlPageDocument.QuerySelectorAll("table[class='table table-hover'] > tbody > tr");

                if (proxy_row_trs == null || proxy_row_trs.Count() == 0)
                {
                    throw new Exception($"{htmlPageDocument.Text}");
                }

                foreach (var proxy in proxy_row_trs)
                {
                    if (proxy.QuerySelectorAll("[class='port']").Count() == 0)
                    {
                        continue;
                    }

                    string cellTextContentAddress = proxy.QuerySelectorAll("td")[1].QuerySelector("a")!.GetAttribute("title")!;
                    string cellTextContentPort = proxy.QuerySelectorAll("td")[2].QuerySelector("span")!.GetAttribute("data-port")!;

                    string address = cellTextContentAddress;
                    string port = base.jsExecuteContextEngine!.Evaluate($"DecodeProxyserversDotPro(\"{cellTextContentPort}\", chash)").AsString();
                    // chash we get from html page and execute as js script ---  < script > var chash = '48f054830000cf88caf9abaa704f02ab'; </ script > */

                    parsedProxy.Add(new ParsedProxy(address, port));
                }
                #endregion

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_ProxyserversDotPro(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_FineproxyDotDe()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                string randomValue = new Random().NextSingle().ToString("F16").Replace(',', '.');

                Dictionary<string, string> queryParams = new Dictionary<string, string>()
                {
                    [$"{randomValue}"] = ""
                };

                //string url = QueryHelpers.AddQueryString($"https://fineproxy.de/wp-content/themes/fineproxyde/proxy-list.php", queryParams);
                string url = QueryHelpers.AddQueryString($"https://proxycompass.com/wp-content/themes/proxycompass/proxy-list.php", queryParams);

                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                //request.Headers.Add("referer", @"https://fineproxy.de/en/free-proxy-server-list/");
                //request.Headers.Add("origin", @"https://fineproxy.de");
                request.Headers.Add("referer", @"https://proxycompass.com/free-proxy/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                dynamic? proxies = JsonConvert.DeserializeObject(content);

                if (proxies == null)
                {
                    throw new Exception($"proxies == null\n{content}");
                }

                foreach (var proxy in proxies)
                {
                    string ip = proxy.ip;
                    string port = proxy.port;

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                if (parsedProxy.Count == 0)
                {
                    throw new Exception($"parsedProxy.Count == 0\n{content}");
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                proxies = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_FineproxyDotDe(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_VpnsideDotCom()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                string url = @"https://www.vpnside.com/proxy/list/";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://google.com/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var proxy_table = htmlPageDocument.QuerySelector("table > tbody");

                if (proxy_table == null)
                {
                    throw new Exception($"proxy_table == null\n{content}");
                }

                var proxy_trs = proxy_table.QuerySelectorAll("tr");

                if (proxy_trs == null || proxy_trs.Count() == 0)
                {
                    throw new Exception($"proxy_trs == null || proxy_trs.Count() == 0\n{content}");
                }

                Regex regexAddress = new Regex(REIpPattern);
                Regex regexPort = new Regex(REPortPattern);

                foreach (var proxy in proxy_trs)
                {
                    if (proxy.GetAttribute("id") == null)
                    {
                        continue;
                    }

                    var cellAddressTextContent = proxy.QuerySelectorAll("td")[0].TextContent;
                    var cellPortTextContent = proxy.QuerySelectorAll("td")[1].TextContent;

                    MatchCollection matchesAddress = regexAddress.Matches(cellAddressTextContent);
                    if (matchesAddress.Count != 1)
                    {
                        throw new Exception($"matchesAddress.Count != 1\n{content}");
                    }
                    string address = matchesAddress[0].Value;

                    MatchCollection matchesPort = regexPort.Matches(cellPortTextContent);
                    if (matchesPort.Count != 1)
                    {
                        throw new Exception($"matchesPort.Count != 1\n{content}");
                    }
                    string port = matchesPort[0].Value;

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                proxy_trs = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_VpnsideDotCom(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GithubDotComTheSpeedXPROXY_List(GithubDotComTheSpeedXPROXY_List_protocol? protocol = null)
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                string url = $@"https://raw.githubusercontent.com/TheSpeedX/SOCKS-List/master/{(protocol is null ? ((GithubDotComTheSpeedXPROXY_List_protocol)new Random().Next(0, 3)) : protocol)}.txt";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);
                Regex regexAddress = new Regex(REIpPattern);
                Regex regexPort = new Regex(REPortPattern);

                MatchCollection? matchesProxy = regexProxy.Matches(content);
                if (matchesProxy == null || matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
                }

                foreach (Match proxy in matchesProxy)
                {
                    if (!proxy.Value.Contains(":"))
                    {
                        continue;
                    }

                    var data = proxy.Value.Split(':');

                    MatchCollection matchesAddress = regexAddress.Matches(data[0]);
                    if (matchesAddress.Count != 1)
                    {
                        throw new Exception($"matchesAddress.Count != 1\n{content}");
                    }
                    string address = matchesAddress[0].Value;

                    MatchCollection matchesPort = regexPort.Matches(data[1]);
                    if (matchesPort.Count != 1)
                    {
                        throw new Exception($"matchesPort.Count != 1\n{content}");
                    }
                    string port = matchesPort[0].Value;

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_GithubDotComTheSpeedXPROXY_List(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_Proxy11DotCom()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                string url = new Random().Next(1, 9) switch
                {
                    1 => @"https://proxy11.com/free-proxy",
                    2 => @"https://proxy11.com/free-proxy/speed",
                    3 => @"https://proxy11.com/free-proxy/us",
                    4 => @"https://proxy11.com/free-proxy/anonymous",
                    5 => @"https://proxy11.com/free-proxy/instagram",
                    6 => @"https://proxy11.com/free-proxy/google",
                    7 => @"https://proxy11.com/free-proxy/prot-8080",
                    _ => @"https://proxy11.com/free-proxy"
                };
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://proxy11.com/free-proxy");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var proxy_row_trs = htmlPageDocument.QuerySelectorAll("table.table > tbody > tr");

                if (proxy_row_trs == null || proxy_row_trs.Count() == 0)
                {
                    throw new Exception($"proxy_row_trs == null || proxy_row_trs.Count() == 0\n{content}");
                }

                Regex regexAddress = new Regex(REIpPattern);
                Regex regexPort = new Regex(REPortPattern);

                foreach (var proxy in proxy_row_trs)
                {
                    if (proxy.QuerySelectorAll("td").Count() == 0)
                    {
                        continue;
                    }

                    var cellAddressTextContent = proxy.QuerySelectorAll("td")[0].TextContent;
                    var cellPortTextContent = proxy.QuerySelectorAll("td")[1].TextContent;

                    MatchCollection matchesAddress = regexAddress.Matches(cellAddressTextContent);
                    if (matchesAddress.Count != 1)
                    {
                        throw new Exception($"matchesAddress.Count != 1\n{content}");
                    }
                    string address = matchesAddress[0].Value;

                    MatchCollection matchesPort = regexPort.Matches(cellPortTextContent);
                    if (matchesPort.Count != 1)
                    {
                        throw new Exception($"matchesPort.Count != 1\n{content}");
                    }
                    string port = matchesPort[0].Value;

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                proxy_row_trs = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_Proxy11DotCom(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_IproyalDotCom()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                Dictionary<string, string> queryParams = new Dictionary<string, string>()
                {
                    ["page"] = $"{new Random().Next(1, 3)}",
                    ["entries"] = "100"
                };

                string url = QueryHelpers.AddQueryString(@"https://iproyal.com/free-proxy-list/", queryParams);
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://iproyal.com/free-proxy-list/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                //var proxy_row_divs = htmlPageDocument.QuerySelectorAll("div.overflow-auto.astro-lmapxigl > div");
                var proxy_row_divs = htmlPageDocument.QuerySelectorAll("div[class=\"max-sm:overflow-auto max-md:pr-16 astro-lmapxigl\"] > div > div");

                if (proxy_row_divs == null || proxy_row_divs.Count() == 0)
                {
                    throw new Exception($"proxy_row_divs == null || proxy_row_divs.Count() == 0\n{content}");
                }

                Regex regexAddress = new Regex(REIpPattern);
                Regex regexPort = new Regex(REPortPattern);

                foreach (var proxy in proxy_row_divs)
                {
                    var attrClassValue = proxy.GetAttribute("class");
                    //if (attrClassValue == null || !attrClassValue.Contains("bg-secondaryContainer"))
                    //{
                    //    continue;
                    //}
                    if (attrClassValue == null || attrClassValue.Contains("grid bg-neutral-50"))
                    {
                        continue;
                    }

                    var cellAddressTextContent = proxy.QuerySelectorAll("div")[0].TextContent;
                    var cellPortTextContent = proxy.QuerySelectorAll("div")[1].TextContent;

                    MatchCollection matchesAddress = regexAddress.Matches(cellAddressTextContent);
                    if (matchesAddress.Count != 1)
                    {
                        throw new Exception($"matchesAddress.Count != 1\n{content}");
                    }
                    string address = matchesAddress[0].Value;

                    MatchCollection matchesPort = regexPort.Matches(cellPortTextContent);
                    if (matchesPort.Count != 1)
                    {
                        throw new Exception($"matchesPort.Count != 1\n{content}");
                    }
                    string port = matchesPort[0].Value;

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                proxy_row_divs = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_IproyalDotCom(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_Free_proxy_listDotCom()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            // site died =(
            return parsedProxy;

            //try
            //{
            //    Uri? endpoint = null;
            //    HttpRequestMessage? request = null;
            //    HttpResponseMessage? responce = null;
            //    string? content = null;
            //    IDocument? htmlPageDocument = null;

            //    string url = @"https://free-proxy-list.com/";
            //    endpoint = new Uri(url);

            //    request = new HttpRequestMessage()
            //    {
            //        Method = HttpMethod.Get,
            //        RequestUri = endpoint,
            //    };
            //    request.Headers.Host = endpoint.Host;

            //    foreach (var header in base.contextHeaders!)
            //    {
            //        request.Headers.Add(header.Key, header.Value);
            //    }
            //    request.Headers.Add("referer", @"https://google.com");

            //    responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            //    content = await responce.Content.ReadAsStringAsync();

            //    if (!responce.IsSuccessStatusCode)
            //    {
            //        throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
            //    }

            //    htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

            //    var proxy_row_trs = htmlPageDocument.QuerySelectorAll("table.table.table-striped.proxy-list > tbody > tr");

            //    if (proxy_row_trs == null || proxy_row_trs.Count() == 0)
            //    {
            //        throw new Exception($"proxy_row_trs == null || proxy_row_trs.Count() == 0\n{content}");
            //    }

            //    Regex regexAddress = new Regex(REIpPattern);
            //    Regex regexPort = new Regex(REPortPattern);

            //    foreach (var proxy in proxy_row_trs)
            //    {
            //        if (proxy.QuerySelectorAll("td[class='report-cell']").Count() == 0)
            //        {
            //            continue;
            //        }

            //        var cellAddressTextContent = proxy.QuerySelectorAll("td")[0].QuerySelector("a")!.TextContent;
            //        var cellPortTextContent = proxy.QuerySelectorAll("td")[2].TextContent;

            //        // из-за того что сайт выдает иногда 1 иногда 0 проксей - бывает ошибка
            //        MatchCollection matchesAddress = regexAddress.Matches(cellAddressTextContent);
            //        if (matchesAddress.Count != 1)
            //        {
            //            return parsedProxy;
            //            //throw new Exception($"matchesAddress.Count != 1\n{content}");
            //        }
            //        string address = matchesAddress[0].Value;

            //        MatchCollection matchesPort = regexPort.Matches(cellPortTextContent);
            //        if (matchesPort.Count != 1)
            //        {
            //            return parsedProxy;
            //            //throw new Exception($"matchesPort.Count != 1\n{content}");
            //        }
            //        string port = matchesPort[0].Value;

            //        parsedProxy.Add(new ParsedProxy(address, port));
            //    }

            //    endpoint = null;
            //    request = null;
            //    responce = null;
            //    content = null;
            //    htmlPageDocument = null;

            //    proxy_row_trs = null;
            //}
            //catch (Exception e)
            //{
            //    throw new Exception($"class ProxyCombinerTask(), method ParseSite_Free_proxy_listDotCom(), error {e.Message}");
            //}

            //return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_Proxy_listDotOrg()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                #region request to get html page with proxy
                // set query parameter
                string language_page = new Random().Next(1, 7) switch
                {
                    1 => @"russian",
                    2 => @"english",
                    3 => @"chinese",
                    4 => @"spanish",
                    5 => @"german",
                    6 => @"french",
                    _ => @"russian"
                };
                string url = @$"https://proxy-list.org/{language_page}/index.php";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://proxy-list.org/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5));
                #endregion

                #region request to get encode js function
                endpoint = new Uri(@"https://proxy-list.org/images/ajax.js");

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @$"https://proxy-list.org/{language_page}/index.php");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                string jsEncodeAndDecodeFunc = content;
                string keyElementForIndexOne = "xmlhttp.send(postData);\n}";
                int index_to_remove_one = jsEncodeAndDecodeFunc.LastIndexOf(keyElementForIndexOne);
                jsEncodeAndDecodeFunc = jsEncodeAndDecodeFunc.Remove(0, index_to_remove_one + keyElementForIndexOne.Length);
                jsEncodeAndDecodeFunc = jsEncodeAndDecodeFunc.Trim().Replace("document.write", "return ");

                base.jsExecuteContextEngine!.Execute(jsEncodeAndDecodeFunc);
                #endregion

                #region parse and decode proxy from html page
                var proxy_row_uls = htmlPageDocument.QuerySelectorAll("div.proxy-table > div.table-wrap > div.table > ul");

                if (proxy_row_uls == null || proxy_row_uls.Count() == 0)
                {
                    throw new Exception($"proxy_row_uls == null || proxy_row_uls.Count() == 0");
                }

                foreach (var proxy in proxy_row_uls)
                {
                    if (proxy.QuerySelectorAll("[class='proxy']").Count() == 0)
                    {
                        continue;
                    }

                    string cellTextContentProxy = proxy.QuerySelector("li[class='proxy']")!.TextContent;
                    //cellTextContentProxy = cellTextContentProxy.Split('<', '>')[2];

                    string proxy_decoded = base.jsExecuteContextEngine!.Evaluate($"{cellTextContentProxy}").AsString();

                    if (!proxy_decoded.Contains(':'))
                    {
                        continue;
                    }

                    string[] data = proxy_decoded.Split(':');
                    string address = data[0];
                    string port = data[1];

                    // или можно использовать представление на с# этой же функции ниже
                    /*string address = AdvancedDotNameDecodeBase64(cellTextContentAddress);
                    string port = AdvancedDotNameDecodeBase64(cellTextContentPort);
                    string AdvancedDotNameDecodeBase64(string t)
                    {
                        string keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
                        string d = "";
                        int s = 0;
                        t = t.Replace("[^A-Za-z0-9\\+\\/\\=]", "");
                        while (s < t.Length)
                        {
                            int n = keyStr.IndexOf(t[s++]);
                            int i = keyStr.IndexOf(t[s++]);
                            int c = keyStr.IndexOf(t[s++]);
                            int h = keyStr.IndexOf(t[s++]);
                            int r = n << 2 | i >> 4;
                            int o = (15 & i) << 4 | c >> 2;
                            int a = (3 & c) << 6 | h;
                            d += Convert.ToChar(r);
                            if (c != 64)
                            {
                                d += Convert.ToChar(o);
                            }
                            if (h != 64)
                            {
                                d += Convert.ToChar(a);
                            }
                        }
                        return Encoding.UTF8.GetString(Array.ConvertAll(Regex.Unescape(d).ToCharArray(), c => (byte)c));
                    }*/

                    parsedProxy.Add(new ParsedProxy(address, port));
                }
                #endregion

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                proxy_row_uls = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_Proxy_listDotOrg(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_Best_proxyDotCom()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                #region request to get html page with proxy
                // set query parameter
                string language_page = new Random().Next(1, 7) switch
                {
                    1 => @"russian",
                    2 => @"english",
                    3 => @"chinese",
                    4 => @"spanish",
                    5 => @"german",
                    6 => @"french",
                    _ => @"russian"
                };
                string url = @$"https://best-proxy.com/{language_page}/index.php";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://best-proxy.com/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5));
                #endregion

                #region request to get encode js function
                endpoint = new Uri(@"https://best-proxy.com/images/ajax.js");

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @$"https://best-proxy.com/{language_page}/index.php");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                string jsEncodeAndDecodeFunc = content;
                string keyElementForIndexOne = "xmlhttp.send(postData);\n}";
                int index_to_remove_one = jsEncodeAndDecodeFunc.LastIndexOf(keyElementForIndexOne);
                jsEncodeAndDecodeFunc = jsEncodeAndDecodeFunc.Remove(0, index_to_remove_one + keyElementForIndexOne.Length);
                jsEncodeAndDecodeFunc = jsEncodeAndDecodeFunc.Trim().Replace("document.write", "return ");

                base.jsExecuteContextEngine!.Execute(jsEncodeAndDecodeFunc);
                #endregion

                #region parse and decode proxy from html page
                var proxy_row_uls = htmlPageDocument.QuerySelectorAll("div.proxy-table > div.table-wrap > div.table > ul");

                if (proxy_row_uls == null || proxy_row_uls.Count() == 0)
                {
                    throw new Exception($"proxy_row_uls == null || proxy_row_uls.Count() == 0");
                }

                foreach (var proxy in proxy_row_uls)
                {
                    if (proxy.QuerySelectorAll("[class='proxy']").Count() == 0)
                    {
                        continue;
                    }

                    string cellTextContentProxy = proxy.QuerySelector("li[class='proxy']")!.TextContent;
                    //cellTextContentProxy = cellTextContentProxy.Split('<', '>')[2];

                    string proxy_decoded = base.jsExecuteContextEngine!.Evaluate($"{cellTextContentProxy}").AsString();

                    if (!proxy_decoded.Contains(':'))
                    {
                        continue;
                    }

                    string[] data = proxy_decoded.Split(':');
                    string address = data[0];
                    string port = data[1];

                    // или можно использовать представление на с# этой же функции ниже
                    /*string address = AdvancedDotNameDecodeBase64(cellTextContentAddress);
                    string port = AdvancedDotNameDecodeBase64(cellTextContentPort);
                    string AdvancedDotNameDecodeBase64(string t)
                    {
                        string keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
                        string d = "";
                        int s = 0;
                        t = t.Replace("[^A-Za-z0-9\\+\\/\\=]", "");
                        while (s < t.Length)
                        {
                            int n = keyStr.IndexOf(t[s++]);
                            int i = keyStr.IndexOf(t[s++]);
                            int c = keyStr.IndexOf(t[s++]);
                            int h = keyStr.IndexOf(t[s++]);
                            int r = n << 2 | i >> 4;
                            int o = (15 & i) << 4 | c >> 2;
                            int a = (3 & c) << 6 | h;
                            d += Convert.ToChar(r);
                            if (c != 64)
                            {
                                d += Convert.ToChar(o);
                            }
                            if (h != 64)
                            {
                                d += Convert.ToChar(a);
                            }
                        }
                        return Encoding.UTF8.GetString(Array.ConvertAll(Regex.Unescape(d).ToCharArray(), c => (byte)c));
                    }*/

                    parsedProxy.Add(new ParsedProxy(address, port));
                }
                #endregion

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                proxy_row_uls = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_Best_proxyDotCom(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_Best_proxiesDotRu()
        {
            // тут очень сложная защита была и они её изменили, миниум день на решение
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;
                Engine? InternalJsEngine = new Engine();

                #region request to get html page with obfuscatedFunction
                endpoint = new Uri(@"https://best-proxies.ru/proxylist/free/");

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("host", @"best-proxies.ru");
                request.Headers.Add("connection", "keep-alive");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                #endregion

                if (responce.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    #region manipulations to bypass site security

                    htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                    string? obfuscatedFunction = null;
                    #region get obfuscatedFunction data
                    foreach (var script in htmlPageDocument.Scripts)
                    {
                        if (script.Text.Contains("function(p,a,c,k,e,d)"))
                        {
                            obfuscatedFunction = script.Text;
                            break;
                        }
                    }
                    if (obfuscatedFunction == null)
                    {
                        throw new Exception("");
                    }
                    #endregion

                    string UnpackedDeobfuscatedJsFunc = "";
                    #region unpack and deobfuscate js func
                    try
                    {
                        string keyElementForIndexOne = "return;\n}";
                        int index_to_remove_one = obfuscatedFunction.LastIndexOf(keyElementForIndexOne);
                        obfuscatedFunction = obfuscatedFunction.Remove(0, index_to_remove_one + keyElementForIndexOne.Length);
                        string keySecondElementForIndexOne = "split('|'),0,{}))";
                        int index_to_remove_two = obfuscatedFunction.LastIndexOf(keySecondElementForIndexOne);
                        obfuscatedFunction = obfuscatedFunction.Remove(index_to_remove_two + keySecondElementForIndexOne.Length);
                        obfuscatedFunction = obfuscatedFunction.Replace("eval(function", "function unpack").Replace(".split('|'),0,{}))", ".split('|'),0,{})");

                        string[] obfuscatedFunctionData = obfuscatedFunction.Split("return p}");
                        obfuscatedFunctionData[1] = obfuscatedFunctionData[1].Replace(",0,{})", ",0,{}").Remove(0, 1);
                        InternalJsEngine.Execute(obfuscatedFunctionData[0] + "return p}");
                        UnpackedDeobfuscatedJsFunc = InternalJsEngine.Evaluate($"unpack({obfuscatedFunctionData[1]})").AsString();
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region unpack and deobfuscate js func': {e.Message}");
                    }
                    #endregion

                    #region get and compute top variable rows
                    try
                    {
                        string keyElementForTopVariableRows = "document.cookie=";
                        int index_to_remove_ForTopVariableRows = UnpackedDeobfuscatedJsFunc.IndexOf(keyElementForTopVariableRows);
                        string data_ForTopVariableRows = UnpackedDeobfuscatedJsFunc.Remove(index_to_remove_ForTopVariableRows);
                        InternalJsEngine.Execute(data_ForTopVariableRows);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get and compute top variable rows': {e.Message}");
                    }
                    #endregion

                    Cookie jsCode = new Cookie();
                    Cookie fragment = new Cookie();
                    #region get value of variable 'c' for jsCode cookie and create(init) two cookies of top rows
                    try
                    {
                        // get parameter 'c' for jsCode cookie 
                        string valueOfVariable_c_jsCodeCookie = InternalJsEngine.Evaluate("c").AsString();

                        // init cookies
                        jsCode.Name = "jsCode";
                        jsCode.Value = valueOfVariable_c_jsCodeCookie;
                        jsCode.Path = "/";
                        jsCode.Secure = true;
                        jsCode.Domain = ".best-proxies.ru";

                        fragment.Name = "fragment";
                        fragment.Value = "";
                        fragment.Path = "/";
                        fragment.Secure = true;
                        fragment.Domain = ".best-proxies.ru";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get value of variable 'c' for jsCode cookie and create(init) two cookies of top rows': {e.Message}");
                    }
                    #endregion

                    #region compute 2 top func pad and ip2int
                    try
                    {
                        // compute func 'pad'
                        InternalJsEngine.Execute("function pad(n,width){n=n+\"\";return n.length>=width?n:new Array(width-n.length+1).join(\"0\")+n}");
                        // compute func 'ip2int'
                        InternalJsEngine.Execute("function ip2int(ip){return ip.split(\".\").reduce(function(ipInt,octet){return(ipInt<<8)+parseInt(octet,10)},0)>>>0}");
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region compute 2 top func pad and ip2int': {e.Message}");
                    }
                    #endregion

                    #region get and create top variables of continueFunc
                    try
                    {
                        string firstkeyfor_CreateTopVariablesofContinueFunc = "clearInterval(continueFunc);";
                        int index_to_remove_firstkeyfor_CreateTopVariablesofContinueFunc = UnpackedDeobfuscatedJsFunc.LastIndexOf(firstkeyfor_CreateTopVariablesofContinueFunc);
                        string data_from_remove_firstkeyfor_CreateTopVariablesofContinueFunc = UnpackedDeobfuscatedJsFunc.Remove(0, index_to_remove_firstkeyfor_CreateTopVariablesofContinueFunc + firstkeyfor_CreateTopVariablesofContinueFunc.Length);
                        string secondkeyfor_CreateTopVariablesofContinueFunc = "pad(ip2int(lip),10);";
                        int index_to_remove_secondkeyfor_CreateTopVariablesofContinueFunc = data_from_remove_firstkeyfor_CreateTopVariablesofContinueFunc.LastIndexOf(secondkeyfor_CreateTopVariablesofContinueFunc);
                        string data_to_execute_from_index_to_remove_secondkeyfor_CreateTopVariablesofContinueFunc = data_from_remove_firstkeyfor_CreateTopVariablesofContinueFunc.Remove(index_to_remove_secondkeyfor_CreateTopVariablesofContinueFunc + secondkeyfor_CreateTopVariablesofContinueFunc.Length);
                        data_to_execute_from_index_to_remove_secondkeyfor_CreateTopVariablesofContinueFunc = data_to_execute_from_index_to_remove_secondkeyfor_CreateTopVariablesofContinueFunc.Replace("var c=", "var c_continueFuncCode=");
                        data_to_execute_from_index_to_remove_secondkeyfor_CreateTopVariablesofContinueFunc = data_to_execute_from_index_to_remove_secondkeyfor_CreateTopVariablesofContinueFunc.Replace("request=new XMLHttpRequest(),", "");
                        InternalJsEngine.Execute(data_to_execute_from_index_to_remove_secondkeyfor_CreateTopVariablesofContinueFunc);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get and create top variables of continueFunc': {e.Message}");
                    }
                    #endregion

                    string c = "";
                    string fp = "";
                    string lip = "";
                    string padScreenWidth = "";
                    string padScreenHeight = "";
                    #region get values of some variables (c, fp, lip, padScreenWidth, padScreenHeight)
                    try
                    {
                        c = InternalJsEngine.Evaluate("c_continueFuncCode").AsString();
                        fp = InternalJsEngine.Evaluate("fp").AsString();
                        lip = InternalJsEngine.Evaluate("lip").AsString();
                        padScreenWidth = InternalJsEngine.Evaluate("pad(1920, 4)").AsString();
                        padScreenHeight = InternalJsEngine.Evaluate("pad(1080, 4)").AsString();
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get values of some variables (c, fp, lip, padScreenWidth, padScreenHeight)': {e.Message}");
                    }
                    #endregion

                    Cookie cookieOfContinueFunc = new Cookie();
                    #region init cookie of continueFunc and set variable of 'code' to value ''
                    try
                    {
                        cookieOfContinueFunc.Name = c;
                        cookieOfContinueFunc.Value = fp + lip + padScreenWidth + padScreenHeight;  // value of this cookie is: fp-fingerprint(32char), lip-ip(10char), width(4char(1920)), height(4char(1080))
                        cookieOfContinueFunc.Path = "/";
                        cookieOfContinueFunc.Domain = ".best-proxies.ru";

                        InternalJsEngine.Execute("code = \"\"");
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region init cookie of continueFunc and set variable of 'code' to value ''.': {e.Message}");
                    }
                    #endregion

                    #region compute func 'forEach' of continueFunc
                    try
                    {
                        InternalJsEngine.Execute("code.split(\"\").forEach(function(val,i){var index=url.length-code.length+i;var letter=url.substring(index,index+1);val=val==letter?\"1\":val;url=url.substr(0,index)+val+url.substr(index+val.length)});");
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region compute func 'forEach' of continueFunc': {e.Message}");

                    }
                    #endregion

                    string url_for_post_request = "";
                    string url_for_redir_request = "";
                    #region get url variable values
                    try
                    {
                        url_for_post_request = InternalJsEngine.Evaluate("url").AsString();
                        url_for_redir_request = InternalJsEngine.Evaluate("redir").AsString();
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region get url variable values': {e.Message}");
                    }
                    #endregion

                    #region add 3 cookies (jsCode, fragment, cookieOfContinueFunc) to CookieContainer
                    try
                    {
                        ProxyParseCookieContainer.Add(jsCode);
                        ProxyParseCookieContainer.Add(fragment);
                        ProxyParseCookieContainer.Add(cookieOfContinueFunc);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region add 3 cookies (jsCode, fragment, cookieOfContinueFunc) to CookieContainer': {e.Message}");
                    }
                    #endregion

                    #region post request to get 'secure' cookie 
                    try
                    {
                        endpoint = new Uri(url_for_post_request);
                        request = new HttpRequestMessage()
                        {
                            Method = HttpMethod.Post,
                            RequestUri = endpoint
                        };

                        foreach (var header in base.contextHeaders!)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }
                        request.Headers.Add("referer", @"https://best-proxies.ru/proxylist/free/");
                        request.Headers.Add("origin", @"https://best-proxies.ru");
                        request.Headers.Add("host", @"best-proxies.ru");
                        request.Headers.Add("connection", "keep-alive");

                        responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region post request to get 'secure' cookie ': {e.Message}");
                    }
                    #endregion

                    #region remove 2 cookeis(valueOf(c), jsCode) from CookieContainer
                    try
                    {
                        ProxyParseCookieContainer.GetAllCookies().Where(cookie => cookie.Name == c || cookie.Name == "jsCode").ToList().ForEach(cookie =>
                        {
                            cookie.Expired = true;
                        });
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region remove 2 cookeis(valueOf(c), jsCode) from CookieContainer ': {e.Message}");
                    }
                    #endregion

                    #region request to get access to page with proxy
                    try
                    {
                        endpoint = new Uri(url_for_redir_request);
                        request = new HttpRequestMessage()
                        {
                            Method = HttpMethod.Get,
                            RequestUri = endpoint
                        };

                        foreach (var header in base.contextHeaders!)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }
                        request.Headers.Add("referer", @"https://best-proxies.ru/proxylist/free/");
                        request.Headers.Add("host", @"best-proxies.ru");
                        request.Headers.Add("connection", "keep-alive");

                        responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                        if (responce.StatusCode == HttpStatusCode.ServiceUnavailable || !(responce.StatusCode == HttpStatusCode.OK || responce.StatusCode == HttpStatusCode.MovedPermanently))
                        {
                            throw new Exception($"status code == {responce.StatusCode}");
                        }

                        content = await responce.Content.ReadAsStringAsync();
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error in '#region request to get access to page with proxy': {e.Message}");
                    }
                    #endregion

                    #endregion
                }

                #region parse proxy from html page

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                string? obfuscatedProxyPortFunction = null;
                foreach (var script in htmlPageDocument.Scripts)
                {
                    if (script.Text.Contains("function(p,a,c,k,e,d)"))
                    {
                        obfuscatedProxyPortFunction = script.Text;
                        break;
                    }
                }
                if (obfuscatedProxyPortFunction == null)
                {
                    throw new Exception("obfuscatedProxyPortFunction == null");
                }
                base.jsExecuteContextEngine!.Execute(obfuscatedProxyPortFunction);

                var proxy_row_trs = htmlPageDocument.QuerySelectorAll("table.table.table-striped > tbody > tr");

                foreach (var proxy in proxy_row_trs)
                {
                    if (proxy.QuerySelectorAll("td[data-time]").Count() == 0)
                    {
                        continue;
                    }

                    string cellTextContentProxy = proxy.QuerySelector("div[class='dropdown'] > a")!.TextContent;

                    string[] data = cellTextContentProxy.Split(':');
                    string address = data[0];
                    string coded_port = data[1].Replace("document.write(", "").Replace(");", "");

                    string port_decoded = base.jsExecuteContextEngine!.Evaluate($"{coded_port}").AsString();

                    parsedProxy.Add(new ParsedProxy(address, port_decoded));
                }

                var proxy_containers = htmlPageDocument.QuerySelectorAll("div.col-xs-12.col-sm-6.col-md-3");
                if (proxy_containers == null || proxy_containers.Count() == 0)
                {
                    throw new Exception("proxy_containers == null || proxy_containers.Count() == 0");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);

                foreach (var container in proxy_containers)
                {
                    MatchCollection matchesProxy = regexProxy.Matches(container.InnerHtml);

                    if (matchesProxy.Count == 0)
                    {
                        throw new Exception("matchesProxy.Count == 0");
                    }

                    foreach (Match match in matchesProxy)
                    {
                        if (!match.Value.Contains(':'))
                        {
                            continue;
                        }

                        string[] data = match.Value.Split(':');
                        string address = data[0];
                        string port = data[1];

                        parsedProxy.Add(new ParsedProxy(address, port));
                    }
                }

                if (parsedProxy.Count == 0)
                {
                    throw new Exception($"parsedProxy.Count == 0\n{content}");
                }
                #endregion

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;
                InternalJsEngine = null;
            }
            catch (Exception e)
            {
                if(!e.Message.Contains("Попытка установить соединение была безуспешной") && !e.Message.Contains("best-proxies.ru:443"))
                {
                    throw new Exception($"class ProxyCombinerTask(), method ParseSite_Best_proxiesDotRu(), error {e.Message}");
                }
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_ProxynovaDotCom()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                #region request to get html page with proxy
                // set query parameter
                string url = @"https://www.proxynova.com/proxy-server-list/";

                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://google.com");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5));
                #endregion

                #region execute some js functions

                //string repeatFunc = "String.prototype.repeat = function repeatString(count) { return Array(count + 1).join(this) };";
                string base64Alg = "var Base64={_keyStr:\"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=\",encode:function(e){var t=\"\";var n,r,i,s,o,u,a;var f=0;e=Base64._utf8_encode(e);while(f<e.length){n=e.charCodeAt(f++);r=e.charCodeAt(f++);i=e.charCodeAt(f++);s=n>>2;o=(n&3)<<4|r>>4;u=(r&15)<<2|i>>6;a=i&63;if(isNaN(r)){u=a=64}else if(isNaN(i)){a=64}t=t+this._keyStr.charAt(s)+this._keyStr.charAt(o)+this._keyStr.charAt(u)+this._keyStr.charAt(a)}return t},decode:function(e){var t=\"\";var n,r,i;var s,o,u,a;var f=0;e=e.replace(/[^A-Za-z0-9\\+\\/\\=]/g,\"\");while(f<e.length){s=this._keyStr.indexOf(e.charAt(f++));o=this._keyStr.indexOf(e.charAt(f++));u=this._keyStr.indexOf(e.charAt(f++));a=this._keyStr.indexOf(e.charAt(f++));n=s<<2|o>>4;r=(o&15)<<4|u>>2;i=(u&3)<<6|a;t=t+String.fromCharCode(n);if(u!=64){t=t+String.fromCharCode(r)}if(a!=64){t=t+String.fromCharCode(i)}}t=Base64._utf8_decode(t);return t},_utf8_encode:function(e){e=e.replace(/\\r\\n/g,\"\\n\");var t=\"\";for(var n=0;n<e.length;n++){var r=e.charCodeAt(n);if(r<128){t+=String.fromCharCode(r)}else if(r>127&&r<2048){t+=String.fromCharCode(r>>6|192);t+=String.fromCharCode(r&63|128)}else{t+=String.fromCharCode(r>>12|224);t+=String.fromCharCode(r>>6&63|128);t+=String.fromCharCode(r&63|128)}}return t},_utf8_decode:function(e){var t=\"\";var n=0;var r=c1=c2=0;while(n<e.length){r=e.charCodeAt(n);if(r<128){t+=String.fromCharCode(r);n++}else if(r>191&&r<224){c2=e.charCodeAt(n+1);t+=String.fromCharCode((r&31)<<6|c2&63);n+=2}else{c2=e.charCodeAt(n+1);c3=e.charCodeAt(n+2);t+=String.fromCharCode((r&15)<<12|(c2&63)<<6|c3&63);n+=3}}return t}}";
                string atobFunc = "var atob = function atob(data) { return Base64.decode(data); }";
                //base.jsExecuteContextEngine!.Execute($"{repeatFunc}");
                base.jsExecuteContextEngine!.Execute($"{base64Alg}");
                base.jsExecuteContextEngine!.Execute($"{atobFunc}");

                #endregion

                #region parse and decode proxy from html page
                var proxy_row_trs = htmlPageDocument.QuerySelectorAll("table[id='tbl_proxy_list'] > tbody > tr");

                Regex regexAddress = new Regex(REIpPattern);
                Regex regexPort = new Regex(REPortPattern);

                foreach (var proxy in proxy_row_trs)
                {
                    if (proxy.GetAttribute("data-proxy-id") == null)
                    {
                        continue;
                    }

                    string cellTextContentAddress = proxy.QuerySelectorAll("td")[0].TextContent.Trim().Replace("document.write", "");
                    string cellTextContentPort = proxy.QuerySelectorAll("td")[1].TextContent;

                    string unchecked_address = base.jsExecuteContextEngine!.Evaluate($"{cellTextContentAddress}").AsString();
                    string unchecked_port = cellTextContentPort.Trim();

                    MatchCollection matchesAddress = regexAddress.Matches(unchecked_address);
                    if (matchesAddress.Count != 1)
                    {
                        throw new Exception($"matchesAddress.Count != 1\n{content}");
                    }
                    string address = matchesAddress[0].Value;

                    MatchCollection matchesPort = regexPort.Matches(unchecked_port);
                    if (matchesPort.Count != 1)
                    {
                        throw new Exception($"matchesPort.Count != 1\n{content}");
                    }
                    string port = matchesPort[0].Value;

                    // или можно использовать представление на с# этой же функции ниже
                    /*string address = AdvancedDotNameDecodeBase64(cellTextContentAddress);
                    string port = AdvancedDotNameDecodeBase64(cellTextContentPort);
                    string AdvancedDotNameDecodeBase64(string t)
                    {
                        string keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
                        string d = "";
                        int s = 0;
                        t = t.Replace("[^A-Za-z0-9\\+\\/\\=]", "");
                        while (s < t.Length)
                        {
                            int n = keyStr.IndexOf(t[s++]);
                            int i = keyStr.IndexOf(t[s++]);
                            int c = keyStr.IndexOf(t[s++]);
                            int h = keyStr.IndexOf(t[s++]);
                            int r = n << 2 | i >> 4;
                            int o = (15 & i) << 4 | c >> 2;
                            int a = (3 & c) << 6 | h;
                            d += Convert.ToChar(r);
                            if (c != 64)
                            {
                                d += Convert.ToChar(o);
                            }
                            if (h != 64)
                            {
                                d += Convert.ToChar(a);
                            }
                        }
                        return Encoding.UTF8.GetString(Array.ConvertAll(Regex.Unescape(d).ToCharArray(), c => (byte)c));
                    }*/

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                if (parsedProxy.Count == 0)
                {
                    throw new Exception($"parsedProxy.Count == 0\n{content}");
                }
                #endregion

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_ProxynovaDotCom(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_ProxydockerDotCom()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                FormUrlEncodedContent? formContent = null;
                IDocument? htmlPageDocument = null;

                string? token = null;

                #region request to get html page with token
                endpoint = new Uri($@"https://www.proxydocker.com/");

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://google.com");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                token = htmlPageDocument.QuerySelector("meta[name='_token']")?.GetAttribute("content");

                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception($"string.IsNullOrEmpty(token)\n{content}");
                }

                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(3));
                #endregion

                #region request to get json proxy
                endpoint = new Uri($@"https://www.proxydocker.com/en/api/proxylist/");

                Dictionary<string, string> keyValuePairsForContent = new Dictionary<string, string>()
                {
                    { "token", token },
                    { "country", "all" },
                    { "city", "all" },
                    { "state", "all" },
                    { "port", "all" },
                    { "type", "all" },
                    { "anonymity", "all" },
                    { "need", "all" },
                    { "page", "1" }
                };

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = endpoint,
                    Content = (formContent = new FormUrlEncodedContent(keyValuePairsForContent))
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://www.proxydocker.com/");
                request.Headers.Add("origin", @"https://www.proxydocker.com");
                request.Headers.Add("x-requested-with", @"XMLHttpRequest");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }
                #endregion

                #region parse proxy from json content
                dynamic? proxies = JsonConvert.DeserializeObject(content);

                if (proxies == null)
                {
                    throw new Exception($"proxies == null\n{content}");
                }

                foreach (var proxy in proxies.proxies)
                {
                    string ip = proxy.ip;
                    string port = proxy.port;

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                if (parsedProxy.Count == 0)
                {
                    throw new Exception($"parsedProxy.Count == 0\n{content}");
                }
                #endregion

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                formContent = null;
                htmlPageDocument = null;

                proxies = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_ProxydockerDotCom(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GoodproxiesDotRu()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                Dictionary<string, string> queryParams = new Dictionary<string, string>()
                {
                    ["count"] = "100",
                    ["ping"] = "8000",
                    ["time"] = "600",
                    ["works"] = "50",
                    ["key"] = "freeproxy"
                };

                string url = QueryHelpers.AddQueryString($@"https://api.good-proxies.ru/getfree.php", queryParams);
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://good-proxies.ru/ifree.php/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                if (responce.StatusCode != HttpStatusCode.TooManyRequests)
                {
                    Regex regexProxy = new Regex(REProxyAddressPattern);
                    Regex regexAddress = new Regex(REIpPattern);
                    Regex regexPort = new Regex(REPortPattern);

                    MatchCollection? matchesProxy = regexProxy.Matches(content);
                    if (matchesProxy == null || matchesProxy.Count == 0)
                    {
                        throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
                    }

                    foreach (Match proxy in matchesProxy)
                    {
                        if (!proxy.Value.Contains(":"))
                        {
                            continue;
                        }

                        var data = proxy.Value.Split(':');

                        MatchCollection matchesAddress = regexAddress.Matches(data[0]);
                        if (matchesAddress.Count != 1)
                        {
                            throw new Exception($"matchesAddress.Count != 1\n{content}");
                        }
                        string address = matchesAddress[0].Value;

                        MatchCollection matchesPort = regexPort.Matches(data[1]);
                        if (matchesPort.Count != 1)
                        {
                            throw new Exception($"matchesPort.Count != 1\n{content}");
                        }
                        string port = matchesPort[0].Value;

                        parsedProxy.Add(new ParsedProxy(address, port));
                    }

                    matchesProxy = null;
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_GoodproxiesDotRu(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_CheckerproxyDotNet()
        {
            // этот сайт постоянно вылетает, с ним явно что-то не то! 
            //сайтик похоже умер =(
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            return parsedProxy;

            //try
            //{
            //    Uri? endpoint = null;
            //    HttpRequestMessage? request = null;
            //    HttpResponseMessage? responce = null;
            //    string? content = null;

            //    string? archiveDate = null;

            //    #region request to get archives with dates
            //    endpoint = new Uri($@"https://checkerproxy.net/api/archive");

            //    request = new HttpRequestMessage()
            //    {
            //        Method = HttpMethod.Get,
            //        RequestUri = endpoint
            //    };
            //    request.Headers.Host = endpoint.Host;

            //    foreach (var header in base.contextHeaders!)
            //    {
            //        request.Headers.Add(header.Key, header.Value);
            //    }
            //    request.Headers.Add("referer", @"https://checkerproxy.net/");
            //    request.Headers.Add("Upgrade-Insecure-Requests", "1");

            //    responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            //    content = await responce.Content.ReadAsStringAsync();

            //    if (!responce.IsSuccessStatusCode)
            //    {
            //        throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
            //    }

            //    dynamic? archives = JsonConvert.DeserializeObject(content);

            //    List<string> proxyArchiveDatesParsedStrs = new List<string>();

            //    foreach (var archive in archives!)
            //    {
            //        string dstr = archive.date;
            //        proxyArchiveDatesParsedStrs.Add(dstr);
            //    }

            //    List<DateOnly> proxyArchiveDates = new List<DateOnly>();
            //    IFormatProvider provider = new CultureInfo("en-GB");

            //    foreach (string str in proxyArchiveDatesParsedStrs)
            //    {
            //        DateOnly date;

            //        if (!DateOnly.TryParse(str, provider, DateTimeStyles.None, out date))
            //        {
            //            throw new Exception($"!DateOnly.TryParse(str, provider, DateTimeStyles.None, out date)\n{content}\n{str}");
            //        }

            //        proxyArchiveDates.Add(date);
            //    }

            //    DateOnly bestDate = proxyArchiveDates.Max();

            //    archiveDate = bestDate.ToString("yyyy-MM-dd");

            //    if (string.IsNullOrEmpty(archiveDate))
            //    {
            //        throw new Exception($"string.IsNullOrEmpty(archiveDate)\n{content}");
            //    }

            //    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5));
            //    #endregion

            //    #region request to get json proxy
            //    endpoint = new Uri($@"https://checkerproxy.net/api/archive/{archiveDate}");

            //    request = new HttpRequestMessage()
            //    {
            //        Method = HttpMethod.Get,
            //        RequestUri = endpoint
            //    };
            //    request.Headers.Host = endpoint.Host;

            //    foreach (var header in base.contextHeaders!)
            //    {
            //        request.Headers.Add(header.Key, header.Value);
            //    }
            //    request.Headers.Add("referer", @"https://checkerproxy.net/archive/");

            //    responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            //    content = await responce.Content.ReadAsStringAsync();

            //    if (!responce.IsSuccessStatusCode)
            //    {
            //        throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
            //    }
            //    #endregion

            //    #region parse proxy from json content
            //    dynamic? proxies = JsonConvert.DeserializeObject(content);

            //    if (proxies == null)
            //    {
            //        throw new Exception($"proxies == null\n{content}");
            //    }

            //    foreach (var proxy in proxies)
            //    {
            //        string proxyAddr = proxy.addr;

            //        if (!proxyAddr.Contains(':'))
            //        {
            //            continue;
            //        }

            //        string[] data = proxyAddr.Split(':');
            //        string ip = data[0];
            //        string port = data[1];

            //        parsedProxy.Add(new ParsedProxy(ip, port));
            //    }

            //    if (parsedProxy.Count == 0)
            //    {
            //        throw new Exception($"parsedProxy.Count == 0\n{content}");
            //    }
            //    #endregion

            //    endpoint = null;
            //    request = null;
            //    responce = null;
            //    content = null;

            //    proxies = null;
            //}
            //catch (Exception e)
            //{
            //    throw new Exception($"class ProxyCombinerTask(), method ParseSite_CheckerproxyDotNet(), error {e.Message}");
            //}

            //return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_XseoDotIn()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                FormUrlEncodedContent? formContent = null;
                IDocument? htmlPageDocument = null;

                string url = @"https://xseo.in/freeproxy";
                endpoint = new Uri(url);

                Dictionary<string, string> keyValuePairsForContent = new Dictionary<string, string>()
                {
                    { "submit", "Показать по 150 прокси на странице" }
                };

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = endpoint,
                    Content = (formContent = new FormUrlEncodedContent(keyValuePairsForContent))
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://xseo.in/freeproxy");
                request.Headers.Add("origin", @"https://xseo.in");
                request.Headers.Add("upgrade-insecure-requests", "1");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var proxy_trs_collection_one = htmlPageDocument.QuerySelectorAll("tr[class='cls8']");
                var proxy_trs_collection_two = htmlPageDocument.QuerySelectorAll("tr[class='cls81']");

                var proxy_trs_collection = proxy_trs_collection_one.Concat(proxy_trs_collection_two);

                if (proxy_trs_collection == null || proxy_trs_collection.Count() <= 1)
                {
                    throw new Exception($"proxy_trs_collection == null || proxy_trs_collection.Count() <= 1\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);

                foreach (var proxy in proxy_trs_collection)
                {
                    if (proxy.QuerySelectorAll("td[colspan=\"1\"]").Count() == 0)
                    {
                        continue;
                    }

                    string? addrTextContent = proxy.QuerySelectorAll("td")[0].QuerySelector("font")?.TextContent;

                    if (String.IsNullOrEmpty(addrTextContent))
                    {
                        continue;
                    }

                    MatchCollection matchesProxy = regexProxy.Matches(addrTextContent);
                    if (matchesProxy.Count == 0)
                    {
                        continue;
                    }

                    string[] data = matchesProxy[0].Value.Split(':');
                    string ip = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                formContent = null;
                htmlPageDocument = null;

                proxy_trs_collection = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_XseoDotIn(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_MarcosblDotCom()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                //string url = @"https://www.marcosbl.com/lab/proxies/";
                string url = @"https://marcosbl.com/lab/proxies/";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://google.com");
                request.Headers.Add("upgrade-insecure-requests", "1");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var proxy_trs_collection = htmlPageDocument.QuerySelectorAll("table > tbody > tr");

                if (proxy_trs_collection == null || proxy_trs_collection.Count() <= 1)
                {
                    throw new Exception($"proxy_trs_collection == null || proxy_trs_collection.Count() <= 1\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);

                foreach (var proxy in proxy_trs_collection)
                {
                    bool? check = proxy.QuerySelector("td[id]")?.GetAttribute("id")?.Contains("proxy") == true ? true : false;
                    if (check == null || check == false)
                    {
                        continue;
                    }

                    string? addrTextContent = proxy.QuerySelectorAll("td")[1]?.TextContent;

                    if (String.IsNullOrEmpty(addrTextContent))
                    {
                        continue;
                    }

                    MatchCollection matchesProxy = regexProxy.Matches(addrTextContent);
                    if (matchesProxy.Count == 0)
                    {
                        continue;
                    }

                    string[] data = matchesProxy[0].Value.Split(':');
                    string ip = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                proxy_trs_collection = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_MarcosblDotCom(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_IpaddressDotCom()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                string url = @"https://www.ipaddress.com/proxy-list/";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://google.com");
                request.Headers.Add("upgrade-insecure-requests", "1");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var proxy_trs_collection = htmlPageDocument.QuerySelectorAll("table.proxylist > tbody > tr");

                if (proxy_trs_collection == null || proxy_trs_collection.Count() <= 1)
                {
                    throw new Exception($"proxy_trs_collection == null || proxy_trs_collection.Count() <= 1\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);

                foreach (var proxy in proxy_trs_collection)
                {
                    if (proxy.QuerySelectorAll("td").Count() != 4)
                    {
                        continue;
                    }

                    string? addrTextContent = proxy.QuerySelectorAll("td")[0]?.TextContent;

                    if (String.IsNullOrEmpty(addrTextContent))
                    {
                        continue;
                    }

                    MatchCollection matchesProxy = regexProxy.Matches(addrTextContent);
                    if (matchesProxy.Count == 0)
                    {
                        continue;
                    }

                    string[] data = matchesProxy[0].Value.Split(':');
                    string ip = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                proxy_trs_collection = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_IpaddressDotCom(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_HttptunnelDotGe()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                string url = @"http://www.httptunnel.ge/ProxyListForFree.aspx";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://google.com");
                request.Headers.Add("upgrade-insecure-requests", "1");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var proxy_trs_collection = htmlPageDocument.QuerySelectorAll("table[id='ctl00_ContentPlaceHolder1_GridViewNEW'] > tbody > tr");

                if (proxy_trs_collection == null || proxy_trs_collection.Count() <= 1)
                {
                    // сайт иногда выдает пару проксей)
                    //throw new Exception($"proxy_trs_collection == null || proxy_trs_collection.Count() <= 1\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);

                foreach (var proxy in proxy_trs_collection!)
                {
                    if (proxy.QuerySelectorAll("td").Count() == 0)
                    {
                        continue;
                    }

                    string? addrTextContent = proxy.QuerySelectorAll("td")[0]?.QuerySelector("a")?.TextContent;

                    if (String.IsNullOrEmpty(addrTextContent))
                    {
                        continue;
                    }

                    MatchCollection matchesProxy = regexProxy.Matches(addrTextContent);
                    if (matchesProxy.Count == 0)
                    {
                        continue;
                    }

                    string[] data = matchesProxy[0].Value.Split(':');
                    string ip = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                proxy_trs_collection = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_HttptunnelDotGe(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_KuaidailiDotCom()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                string url = new Random().Next(1, 3) switch
                {
                    1 => @"https://www.kuaidaili.com/free/inha/1",
                    2 => @"https://www.kuaidaili.com/free/intr/",
                    _ => @"https://www.kuaidaili.com/free/inha/1"
                };
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://google.com");
                request.Headers.Add("upgrade-insecure-requests", "1");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                string contentWithJsProxy = content;
                string keyElementForIndexOne = "const fpsList = ";
                string keyElementForIndexTwo = "let totalCount";

                int index_to_remove_one = contentWithJsProxy.LastIndexOf(keyElementForIndexOne);
                contentWithJsProxy = contentWithJsProxy.Remove(0, index_to_remove_one + keyElementForIndexOne.Length);

                int index_to_remove_two = contentWithJsProxy.IndexOf(keyElementForIndexTwo);
                contentWithJsProxy = contentWithJsProxy.Remove(index_to_remove_two);

                contentWithJsProxy = contentWithJsProxy.Replace(";", "");

                dynamic? proxies = JsonConvert.DeserializeObject(contentWithJsProxy);

                if (proxies == null)
                {
                    throw new Exception($"proxies == null\n{content}");
                }

                foreach (var proxy in proxies)
                {
                    string ip = proxy.ip;
                    string port = proxy.port;

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                #region Старая реализация, сайт изменился
                //htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                //var proxy_row_trs = htmlPageDocument.QuerySelectorAll("table.table > tbody > tr");

                //if (proxy_row_trs == null || proxy_row_trs.Count() == 0)
                //{
                //    throw new Exception($"proxy_row_trs == null || proxy_row_trs.Count() == 0\n{content}");
                //}

                //Regex regexAddress = new Regex(REIpPattern);
                //Regex regexPort = new Regex(REPortPattern);

                //foreach (var proxy in proxy_row_trs)
                //{
                //    if (proxy.QuerySelectorAll("td[data-title='IP'], td[data-title='PORT']").Count() == 0)
                //    {
                //        continue;
                //    }

                //    var cellAddressTextContent = proxy.QuerySelector("td[data-title='IP']")?.TextContent;
                //    var cellPortTextContent = proxy.QuerySelector("td[data-title='PORT']")?.TextContent;

                //    if (cellAddressTextContent == null || cellPortTextContent == null)
                //    {
                //        throw new Exception($"cellAddressTextContent == null || cellPortTextContent == null\n{content}");
                //    }

                //    MatchCollection matchesAddress = regexAddress.Matches(cellAddressTextContent);
                //    if (matchesAddress.Count != 1)
                //    {
                //        throw new Exception($"matchesAddress.Count != 1\n{content}");
                //    }
                //    string address = matchesAddress[0].Value;

                //    MatchCollection matchesPort = regexPort.Matches(cellPortTextContent);
                //    if (matchesPort.Count != 1)
                //    {
                //        throw new Exception($"matchesPort.Count != 1\n{content}");
                //    }
                //    string port = matchesPort[0].Value;

                //    parsedProxy.Add(new ParsedProxy(address, port));
                //} 
                #endregion

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                //proxy_row_trs = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_KuaidailiDotCom(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_Ip3366DotNet()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;

                string url = new Random().Next(1, 3) switch
                {
                    1 => @"http://www.ip3366.net/free/?stype=1",
                    2 => @"http://www.ip3366.net/free/?stype=2",
                    _ => @"http://www.ip3366.net/free/?stype=1"
                };
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"http://www.ip3366.net/free/");
                request.Headers.Add("upgrade-insecure-requests", "1");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));

                var proxy_row_trs = htmlPageDocument.QuerySelectorAll("table.table > tbody > tr");

                if (proxy_row_trs == null || proxy_row_trs.Count() == 0)
                {
                    throw new Exception($"proxy_row_trs == null || proxy_row_trs.Count() == 0\n{content}");
                }

                Regex regexAddress = new Regex(REIpPattern);
                Regex regexPort = new Regex(REPortPattern);

                foreach (var proxy in proxy_row_trs)
                {
                    if (proxy.QuerySelectorAll("td").Count() != 7)
                    {
                        continue;
                    }

                    var cellAddressTextContent = proxy.QuerySelectorAll("td")[0].TextContent;
                    var cellPortTextContent = proxy.QuerySelectorAll("td")[1].TextContent;

                    if (cellAddressTextContent == null || cellPortTextContent == null)
                    {
                        throw new Exception($"cellAddressTextContent == null || cellPortTextContent == null\n{content}");
                    }

                    MatchCollection matchesAddress = regexAddress.Matches(cellAddressTextContent);
                    if (matchesAddress.Count != 1)
                    {
                        throw new Exception($"matchesAddress.Count != 1\n{content}");
                    }
                    string address = matchesAddress[0].Value;

                    MatchCollection matchesPort = regexPort.Matches(cellPortTextContent);
                    if (matchesPort.Count != 1)
                    {
                        throw new Exception($"matchesPort.Count != 1\n{content}");
                    }
                    string port = matchesPort[0].Value;

                    parsedProxy.Add(new ParsedProxy(address, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                proxy_row_trs = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_Ip3366DotNet(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_CybersyndromeDotNet()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                IDocument? htmlPageDocument = null;
                Jint.Native.Array.ArrayInstance? proxies = null;

                #region request to get html page with js func
                string url = new Random().Next(1, 3) switch
                {
                    1 => @"https://www.cybersyndrome.net/pla6.html",
                    2 => @"https://www.cybersyndrome.net/pld6.html",
                    _ => @"https://www.cybersyndrome.net/pla6.html"
                };
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                request.Headers.Add("referer", @"https://www.cybersyndrome.net/");

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                htmlPageDocument = await new BrowsingContext().OpenAsync(req => req.Content(content));
                #endregion

                #region deobfuscate js func and get proxy array
                string? obfuscatedProxyFunction = null;
                foreach (var script in htmlPageDocument.Scripts)
                {
                    if (script.Text.Contains("var as"))
                    {
                        obfuscatedProxyFunction = script.Text;
                        break;
                    }
                }
                if (obfuscatedProxyFunction == null)
                {
                    throw new Exception($"obfuscatedProxyFunction == null\n{content}");
                }

                obfuscatedProxyFunction = obfuscatedProxyFunction.Replace("<!--", "").Replace("//-->", "").Trim();
                obfuscatedProxyFunction = obfuscatedProxyFunction.Replace("document.getElementById('n'+(1+j)).innerHTML=addrs[j]+\":\"+ps[j];", "CybersyndromeDotNet_proxy_array[j] = addrs[j] + \":\" + ps[j];");
                base.jsExecuteContextEngine!.Execute("var CybersyndromeDotNet_proxy_array = [];");
                base.jsExecuteContextEngine!.Execute(obfuscatedProxyFunction);

                proxies = base.jsExecuteContextEngine!.Evaluate("CybersyndromeDotNet_proxy_array").AsArray();

                if (proxies == null || proxies.Count() == 0)
                {
                    throw new Exception($"proxies == null || proxies.GetLength() == 0\n{content}");
                }
                #endregion

                #region parse proxy from html page
                Regex regexProxy = new Regex(REProxyAddressPattern);

                foreach (var proxy in proxies)
                {
                    MatchCollection matchesProxy = regexProxy.Matches(proxy.AsString());
                    if (matchesProxy.Count == 0)
                    {
                        continue;
                    }

                    string[] data = matchesProxy[0].Value.Split(':');
                    string ip = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }
                #endregion

                endpoint = null;
                request = null;
                responce = null;
                content = null;
                htmlPageDocument = null;

                proxies = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_CybersyndromeDotNet(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GithubDotComHookzofSocks5_list()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                string url = @"https://raw.githubusercontent.com/hookzof/socks5_list/master/proxy.txt";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);

                MatchCollection? matchesProxy = regexProxy.Matches(content);
                if (matchesProxy == null || matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
                }

                foreach (Match proxy in matchesProxy)
                {
                    if (!proxy.Value.Contains(":"))
                    {
                        continue;
                    }

                    string[] data = proxy.Value.Split(':');
                    string ip = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_GithubDotComHookzofSocks5_list(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GithubDotComSunny9577Proxy_scraper()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                string url = @"https://sunny9577.github.io/proxy-scraper/proxies.txt";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);

                MatchCollection? matchesProxy = regexProxy.Matches(content);

                if (matchesProxy == null || matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
                }

                foreach (Match proxy in matchesProxy)
                {
                    if (!proxy.Value.Contains(":"))
                    {
                        continue;
                    }

                    string[] data = proxy.Value.Split(':');
                    string ip = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_GithubDotComSunny9577Proxy_scraper(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GithubDotComRoosterkidOpenproxylist()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                string url = new Random().Next(1, 4) switch
                {
                    1 => @"https://raw.githubusercontent.com/roosterkid/openproxylist/main/HTTPS_RAW.txt",
                    2 => @"https://raw.githubusercontent.com/roosterkid/openproxylist/main/SOCKS4_RAW.txt",
                    3 => @"https://raw.githubusercontent.com/roosterkid/openproxylist/main/SOCKS5_RAW.txt",
                    _ => @"https://raw.githubusercontent.com/roosterkid/openproxylist/main/HTTPS_RAW.txt"
                };
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);

                MatchCollection? matchesProxy = regexProxy.Matches(content);

                if (matchesProxy == null || matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
                }

                foreach (Match proxy in matchesProxy)
                {
                    if (!proxy.Value.Contains(":"))
                    {
                        continue;
                    }

                    string[] data = proxy.Value.Split(':');
                    string ip = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_GithubDotComRoosterkidOpenproxylist(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GithubDotComMuRongPIGProxy_Master()
        {
            // прокси на этом гите не обновлялись уже месяц( пока не используем
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            //try
            //{
            //    Uri? endpoint = null;
            //    HttpRequestMessage? request = null;
            //    HttpResponseMessage? responce = null;
            //    string? content = null;

            //    string url = new Random().Next(1, 4) switch
            //    {
            //        1 => @"https://raw.githubusercontent.com/MuRongPIG/Proxy-Master/main/http.txt",
            //        2 => @"https://raw.githubusercontent.com/MuRongPIG/Proxy-Master/main/socks4.txt",
            //        3 => @"https://raw.githubusercontent.com/MuRongPIG/Proxy-Master/main/socks5.txt",
            //        _ => @"https://raw.githubusercontent.com/MuRongPIG/Proxy-Master/main/http.txt"
            //    };
            //    endpoint = new Uri(url);

            //    request = new HttpRequestMessage()
            //    {
            //        Method = HttpMethod.Get,
            //        RequestUri = endpoint,
            //    };
            //    request.Headers.Host = endpoint.Host;

            //    foreach (var header in base.contextHeaders!)
            //    {
            //        request.Headers.Add(header.Key, header.Value);
            //    }

            //    responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            //    content = await responce.Content.ReadAsStringAsync();

            //    if (!responce.IsSuccessStatusCode)
            //    {
            //        throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
            //    }

            //    Regex regexProxy = new Regex(REProxyAddressPattern);

            //    MatchCollection? matchesProxy = regexProxy.Matches(content);

            //    if (matchesProxy == null || matchesProxy.Count == 0)
            //    {
            //        throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
            //    }

            //    foreach (Match proxy in matchesProxy)
            //    {
            //        if (!proxy.Value.Contains(":"))
            //        {
            //            continue;
            //        }

            //        string[] data = proxy.Value.Split(':');
            //        string ip = data[0];
            //        string port = data[1];

            //        parsedProxy.Add(new ParsedProxy(ip, port));
            //    }

            //    endpoint = null;
            //    request = null;
            //    responce = null;
            //    content = null;

            //    matchesProxy = null;
            //}
            //catch (Exception e)
            //{
            //    throw new Exception($"class ProxyCombinerTask(), method ParseSite_GithubDotComMuRongPIGProxy_Master(), error {e.Message}");
            //}

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GithubDotComPrxchkProxy_list()
        {
            //прокси на этом гите не обновлялись уже 4 месяца (пока не используем
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            //try
            //{
            //    Uri? endpoint = null;
            //    HttpRequestMessage? request = null;
            //    HttpResponseMessage? responce = null;
            //    string? content = null;

            //    string url = @"https://raw.githubusercontent.com/prxchk/proxy-list/main/all.txt";
            //    endpoint = new Uri(url);

            //    request = new HttpRequestMessage()
            //    {
            //        Method = HttpMethod.Get,
            //        RequestUri = endpoint,
            //    };
            //    request.Headers.Host = endpoint.Host;

            //    foreach (var header in base.contextHeaders!)
            //    {
            //        request.Headers.Add(header.Key, header.Value);
            //    }

            //    responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            //    content = await responce.Content.ReadAsStringAsync();

            //    if (!responce.IsSuccessStatusCode)
            //    {
            //        throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
            //    }

            //    Regex regexProxy = new Regex(REProxyAddressPattern);

            //    MatchCollection? matchesProxy = regexProxy.Matches(content);

            //    if (matchesProxy == null || matchesProxy.Count == 0)
            //    {
            //        throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
            //    }

            //    foreach (Match proxy in matchesProxy)
            //    {
            //        if (!proxy.Value.Contains(":"))
            //        {
            //            continue;
            //        }

            //        string[] data = proxy.Value.Split(':');
            //        string ip = data[0];
            //        string port = data[1];

            //        parsedProxy.Add(new ParsedProxy(ip, port));
            //    }

            //    endpoint = null;
            //    request = null;
            //    responce = null;
            //    content = null;

            //    matchesProxy = null;
            //}
            //catch (Exception e)
            //{
            //    throw new Exception($"class ProxyCombinerTask(), method ParseSite_GithubDotComPrxchkProxy_list(), error {e.Message}");
            //}

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GithubDotComErcinDedeogluProxies()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                string url = new Random().Next(1, 4) switch
                {
                    1 => @"https://raw.githubusercontent.com/ErcinDedeoglu/proxies/main/proxies/http.txt",
                    2 => @"https://raw.githubusercontent.com/ErcinDedeoglu/proxies/main/proxies/https.txt",
                    3 => @"https://raw.githubusercontent.com/ErcinDedeoglu/proxies/main/proxies/socks4.txt",
                    4 => @"https://raw.githubusercontent.com/ErcinDedeoglu/proxies/main/proxies/socks5.txt",
                    _ => @"https://raw.githubusercontent.com/ErcinDedeoglu/proxies/main/proxies/http.txt"
                };
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);

                MatchCollection? matchesProxy = regexProxy.Matches(content);

                if (matchesProxy == null || matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
                }

                foreach (Match proxy in matchesProxy)
                {
                    if (!proxy.Value.Contains(":"))
                    {
                        continue;
                    }

                    string[] data = proxy.Value.Split(':');
                    string ip = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_GithubDotComErcinDedeogluProxies(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GithubDotComOfficialputuidKangProxy()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                string url = new Random().Next(1, 4) switch
                {
                    1 => @"https://raw.githubusercontent.com/officialputuid/KangProxy/KangProxy/http/http.txt",
                    2 => @"https://raw.githubusercontent.com/officialputuid/KangProxy/KangProxy/https/https.txt",
                    3 => @"https://raw.githubusercontent.com/officialputuid/KangProxy/KangProxy/socks4/socks4.txt",
                    4 => @"https://raw.githubusercontent.com/officialputuid/KangProxy/KangProxy/socks5/socks5.txt",
                    _ => @"https://raw.githubusercontent.com/officialputuid/KangProxy/KangProxy/http/http.txt"
                };
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);

                MatchCollection? matchesProxy = regexProxy.Matches(content);

                if (matchesProxy == null || matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
                }

                foreach (Match proxy in matchesProxy)
                {
                    if (!proxy.Value.Contains(":"))
                    {
                        continue;
                    }

                    string[] data = proxy.Value.Split(':');
                    string ip = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_GithubDotComOfficialputuidKangProxy(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GithubDotComZloi_userHideipDotMe()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                string url = new Random().Next(1, 4) switch
                {
                    1 => @"https://raw.githubusercontent.com/zloi-user/hideip.me/main/http.txt",
                    2 => @"https://raw.githubusercontent.com/zloi-user/hideip.me/main/https.txt",
                    3 => @"https://raw.githubusercontent.com/zloi-user/hideip.me/main/socks4.txt",
                    4 => @"https://raw.githubusercontent.com/zloi-user/hideip.me/main/socks5.txt",
                    _ => @"https://raw.githubusercontent.com/zloi-user/hideip.me/main/http.txt"
                };
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);

                MatchCollection? matchesProxy = regexProxy.Matches(content);

                if (matchesProxy == null || matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
                }

                foreach (Match proxy in matchesProxy)
                {
                    if (!proxy.Value.Contains(":"))
                    {
                        continue;
                    }

                    string[] data = proxy.Value.Split(':');
                    string ip = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_GithubDotComZloi_userHideipDotMe(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GithubDotComProxiflyFree_proxy_list()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                string url = @"https://raw.githubusercontent.com/proxifly/free-proxy-list/main/proxies/all/data.txt";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);

                MatchCollection? matchesProxy = regexProxy.Matches(content);

                if (matchesProxy == null || matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
                }

                foreach (Match proxy in matchesProxy)
                {
                    if (!proxy.Value.Contains(":"))
                    {
                        continue;
                    }

                    string[] data = proxy.Value.Split(':');
                    string ip = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_GithubDotComProxiflyFree_proxy_list(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GithubDotComObcbOGetproxy()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                string url = @"https://raw.githubusercontent.com/ObcbO/getproxy/master/file/all.txt";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);

                MatchCollection? matchesProxy = regexProxy.Matches(content);

                if (matchesProxy == null || matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
                }

                foreach (Match proxy in matchesProxy)
                {
                    if (!proxy.Value.Contains(":"))
                    {
                        continue;
                    }

                    string[] data = proxy.Value.Split(':');
                    string ip = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_GithubDotComObcbOGetproxy(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GithubDotComAnonym0usWork1221Free_Proxies()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                string url = new Random().Next(1, 4) switch
                {
                    1 => @"https://raw.githubusercontent.com/Anonym0usWork1221/Free-Proxies/main/proxy_files/http_proxies.txt",
                    2 => @"https://raw.githubusercontent.com/Anonym0usWork1221/Free-Proxies/main/proxy_files/https_proxies.txt",
                    3 => @"https://raw.githubusercontent.com/Anonym0usWork1221/Free-Proxies/main/proxy_files/socks4_proxies.txt",
                    4 => @"https://raw.githubusercontent.com/Anonym0usWork1221/Free-Proxies/main/proxy_files/socks5_proxies.txt",
                    _ => @"https://raw.githubusercontent.com/Anonym0usWork1221/Free-Proxies/main/proxy_files/http_proxies.txt"
                };
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);

                MatchCollection? matchesProxy = regexProxy.Matches(content);

                if (matchesProxy == null || matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
                }

                foreach (Match proxy in matchesProxy)
                {
                    if (!proxy.Value.Contains(":"))
                    {
                        continue;
                    }

                    string[] data = proxy.Value.Split(':');
                    string ip = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_GithubDotComAnonym0usWork1221Free_Proxies(), error {e.Message}");
            }

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GithubDotComCasals_arProxy_list()
        {
            // прокси на этом гите не обновлялись уже 7 месяца (пока не используем
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            //try
            //{
            //    Uri? endpoint = null;
            //    HttpRequestMessage? request = null;
            //    HttpResponseMessage? responce = null;
            //    string? content = null;

            //    string url = new Random().Next(1, 4) switch
            //    {
            //        1 => @"https://raw.githubusercontent.com/casals-ar/proxy-list/main/http",
            //        2 => @"https://raw.githubusercontent.com/casals-ar/proxy-list/main/https",
            //        3 => @"https://raw.githubusercontent.com/casals-ar/proxy-list/main/socks4",
            //        4 => @"https://raw.githubusercontent.com/casals-ar/proxy-list/main/socks5",
            //        _ => @"https://raw.githubusercontent.com/casals-ar/proxy-list/main/http"
            //    };
            //    endpoint = new Uri(url);

            //    request = new HttpRequestMessage()
            //    {
            //        Method = HttpMethod.Get,
            //        RequestUri = endpoint,
            //    };
            //    request.Headers.Host = endpoint.Host;

            //    foreach (var header in base.contextHeaders!)
            //    {
            //        request.Headers.Add(header.Key, header.Value);
            //    }

            //    responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            //    content = await responce.Content.ReadAsStringAsync();

            //    if (!responce.IsSuccessStatusCode)
            //    {
            //        throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
            //    }

            //    Regex regexProxy = new Regex(REProxyAddressPattern);

            //    MatchCollection? matchesProxy = regexProxy.Matches(content);

            //    if (matchesProxy == null || matchesProxy.Count == 0)
            //    {
            //        throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
            //    }

            //    foreach (Match proxy in matchesProxy)
            //    {
            //        if (!proxy.Value.Contains(":"))
            //        {
            //            continue;
            //        }

            //        string[] data = proxy.Value.Split(':');
            //        string ip = data[0];
            //        string port = data[1];

            //        parsedProxy.Add(new ParsedProxy(ip, port));
            //    }

            //    endpoint = null;
            //    request = null;
            //    responce = null;
            //    content = null;

            //    matchesProxy = null;
            //}
            //catch (Exception e)
            //{
            //    throw new Exception($"class ProxyCombinerTask(), method ParseSite_GithubDotComCasals_arProxy_list(), error {e.Message}");
            //}

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GithubDotComProxy4parsingProxy_list()
        {
            //прокси на этом гите не обновлялись уже 4 месяца (пока не используем
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            //try
            //{
            //    Uri? endpoint = null;
            //    HttpRequestMessage? request = null;
            //    HttpResponseMessage? responce = null;
            //    string? content = null;

            //    string url = @"https://raw.githubusercontent.com/proxy4parsing/proxy-list/main/http.txt";
            //    endpoint = new Uri(url);

            //    request = new HttpRequestMessage()
            //    {
            //        Method = HttpMethod.Get,
            //        RequestUri = endpoint,
            //    };
            //    request.Headers.Host = endpoint.Host;

            //    foreach (var header in base.contextHeaders!)
            //    {
            //        request.Headers.Add(header.Key, header.Value);
            //    }

            //    responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            //    content = await responce.Content.ReadAsStringAsync();

            //    if (!responce.IsSuccessStatusCode)
            //    {
            //        throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
            //    }

            //    Regex regexProxy = new Regex(REProxyAddressPattern);

            //    MatchCollection? matchesProxy = regexProxy.Matches(content);

            //    if (matchesProxy == null || matchesProxy.Count == 0)
            //    {
            //        throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
            //    }

            //    foreach (Match proxy in matchesProxy)
            //    {
            //        if (!proxy.Value.Contains(":"))
            //        {
            //            continue;
            //        }

            //        string[] data = proxy.Value.Split(':');
            //        string ip = data[0];
            //        string port = data[1];

            //        parsedProxy.Add(new ParsedProxy(ip, port));
            //    }

            //    endpoint = null;
            //    request = null;
            //    responce = null;
            //    content = null;

            //    matchesProxy = null;
            //}
            //catch (Exception e)
            //{
            //    throw new Exception($"class ProxyCombinerTask(), method ParseSite_GithubDotComProxy4parsingProxy_list(), error {e.Message}");
            //}

            return parsedProxy;
        }
        private async Task<List<ParsedProxy>> ParseSite_GithubDotComMmpx12Proxy_list()
        {
            List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            try
            {
                Uri? endpoint = null;
                HttpRequestMessage? request = null;
                HttpResponseMessage? responce = null;
                string? content = null;

                string url = @"https://raw.githubusercontent.com/mmpx12/proxy-list/master/proxies.txt";
                endpoint = new Uri(url);

                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint,
                };
                request.Headers.Host = endpoint.Host;

                foreach (var header in base.contextHeaders!)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                responce = await ProxyParseHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"!responce.IsSuccessStatusCode\n{content}");
                }

                Regex regexProxy = new Regex(REProxyAddressPattern);

                MatchCollection? matchesProxy = regexProxy.Matches(content);

                if (matchesProxy == null || matchesProxy.Count == 0)
                {
                    throw new Exception($"matchesProxy == null || matchesProxy.Count == 0\n{content}");
                }

                foreach (Match proxy in matchesProxy)
                {
                    if (!proxy.Value.Contains(":"))
                    {
                        continue;
                    }

                    string[] data = proxy.Value.Split(':');
                    string ip = data[0];
                    string port = data[1];

                    parsedProxy.Add(new ParsedProxy(ip, port));
                }

                endpoint = null;
                request = null;
                responce = null;
                content = null;

                matchesProxy = null;
            }
            catch (Exception e)
            {
                throw new Exception($"class ProxyCombinerTask(), method ParseSite_GithubDotComMmpx12Proxy_list(), error {e.Message}");
            }

            return parsedProxy;
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
