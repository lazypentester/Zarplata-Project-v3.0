using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.ProxyCombiner.Models
{
    public class SiteParseBalancer
    {
        [BsonId]
        [BsonElement("Id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null;
        public MethodName Site { get; set; }
        public string SiteStringRepresentation { get; set; }
        public DateTime LastTimeParseSite { get; set; }
        public string LastClientIdParseSite { get; set; }
        public int CountParseSite { get; set; }

        public SiteParseBalancer(MethodName site, DateTime lastTimeParseSite, string lastClientIdParseSite, int countParseSite)
        {
            Site = site;
            SiteStringRepresentation = site.ToString();
            LastTimeParseSite = lastTimeParseSite;
            LastClientIdParseSite = lastClientIdParseSite;
            CountParseSite = countParseSite;
        }
    }

    #region Method(Site) Names Enum
    public enum MethodName
    {
        ParseSite_SpysDotOne = 1,
        ParseSite_GeonodeDotCom = 2,
        ParseSite_ProxyscrapeDotCom = 3,
        ParseSite_FreeproxyDotWorld = 4,
        ParseSite_OpenproxyDotSpace = 5,
        ParseSite_AdvancedDotName = 6,
        ParseSite_ProxypremiumDotTop = 7,
        ParseSite_FineproxyDotOrg = 8,
        ParseSite_Free_proxy_listDotNet = 9,
        ParseSite_SslproxiesDotOrg = 10,
        ParseSite_Hide_my_ipDotcom = 11,
        ParseSite_GhostealthDotCom = 12,
        ParseSite_Us_proxyDotOrg = 13,
        ParseSite_My_proxyDotCom = 14,
        ParseSite_Socks_proxyDotNet = 15,
        ParseSite_Google_proxyDotNet = 16,
        ParseSite_SpysDotMe = 17,
        ParseSite_FreeproxyDotlunaproxyDotCom = 18,
        ParseSite_Proxy_dailyDotCom = 19,
        ParseSite_FreeproxylistDotRu = 20,
        ParseSite_ProxyserversDotPro = 21,
        ParseSite_FineproxyDotDe = 22,
        ParseSite_VpnsideDotCom = 23,
        ParseSite_GithubDotComTheSpeedXPROXY_List = 24,
        ParseSite_Proxy11DotCom = 25,
        ParseSite_IproyalDotCom = 26,
        ParseSite_Free_proxy_listDotCom = 27,
        ParseSite_Proxy_listDotOrg = 28,
        ParseSite_Best_proxyDotCom = 29,
        ParseSite_Best_proxiesDotRu = 30,
        ParseSite_ProxynovaDotCom = 31,
        ParseSite_ProxydockerDotCom = 32,
        ParseSite_GoodproxiesDotRu = 33,
        ParseSite_CheckerproxyDotNet = 34,
        ParseSite_XseoDotIn = 35,
        ParseSite_MarcosblDotCom = 36,
        ParseSite_IpaddressDotCom = 37,
        ParseSite_HttptunnelDotGe = 38,
        ParseSite_KuaidailiDotCom = 39,
        ParseSite_Ip3366DotNet = 40,
        ParseSite_CybersyndromeDotNet = 41,
        ParseSite_GithubDotComHookzofSocks5_list = 42,
        ParseSite_GithubDotComSunny9577Proxy_scraper = 43,
        ParseSite_GithubDotComRoosterkidOpenproxylist = 44,
        ParseSite_GithubDotComMuRongPIGProxy_Master = 45,
        ParseSite_GithubDotComPrxchkProxy_list = 46,
        ParseSite_GithubDotComErcinDedeogluProxies = 47,
        ParseSite_GithubDotComOfficialputuidKangProxy = 48,
        ParseSite_GithubDotComZloi_userHideipDotMe = 49,
        ParseSite_GithubDotComProxiflyFree_proxy_list = 50,
        ParseSite_GithubDotComObcbOGetproxy = 51,
        ParseSite_GithubDotComAnonym0usWork1221Free_Proxies = 52,
        ParseSite_GithubDotComCasals_arProxy_list = 53,
        ParseSite_GithubDotComProxy4parsingProxy_list = 54,
        ParseSite_GithubDotComMmpx12Proxy_list = 55
    }
    #endregion
}
