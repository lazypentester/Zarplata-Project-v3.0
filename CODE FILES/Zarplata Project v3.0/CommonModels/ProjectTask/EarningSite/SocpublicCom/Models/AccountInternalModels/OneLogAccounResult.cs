using CommonModels.Captcha;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models;
using CommonModels.UserAgentClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels
{
    public class OneLogAccounResult
    {
        public CheckedProxy? Proxy { get; set; } = null;
        public UserAgent? Useragent { get; set; } = null;
        public Dictionary<string, List<string>>? Headers { get; set; } = null;
        public List<Cookie>? Cookies { get; set; } = null;
        public DateTime? DateTimeStart { get; set; } = null;
        public DateTime? DateTimeEnd { get; set; } = null;
        public double? TimeOfWorkInSeconds { get; set; } = null;
        public double? MoneyMainBalancePlus { get; set; } = null;
        public double? MoneyMainBalanceMinus { get; set; } = null;
        public double? MoneyAdvertisingBalancePlus { get; set; } = null;
        public double? MoneyAdvertisingBalanceMinus { get; set; } = null;
        public bool? CareerLadderIsChanged { get; set; } = null;
        public CareerLadder? CareerLadder { get; set; } = null;
        public DailyBonus? DailyBonus { get; set; } = null;
        public List<GiftBox>? ReceivedGifts { get; set; } = null;
        public ReCaptchaV2? LoginCaptchaReCaptchaV2 { get; set; } = null;
        public CloudflareTurnstile? LoginCaptchaCloudflareTurnstile { get; set; } = null;
        public List<Operation>? Operations { get; set; } = null;

        public OneLogAccounResult(CheckedProxy? proxy, UserAgent? useragent, Dictionary<string, List<string>>? headers, List<Cookie>? cookies, DateTime? dateTimeStart, ReCaptchaV2? loginCaptchaReCaptchaV2, CloudflareTurnstile? loginCaptchaCloudflareTurnstile, List<Operation>? operations)
        {
            Proxy = proxy;
            Useragent = useragent;
            Headers = headers;
            Cookies = cookies;
            DateTimeStart = dateTimeStart;
            LoginCaptchaReCaptchaV2 = loginCaptchaReCaptchaV2;
            LoginCaptchaCloudflareTurnstile = loginCaptchaCloudflareTurnstile;
            Operations = operations;
        }

        public void SetTimeOfWorkInSeconds()
        {
            TimeOfWorkInSeconds = DateTimeEnd!.Value.Subtract((DateTime)DateTimeStart!).TotalSeconds;
        }
    }
}
