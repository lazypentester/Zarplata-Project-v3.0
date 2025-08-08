using CommonModels.Captcha;
using Newtonsoft.Json;
using System.Net;

namespace Server.ResolveCaptcha.CaptchaGuruRu
{
    public static class CaptchaGuruRu
    {
        private static HttpClient httpClientCaptcha = new HttpClient();
        private static readonly string PERSONAL_API_KEY = "4d9a8fcbca54dbdf6e6470b89f4744a5";
        private static Random randomUrlValue = new Random();


        #region ReCaptchaV2

        public static async Task CreateResolveCaptchaTask(ReCaptchaV2 captcha)
        {
            string? endpointString = null;
            Uri? endpoint = null;
            JsonContent? jsonContent = null;
            HttpResponseMessage? responce = null;
            string? responceContent = null;
            Dictionary<string, string>? resultFromServer = null;


            //endpointString = randomUrlValue.Next(1, 5) switch
            //{
            //    1 => "http://api.cap.guru/in.php",
            //    2 => "http://api2.cap.guru/in.php",
            //    3 => "http://api3.cap.guru/in.php",
            //    4 => "http://api4.cap.guru/in.php",
            //    _ => "http://api.cap.guru/in.php"
            //};
            //endpoint = new Uri(endpointString);
            endpoint = new Uri("http://api2.cap.guru/in.php");
            jsonContent = JsonContent.Create(new
            {
                key = PERSONAL_API_KEY,
                method = "userrecaptcha",
                googlekey = captcha.siteKey,
                pageurl = captcha.pageUrl,
                cookies = captcha.cookies,
                userAgent = captcha.userAgent,
                json = 1
            });

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = endpoint,
                Content = jsonContent
            };

            responce = await httpClientCaptcha.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            responceContent = await responce.Content.ReadAsStringAsync();

            if (!responce.IsSuccessStatusCode)
            {
                throw new Exception($"Не удалось отправить запрос на решение капчи\n{responceContent}");
            }

            resultFromServer = JsonConvert.DeserializeObject<Dictionary<string, string>>(responceContent);
            var status = resultFromServer!.Where(item => item.Key == "status").FirstOrDefault().Value;
            if (status == "1")
            {
                captcha.id = resultFromServer!.Where(item => item.Key == "request").FirstOrDefault().Value;
            }
            else
            {
                throw new Exception($"Не удалось создать задание в сервисе решения капчи\n{responceContent}");
            }

            captcha.status = CaptchaStatus.Created;

            endpointString = null;
            endpoint = null;
            jsonContent = null;
            responce = null;
            responceContent = null;
            resultFromServer = null;
        }

        public static async Task CheckStatusResolveCaptchaTask(ReCaptchaV2 captcha)
        {
            string? endpointString = null;
            Uri? endpoint = null;
            JsonContent? jsonContent = null;
            HttpResponseMessage? responce = null;
            string? responceContent = null;
            Dictionary<string, string>? resultFromServer = null;

            //endpointString = randomUrlValue.Next(1, 5) switch
            //{
            //    1 => "http://api.cap.guru/in.php",
            //    2 => "http://api2.cap.guru/in.php",
            //    3 => "http://api3.cap.guru/in.php",
            //    4 => "http://api4.cap.guru/in.php",
            //    _ => "http://api.cap.guru/in.php"
            //};
            //endpoint = new Uri(endpointString);
            endpoint = new Uri("http://api2.cap.guru/res.php");
            jsonContent = JsonContent.Create(new
            {
                key = PERSONAL_API_KEY,
                action = "get",
                id = captcha.id,
                json = 1
            });

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = endpoint,
                Content = jsonContent
            };

            responce = await httpClientCaptcha.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            responceContent = await responce.Content.ReadAsStringAsync();

            if (!responce.IsSuccessStatusCode)
            {
                throw new Exception($"Не удалось проверить статус решения капчи\n{responceContent}");
            }

            resultFromServer = JsonConvert.DeserializeObject<Dictionary<string, string>>(responceContent);
            var status = resultFromServer!.Where(item => item.Key == "status").FirstOrDefault().Value;
            var req = resultFromServer!.Where(item => item.Key == "request").FirstOrDefault().Value;

