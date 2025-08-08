
using System.Net;
using Wasmtime;

namespace ConsoleApp1
{
    internal class Program
    {
        private static readonly string REIpPattern = @"(((25[0-5])|(2[0-4][0-9])|(1[0-9][0-9])|([1-9][0-9]|[0-9]))(\.)){3}((25[0-5])|(2[0-4][0-9])|(1[0-9][0-9])|([1-9][0-9]|[0-9]))";
        private static readonly string REPortPattern = @"((6553[0-5])|(655[0-2][0-9])|(65[0-4][0-9][0-9])|(6[0-4][0-9][0-9][0-9])|([1-5][0-9][0-9][0-9][0-9])|([1-9][0-9][0-9][0-9])|([1-9][0-9][0-9])|([1-9][0-9])|([0-9]))";
        private static readonly string REProxyAddressPattern = @$"({REIpPattern}" + @"(\:)" + @$"{REPortPattern})";
        public static Random random = new Random();

        static CookieContainer cookieContainer = new CookieContainer();
        static HttpClientHandler httpClientHandler = new HttpClientHandler()
        {
            CookieContainer = cookieContainer,
            UseCookies = true,
            AutomaticDecompression = DecompressionMethods.All
        };
        static HttpClient ProxyParseHttpClient = new HttpClient(httpClientHandler);
        static Engine jsExecuteContextEngine { get; set; } = new Engine();

