namespace Server.Database.Models
{
    public class ZarplataDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string ClientsCollectionName { get; set; } = null!;
        public string SocpublicAccountsCollectionName { get; set; } = null!;
        public string WithdrawalOfMoneyCollectionName { get; set; } = null!;
        public string BlockedMachinesCollectionName { get; set; } = null!;
        public string UsersCollectionName { get; set; } = null!;
        public string UserSessionsCollectionName { get; set; } = null!;
        public string EnvironmentProxiesCollectionName { get; set; } = null!;
        public string AccountReservedProxiesCollectionName { get; set; } = null!;
        public string SiteParseBalancerCollectionName { get; set; } = null!;
        public string EarnSiteTasksCollectionName { get; set; } = null!;
        public string ProxyTasksCollectionName { get; set; } = null!;
        public string ProxyTasksErrorsLogCollectionName { get; set; } = null!;
        public string RunTasksSettingsCollectionName { get; set; } = null!;
        public string PlatformInternalAccountTasksCollectionName { get; set; } = null!;
    }
}