            if (status == "1")
            {
                captcha.status = CaptchaStatus.Solved;
                captcha.resolvedCaptchaHash = req;
            }
            else if (status == "0" && req == "CAPCHA_NOT_READY")
            {
                captcha.status = CaptchaStatus.inProgress;
            }
            else
            {
                throw new Exception($"Ошибка при решении капчи на стороне сервиса\n{responceContent}");
            }

            endpoint = null;
            jsonContent = null;
            responce = null;
            responceContent = null;
            resultFromServer = null;
        }

        #endregion

        #region CloudflareTurnstile

        public static async Task CreateResolveCaptchaTask(CloudflareTurnstile captcha)
        {
            string? endpointString = null;
            Uri? endpoint = null;
            JsonContent? jsonContent = null;
            HttpResponseMessage? responce = null;
            string? responceContent = null;
            Dictionary<string, string>? resultFromServer = null;

            endpoint = new Uri("http://api2.cap.guru/in.php");
            jsonContent = JsonContent.Create(new
            {
                key = PERSONAL_API_KEY,
                method = "turnstile",
                sitekey = captcha.siteKey,
                pageurl = captcha.pageUrl,
                json = 1
            });

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = endpoint,
                Content = jsonContent
            };

            responce = await httpClientCaptcha.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            responceContent = await responce.Content.ReadAsStringAsync();

            if (!responce.IsSuccessStatusCode)
            {
                throw new Exception($"Не удалось отправить запрос на решение капчи\n{responceContent}");
            }

            resultFromServer = JsonConvert.DeserializeObject<Dictionary<string, string>>(responceContent);
            var status = resultFromServer!.Where(item => item.Key == "status").FirstOrDefault().Value;
            if (status == "1")
            {
                captcha.id = resultFromServer!.Where(item => item.Key == "request").FirstOrDefault().Value;
            }
            else
            {
                throw new Exception($"Не удалось создать задание в сервисе решения капчи\n{responceContent}");
            }

            captcha.status = CaptchaStatus.Created;

            endpointString = null;
            endpoint = null;
            jsonContent = null;
            responce = null;
            responceContent = null;
            resultFromServer = null;
        }

        public static async Task CheckStatusResolveCaptchaTask(CloudflareTurnstile captcha)
        {
            string? endpointString = null;
            Uri? endpoint = null;
            JsonContent? jsonContent = null;
            HttpResponseMessage? responce = null;
            string? responceContent = null;
            Dictionary<string, string>? resultFromServer = null;

            endpoint = new Uri("http://api2.cap.guru/res.php");
            jsonContent = JsonContent.Create(new
            {
                key = PERSONAL_API_KEY,
                action = "get",
                id = captcha.id,
                json = 1
            });

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = endpoint,
                Content = jsonContent
            };

            responce = await httpClientCaptcha.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            responceContent = await responce.Content.ReadAsStringAsync();

            if (!responce.IsSuccessStatusCode)
            {
                throw new Exception($"Не удалось проверить статус решения капчи\n{responceContent}");
            }

            resultFromServer = JsonConvert.DeserializeObject<Dictionary<string, string>>(responceContent);
            var status = resultFromServer!.Where(item => item.Key == "status").FirstOrDefault().Value;
            var req = resultFromServer!.Where(item => item.Key == "request").FirstOrDefault().Value;

            if (status == "1")
            {
                captcha.status = CaptchaStatus.Solved;
                captcha.resolvedCaptchaHash = req;
            }
            else if (status == "0" && (req == "CAPCHA_NOT_READY" || req.Contains(captcha.siteKey)))
            {
                captcha.status = CaptchaStatus.inProgress;
            }
            else
            {
                throw new Exception($"Ошибка при решении капчи на стороне сервиса\n{responceContent}");
            }

            endpoint = null;
            jsonContent = null;
            responce = null;
            responceContent = null;
            resultFromServer = null;
        }

        #endregion
    }
}
