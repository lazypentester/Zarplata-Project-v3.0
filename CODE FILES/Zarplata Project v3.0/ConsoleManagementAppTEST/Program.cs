
using CommonModels.ProjectTask.Platform.SocpublicCom.Models;
using Microsoft.AspNetCore.SignalR.Client;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using CommonModels.ProjectTask.Platform;
using System.Net.Sockets;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth;
using static CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums;
using SocpublicComTaskSubtype = CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums.SocpublicComTaskSubtype;
using TaskVisitWithoutTimer = CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth.SelectiveTask.TaskVisitWithoutTimer.TaskVisitWithoutTimer;
using CheckCareerLadder = CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth.SelectiveTask.CheckCareerLadder.CheckCareerLadder;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;

//HubConnection serverHubConnection = new HubConnectionBuilder()
//                .WithUrl($"{args[0]}/hubs/management", options =>
//                {
//                    /*options.AccessTokenProvider = () =>
//                    {
//                        return Task.FromResult(client.ReadEnvironmentVariable("TOKEN", EnvironmentVariableTarget.Process));
//                    };*/
//                })
//                .Build();
//await serverHubConnection.StartAsync();


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

//GroupSelectiveTaskWithAuth task = new GroupSelectiveTaskWithAuth("1", TaskFrom.Management, PlatformTaskUrls.SocpublicComUrl.http, "007", account, socpublicComTaskSubtypes)
//{
//    taskVisitWithoutTimer = new TaskVisitWithoutTimer(),
//    checkCareerLadder = new CheckCareerLadder()
//};

//await serverHubConnection.SendAsync("do_new_platform_socpubliccom_task", task);


//Console.WriteLine("Задание отправлено на выполнение");

//serverHubConnection.On<GroupSelectiveTaskWithAuth>("platform_socpubliccom_task_status_changed", (task) =>
//{
//    switch (task.Platform)
//    {
//        case Platform.SocpublicDotCom:
//            Console.WriteLine(task.Id);
//            Console.WriteLine(task.Status.ToString());
//            Console.WriteLine(task.ResultStatus.ToString());
//            Console.WriteLine(task.Account!.History![0].MoneyMainBalancePlus);
//            break;
//    }
//});




Thread.Sleep(Timeout.Infinite);
