using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Server.Database.Services;
using Server.Hubs;
using Server.Tokens;
using ServerExtensions;
using CommonModels;
using Server.Attributes.Authorization;
using CommonModels.Client;
using CommonModels.Client.Models;
using CommonModels.ProjectTask.ProxyCombiner.Models;
using static CommonModels.ProjectTask.ProxyCombiner.Models.EnvironmentProxy;
using System.Security.Claims;
using static CommonModels.Client.Client;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IOptions<AuthOptions> authOptions;
        private readonly ClientsService clientsService;
        private readonly EnvironmentProxiesService environmentProxiesService;
        private readonly BlockedMachinesService blockedMachinesService;

        private readonly IHubContext<ClientHub> clientHubContext;
        private readonly IHubContext<ManagementHub> managementHubContext;

        public ClientController(IOptions<AuthOptions> authOptions, ClientsService clientsService, EnvironmentProxiesService environmentProxiesService, BlockedMachinesService blockedMachinesService, IHubContext<ClientHub> clientHubContext, IHubContext<ManagementHub> managementHubContext)
        {
            this.authOptions = authOptions;
            this.clientsService = clientsService;
            this.environmentProxiesService = environmentProxiesService;
            this.blockedMachinesService = blockedMachinesService;

            this.clientHubContext = clientHubContext;
            this.managementHubContext = managementHubContext;
        }

        [HttpPost]
        [Route("session/create")]
        public async Task<IActionResult> CreateClientSession([FromBody] ModelClient client)
        {
            var ip = HttpContext.GetRemoteIPAddress();
            if (ip == null)
                //return StatusCode(405, HttpContextExtensions.BrokenHeaders);
                return StatusCode(405, $"Unable to detect your ip address.");
            client.IP = ip.ToString();

            // проверка на заблокированный комп.
            if(await CheckBotOnBlock(client))
            {
                return StatusCode(400, "You are blocked.");
            }

            client.RegistrationDateTime = DateTime.UtcNow;

            // добавление нового клиента в бд
            Task create = clientsService.CreateAsync(client);
            try
            {
                await create;
            }
            catch (Exception e)
            {
                return StatusCode(400, e.Message);
            }
            create.Dispose();

            // создание токена клиента
            string Token = "";
            try
            {
                Token = AccessToken.GenerateJWTForBotClient(client, authOptions.Value.BotClientTokenLifetime, authOptions);
            }
            catch (Exception e)
            {
                return StatusCode(400, e.Message);
            }

            // уведомление всех менеджерский приложений о появлении нового бота // переместить в слент сервис при подключении с токеном?
            try
            {
                await managementHubContext.Clients.All.SendAsync("new_client_bot_connected", client);
            }
            catch { }

            return Ok(new
            {
                id = client.ID,
                ip = client.IP,
                token = Token
            });
        }

        [HttpPost]
        [Route("session/renew")]
        [Authorize(Roles = $"{nameof(BotRole.EarningSiteBot)}, {nameof(BotRole.ProxyCombineBot)}, {nameof(BotRole.BotManager)}")]
        [ControllerAuthorizeIP]
        public async Task<IActionResult> RenewClientSession([FromBody] ModelClient client)
        {
            var ip = HttpContext.GetRemoteIPAddress();
            if (ip == null)
                return StatusCode(405, $"Unable to detect your ip address.");
            if (ip.ToString() != client.IP)
                return StatusCode(405, $"Access denied.");

            // проверка на заблокированный комп.
            if (await CheckBotOnBlock(client))
            {
                return StatusCode(400, "You are blocked.");
            }

            // создание токена клиента
            string Token = AccessToken.GenerateJWTForBotClient(client, 86400, authOptions);

            return Ok(new
            {
                token = Token
            });
        }

        [HttpGet]
        [Route("environmentproxy")]
        [Authorize(Roles = $"{nameof(BotRole.ProxyCombineBot)}, {nameof(BotRole.BotManager)}")]
        [ControllerAuthorizeIP]
        public async Task<IActionResult> GetEnvironmentProxy()
        {
            // получние проксей из базы данных
            List<EnvironmentProxy> environmentProxies;

            Task<List<EnvironmentProxy>> getProxies = environmentProxiesService.GetAsync();
            try
            {
                environmentProxies = await getProxies;
            }
            catch (Exception e)
            {
                return StatusCode(400, e.Message);
            }
            getProxies.Dispose();

            return Ok(new
            {
                environmentProxies
            });
        }

        [HttpGet]
        [Route("environmentproxy/{marker}")]
        [Authorize(Roles = $"{nameof(BotRole.ProxyCombineBot)}, {nameof(BotRole.BotManager)}")]
        [ControllerAuthorizeIP]
        public async Task<IActionResult> GetEnvironmentProxy([FromRoute] EPMarker marker)
        {
            // получние проксей из базы данных
            List<EnvironmentProxy> environmentProxies;

            Task<List<EnvironmentProxy>> getProxies = environmentProxiesService.GetAsync(marker);
            try
            {
                environmentProxies = await getProxies;
            }
            catch (Exception e)
            {
                return StatusCode(400, e.Message);
            }
            getProxies.Dispose();

            return Ok(environmentProxies);
        }

        private async Task<bool> CheckBotOnBlock(ModelClient client)
        {
            // проверка ип и ключа по блок листу в бд, если есть совпадения - возвращать ошибку
            bool botBlocked = false;

            List<ModelBlockedMachine>? blockedMachinesList;
            Task<List<ModelBlockedMachine>> checkOnBlock = blockedMachinesService.GetAsync(client.IP!, client.MACHINE.IDENTITY_KEY!);
            try
            {
                blockedMachinesList = await checkOnBlock;

                if (blockedMachinesList != null && blockedMachinesList.Any())
                {
                    botBlocked = true;
                }
                else
                {
                    botBlocked = false;
                }
            }
            catch
            {
                botBlocked = true;
            }
            checkOnBlock.Dispose();

            return botBlocked;
        }
    }
}
