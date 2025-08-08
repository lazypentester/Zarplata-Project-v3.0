using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.UserAgentClasses.UserAgentEnums;

namespace CommonModels.UserAgentClasses
{
    public class UserAgent
    {
        public string? Useragent { get; set; } = null;

        public UserAgent()
        {

        }
        public UserAgent(string useragent)
        {
            Useragent = useragent;
        }

        public static string? CreateRandom()
        {
            string? ua = null;

            try
            {
                Random random = new Random();
                var product = Product.products.First();
                var platform = Platform.platforms[random.Next(0, Platform.platforms.Length - 1)];
                var gecko = Gecko.geckos[random.Next(0, Platform.platforms.Length - 1)];
                var browser = Browser.browsers[random.Next(0, Platform.platforms.Length - 1)];

                ua = $"{product} ({platform}) {gecko} {browser}";
            }
            catch { }

            return ua;
        }
    }
}
