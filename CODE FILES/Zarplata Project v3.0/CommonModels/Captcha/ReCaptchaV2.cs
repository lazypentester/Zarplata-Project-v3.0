using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.Captcha
{
    public class ReCaptchaV2
    {
        public string? id { get; set; } = null;
        public string? pageUrl { get; set; } = null;
        public string? siteKey { get; set; } = null;
        public string? cookies { get; set; } = null;
        public string? userAgent { get; set; } = null;
        public double? waitTime { get; set; } = 0;
        public string? resolvedCaptchaHash { get; set; } = null;
        public CaptchaStatus? status { get; set; } = CaptchaStatus.Created;
        public string? statusMessage { get; set; } = null;

        public ReCaptchaV2(string? pageUrl, string? siteKey, string? cookies, string? userAgent, double? waitTime = 0)
        {
            this.pageUrl = pageUrl;
            this.siteKey = siteKey;
            this.cookies = cookies;
            this.userAgent = userAgent;
            this.waitTime = waitTime;
        }
    }

    public enum CaptchaStatus
    {
        Created,
        inProgress,
        Solved,
        Error
    }
}
