

using CommonModels.ProjectTask.ProxyCombiner.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using Server.Database.Models;
using Server.Database.Services;
using Server.Hubs;
using Server.Hubs.HubFilters;
using Server.Tokens;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.SelectiveTaskWithAuth;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.SpecialCombineTask;
using CommonModels.ProjectTask.EarningSite.SocpublicCom;
using CommonModels.ProjectTask;
using CommonModels.ProjectTask.EarningSite.SocpublicCom.TaskType.GSelectiveTaskWithAuth.SelectiveTask.WithdrawMoney;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

var builder = WebApplication.CreateBuilder(args); // get builder-object

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// db services
BsonClassMap.RegisterClassMap<GroupSelectiveTaskWithAuth>(cm => {
    cm.AutoMap();
    cm.SetDiscriminator("GroupSelectiveTaskWithAuth");
});
BsonClassMap.RegisterClassMap<SocpublicComAutoregTask>(cm => {
    cm.AutoMap();
    cm.SetDiscriminator("SocpublicComAutoregTask");
});
BsonClassMap.RegisterClassMap<WithdrawMoneyGroupSelectiveTaskWithAuth>(cm => {
    cm.AutoMap();
    cm.SetDiscriminator("WithdrawMoneyGroupSelectiveTaskWithAuth");
});
BsonClassMap.RegisterClassMap<CheckTask>(cm => {
    cm.AutoMap();
    cm.SetDiscriminator("CheckTask");
});
BsonClassMap.RegisterClassMap<SpecialCombineTask>(cm => {
    cm.AutoMap();
    cm.SetDiscriminator("SpecialCombineTask");
});
//
var objectSerializer = new ObjectSerializer(ObjectSerializer.AllAllowedTypes);
BsonSerializer.RegisterSerializer(objectSerializer);
//
builder.Services.Configure<ZarplataDatabaseSettings>(
builder.Configuration.GetSection("ZarplataDatabase"));
builder.Services.AddSingleton<ClientsService>();
builder.Services.AddSingleton<SocpublicAccountsService>();
builder.Services.AddSingleton<BlockedMachinesService>();
builder.Services.AddSingleton<UsersService>();
builder.Services.AddSingleton<UserSessionService>();
builder.Services.AddSingleton<EnvironmentProxiesService>();
builder.Services.AddSingleton<AccountReservedProxyService>();
builder.Services.AddSingleton<SiteParseBalancerService>();
builder.Services.AddSingleton<EarnSiteTasksService>();
builder.Services.AddSingleton<EarnSiteTasksManagementService>();
builder.Services.AddSingleton<ClientsManagementService>();
builder.Services.AddSingleton<ProxyTasksService>();
builder.Services.AddSingleton<ProxyTasksErorrsLog>();
builder.Services.AddSingleton<RunTasksSettingsService>();
builder.Services.AddSingleton<PlatformInternalAccountTaskService>();

// auth services
var authOptionsConfiguration = builder.Configuration.GetSection("Auth");
builder.Services.Configure<AuthOptions>(authOptionsConfiguration);

var authOptions = builder.Configuration.GetSection("Auth").Get<AuthOptions>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = authOptions.Issuer,

        ValidateAudience = true,
        ValidAudience = authOptions.Audience,

        ValidateLifetime = true,

        IssuerSigningKey = authOptions.GetSymmetricSecurityKey(),
        ValidateIssuerSigningKey = true
    };
});

//signalr sett
//builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", builder => {
//    builder
//    .AllowAnyMethod()
//    .AllowAnyHeader()
//    .AllowCredentials();
//    //.WithOrigins("http://93.183.125.27");
//})); /// может быть очень важным

// signalR services
builder.Services.AddSignalR();

// signalR ����������� ������� - �������� 'HubAuthorizeIPAttribute'
builder.Services.AddSignalR().AddHubOptions<ClientHub>(options =>
{
    options.MaximumReceiveMessageSize = null;
    options.AddFilter(new HubAuthorizeIPFilter());
});
builder.Services.AddSignalR().AddHubOptions<ManagementHub>(options =>
{
    options.MaximumReceiveMessageSize = null;
    options.AddFilter(new HubAuthorizeIPFilter());
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardLimit = 1;
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    //options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
});

var app = builder.Build(); // get app-object

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ClientHub>("/hubs/client");
app.MapHub<ManagementHub>("/hubs/management");

/*app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<TasksHub>("/tasks");
    endpoints.MapControllers();
});*/

app.Run();
