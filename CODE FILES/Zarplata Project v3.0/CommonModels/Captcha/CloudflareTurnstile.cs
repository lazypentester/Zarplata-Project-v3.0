using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.Captcha
{
    public class CloudflareTurnstile
    {
        public string? id { get; set; } = null;
        public string? pageUrl { get; set; } = null;
        public string? siteKey { get; set; } = null;
        public double? waitTime { get; set; } = 0;
        public string? resolvedCaptchaHash { get; set; } = null;
        public CaptchaStatus? status { get; set; } = CaptchaStatus.Created;
        public string? statusMessage { get; set; } = null;

        public CloudflareTurnstile(string? pageUrl, string? siteKey, double? waitTime = 0)
        {
            this.pageUrl = pageUrl;
            this.siteKey = siteKey;
            this.waitTime = waitTime;
        }
    }
}