        static async Task Main(string[] args)
        {

            #region REMOVE AFTER TEST PROXYPARSE

            //List<ParsedProxy> parsedProxy = new List<ParsedProxy>();

            //var assembly = Assembly.GetExecutingAssembly();
            //string jsProjectFunctions = await ReadResourceFileToString("ProxyCombinerJavaScriptFile.js", assembly);
            //jsExecuteContextEngine!.Execute(jsProjectFunctions);



            #endregion

            #region auto generate acc tasks

            ////dynamic? data_for_auto_gen_tasks = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(@"C:\Users\glebg\Desktop\Zarplata Project v3.0\platform socpublic internal wirthdraw tasks files\json_data_for_auto_generate_auto_acc_tasks.json"));
            //dynamic? data_for_auto_gen_tasks = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(@"C:\Users\glebg\Desktop\Zarplata Project v3.0\platform socpublic internal wirthdraw tasks files\json_data_for_auto_generate_hand_acc_tasks.json"));
            //if (data_for_auto_gen_tasks == null) throw new Exception();

            //List<PlatformInternalAccountTaskModel> autoGenTasks = new List<PlatformInternalAccountTaskModel>();

            //// generate
            //Random random = new Random();
            //int[] repeat_values =
            //{
            //    (int)TimeSpan.FromMinutes(5).TotalSeconds,
            //    (int)TimeSpan.FromMinutes(15).TotalSeconds,
            //    (int)TimeSpan.FromMinutes(30).TotalSeconds,
            //    (int)TimeSpan.FromMinutes(60).TotalSeconds,
            //    (int)TimeSpan.FromMinutes(180).TotalSeconds,
            //    (int)TimeSpan.FromMinutes(360).TotalSeconds,
            //    (int)TimeSpan.FromDays(1).TotalSeconds,
            //    (int)TimeSpan.FromDays(2).TotalSeconds
            //};
            //int[] work_times =
            //{
            //    (int)TimeSpan.FromMinutes(5).TotalSeconds,
            //    (int)TimeSpan.FromMinutes(15).TotalSeconds,
            //    (int)TimeSpan.FromMinutes(30).TotalSeconds,
            //    (int)TimeSpan.FromMinutes(60).TotalSeconds
            //};
            //int[] user_xps =
            //{
            //    50,
            //    55,
            //    60,
            //    65,
            //    70,
            //    75,
            //    80,
            //    85,
            //    90,
            //    95,
            //    100
            //};

            //Dictionary<string, List<string>> sinonims = new Dictionary<string, List<string>>()
            //{
            //    { "сделать", new List<string>() { "сделать", "выполнить", "реализовать", "проделать", } },
            //    { "действия", new List<string>() { "действия", "события", "деяния", } },
            //    { "ваше", new List<string>() { "твое", "твоё", "ваше" } },
            //    { "далее", new List<string>() { "далее", "дальше", } },
            //    { "часы", new List<string>() { "часы", "время", } },
            //    { "сколько", new List<string>() { "сколько", "столько", } },
            //    { "текст", new List<string>() { "текст", "слова", } },
            //    { "скачиваем", new List<string>() { "скачиваем", "загружаем", "достаём" } },
            //    { "приложение", new List<string>() { "приложение", "апп", "приложуха", "программа" } },
            //    { "перейти", new List<string>() { "перейти", "зайти", "войти", "перейти к" } },
            //    { "найдите", new List<string>() { "найдите", "находите", "ищите", "найдите" } },
            //    { "подписаться", new List<string>() { "подписатся", "подписаться", } },
            //    { "нужно", new List<string>() { "нужно", "необходимо", } },
            //    { "логин", new List<string>() { "логин", "никнейм", } },
            //    { "тг", new List<string>() { "тг", "телеграм", "telega", "telegram" } },
            //    { "телеграм", new List<string>() { "тг", "телеграм", "telega", "telegram" } },
            //    { "привет", new List<string>() { "привет", "приветствую", "здравствуйте", "приветик" } },
            //    { "сайте", new List<string>() { "страничке", "веб сайте", } },
            //    { "ссылки", new List<string>() { "линки", "ссылки", "веб ссылка" } },
            //    { "слово", new List<string>() { "слово", "словосочетание", } },
            //};

            //List<string> approve_text_for_hand_tasks = new List<string>()
            //{
            //    "Все выполнила, надеюсь верно) вот мой логин(никнейм)\n@bransd",
            //    "my channel: https://www.youtube.com/@SantanuKumarArt\nВидео которое посмотрела и поставила лайк: https://www.youtube.com/watch?v=FqcYmr5leJI",
            //    "Привет! это бот Miniopros.\nС чего начнем знакомство?",
            //    "assdffff32",
            //    "Zippy",
            //    "Captain Cuddles",
            //    "Bubbles",
            //    "Whiskers",
            //    "Jamboree",
            //    "Snickerdoodle Snickerdoodle",
            //    "Snickerdoodle Snickerdoodle",
            //    "Gizmo",
            //    "Blaze Maverick",
            //    "Sparkle Shark",
            //    "Snazzy Muffin",
            //    "QuantumWhiskers",
            //    "PixelProwler",
            //    "Boomerang",
            //    "@Boomerang",
            //    "@RocketFox",
            //    "@Karma",
            //    "@Taz",
            //    "@Jamboree",
            //    "Важно учитывать индивидуальные особенности организма, следить за балансом белков, жиров и углеводов, а также регулярно консультироваться с диетологом.",
            //    "Чтение развивает воображение, улучшает память и расширяет словарный запас, а также помогает расслабиться и отвлечься от повседневных забот.",
            //    "Спорт укрепляет здоровье, повышает выносливость, улучшает настроение и помогает поддерживать форму, снижая риск многих заболеваний.",
            //    "  Планирование помогает эффективно распределять задачи, избегать стресса и достигать целей быстрее, улучшая качество жизни.",
            //    " Эмоциональный интеллект позволяет понимать свои эмоции и чувства других людей, что способствует успешной коммуникации и решению конфликтов.",
            //    "Волонтеры оказывают важную поддержку нуждающимся, способствуют развитию социальных инициатив и укрепляют дух сообщества.",
            //    "Путешествия расширяют кругозор, знакомят с новыми культурами, учат адаптироваться к разным условиям и приносят незабываемые впечатления.",
            //    "Музыка может влиять на наше настроение, помогать расслабляться или концентрироваться, а также выражать наши мысли и чувства.",
            //    "Знание иностранных языков открывает доступ к новой информации, культуре и возможностям общения, а также улучшает когнитивные способности.",
            //    "Сон необходим для восстановления физических сил, поддержания психического здоровья и укрепления иммунной системы. Он также важен для памяти и концентрации внимания.",
            //    " Соблюдение правил дорожного движения снижает вероятность аварий, защищает жизнь и здоровье участников движения, а также поддерживает порядок на дорогах.",
            //    "Вода необходима для нормального функционирования всех органов и систем организма, она участвует в обмене веществ, регулирует температуру тела и выводит токсины.",
            //    "Чтение вслух улучшает дикцию, развивает голосовые навыки, помогает лучше усваивать информацию и тренирует память. Оно также может быть полезным для публичных выступлений.",
            //    "Природа успокаивает ум, снижает уровень стресса, улучшает физическое состояние и заряжает энергией. Контакт с природой способствует общему благополучию.",
            //    "Творчество стимулирует мозг, развивает креативность, помогает находить нестандартные решения проблем и выражать себя через искусство.",
            //    "Образование способствует личностному росту, повышению уровня знаний и профессиональных навыков, а также развитию критического мышления и социальной адаптации.",
            //    "Поддержка близких людей помогает справляться со стрессовыми ситуациями, улучшает эмоциональное состояние и создает ощущение безопасности и уверенности.",
            //    "Медитация уменьшает тревожность, улучшает концентрацию, снижает уровень стресса и способствует внутреннему спокойствию и гармонии.",
            //    "Уход за собой повышает самооценку, улучшает настроение и уверенность в себе, а также создает положительное впечатление на окружающих.",
            //    "Каждые несколько месяцев.",
            //    "1024 гигабайт.",
            //    "66%.",
            //    "Два уровня.",
            //    "До 18 месяцев",
            //    "1024 байта",
            //    "Около 80%",
            //    "23 символа",
            //    "Около 60%",
            //    "1000 мегабит",
            //    "12 мегапикселей",
            //    "8K (7680x4320)",
            //    "На 50% (по закону Мура)",
            //    "В 1976 году",
            //    "XP, Vista, 7, 8, 10, 11",
            //    "Билл Гейтс и Пол Аллен",
            //    "Маунтин-Вью, Калифорния",
            //    "HTTP (HyperText Transfer Protocol).",
            //    "Через 31 год (2007 год)",
            //    "От $399",
            //    "1997 год",
            //    "Высокоуровневый интерпретируемый язык",
            //    "1946 год (ENIAC)",
            //    "Семь слоев",
            //    "Virtual Private Network (виртуальная частная сеть)",
            //    "Менее 3 секунд.",
            //    "30 миллиардов.",
            //    "256 символов",
            //    "Более 1 миллиарда",
            //    "Каждые несколько недель",
            //    "Около 7%",
            //    "8 минут 56 секунд («Baby Shark Dance»)",
            //    "Около 70%",
            //    "До 280 символов",
            //    "Более 1 миллиарда часов",
            //    "Около 69%",
            //    "До 280 символов.",
            //    "Около 1 миллиона.",
            //    "Около 62%.",
            //    "Более 500 миллионов.",
            //    "Около 95%.",
            //    "Около 33 минут в день.",
            //    "FastFox",
            //    "NightOwl",
            //    "TechGuru",
            //    "SkyWalker",
            //    "CryptoMaster",
            //    "@ArtLover",
            //    "@TravelBug",
            //    "BookWorm",
            //    "@BookWorm",
            //    "@MusicMania",
            //    "MusicMania",
            //    "@EcoWarrior",
            //    "EcoWarrior",
            //    "TravelBug",
            //    "ArtLover",
            //    "@CryptoMaster",
            //    "@TechGuru",
            //    "@NightOwl",
            //};

            //SocpublicAnketa dashashendel = new SocpublicAnketa("dashshendel", "business_real", "yes", "female", "23", "24");
            //SocpublicAnketa ikus = new SocpublicAnketa("ikuc365", "white", "no", "male", "30", "31");
            //SocpublicAnketa currentAccount = dashashendel;

            //int countTasks = 0;
            //bool changed = false;

            //foreach (dynamic item in data_for_auto_gen_tasks)
            //{
            //    if(countTasks >= 465 && !changed)
            //    {
            //        currentAccount = ikus;
            //        changed = true;
            //    }

            //    //change price
            //    if (item.price_user > 10 || item.price_user < 2)
            //        item.price_user = random.Next(3, 11);

            //    if (item.approve_type == "auto")
            //    {
            //        //change sinonims
            //        //if((description as string).Split(' ').ToList().ForEach())

            //        autoGenTasks.Add(new PlatformInternalAccountTaskModel()
            //        {
            //            MainAccountWhoExecuteTaskLogin = $"{currentAccount.login}",
            //            session = "",
            //            name = $"{item.name}",
            //            url = $"{item.url}",
            //            url_count = "",
            //            type = $"{item.type}",
            //            description = $"{item.description}",
            //            approve_type = $"{item.approve_type}",
            //            approve_count = "1",
            //            approve_text = $"{item.approve_text}",
            //            approve_quest_0 = "",
            //            approve_answer_0_1 = "",
            //            approve_answer_0_count = "1",
            //            approve_quest_1 = $"{item.approve_quest_1}",
            //            approve_answer_1_1 = $"{item.approve_answer_1_1}",
            //            approve_answer_1_count = "1",
            //            day_1 = "1",
            //            day_2 = "1",
            //            day_3 = "1",
            //            day_4 = "1",
            //            day_5 = "1",
            //            day_6 = "1",
            //            day_7 = "1",
            //            time_6_9_flag = "1",
            //            time_6_9 = "неогр.",
            //            time_9_12_flag = "1",
            //            time_9_12 = "неогр.",
            //            time_12_15_flag = "1",
            //            time_12_15 = "неогр.",
            //            time_15_18_flag = "1",
            //            time_15_18 = "неогр.",
            //            time_18_21_flag = "1",
            //            time_18_21 = "неогр.",
            //            time_21_24_flag = "1",
            //            time_21_24 = "неогр.",
            //            time_0_3_flag = "1",
            //            time_0_3 = "неогр.",
            //            time_3_6_flag = "1",
            //            time_3_6 = "неогр.",
            //            timeout = $"{random.Next(0, 31)}",
            //            work_filter = $"{currentAccount.work_filter}",
            //            family_filter = $"{currentAccount.family_filter}",
            //            gender_filter = $"{currentAccount.gender_filter}",
            //            age_from = $"{currentAccount.age_from}",
            //            age_to = $"{currentAccount.age_to}",
            //            geo_filter = "0",
            //            geo_ru = "1",
            //            geo_ru_48 = "1",
            //            geo_ru_47 = "1",
            //            geo_ru_66 = "1",
            //            geo_ru_71 = "1",
            //            geo_ru_38 = "1",
            //            geo_ru_111 = "1",
            //            geo_ru_13 = "1",
            //            geo_ru_53 = "1",
            //            geo_ru_65 = "1",
            //            geo_ru_86 = "1",
            //            geo_ru_51 = "1",
            //            geo_ru_8 = "1",
            //            geo_ru_29 = "1",
            //            geo_ru_73 = "1",
            //            geo_ru_61 = "1",
            //            geo_ua_5 = "1",
            //            geo_ua_14 = "1",
            //            geo_ru_other = "1",
            //            geo_ua = "1",
            //            geo_ua_13 = "1",
            //            geo_ua_12 = "1",
            //            geo_ua_4 = "1",
            //            geo_ua_7 = "1",
            //            geo_ua_26 = "1",
            //            geo_ua_17 = "1",
            //            geo_ua_other = "1",
            //            per_24 = $"{random.Next(0, 1000)}",
            //            repeat_value = $"{repeat_values[random.Next(0, repeat_values.Length)]}",
            //            work_time = $"{work_times[random.Next(0, work_times.Length)]}",
            //            repeat_before_check = "1",
            //            user_xp = $"{user_xps[random.Next(0, user_xps.Length)]}",
            //            ip_filter = "all",
            //            captcha_type = "no",
            //            ref_filter = "no_ref",
            //            proove_email = "1",
            //            proove_phone = "1",
            //            price_user = $"{item.price_user}",
            //            auto_funds = "0"
            //        });
            //    }
            //    else if (item.approve_type == "hand")
            //    {
            //        if(item.platformTaskTextForSolveTask == null)
            //        {
            //            string description = item.description;
            //            string approve_text = item.approve_text;

            //            if (description.Contains("скрин") || approve_text.Contains("скрин"))
            //            {
            //                item.platformTaskTextForSolveTask = $"http://socpublic.com/storage/uploads/2024/11/04/12/{random.Next(1718269285, 1718279285)}_{random.Next(9160119, 9170119)}_Снимок-экрана-{random.Next(212121, 291234)}.jpg";
            //            }
            //            else
            //            {
            //                item.platformTaskTextForSolveTask = approve_text_for_hand_tasks[random.Next(0, approve_text_for_hand_tasks.Count)];
            //            }
            //        }

            //        autoGenTasks.Add(new PlatformInternalAccountTaskModel()
            //        {
            //            MainAccountWhoExecuteTaskLogin = $"{currentAccount.login}",
            //            session = "",
            //            name = $"{item.name}",
            //            url = $"{item.url}",
            //            url_count = "",
            //            type = $"{item.type}",
            //            description = $"{item.description}",
            //            approve_type = $"{item.approve_type}",
            //            approve_count = "1",
            //            approve_text = $"{item.approve_text}",
            //            approve_quest_0 = "",
            //            approve_answer_0_1 = "",
            //            approve_answer_0_count = "1",
            //            approve_quest_1 = "",
            //            approve_answer_1_1 = "",
            //            approve_answer_1_count = "1",
            //            day_1 = "1",
            //            day_2 = "1",
            //            day_3 = "1",
            //            day_4 = "1",
            //            day_5 = "1",
            //            day_6 = "1",
            //            day_7 = "1",
            //            time_6_9_flag = "1",
            //            time_6_9 = "неогр.",
            //            time_9_12_flag = "1",
            //            time_9_12 = "неогр.",
            //            time_12_15_flag = "1",
            //            time_12_15 = "неогр.",
            //            time_15_18_flag = "1",
            //            time_15_18 = "неогр.",
            //            time_18_21_flag = "1",
            //            time_18_21 = "неогр.",
            //            time_21_24_flag = "1",
            //            time_21_24 = "неогр.",
            //            time_0_3_flag = "1",
            //            time_0_3 = "неогр.",
            //            time_3_6_flag = "1",
            //            time_3_6 = "неогр.",
            //            timeout = $"{random.Next(0, 31)}",
            //            work_filter = $"{currentAccount.work_filter}",
            //            family_filter = $"{currentAccount.family_filter}",
            //            gender_filter = $"{currentAccount.gender_filter}",
            //            age_from = $"{currentAccount.age_from}",
            //            age_to = $"{currentAccount.age_to}",
            //            geo_filter = "0",
            //            geo_ru = "1",
            //            geo_ru_48 = "1",
            //            geo_ru_47 = "1",
            //            geo_ru_66 = "1",
            //            geo_ru_71 = "1",
            //            geo_ru_38 = "1",
            //            geo_ru_111 = "1",
            //            geo_ru_13 = "1",
            //            geo_ru_53 = "1",
            //            geo_ru_65 = "1",
            //            geo_ru_86 = "1",
            //            geo_ru_51 = "1",
            //            geo_ru_8 = "1",
            //            geo_ru_29 = "1",
            //            geo_ru_73 = "1",
            //            geo_ru_61 = "1",
            //            geo_ua_5 = "1",
            //            geo_ua_14 = "1",
            //            geo_ru_other = "1",
            //            geo_ua = "1",
            //            geo_ua_13 = "1",
            //            geo_ua_12 = "1",
            //            geo_ua_4 = "1",
            //            geo_ua_7 = "1",
            //            geo_ua_26 = "1",
            //            geo_ua_17 = "1",
            //            geo_ua_other = "1",
            //            per_24 = $"{random.Next(0, 1000)}",
            //            repeat_value = $"{repeat_values[random.Next(0, repeat_values.Length)]}",
            //            work_time = $"{work_times[random.Next(0, work_times.Length)]}",
            //            repeat_before_check = "1",
            //            user_xp = $"{user_xps[random.Next(0, user_xps.Length)]}",
            //            ip_filter = "all",
            //            captcha_type = "no",
            //            ref_filter = "no_ref",
            //            proove_email = "1",
            //            proove_phone = "1",
            //            price_user = $"{item.price_user}",
            //            auto_funds = "0",
            //            platformTaskTextForSolveTask = $"{item.platformTaskTextForSolveTask}",
            //        });
            //    }

            //    countTasks++;
            //}

            //// save result
            ////await File.WriteAllTextAsync(@"C:\Users\glebg\Desktop\Zarplata Project v3.0\platform socpublic internal wirthdraw tasks files\json_auto_generated_auto_acc_tasks.json", JsonConvert.SerializeObject(autoGenTasks));
            //await File.WriteAllTextAsync(@"C:\Users\glebg\Desktop\Zarplata Project v3.0\platform socpublic internal wirthdraw tasks files\json_auto_generated_hand_acc_tasks.json", JsonConvert.SerializeObject(autoGenTasks));

            #endregion

            //dynamic? accs = JsonConvert.DeserializeObject<dynamic?>(File.ReadAllText(@"C:\Users\glebg\Desktop\Accounts.json"));

            //foreach (dynamic acc in accs!)
            //{
            //    acc.AuthBlumRefreshToken = "";
            //    acc.AuthBlumAccessToken = "";
            //}

            //File.WriteAllText(@"C:\Users\glebg\Desktop\Accounts.json", JsonConvert.SerializeObject(accs));








            //string timedata = await ProxyParseHttpClient.GetStringAsync("https://timeapi.io/api/time/current/ip?ipAddress=201.150.33.188");
            ////string timedata = await ProxyParseHttpClient.GetStringAsync("https://timeapi.io/api/time/current/ip?ipAddress=5.199.232.45");
            //dynamic? timeApiJsonData = JsonConvert.DeserializeObject<dynamic?>(timedata);
            //DateTime timeOfCurrentTimeZoneByIp = (DateTime)timeApiJsonData!.dateTime;
            //char offsetSymbol = '-';
            //string offset = "";
            //if(timeOfCurrentTimeZoneByIp < DateTime.UtcNow)
            //{
            //    offsetSymbol = '+';

            //    if(DateTime.UtcNow.Day > timeOfCurrentTimeZoneByIp.Day)
            //    {
            //        offset = ((DateTime.UtcNow.Hour + 24 - timeOfCurrentTimeZoneByIp.Hour) * 60).ToString();
            //    }
            //    else
            //    {
            //        offset = ((DateTime.UtcNow.Hour - timeOfCurrentTimeZoneByIp.Hour) * 60).ToString();
            //    }
            //}
            //else
            //{
            //    if (timeOfCurrentTimeZoneByIp.Day > DateTime.UtcNow.Day)
            //    {
            //        offset = ((timeOfCurrentTimeZoneByIp.Hour + 24 - DateTime.UtcNow.Hour) * 60).ToString();
            //    }
            //    else
            //    {
            //        offset = ((timeOfCurrentTimeZoneByIp.Hour - DateTime.UtcNow.Hour) * 60).ToString();
            //    }
            //}

            //offset = offsetSymbol + offset;







            //using var engine = new Engine();

            //using var module = Module.FromTextFile(engine, "C:\\Users\\glebg\\Desktop\\BlumPayloadGenerator-master\\wasm.wat");

            //var ___h = module.Exports.Where(e => e.Name == "_h").FirstOrDefault();

            

            //using var linker = new Linker(engine);
            //using var store = new Store(engine);


            

            //var instance = linker.Instantiate(store, module);

            //var _h = instance.GetAction("_h")!;
            //_h();






            Console.ReadKey();
        }


        //public static async Task<string> ReadResourceFileToString(string resourceFileName, Assembly assembly)
        //{
        //    string result = "";
        //    string fullResourceFileName = "";

        //    string[] resourceNamesArray = assembly.GetManifestResourceNames();
        //    if (resourceNamesArray == null || resourceNamesArray.Length == 0)
        //    {
        //        throw new Exception($"resource file with name '{resourceFileName}' is not exists in current assembly");
        //    }
        //    foreach (string resourceName in resourceNamesArray)
        //    {
        //        if (resourceName.Contains(resourceFileName))
        //        {
        //            fullResourceFileName = resourceName;
        //            break;
        //        }
        //    }

        //    using (Stream? resourceStream = assembly.GetManifestResourceStream(fullResourceFileName))
        //    {
        //        if (resourceStream == null)
        //        {
        //            throw new Exception($"resource file with name '{resourceFileName}' is not exists");
        //        }

        //        using (StreamReader reader = new StreamReader(resourceStream))
        //        {
        //            result = await reader.ReadToEndAsync();
        //        }
        //    }

        //    return result;
        //}
    }
}